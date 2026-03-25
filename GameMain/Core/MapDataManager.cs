using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GameDuMouse.GameMain.Core
{
    public static class MapDataManager
    {
        private static readonly string mapDataPath = Path.Combine("Content", "maps.json");

        public static List<MapData> LoadAll()
        {
            if (!File.Exists(mapDataPath))
                return new List<MapData>();

            string json = File.ReadAllText(mapDataPath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<MapData>();

            try
            {
                var maps = JsonSerializer.Deserialize<List<MapData>>(json);
                if (maps != null && maps.Count > 0)
                    return maps;
            }
            catch (JsonException)
            {
                // fall through to legacy string list
            }

            // fallback to legacy list of names
            var names = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            var converted = new List<MapData>();
            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                    converted.Add(new MapData { MapName = name.Trim() });
            }
            return converted;
        }

        public static MapData LoadByName(string mapName)
        {
            if (string.IsNullOrWhiteSpace(mapName))
                return null;

            var maps = LoadAll();
            return maps.Find(m => m.MapName == mapName.Trim());
        }

        public static void SaveMap(MapData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.MapName))
                return;

            var maps = LoadAll();
            int index = maps.FindIndex(m => m.MapName == data.MapName);
            if (index >= 0)
                maps[index] = data;
            else
                maps.Add(data);

            string json = JsonSerializer.Serialize(maps, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(mapDataPath, json);
        }

        public static void DeleteByName(string mapName)
        {
            if (string.IsNullOrWhiteSpace(mapName))
                return;

            var maps = LoadAll();
            int index = maps.FindIndex(m => m.MapName == mapName.Trim());
            if (index < 0)
                return;

            maps.RemoveAt(index);
            string json = JsonSerializer.Serialize(maps, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(mapDataPath, json);
        }
    }
}
