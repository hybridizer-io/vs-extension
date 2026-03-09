using System;
using HybridizerExtension.Telemetry;
using Microsoft.Win32;
using Xunit;

namespace HybridizerExtension.Tests
{
    [Collection("TelemetrySettings")]
    public class TelemetrySettingsTests : IDisposable
    {
        private const string RegistryPath = @"Software\ALTIMESH\HYBRIDIZER";
        private readonly int? _originalTelemetryValue;
        private readonly int? _originalConsentValue;

        public TelemetrySettingsTests()
        {
            // Save original registry values
            using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                _originalTelemetryValue = key?.GetValue("TelemetryEnabled") as int?;
                _originalConsentValue = key?.GetValue("TelemetryConsentGiven") as int?;
            }
        }

        public void Dispose()
        {
            // Restore original registry values
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                if (_originalTelemetryValue.HasValue)
                    key.SetValue("TelemetryEnabled", _originalTelemetryValue.Value, RegistryValueKind.DWord);
                else
                    key.DeleteValue("TelemetryEnabled", throwOnMissingValue: false);

                if (_originalConsentValue.HasValue)
                    key.SetValue("TelemetryConsentGiven", _originalConsentValue.Value, RegistryValueKind.DWord);
                else
                    key.DeleteValue("TelemetryConsentGiven", throwOnMissingValue: false);
            }
        }

        [Fact]
        public void IsEnabled_RoundTrips_True()
        {
            TelemetrySettings.IsEnabled = true;
            Assert.True(TelemetrySettings.IsEnabled);
        }

        [Fact]
        public void IsEnabled_RoundTrips_False()
        {
            TelemetrySettings.IsEnabled = true;
            TelemetrySettings.IsEnabled = false;
            Assert.False(TelemetrySettings.IsEnabled);
        }

        [Fact]
        public void HasUserResponded_RoundTrips_True()
        {
            TelemetrySettings.HasUserResponded = true;
            Assert.True(TelemetrySettings.HasUserResponded);
        }

        [Fact]
        public void HasUserResponded_RoundTrips_False()
        {
            TelemetrySettings.HasUserResponded = true;
            TelemetrySettings.HasUserResponded = false;
            Assert.False(TelemetrySettings.HasUserResponded);
        }

        [Fact]
        public void SetEnvironmentVariable_SetsTo1_WhenEnabled()
        {
            TelemetrySettings.IsEnabled = true;
            TelemetrySettings.SetEnvironmentVariable();

            Assert.Equal("1", Environment.GetEnvironmentVariable("HYBRIDIZER_TELEMETRY_ENABLED"));
        }

        [Fact]
        public void SetEnvironmentVariable_SetsTo0_WhenDisabled()
        {
            TelemetrySettings.IsEnabled = false;
            TelemetrySettings.SetEnvironmentVariable();

            Assert.Equal("0", Environment.GetEnvironmentVariable("HYBRIDIZER_TELEMETRY_ENABLED"));
        }

        [Fact]
        public void IsEnabled_And_HasUserResponded_AreIndependent()
        {
            TelemetrySettings.IsEnabled = true;
            TelemetrySettings.HasUserResponded = false;

            Assert.True(TelemetrySettings.IsEnabled);
            Assert.False(TelemetrySettings.HasUserResponded);
        }
    }
}
