using System;
using HybridizerExtension.Commands;
using Xunit;

namespace HybridizerExtension.Tests
{
    public class CommandIdsTests
    {
        [Fact]
        public void PackageGuidString_IsValidGuid()
        {
            Assert.True(Guid.TryParse(CommandIds.PackageGuidString, out _));
        }

        [Fact]
        public void PackageGuid_MatchesPackageGuidString()
        {
            Assert.Equal(new Guid(CommandIds.PackageGuidString), CommandIds.PackageGuid);
        }

        [Fact]
        public void CommandSetGuidString_IsValidGuid()
        {
            Assert.True(Guid.TryParse(CommandIds.CommandSetGuidString, out _));
        }

        [Fact]
        public void CommandSetGuid_MatchesCommandSetGuidString()
        {
            Assert.Equal(new Guid(CommandIds.CommandSetGuidString), CommandIds.CommandSetGuid);
        }

        [Fact]
        public void HybridizeProjectContextCommandId_HasExpectedValue()
        {
            Assert.Equal(0x0101, CommandIds.HybridizeProjectContextCommandId);
        }

        [Fact]
        public void TelemetrySettingsCommandId_HasExpectedValue()
        {
            Assert.Equal(0x0102, CommandIds.TelemetrySettingsCommandId);
        }

        [Fact]
        public void CommandIds_AreDistinct()
        {
            Assert.NotEqual(CommandIds.HybridizeProjectContextCommandId, CommandIds.TelemetrySettingsCommandId);
        }

        [Fact]
        public void PackageGuid_DiffersFromCommandSetGuid()
        {
            Assert.NotEqual(CommandIds.PackageGuid, CommandIds.CommandSetGuid);
        }
    }
}
