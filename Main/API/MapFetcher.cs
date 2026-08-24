using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using Valorant_Instalocker.Utils.Constants;

namespace Valorant_Instalocker.Main.API
{
    public sealed record ValorantMap(string DisplayName, string MapId, string ImagePath)
    {
        public override string ToString() => DisplayName;
    }

    public static class MapFetcher
    {
        private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(5) };

        private sealed record MapDto(
            [property: JsonPropertyName("displayName")] string DisplayName,
            [property: JsonPropertyName("mapUrl")] string MapUrl);

        private sealed record MapsResponse([property: JsonPropertyName("data")] IReadOnlyList<MapDto> Data);

        private static readonly IReadOnlyList<ValorantMap> FallbackMaps = new[]
        {
            Create("Abyss", "/Game/Maps/Infinity/Infinity"),
            Create("Ascent", "/Game/Maps/Ascent/Ascent"),
            Create("Bind", "/Game/Maps/Duality/Duality"),
            Create("Breeze", "/Game/Maps/Foxtrot/Foxtrot"),
            Create("Corrode", "/Game/Maps/Rook/Rook"),
            Create("Fracture", "/Game/Maps/Canyon/Canyon"),
            Create("Haven", "/Game/Maps/Triad/Triad"),
            Create("Icebox", "/Game/Maps/Port/Port"),
            Create("Lotus", "/Game/Maps/Jam/Jam"),
            Create("Pearl", "/Game/Maps/Pitt/Pitt"),
            Create("Split", "/Game/Maps/Bonsai/Bonsai"),
            Create("Summit", "/Game/Maps/Plummet/Plummet"),
            Create("Sunset", "/Game/Maps/Juliett/Juliett")
        };

        public static async Task<IReadOnlyList<ValorantMap>> GetMapsAsync()
        {
            try
            {
                using var response = await Client.GetAsync(Constants.MapsApiUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<MapsResponse>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var maps = result?.Data
                    .Where(IsPlayableMap)
                    .Select(map => Create(map.DisplayName, map.MapUrl))
                    .GroupBy(map => map.MapId, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .Where(map => ImageExists(map.DisplayName))
                    .OrderBy(map => map.DisplayName)
                    .ToList();

                if (maps?.Count > 0) return maps;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[MapFetcher] Map catalog refresh failed. Bundled catalog will be used.");
            }

            return FallbackMaps;
        }

        public static string GetImagePath(string? mapName)
        {
            var slug = Slug(mapName);
            return string.IsNullOrEmpty(slug) ? string.Empty : $"/Assets/maps/{slug}.png";
        }

        public static string NormalizeMapId(string? mapId)
        {
            if (string.IsNullOrWhiteSpace(mapId)) return string.Empty;
            var normalized = mapId.Trim().Replace('\\', '/');
            var assetSuffix = normalized.IndexOf('.', StringComparison.Ordinal);
            return assetSuffix >= 0 ? normalized[..assetSuffix] : normalized;
        }

        private static ValorantMap Create(string displayName, string mapId) =>
            new(displayName, NormalizeMapId(mapId), GetImagePath(displayName));

        private static bool IsPlayableMap(MapDto map) =>
            !string.IsNullOrWhiteSpace(map.DisplayName)
            && !string.IsNullOrWhiteSpace(map.MapUrl)
            && map.MapUrl.StartsWith("/Game/Maps/", StringComparison.OrdinalIgnoreCase)
            && !map.MapUrl.Contains("/HURM/", StringComparison.OrdinalIgnoreCase)
            && !map.DisplayName.Contains("Range", StringComparison.OrdinalIgnoreCase)
            && !map.DisplayName.Contains("Training", StringComparison.OrdinalIgnoreCase)
            && !map.DisplayName.StartsWith("Skirmish", StringComparison.OrdinalIgnoreCase);

        private static bool ImageExists(string mapName) => FallbackMaps.Any(map => map.DisplayName.Equals(mapName, StringComparison.OrdinalIgnoreCase));

        private static string Slug(string? value) => value?.Trim().ToLowerInvariant().Replace(" ", "-") ?? string.Empty;
    }
}
