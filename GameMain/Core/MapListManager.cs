using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GameDuMouse.GameMain.Core
{
    public static class MapListManager
    {
        private static readonly string mapPath = Path.Combine("Content", "maps.json");

        public static List<string> LoadAllMaps()
        {
            if (!File.Exists(mapPath))
                return new List<string>();

            string json = File.ReadAllText(mapPath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            var maps = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            maps.RemoveAll(m => string.IsNullOrWhiteSpace(m) || m.Trim().Length < 4);
            return maps;
        }

        public static void AddMap(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            string trimmed = name.Trim();
            if (trimmed.Length < 4)
                return;

            var maps = LoadAllMaps();
            if (maps.Contains(trimmed))
                return;

            maps.Add(trimmed);
            string json = JsonSerializer.Serialize(maps, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(mapPath, json);
        }
    }
}
