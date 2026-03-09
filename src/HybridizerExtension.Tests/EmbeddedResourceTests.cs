using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace HybridizerExtension.Tests
{
    public class EmbeddedResourceTests
    {
        private static readonly Assembly _assembly;

        static EmbeddedResourceTests()
        {
            // Use CodeBase to get original path before shadow-copying
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            string localPath = new System.Uri(codeBase).LocalPath;
            string assemblyPath = Path.Combine(
                Path.GetDirectoryName(localPath),
                "HybridizerExtension.dll");
            byte[] bytes = File.ReadAllBytes(assemblyPath);
            _assembly = Assembly.Load(bytes);
        }

        private static string ReadResource(string resourceName)
        {
            using (var stream = _assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        [Fact]
        public void DirectoryBuildPropsTemplate_IsEmbedded()
        {
            Assert.Contains("HybridizerExtension.Templates.Directory.Build.props.template",
                _assembly.GetManifestResourceNames());
        }

        [Fact]
        public void DirectoryBuildPropsTemplate_IsValidXml()
        {
            string content = ReadResource("HybridizerExtension.Templates.Directory.Build.props.template");
            var doc = XDocument.Parse(content);
            Assert.NotNull(doc.Root);
        }

        [Fact]
        public void DirectoryBuildPropsTemplate_ContainsHybridizerEnabled()
        {
            string content = ReadResource("HybridizerExtension.Templates.Directory.Build.props.template");
            var doc = XDocument.Parse(content);

            var element = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "HybridizerEnabled");
            Assert.NotNull(element);
            Assert.Equal("true", element.Value);
        }

        [Fact]
        public void DirectoryBuildTargetsTemplate_IsEmbedded()
        {
            Assert.Contains("HybridizerExtension.Templates.Directory.Build.targets.template",
                _assembly.GetManifestResourceNames());
        }

        [Fact]
        public void DirectoryBuildTargetsTemplate_IsValidXml()
        {
            string content = ReadResource("HybridizerExtension.Templates.Directory.Build.targets.template");
            var doc = XDocument.Parse(content);
            Assert.NotNull(doc.Root);
        }

        [Fact]
        public void DirectoryBuildTargetsTemplate_ContainsGenerateCUDATarget()
        {
            string content = ReadResource("HybridizerExtension.Templates.Directory.Build.targets.template");
            var doc = XDocument.Parse(content);

            Assert.Contains(doc.Descendants(),
                e => e.Name.LocalName == "Target" &&
                     e.Attribute("Name")?.Value == "GenerateCUDA");
        }

        [Fact]
        public void DirectoryBuildTargetsTemplate_ContainsCompileCUDATarget()
        {
            string content = ReadResource("HybridizerExtension.Templates.Directory.Build.targets.template");
            var doc = XDocument.Parse(content);

            Assert.Contains(doc.Descendants(),
                e => e.Name.LocalName == "Target" &&
                     e.Attribute("Name")?.Value == "CompileCUDA");
        }

        [Fact]
        public void DirectoryBuildTargetsTemplate_ContainsDetectGPUArchTarget()
        {
            string content = ReadResource("HybridizerExtension.Templates.Directory.Build.targets.template");
            var doc = XDocument.Parse(content);

            Assert.Contains(doc.Descendants(),
                e => e.Name.LocalName == "Target" &&
                     e.Attribute("Name")?.Value == "DetectGPUArch");
        }

        [Fact]
        public void DirectoryBuildTargetsTemplate_ContainsDetectVisualStudioTarget()
        {
            string content = ReadResource("HybridizerExtension.Templates.Directory.Build.targets.template");
            var doc = XDocument.Parse(content);

            Assert.Contains(doc.Descendants(),
                e => e.Name.LocalName == "Target" &&
                     e.Attribute("Name")?.Value == "DetectVisualStudio");
        }
    }
}
