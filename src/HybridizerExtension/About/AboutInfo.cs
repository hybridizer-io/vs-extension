using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace HybridizerExtension.About
{
    /// <summary>What the About window shows.</summary>
    internal static class AboutInfo
    {
        public const string Title = "Hybridizer";
        public const string Copyright = "© 2026 Hybridizer";
        public const string AllRightsReserved = "All rights reserved";
        public const string Website = "https://hybridizer.net";

        /// <summary>
        /// The VSIX version, read from the extension.vsixmanifest installed next
        /// to the assembly; the assembly version when there is none (tests).
        /// </summary>
        public static string Version
        {
            get
            {
                var assembly = typeof(AboutInfo).Assembly;
                string manifest = Path.Combine(Path.GetDirectoryName(assembly.Location), "extension.vsixmanifest");
                return ReadVersion(manifest) ?? assembly.GetName().Version.ToString(3);
            }
        }

        internal static string ReadVersion(string manifestPath)
        {
            if (!File.Exists(manifestPath))
                return null;

            try
            {
                var identity = XDocument.Load(manifestPath).Descendants()
                    .FirstOrDefault(e => e.Name.LocalName == "Identity");
                string version = identity?.Attribute("Version")?.Value;
                return string.IsNullOrWhiteSpace(version) ? null : version;
            }
            catch (System.Xml.XmlException)
            {
                return null;
            }
        }
    }
}
