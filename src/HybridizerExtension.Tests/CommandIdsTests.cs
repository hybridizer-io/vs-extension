using System;
using System.Linq;
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
        public void AboutCommandId_HasExpectedValue()
        {
            Assert.Equal(0x0103, CommandIds.AboutCommandId);
        }

        [Fact]
        public void CommandIds_AreDistinct()
        {
            var ids = new[]
            {
                CommandIds.HybridizeProjectContextCommandId,
                CommandIds.TelemetrySettingsCommandId,
                CommandIds.AboutCommandId,
            };
            Assert.Equal(ids.Length, ids.Distinct().Count());
        }

        [Fact]
        public void PackageGuid_DiffersFromCommandSetGuid()
        {
            Assert.NotEqual(CommandIds.PackageGuid, CommandIds.CommandSetGuid);
        }
    }
}
