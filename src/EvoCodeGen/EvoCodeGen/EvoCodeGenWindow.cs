using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace EvoCodeGen
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("2832a603-97ed-437a-886b-fa8825059c8a")]
    public class EvoCodeGenWindow : ToolWindowPane
    {
        // mdl for T4 templates
        // sbn for scriban templates
        static readonly string[] _supportedExtensions = { ".mdl", ".sbn" };
        internal EvoCodeGenWindowViewModel ViewModel;
        private DTE2 _dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvoCodeGenWindow"/> class.
        /// </summary>
        public EvoCodeGenWindow() : base(null)
        {
            this.Caption = "Evo Code Generator";
        }

        protected override void Initialize()
        {
            base.Initialize();

            ThreadHelper.ThrowIfNotOnUIThread();

            _dte = EvoCodeGenPackage._dte;// this.GetService(typeof(DTE)) as DTE2;

            ViewModel = new EvoCodeGenWindowViewModel();

            this.Content = new EvoCodeGenWindowControl(ViewModel);
        }

    }
}
