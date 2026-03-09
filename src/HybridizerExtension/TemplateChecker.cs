using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace HybridizerExtension
{
    internal static class TemplateChecker
    {
        public static bool IsHybridizerTemplateInstalled()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "new list hybridizer-app",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(10000);
                    return output.IndexOf("hybridizer-app", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task InstallTemplateAsync(AsyncPackage package)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "new install Hybridizer.App.Template",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit(60000);

                    await package.JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (process.ExitCode == 0)
                    {
                        VsShellUtilities.ShowMessageBox(
                            package,
                            "Hybridizer project template installed successfully.\nYou can now create new Hybridizer projects from File > New > Project.",
                            "Hybridizer",
                            OLEMSGICON.OLEMSGICON_INFO,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    }
                    else
                    {
                        VsShellUtilities.ShowMessageBox(
                            package,
                            $"Failed to install template. Please run manually:\ndotnet new install Hybridizer.App.Template\n\nError: {error}",
                            "Hybridizer",
                            OLEMSGICON.OLEMSGICON_WARNING,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    }
                }
            }
            catch (Exception ex)
            {
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();
                VsShellUtilities.ShowMessageBox(
                    package,
                    $"Failed to install template: {ex.Message}\nPlease run manually: dotnet new install Hybridizer.App.Template",
                    "Hybridizer",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
    }
}
