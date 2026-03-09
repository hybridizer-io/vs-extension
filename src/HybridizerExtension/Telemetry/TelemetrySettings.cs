using System;
using Microsoft.Win32;

namespace HybridizerExtension.Telemetry
{
    internal static class TelemetrySettings
    {
        private const string RegistryPath = @"Software\ALTIMESH\HYBRIDIZER";
        private const string TelemetryValueName = "TelemetryEnabled";
        private const string ConsentValueName = "TelemetryConsentGiven";
        private const string EnvironmentVariableName = "HYBRIDIZER_TELEMETRY_ENABLED";

        public static bool IsEnabled
        {
            get
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                    {
                        if (key == null) return false;
                        var value = key.GetValue(TelemetryValueName);
                        return value is int intVal && intVal == 1;
                    }
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                    {
                        key.SetValue(TelemetryValueName, value ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
                catch
                {
                    // Silently fail if registry is not accessible
                }
            }
        }

        public static bool HasUserResponded
        {
            get
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                    {
                        if (key == null) return false;
                        var value = key.GetValue(ConsentValueName);
                        return value is int intVal && intVal == 1;
                    }
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                    {
                        key.SetValue(ConsentValueName, value ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
                catch
                {
                    // Silently fail
                }
            }
        }

        public static void SetEnvironmentVariable()
        {
            Environment.SetEnvironmentVariable(EnvironmentVariableName, IsEnabled ? "1" : "0");
        }
    }
}
