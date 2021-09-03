using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace EvoCodeGen
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(EvoCodeGenPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(EvoCodeGenWindow))]
    public sealed class EvoCodeGenPackage : AsyncPackage
    {
        /// <summary>
        /// EvoCodeGenPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "9979a4b5-a2d3-4924-b788-2e806545c5e1";
        internal static DTE2 _dte;

        [Import]
        internal SVsServiceProvider ServiceProvider = null;

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            _dte = (await this.GetServiceAsync(typeof(DTE))) as DTE2;

            await EvoCodeGenWindowCommand.InitializeAsync(this);
        }

        #endregion
    }
}
