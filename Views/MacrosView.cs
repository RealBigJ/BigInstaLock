using System.Windows;
using System.Windows.Controls;
using RadiantConnect.Methods;
using Serilog;
using Valorant_Instalocker.Main;
using Valorant_Instalocker.Main.API;
using Valorant_Instalocker.Utils.Configs;

namespace Valorant_Instalocker.Views
{
    public partial class MacrosView : UserControl
    {
        private string? _editingMapId;
        private bool _ready;
        private bool _suppressSettingsEvents;

        private sealed record RuleRow(MapAgentRule Rule)
        {
            public string MapName => Rule.MapName;
            public string AgentLabel => Rule.AgentName;
            public string AgentImage => TryGetAgentImage(Rule.AgentName);
            public string ModeLabel => Rule.OnlySelect ? Resource("SelectMode") : Resource("LockMode");

            private static string TryGetAgentImage(string agentName)
            {
                if (!Enum.TryParse<ValorantTables.Agent>(agentName, true, out var agent)) return string.Empty;
                return AppStateManager.Agents.TryGetValue(agent, out var image) ? image : string.Empty;
            }
        }

        public MacrosView()
        {
            InitializeComponent();
            SyncEnabledState();
            Loaded += MacrosView_Loaded;
            RefreshRules();
        }

        private async void MacrosView_Loaded(object sender, RoutedEventArgs e)
        {
            SyncEnabledState();
            RefreshRules();
            if (_ready) return;

            try
            {
                var mapsTask = MapFetcher.GetMapsAsync();
                if (AppStateManager.Agents.Count == 0) await AgentFetcher.UpdateAgents();
                MapComboBox.ItemsSource = await mapsTask;
                AgentComboBox.ItemsSource = ValorantTables.AgentToId.Keys.Select(agent => agent.ToString()).OrderBy(name => name).ToList();
                if (MapComboBox.Items.Count > 0) MapComboBox.SelectedIndex = 0;
                if (AgentComboBox.Items.Count > 0) AgentComboBox.SelectedIndex = 0;
                _ready = true;
                RefreshRules();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[MapRules] Editor data could not be loaded.");
                ShowMessage(Resource("MapDataError"));
            }
        }

        private void SaveRule_Click(object sender, RoutedEventArgs e)
        {
            if (MapComboBox.SelectedItem is not ValorantMap map || AgentComboBox.SelectedItem is not string agentName)
            {
                ShowMessage(Resource("MapRuleValidation"));
                return;
            }

            var rules = AppStateManager.CurrentUserSettings.MapAgentRules;
            var lookupId = _editingMapId ?? map.MapId;
            var existing = rules.FirstOrDefault(rule => string.Equals(MapFetcher.NormalizeMapId(rule.MapId), MapFetcher.NormalizeMapId(lookupId), StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                existing = new MapAgentRule();
                rules.Add(existing);
            }

            existing.MapId = map.MapId;
            existing.MapName = map.DisplayName;
            existing.AgentName = agentName;
            existing.OnlySelect = SelectModeRadio.IsChecked == true;
            existing.Enabled = true;
            SaveAndRefresh();
            ResetEditor();
        }

        private void EditRule_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: RuleRow row }) return;
            _editingMapId = row.Rule.MapId;
            MapComboBox.SelectedItem = MapComboBox.Items.Cast<ValorantMap>().FirstOrDefault(map => string.Equals(MapFetcher.NormalizeMapId(map.MapId), MapFetcher.NormalizeMapId(row.Rule.MapId), StringComparison.OrdinalIgnoreCase));
            AgentComboBox.SelectedItem = row.Rule.AgentName;
            LockModeRadio.IsChecked = !row.Rule.OnlySelect;
            SelectModeRadio.IsChecked = row.Rule.OnlySelect;
            SaveRuleButton.Content = Resource("UpdateRuleButton");
            CancelEditButton.Visibility = Visibility.Visible;
        }

        private void DeleteRule_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: RuleRow row }) return;
            AppStateManager.CurrentUserSettings.MapAgentRules.Remove(row.Rule);
            if (string.Equals(_editingMapId, row.Rule.MapId, StringComparison.OrdinalIgnoreCase)) ResetEditor();
            SaveAndRefresh();
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e) => ResetEditor();

        private void MapRulesEnabled_Changed(object sender, RoutedEventArgs e)
        {
            if (_suppressSettingsEvents || MapRulesEnabledCheckBox is null) return;
            AppStateManager.CurrentUserSettings.MapRulesEnabled = MapRulesEnabledCheckBox.IsChecked == true;
            ConfigManager.SaveSettings(AppStateManager.CurrentUserSettings);
        }

        private void MapComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MapComboBox.SelectedItem is not ValorantMap map) return;
            MapPreviewImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(map.ImagePath, UriKind.Relative));
            MapPreviewName.Text = map.DisplayName;
        }

        private void AgentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AgentComboBox.SelectedItem is not string agentName) return;
            AgentPreviewName.Text = agentName;
            if (!Enum.TryParse<ValorantTables.Agent>(agentName, true, out var agent)
                || !AppStateManager.Agents.TryGetValue(agent, out var imagePath)
                || string.IsNullOrWhiteSpace(imagePath))
            {
                AgentPreviewImage.Source = null;
                return;
            }

            AgentPreviewImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath, UriKind.Absolute));
        }

        private void SaveAndRefresh()
        {
            ConfigManager.SaveSettings(AppStateManager.CurrentUserSettings);
            RefreshRules();
            EditorMessage.Visibility = Visibility.Collapsed;
        }

        private void RefreshRules()
        {
            var rows = AppStateManager.CurrentUserSettings.MapAgentRules.OrderBy(rule => rule.MapName).Select(rule => new RuleRow(rule)).ToList();
            RulesItemsControl.ItemsSource = rows;
            EmptyRulesPanel.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            RulesItemsControl.Visibility = rows.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            RulesCountText.Text = string.Format(Resource("RulesCount"), rows.Count);
        }

        private void ResetEditor()
        {
            _editingMapId = null;
            if (MapComboBox.Items.Count > 0) MapComboBox.SelectedIndex = 0;
            if (AgentComboBox.Items.Count > 0) AgentComboBox.SelectedIndex = 0;
            LockModeRadio.IsChecked = true;
            SaveRuleButton.Content = Resource("AddRuleButton");
            CancelEditButton.Visibility = Visibility.Collapsed;
            EditorMessage.Visibility = Visibility.Collapsed;
        }

        private void SyncEnabledState()
        {
            _suppressSettingsEvents = true;
            MapRulesEnabledCheckBox.IsChecked = AppStateManager.CurrentUserSettings.MapRulesEnabled;
            _suppressSettingsEvents = false;
        }

        private void ShowMessage(string message)
        {
            EditorMessage.Text = message;
            EditorMessage.Visibility = Visibility.Visible;
        }

        private static string Resource(string key) => Application.Current.TryFindResource(key) as string ?? key;
    }
}
