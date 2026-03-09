using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace HybridizerExtension.Commands
{
    internal sealed class HybridizeProjectCommand
    {
        private readonly AsyncPackage _package;

        private HybridizeProjectCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package;

            // Register command in project context menu
            var contextCommandId = new CommandID(CommandIds.CommandSetGuid, CommandIds.HybridizeProjectContextCommandId);
            var contextItem = new MenuCommand(Execute, contextCommandId);
            commandService.AddCommand(contextItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                new HybridizeProjectCommand(package, commandService);
            }
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dte == null) return;

            var project = GetSelectedProject(dte);
            if (project == null)
            {
                VsShellUtilities.ShowMessageBox(
                    _package,
                    "Please select a C# project in Solution Explorer.",
                    "Hybridizer",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            string projectDir = Path.GetDirectoryName(project.FullName);
            if (string.IsNullOrEmpty(projectDir)) return;

            var report = new StringBuilder();
            var warnings = new StringBuilder();

            // 1. Validate prerequisites
            if (!CheckHybridizerTool(warnings))
            {
                warnings.AppendLine("- hybridizer dotnet tool not found. Install with: dotnet tool install -g hybridizer");
            }

            // 2. Check if already hybridized
            string propsPath = Path.Combine(projectDir, "Directory.Build.props");
            string targetsPath = Path.Combine(projectDir, "Directory.Build.targets");

            if (File.Exists(propsPath) || File.Exists(targetsPath))
            {
                var overwrite = VsShellUtilities.ShowMessageBox(
                    _package,
                    "Directory.Build.props and/or Directory.Build.targets already exist in this project directory.\n\nOverwrite them with Hybridizer build integration?",
                    "Hybridizer",
                    OLEMSGICON.OLEMSGICON_QUERY,
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND);

                if (overwrite != 6) // 6 = Yes
                    return;
            }

            // 3. Generate Directory.Build.props
            string propsContent = ReadEmbeddedResource("HybridizerExtension.Templates.Directory.Build.props.template");
            File.WriteAllText(propsPath, propsContent, Encoding.UTF8);
            report.AppendLine("Created: Directory.Build.props");

            // 4. Generate Directory.Build.targets
            string targetsContent = ReadEmbeddedResource("HybridizerExtension.Templates.Directory.Build.targets.template");
            File.WriteAllText(targetsPath, targetsContent, Encoding.UTF8);
            report.AppendLine("Created: Directory.Build.targets");

            // 5. Add Hybridizer.Runtime.CUDAImports PackageReference if missing
            if (AddCudaImportsReference(project.FullName))
            {
                report.AppendLine("Added PackageReference: Hybridizer.Runtime.CUDAImports");
            }
            else
            {
                report.AppendLine("PackageReference Hybridizer.Runtime.CUDAImports already present.");
            }

            // 6. Show results
            string message = "Project hybridized successfully!\n\n"
                + "Files created:\n" + report.ToString();

            if (warnings.Length > 0)
            {
                message += "\nAction required:\n" + warnings.ToString()
                    + "\nAdditional requirements:\n"
                    + "- NVIDIA CUDA Toolkit must be installed (nvcc in PATH)\n"
                    + "- Visual Studio 'Desktop development with C++' workload must be installed\n"
                    + "- An NVIDIA GPU must be present for compilation";
            }

            VsShellUtilities.ShowMessageBox(
                _package,
                message,
                "Hybridizer - Project Configured",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private Project GetSelectedProject(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Try to get from selected items in Solution Explorer
            if (dte.SelectedItems != null && dte.SelectedItems.Count > 0)
            {
                var selectedItem = dte.SelectedItems.Item(1);
                if (selectedItem.Project != null)
                    return selectedItem.Project;
            }

            // Fallback: active project
            if (dte.Solution?.Projects != null)
            {
                var activeProjects = dte.ActiveSolutionProjects as Array;
                if (activeProjects != null && activeProjects.Length > 0)
                    return activeProjects.GetValue(0) as Project;
            }

            return null;
        }

        private bool CheckHybridizerTool(StringBuilder warnings)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "tool list -g",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(10000);
                    return output.IndexOf("hybridizer", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool AddCudaImportsReference(string csprojPath)
        {
            try
            {
                var doc = XDocument.Load(csprojPath);
                XNamespace ns = doc.Root.GetDefaultNamespace();

                // Check if already referenced
                bool alreadyReferenced = doc.Descendants()
                    .Where(e => e.Name.LocalName == "PackageReference")
                    .Any(e => string.Equals(
                        e.Attribute("Include")?.Value,
                        "Hybridizer.Runtime.CUDAImports",
                        StringComparison.OrdinalIgnoreCase));

                if (alreadyReferenced)
                    return false;

                // Find or create an ItemGroup for PackageReferences
                var itemGroup = doc.Descendants()
                    .Where(e => e.Name.LocalName == "ItemGroup")
                    .FirstOrDefault(g => g.Elements()
                        .Any(e => e.Name.LocalName == "PackageReference"));

                if (itemGroup == null)
                {
                    itemGroup = new XElement(ns + "ItemGroup");
                    doc.Root.Add(itemGroup);
                }

                var packageRef = new XElement(ns + "PackageReference",
                    new XAttribute("Include", "Hybridizer.Runtime.CUDAImports"),
                    new XAttribute("Version", "3.0.0"));

                itemGroup.Add(packageRef);
                doc.Save(csprojPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
