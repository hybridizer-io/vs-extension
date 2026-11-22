using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace HybridizerExtension.About
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            TitleText.Text = AboutInfo.Title;
            VersionText.Text = "Version " + AboutInfo.Version;
            CopyrightText.Text = AboutInfo.Copyright;
            RightsText.Text = AboutInfo.AllRightsReserved;
            WebsiteLink.NavigateUri = new Uri(AboutInfo.Website);
            WebsiteText.Text = AboutInfo.Website;
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
