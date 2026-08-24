namespace Valorant_Instalocker.Utils.Configs
{
    public sealed record MapAgentRule
    {
        public string MapId { get; set; } = "";
        public string MapName { get; set; } = "";
        public string AgentName { get; set; } = "";
        public bool OnlySelect { get; set; }
        public bool Enabled { get; set; } = true;
    }

    public record UserSettings
    {
        public bool MapRulesEnabled { get; set; } = true;
        public bool EnableAnimations { get; set; } = true;
        public List<MapAgentRule> MapAgentRules { get; set; } = new();
    }
}
