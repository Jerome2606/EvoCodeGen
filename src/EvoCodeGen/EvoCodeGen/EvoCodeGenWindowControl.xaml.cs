using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace EvoCodeGen
{
    /// <summary>
    /// Interaction logic for EvoCodeGenWindowControl.
    /// </summary>
    public partial class EvoCodeGenWindowControl : UserControl
    {
        private static List<string> _tips = new List<string> {
            "Tip: 'folder/file.ext' also creates a new folder for the file",
            "Tip: You can create files starting with a dot, like '.gitignore'",
            "Tip: You can create files without file extensions, like 'LICENSE'",
            "Tip: Create folder by ending the name with a forward slash",
            "Tip: Use glob style syntax to add related files, like 'widget.(html,js)'",
            "Tip: Separate names with commas to add multiple files and folders"
        };
        public EvoCodeGenWindowViewModel viewModel { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="EvoCodeGenWindowControl"/> class.
        /// </summary>
        public EvoCodeGenWindowControl(EvoCodeGenWindowViewModel viewModel)
        {
            this.InitializeComponent();

            this.viewModel = viewModel;
            this.DataContext = this.viewModel;

            this.viewModel.GenerateCode += (ok) =>
            {
                (Window.GetWindow(this)).Close();
            };
        }
    }
}