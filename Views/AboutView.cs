using System.Windows;
using System.Windows.Controls;
using Valorant_Instalocker.Utils.Constants;

namespace Valorant_Instalocker.Views
{
    public partial class AboutView : UserControl
    {
        public AboutView() => InitializeComponent();

        private void ProjectGithub_Click(object sender, RoutedEventArgs e) => OpenUrl(Constants.ProjectGithubUrl);
        private void BerkweGithub_Click(object sender, RoutedEventArgs e) => OpenUrl(Constants.BerkweProjectUrl);

        private static void OpenUrl(string url)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
