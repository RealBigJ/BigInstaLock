using Serilog;
using System.IO;
using System.Text.Json;
using AppConstants = Valorant_Instalocker.Utils.Constants.Constants;

namespace Valorant_Instalocker.Utils.Configs
{
    internal static class ConfigManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static void SaveSettings(UserSettings settings)
        {
            try
            {
                Directory.CreateDirectory(AppConstants.InstalockerPath);
                File.WriteAllText(AppConstants.SettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ConfigManager] Settings could not be saved to {SettingsPath}.", AppConstants.SettingsPath);
                throw;
            }
        }

        public static UserSettings LoadSettings()
        {
            try
            {
                MigrateLegacySettings();
                if (!File.Exists(AppConstants.SettingsPath)) return new UserSettings();

                var settings = JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(AppConstants.SettingsPath)) ?? new UserSettings();
                settings.MapAgentRules ??= new List<MapAgentRule>();
                return settings;
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                Log.Error(ex, "[ConfigManager] Settings could not be loaded. Defaults will be used.");
                return new UserSettings();
            }
        }

        public static UserSettings InitializeSettings() => LoadSettings();

        private static void MigrateLegacySettings()
        {
            if (File.Exists(AppConstants.SettingsPath) || !File.Exists(AppConstants.LegacySettingsPath)) return;

            Directory.CreateDirectory(AppConstants.InstalockerPath);
            File.Copy(AppConstants.LegacySettingsPath, AppConstants.SettingsPath, false);
            Log.Information("[ConfigManager] Legacy settings migrated to Big Instalock.");
        }
    }
}
