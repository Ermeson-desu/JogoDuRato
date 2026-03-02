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

            var saves = JsonSerializer.Deserialize<List<SaveData>>(json) ?? new List<SaveData>();

            // remove entries inválidas que eventualmente tenham sido gravadas em versões anteriores
            saves.RemoveAll(s => string.IsNullOrWhiteSpace(s?.PlayerName) || s.PlayerName.Length <= 3);
            return saves;
        }

        public static void SaveGame(SaveData save)
        {
            if (save == null)
                return;

            // normaliza o nome e rejeita casos inválidos
            save.PlayerName = save.PlayerName?.Trim();
            if (string.IsNullOrWhiteSpace(save.PlayerName) || save.PlayerName.Length <= 3)
                return;

            var saves = LoadAllSaves();

            // remove entradas antigas que não obedecem às regras (por precaução)
            saves.RemoveAll(s => string.IsNullOrWhiteSpace(s.PlayerName) || s.PlayerName.Length <= 3);

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