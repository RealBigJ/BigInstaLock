using RadiantConnect.Methods;
using Serilog;
using Valorant_Instalocker.Main.API;

namespace Valorant_Instalocker.Main.Game
{
    internal static class MapRuleController
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized || !AppStateManager.Instance.IsClientLoggedIn) return;
            AppStateManager.initiator.GameEvents.PreGame.OnPreGameMatchLoaded += HandleMatchLoaded;
            _initialized = true;
            Log.Information("[MapRuleController] Map rules listener initialized.");
        }

        private static async void HandleMatchLoaded(string matchId)
        {
            try
            {
                var settings = AppStateManager.CurrentUserSettings;
                if (!settings.MapRulesEnabled || settings.MapAgentRules.Count == 0) return;
                if (AppStateManager.Instance.IsInstalockRunning) return;

                var match = await AppStateManager.initiator.Endpoints.PreGameEndpoints.FetchPreGameMatchAsync();
                var currentMapId = MapFetcher.NormalizeMapId(match?.MapId);
                var rule = settings.MapAgentRules.FirstOrDefault(candidate =>
                    candidate.Enabled && string.Equals(
                        MapFetcher.NormalizeMapId(candidate.MapId),
                        currentMapId,
                        StringComparison.OrdinalIgnoreCase));

                if (rule is null)
                {
                    Log.Information("[MapRuleController] No rule configured for map {MapId}.", currentMapId);
                    return;
                }

                if (!Enum.TryParse<ValorantTables.Agent>(rule.AgentName, true, out var agent)
                    || !ValorantTables.AgentToId.ContainsKey(agent))
                {
                    Log.Warning("[MapRuleController] Unknown agent in map rule: {AgentName}.", rule.AgentName);
                    return;
                }

                AppStateManager.SelectedAgent = agent;
                AppStateManager.OnlySelect = rule.OnlySelect;
                AppStateManager.Instance.SelectedAgentName = rule.AgentName;
                AppStateManager.Instance.SelectedAgentImage = AppStateManager.Agents.TryGetValue(agent, out var image)
                    ? image
                    : string.Empty;
                AppStateManager.Instance.ActiveMapName = rule.MapName;
                AppStateManager.Instance.IsMapRuleActive = true;

                Log.Information("[MapRuleController] Applying {Agent} on {Map} ({Mode}).",
                    rule.AgentName, rule.MapName, rule.OnlySelect ? "select" : "lock");
                Controller.GameControlHandler(matchId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[MapRuleController] Failed to apply map rule for match {MatchId}.", matchId);
            }
        }
    }
}
