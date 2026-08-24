using RadiantConnect.Methods;
using Serilog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Valorant_Instalocker.Main;
using Valorant_Instalocker.Main.API;
using Valorant_Instalocker.Main.Game;
using Valorant_Instalocker.Main.Helpers;

namespace Valorant_Instalocker.Views
{
    public partial class DashboardView : UserControl
    {
        public event EventHandler? OpenMapRulesRequested;

        private string? _selectedAgent;
        private Border? _activeCard;
        private readonly Brush _selectionBrush;

        public static readonly IReadOnlyDictionary<string, ValorantTables.Agent> NameToAgent =
            ValorantTables.AgentToId.ToDictionary(pair => pair.Key.ToString(), pair => pair.Key, StringComparer.OrdinalIgnoreCase);

        public DashboardView()
        {
            InitializeComponent();
            _selectionBrush = (Brush)FindResource("AccentPrimary");
            Loaded += DashboardView_Loaded;
            _ = LoadAgentsAsync();
        }

        private void DashboardView_Loaded(object sender, RoutedEventArgs e) => RefreshRulesSummary();

        public static string TranslatePhase(string phase) => Resource(phase.ToLowerInvariant() switch
        {
            "idle" or "menus" or "default" => "PhaseIdle",
            "pregame" => "PhasePregame",
            "ingame" or "gameplay" => "PhaseIngame",
            _ => "PhaseUnknown"
        });

        private async Task LoadAgentsAsync()
        {
            try
            {
                await AgentFetcher.UpdateAgents();
                var style = (Style)FindResource("AgentCardStyle");
                AgentsContainer.Children.Clear();
                foreach (var agent in AppStateManager.Agents.OrderBy(pair => pair.Key.ToString()))
                    AgentsContainer.Children.Add(AgentUIHelper.CreateAgentCard(agent.Value, agent.Key.ToString(), AgentCard_Click, style));

                AgentsMessage.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                AgentsMessage.Text = "Agents could not be loaded.";
                Log.Error(ex, "[Dashboard] Agent catalog could not be loaded.");
            }
        }

        private void AgentCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not Border clickedCard) return;
            if (_activeCard is not null)
            {
                _activeCard.BorderBrush = (Brush)FindResource("DefaultBorder");
                _activeCard.BorderThickness = new Thickness(1);
            }

            if (_activeCard == clickedCard)
            {
                _activeCard = null;
                _selectedAgent = null;
                ActionBtn.IsEnabled = false;
                ActionBtn.Content = "Choose an agent";
                return;
            }

            _activeCard = clickedCard;
            _activeCard.BorderBrush = _selectionBrush;
            _activeCard.BorderThickness = new Thickness(2);
            _selectedAgent = clickedCard.Tag?.ToString();
            ActionBtn.IsEnabled = AppStateManager.Instance.IsClientLoggedIn && !string.IsNullOrEmpty(_selectedAgent);
            ActionBtn.Content = AppStateManager.Instance.IsClientLoggedIn
                ? AppStateManager.OnlySelect ? $"Arm select: {_selectedAgent}" : $"Arm lock: {_selectedAgent}"
                : "VALORANT is offline";
        }

        private void OnlySelectCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (ActionBtn is null) return;
            AppStateManager.OnlySelect = OnlySelectCheckBox.IsChecked == true;
            if (!string.IsNullOrWhiteSpace(_selectedAgent))
                ActionBtn.Content = AppStateManager.Instance.IsClientLoggedIn
                    ? AppStateManager.OnlySelect ? $"Arm select: {_selectedAgent}" : $"Arm lock: {_selectedAgent}"
                    : "VALORANT is offline";
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedAgent) || !NameToAgent.TryGetValue(_selectedAgent, out var selectedAgent)) return;
            Controller.CreateInstalockGameHandler(selectedAgent, _selectedAgent, AppStateManager.Agents[selectedAgent]);
        }

        private void CancelInstalockAction_Click(object sender, RoutedEventArgs e) => Controller.CancelControlHandler();

        private void OpenMapRules_Click(object sender, RoutedEventArgs e) => OpenMapRulesRequested?.Invoke(this, EventArgs.Empty);

        private async void QuitPreGameAction_Click(object sender, RoutedEventArgs e)
        {
            await Controller.QuitPreGame();
            AppStateManager.Instance.QuitPreGameBtnBool = false;
        }

        private static string Resource(string key) => Application.Current.TryFindResource(key) as string ?? key;

        private void RefreshRulesSummary()
        {
            var settings = AppStateManager.CurrentUserSettings;
            var count = settings.MapAgentRules.Count;
            RulesModeButton.Content = count == 0 ? "Map rules" : $"Map rules · {count}";
        }
    }
}
