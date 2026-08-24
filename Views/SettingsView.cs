using System.IO;
using System.Windows;
using System.Windows.Controls;
using Valorant_Instalocker.Main;
using Valorant_Instalocker.Utils.Configs;
using Valorant_Instalocker.Utils.Constants;

namespace Valorant_Instalocker.Views
{
    public partial class SettingsView : UserControl
    {
        private bool _ready;

        public SettingsView()
        {
            InitializeComponent();
            StoragePathText.Text = Constants.InstalockerPath;
            Loaded += SettingsView_Loaded;
        }

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            _ready = false;
            MapRulesCheckBox.IsChecked = AppStateManager.CurrentUserSettings.MapRulesEnabled;
            AnimationsCheckBox.IsChecked = AppStateManager.CurrentUserSettings.EnableAnimations;
            ClearRulesButton.IsEnabled = AppStateManager.CurrentUserSettings.MapAgentRules.Count > 0;
            _ready = true;
        }

        private void Preference_Changed(object sender, RoutedEventArgs e)
        {
            if (!_ready) return;
            AppStateManager.CurrentUserSettings.MapRulesEnabled = MapRulesCheckBox.IsChecked == true;
            AppStateManager.CurrentUserSettings.EnableAnimations = AnimationsCheckBox.IsChecked == true;
            ConfigManager.SaveSettings(AppStateManager.CurrentUserSettings);
            SettingsMessage.Visibility = Visibility.Visible;
        }

        private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(Constants.InstalockerPath);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", Constants.InstalockerPath) { UseShellExecute = true });
        }

        private void ClearRules_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CustomMessageBox("Remove every saved map rule?", "Clear map rules") { Owner = Window.GetWindow(this) };
            dialog.ShowDialog();
            if (!dialog.Result) return;

            AppStateManager.CurrentUserSettings.MapAgentRules.Clear();
            ConfigManager.SaveSettings(AppStateManager.CurrentUserSettings);
            ClearRulesButton.IsEnabled = false;
            SettingsMessage.Text = "Map rules cleared";
            SettingsMessage.Visibility = Visibility.Visible;
        }
    }
}
