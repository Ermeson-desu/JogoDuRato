using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GameDuMouse.GameMain.Core
{
    public static class MapListManager
    {
        private static readonly string mapPath = Path.Combine("Content", "maps.json");
        public static string CurrentMapName { get; private set; }

        public static List<string> LoadAllMaps()
        {
            if (!File.Exists(mapPath))
                return new List<string>();

            string json = File.ReadAllText(mapPath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            var mapData = MapDataManager.LoadAll();
            var names = new List<string>();
            foreach (var map in mapData)
            {
                if (!string.IsNullOrWhiteSpace(map.MapName) && map.MapName.Trim().Length >= 4)
                    names.Add(map.MapName.Trim());
            }
            return names;
        }

        public static void AddMap(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            string trimmed = name.Trim();
            if (trimmed.Length < 4)
                return;

            var maps = MapDataManager.LoadAll();
            if (maps.Exists(m => m.MapName == trimmed))
                return;

            maps.Add(new MapData { MapName = trimmed, CreatedAtUtc = System.DateTime.UtcNow.ToString("o") });
            string json = JsonSerializer.Serialize(maps, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(mapPath, json);
        }

        public static void SetCurrentMap(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            CurrentMapName = name.Trim();
        }
    }
}
