using System.IO;

namespace Valorant_Instalocker.Utils.Constants
{
    internal static class Constants
    {
        public const string AgentApiUrl = "https://valorant-api.com/v1/agents/";
        public const string MapsApiUrl = "https://valorant-api.com/v1/maps";
        public const string ProjectGithubUrl = "https://github.com/RealBigJ/BigInstaLock";
        public const string BerkweProjectUrl = "https://github.com/Berkwe/Valorant-Instalocker";

        private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string InstalockerPath = Path.Combine(LocalAppData, "BigInstalock");
        public static readonly string LegacyInstalockerPath = Path.Combine(LocalAppData, "VALORANT", "Instalocker-gui");
        public static readonly string LogsPath = Path.Combine(InstalockerPath, "Logs");
        public static readonly string LogPath = Path.Combine(LogsPath, "BigInstalock.log");
        public static readonly string CachePath = Path.Combine(InstalockerPath, "Cache");
        public static readonly string CacheImagesPath = Path.Combine(CachePath, "Agents");
        public static readonly string SettingsPath = Path.Combine(InstalockerPath, "settings.json");
        public static readonly string LegacySettingsPath = Path.Combine(LegacyInstalockerPath, "settings.json");
    }
}
