using System.IO;
using System.Linq;
using System.Xml.Linq;
using HybridizerExtension.Commands;
using Xunit;

namespace HybridizerExtension.Tests
{
    public class AddCudaImportsReferenceTests
    {
        private readonly HybridizeProjectCommand _command;

        public AddCudaImportsReferenceTests()
        {
            // Create instance without VS services (only using AddCudaImportsReference)
            _command = TestHelpers.CreateHybridizeProjectCommand();
        }

        [Fact]
        public void AddsPackageReference_WhenNoneExists()
        {
            string path = CreateTempCsproj(@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.1"" />
  </ItemGroup>
</Project>");

            try
            {
                bool result = _command.AddCudaImportsReference(path);

                Assert.True(result);
                var doc = XDocument.Load(path);
                Assert.Contains(doc.Descendants(),
                    e => e.Name.LocalName == "PackageReference" &&
                         e.Attribute("Include")?.Value == "Hybridizer.Runtime.CUDAImports");
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void ReturnsFalse_WhenAlreadyReferenced()
        {
            string path = CreateTempCsproj(@"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""Hybridizer.Runtime.CUDAImports"" Version=""3.0.0"" />
  </ItemGroup>
</Project>");

            try
            {
                bool result = _command.AddCudaImportsReference(path);
                Assert.False(result);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void AlreadyReferenced_CaseInsensitive()
        {
            string path = CreateTempCsproj(@"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""hybridizer.runtime.cudaimports"" Version=""2.0.0"" />
  </ItemGroup>
</Project>");

            try
            {
                bool result = _command.AddCudaImportsReference(path);
                Assert.False(result);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void CreatesItemGroup_WhenNoPackageReferenceGroupExists()
        {
            string path = CreateTempCsproj(@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>");

            try
            {
                bool result = _command.AddCudaImportsReference(path);

                Assert.True(result);
                var doc = XDocument.Load(path);
                Assert.Contains(doc.Descendants(),
                    e => e.Name.LocalName == "PackageReference" &&
                         e.Attribute("Include")?.Value == "Hybridizer.Runtime.CUDAImports");
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void PreservesExistingReferences()
        {
            string path = CreateTempCsproj(@"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.1"" />
  </ItemGroup>
</Project>");

            try
            {
                _command.AddCudaImportsReference(path);

                var doc = XDocument.Load(path);
                Assert.Contains(doc.Descendants(),
                    e => e.Name.LocalName == "PackageReference" &&
                         e.Attribute("Include")?.Value == "Newtonsoft.Json");
                Assert.Contains(doc.Descendants(),
                    e => e.Name.LocalName == "PackageReference" &&
                         e.Attribute("Include")?.Value == "Hybridizer.Runtime.CUDAImports");
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void AddedReference_HasVersion3()
        {
            string path = CreateTempCsproj(@"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.1"" />
  </ItemGroup>
</Project>");

            try
            {
                _command.AddCudaImportsReference(path);

                var doc = XDocument.Load(path);
                var pkgRef = doc.Descendants()
                    .First(e => e.Name.LocalName == "PackageReference" &&
                                e.Attribute("Include")?.Value == "Hybridizer.Runtime.CUDAImports");
                Assert.Equal("3.0.0", pkgRef.Attribute("Version")?.Value);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void PreservesNamespaceInLegacyProject()
        {
            string path = CreateTempCsproj(@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup>
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.1"" />
  </ItemGroup>
</Project>");

            try
            {
                bool result = _command.AddCudaImportsReference(path);

                Assert.True(result);
                var doc = XDocument.Load(path);
                Assert.Contains(doc.Descendants(),
                    e => e.Name.LocalName == "PackageReference" &&
                         e.Attribute("Include")?.Value == "Hybridizer.Runtime.CUDAImports");
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void ReturnsFalse_WhenFileDoesNotExist()
        {
            bool result = _command.AddCudaImportsReference(@"C:\nonexistent\path\project.csproj");
            Assert.False(result);
        }

        [Fact]
        public void ReturnsFalse_WhenFileIsMalformedXml()
        {
            string path = CreateTempCsproj("this is not xml <<>>");

            try
            {
                bool result = _command.AddCudaImportsReference(path);
                Assert.False(result);
            }
            finally { File.Delete(path); }
        }

        private static string CreateTempCsproj(string content)
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csproj");
            File.WriteAllText(path, content);
            return path;
        }
    }
}
