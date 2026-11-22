using System;
using System.IO;
using HybridizerExtension.About;
using Xunit;

namespace HybridizerExtension.Tests
{
    public class AboutInfoTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "AboutInfoTests_" + Guid.NewGuid().ToString("N"));

        public AboutInfoTests()
        {
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            Directory.Delete(_dir, true);
        }

        private string WriteManifest(string content)
        {
            string path = Path.Combine(_dir, "extension.vsixmanifest");
            File.WriteAllText(path, content);
            return path;
        }

        [Fact]
        public void Texts_SayHybridizerNotAltimesh()
        {
            foreach (var text in new[] { AboutInfo.Title, AboutInfo.Copyright, AboutInfo.Website })
            {
                Assert.Contains("ybridizer", text);
                Assert.DoesNotContain("ltimesh", text, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void Copyright_HasThisYear()
        {
            Assert.Equal("© 2026 Hybridizer", AboutInfo.Copyright);
        }

        [Fact]
        public void ReadVersion_ReturnsIdentityVersion()
        {
            string path = WriteManifest(
                @"<PackageManifest Version=""2.0.0"" xmlns=""http://schemas.microsoft.com/developer/vsx-schema/2011"">
                    <Metadata><Identity Id=""x"" Version=""1.2.3"" Language=""en-US"" Publisher=""p"" /></Metadata>
                  </PackageManifest>");

            Assert.Equal("1.2.3", AboutInfo.ReadVersion(path));
        }

        [Fact]
        public void ReadVersion_MissingFile_ReturnsNull()
        {
            Assert.Null(AboutInfo.ReadVersion(Path.Combine(_dir, "none.vsixmanifest")));
        }

        [Fact]
        public void ReadVersion_InvalidXml_ReturnsNull()
        {
            Assert.Null(AboutInfo.ReadVersion(WriteManifest("<not xml")));
        }

        [Fact]
        public void Version_FallsBackToAssemblyVersion()
        {
            // No extension.vsixmanifest next to the test copy of the assembly
            Assert.Matches(@"^\d+\.\d+\.\d+$", AboutInfo.Version);
        }
    }
}
