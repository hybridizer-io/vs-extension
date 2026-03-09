using System.Windows;

namespace HybridizerExtension.Telemetry
{
    public partial class TelemetryConsentDialog : Window
    {
        public TelemetryConsentDialog()
        {
            InitializeComponent();
        }

        private void OnAccept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnDecline_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
