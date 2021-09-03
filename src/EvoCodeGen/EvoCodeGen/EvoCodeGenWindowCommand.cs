using EnvDTE;
using EnvDTE80;
using EvoCodeGen.Core.Models;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace EvoCodeGen
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class EvoCodeGenWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("14080aa9-4ace-4ed4-8c93-6192803a50da");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;
        // mdl for T4 templates
        // sbn for scriban templates
        static readonly string[] _supportedExtensions = { ".mdl", ".sbn" };
        private EvoCodeGenWindow _window;
        private Project _current_project;

        internal EvoCodeGenWindow Window
        {
            get
            {
                return _window;
            }
            set
            {
                if (_window == value) return;
                _window = value;
                _window.ViewModel.GenerateCode += (ok) =>
                {

                    this.package.JoinableTaskFactory.RunAsync(async delegate
                    {
                        var result = await new EvoCodeGenerator(new EvoCodeGeneratorConfiguration()).GenerateCode(new EvoCodeGeneratorModel
                        {
                            FolderItem = _window.ViewModel.SelectFolder,
                            FolderProject = _window.ViewModel.ProjectFolder,
                            JsonModel = _window.ViewModel.JsonModel,
                            Prefix = _window.ViewModel.ModelName,
                            TemplateFiles = _window.ViewModel.Templates.Cast<string>(),
                            StaticReplacer = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("[GLOBAL_NAMESPACE", _current_project.GetRootNamespace())
                        }
                        });

                        try
                        {
                            foreach (var fileCreated in result.Files.Where(f => f.FileStatus == GenerationResultFile.Status.Created))
                            {
                                if (_current_project != null)
                                {
                                    _current_project.AddFileToProject(new FileInfo(fileCreated.FilePath));
                                }

                                //VsShellUtilities.OpenDocument(this, file.FullName);

                                // Move cursor into position
                                //if (position > 0)
                                //{
                                //    Microsoft.VisualStudio.Text.Editor.IWpfTextView view = ProjectHelpers.GetCurentTextView();

                                //    if (view != null)
                                //        view.Caret.MoveTo(new SnapshotPoint(view.TextBuffer.CurrentSnapshot, position));
                                //}

                                //_dte.ExecuteCommand("SolutionExplorer.SyncWithActiveDocument");
                                //_dte.ActiveDocument.Activate();
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }
                    });
                };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvoCodeGenWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private EvoCodeGenWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static EvoCodeGenWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in EvoCodeGenWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new EvoCodeGenWindowCommand(package, commandService);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                Window = (EvoCodeGenWindow)await this.package.ShowToolWindowAsync(typeof(EvoCodeGenWindow), 0, true, this.package.DisposalToken);
                
                if ((null == Window) || (null == Window.Frame))
                {
                    throw new NotSupportedException("Cannot create tool window");
                }

                var target = NewItemTarget.Create(EvoCodeGenPackage._dte);
                if (target == null)
                {
                    MessageBox.Show(
                            "Could not determine where to generate code. Select a file or folder in Solution Explorer and try again.",
                            "Evo Code Generator",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    return;
                }

                var folder = target.Directory;
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                {
                    MessageBox.Show(
                            "Could not determine where to generate code. Select a file or folder in Solution Explorer and try again.",
                            "Evo Code Generator",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    return;
                }

                _current_project = target.Project ?? ProjectHelpers.GetActiveProject();
                if (_current_project == null)
                {
                    MessageBox.Show(
                            "Could not determine where to generate code. Select a file or folder in Solution Explorer and try again.",
                            "Evo Code Generator",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    return;
                }
                var dir = new DirectoryInfo(folder);
                var rootNamespace = _current_project.GetRootNamespace();

                // Fetch template files to give user option to select all or unselect one or more template generated
                var folderProject = _current_project.GetRootFolder();
                var templateFiles = Directory.EnumerateFiles(folderProject, "*.*", SearchOption.AllDirectories)
                    .Where(f => _supportedExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                if (templateFiles == null || !templateFiles.Any())
                    return;

                // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
                // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
                // the object returned by the Content property.
                Window.ViewModel.SelectFolder = folder;
                Window.ViewModel.ProjectFolder = folderProject;
                Window.ViewModel.RootNamespace = rootNamespace;
                Window.ViewModel.Templates = new System.Windows.Data.CollectionView(templateFiles);

                
            });
        }
    }
}
