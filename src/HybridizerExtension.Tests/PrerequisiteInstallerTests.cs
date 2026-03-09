using Xunit;

namespace HybridizerExtension.Tests
{
    [Trait("Category", "Integration")]
    public class PrerequisiteInstallerTests
    {
        [Fact]
        public void IsHybridizerToolInstalled_DoesNotThrow()
        {
            // Should complete without exception regardless of whether the tool is installed
            bool result = PrerequisiteInstaller.IsHybridizerToolInstalled();
            // result is either true or false — just verify no exception
            Assert.True(result || !result);
        }

        [Fact]
        public void IsHybridizerTemplateInstalled_DoesNotThrow()
        {
            bool result = PrerequisiteInstaller.IsHybridizerTemplateInstalled();
            Assert.True(result || !result);
        }

        [Fact]
        public void IsRuntimePackageAvailable_DoesNotThrow()
        {
            bool result = PrerequisiteInstaller.IsRuntimePackageAvailable();
            Assert.True(result || !result);
        }

        [Fact]
        public async System.Threading.Tasks.Task EnsureAllInstalledAsync_DoesNotThrow()
        {
            var failures = await PrerequisiteInstaller.EnsureAllInstalledAsync();
            Assert.NotNull(failures);
        }
    }
}
