using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;

namespace HybridizerExtension
{
    internal static class PrerequisiteInstaller
    {
        public static bool IsHybridizerToolInstalled()
        {
            return RunDotnetCheck("tool list -g", "hybridizer");
        }

        public static bool IsHybridizerTemplateInstalled()
        {
            return RunDotnetCheck("new list hybridizer-app", "hybridizer-app");
        }

        public static bool IsRuntimePackageAvailable()
        {
            // NuGet packages are per-project; just check nuget.org can resolve it
            return RunDotnetCheck("package search Hybridizer.Runtime.CUDAImports --take 1", "Hybridizer.Runtime.CUDAImports");
        }

        /// <summary>
        /// Checks all prerequisites and installs any that are missing.
        /// Returns a list of items that failed to install.
        /// </summary>
        public static async Task<List<string>> EnsureAllInstalledAsync()
        {
            var failures = new List<string>();

            await Task.Run(() =>
            {
                if (!IsHybridizerToolInstalled())
                {
                    if (!RunDotnetCommand("tool install -g hybridizer"))
                        failures.Add("dotnet tool install -g hybridizer");
                }

                if (!IsHybridizerTemplateInstalled())
                {
                    if (!RunDotnetCommand("new install Hybridizer.App.Template"))
                        failures.Add("dotnet new install Hybridizer.App.Template");
                }

                // Hybridizer.Runtime.CUDAImports is a per-project PackageReference,
                // not a global install. We just verify it exists on nuget.org.
                // It gets added to individual projects by the "Hybridize Project" command.
            });

            return failures;
        }

        private static bool RunDotnetCheck(string arguments, string expectedOutput)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(15000);
                    return output.IndexOf(expectedOutput, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool RunDotnetCommand(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process.StandardOutput.ReadToEnd();
                    process.StandardError.ReadToEnd();
                    process.WaitForExit(120000);
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
