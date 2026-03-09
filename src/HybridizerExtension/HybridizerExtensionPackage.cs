using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Imaging;
using HybridizerExtension.Commands;
using HybridizerExtension.Telemetry;
using Task = System.Threading.Tasks.Task;

namespace HybridizerExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CommandIds.PackageGuidString)]
    [ProvideBindingPath]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(TelemetryOptionPage), "Hybridizer", "Telemetry", 0, 0, true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class HybridizerExtensionPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await HybridizeProjectCommand.InitializeAsync(this);
            await RegisterTelemetrySettingsCommandAsync();

            TelemetrySettings.SetEnvironmentVariable();

            if (!TelemetrySettings.HasUserResponded)
            {
                ShowTelemetryConsentDialog();
            }

            CheckTemplateInstallationAsync().Forget();
        }

        private async Task RegisterTelemetrySettingsCommandAsync()
        {
            var commandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var cmdId = new CommandID(CommandIds.CommandSetGuid, CommandIds.TelemetrySettingsCommandId);
                var menuItem = new MenuCommand(OnTelemetrySettings, cmdId);
                commandService.AddCommand(menuItem);
            }
        }

        private void OnTelemetrySettings(object sender, EventArgs e)
        {
            ShowOptionPage(typeof(TelemetryOptionPage));
        }

        private void ShowTelemetryConsentDialog()
        {
            var dialog = new TelemetryConsentDialog();
            var result = dialog.ShowDialog();

            TelemetrySettings.IsEnabled = result == true;
            TelemetrySettings.HasUserResponded = true;
            TelemetrySettings.SetEnvironmentVariable();
        }

        private async Task CheckTemplateInstallationAsync()
        {
            bool installed = await Task.Run(() => TemplateChecker.IsHybridizerTemplateInstalled());

            if (!installed)
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();
                ShowTemplateInfoBar();
            }
        }

        private void ShowTemplateInfoBar()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var shell = GetService(typeof(SVsShell)) as IVsShell;
            if (shell == null) return;

            shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var obj);
            if (obj is IVsInfoBarHost host)
            {
                var textSpans = new[]
                {
                    new InfoBarTextSpan("Hybridizer project template not found. Run: "),
                    new InfoBarTextSpan("dotnet new install Hybridizer.App.Template"),
                    new InfoBarTextSpan(" to enable the 'New Hybridizer Project' option.")
                };

                var actionItems = new[]
                {
                    new InfoBarHyperlink("Install Now")
                };

                var infoBar = new InfoBarModel(textSpans, actionItems, KnownMonikers.StatusInformation, isCloseButtonVisible: true);

                var factory = GetService(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
                if (factory == null) return;

                var element = factory.CreateInfoBar(infoBar);
                if (element != null)
                {
                    var events = new TemplateInfoBarEvents(this);
                    element.Advise(events, out _);
                    host.AddInfoBar(element);
                }
            }
        }
    }

    internal class TemplateInfoBarEvents : IVsInfoBarUIEvents
    {
        private readonly AsyncPackage _package;

        public TemplateInfoBarEvents(AsyncPackage package)
        {
            _package = package;
        }

        public void OnClosed(IVsInfoBarUIElement infoBarUIElement)
        {
        }

        public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (actionItem.Text == "Install Now")
            {
                _ = TemplateChecker.InstallTemplateAsync(_package);
                infoBarUIElement.Close();
            }
        }
    }

    internal static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
            if (!task.IsCompleted || task.IsFaulted)
            {
                _ = ForgetAwaited(task);
            }

            static async Task ForgetAwaited(Task task)
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch
                {
                    // Intentionally swallowed
                }
            }
        }
    }
}
