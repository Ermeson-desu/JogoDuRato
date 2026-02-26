using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GameDuMouse.GameMain.Core
{
    public static class SaveManager
    {
        private static string savePath = Path.Combine("Content", "saves.json");

        public static List<SaveData> LoadAllSaves()
        {
            if (!File.Exists(savePath))
                return new List<SaveData>();

            string json = File.ReadAllText(savePath);

            if (string.IsNullOrWhiteSpace(json))
                return new List<SaveData>(); 

            return JsonSerializer.Deserialize<List<SaveData>>(json) ?? new List<SaveData>();
        }

        public static void SaveGame(SaveData save)
        {
            var saves = LoadAllSaves();

            var existing = saves.Find(s => s.PlayerName == save.PlayerName);
            if (existing != null)
            {
                existing.CurrentFaseIndex = save.CurrentFaseIndex;
                existing.IsReturning = save.IsReturning;
            }
            else
            {
                saves.Add(save);
            }

            string json = JsonSerializer.Serialize(saves, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(savePath, json);
        }
    }
}