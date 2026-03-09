using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace HybridizerExtension.Telemetry
{
    [ComVisible(true)]
    [Guid("c3d4e5f6-a7b8-9012-cdef-345678901234")]
    public class TelemetryOptionPage : DialogPage
    {
        private bool _enableTelemetry;

        [Category("Hybridizer Telemetry")]
        [DisplayName("Enable Telemetry")]
        [Description(
            "When enabled, Hybridizer collects anonymous usage metrics (compilation time, " +
            "amount of generated code, generation time, GPU architecture, build success/failure). " +
            "No source code, file names, or personal information is ever collected. " +
            "Data is used solely by Hybridizer and is never shared with third parties.")]
        public bool EnableTelemetry
        {
            get => _enableTelemetry;
            set
            {
                _enableTelemetry = value;
                TelemetrySettings.IsEnabled = value;
                TelemetrySettings.SetEnvironmentVariable();
            }
        }

        public override void LoadSettingsFromStorage()
        {
            _enableTelemetry = TelemetrySettings.IsEnabled;
        }

        public override void SaveSettingsToStorage()
        {
            TelemetrySettings.IsEnabled = _enableTelemetry;
            TelemetrySettings.SetEnvironmentVariable();
        }
    }
}
