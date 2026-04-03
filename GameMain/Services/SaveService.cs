using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameDuMouse.GameMain.Core;

namespace GameDuMouse.GameMain.Services
{
    /// <summary>
    /// Handles cached save data and background persistence.
    /// </summary>
    public sealed class SaveService : System.IDisposable
    {
        private readonly string savePath = Path.Combine("Content", "saves.json");
        private readonly object sync = new object();
        private readonly SemaphoreSlim ioSemaphore = new SemaphoreSlim(1, 1);
        private List<SaveData> saves = new List<SaveData>();
        private Task refreshTask;
        private int version;
        private bool disposed;

        public SaveService()
        {
            _ = RefreshAsync();
        }

        public int Version => Volatile.Read(ref version);

        public List<SaveData> GetSavesSnapshot()
        {
            lock (sync)
            {
                return new List<SaveData>(saves);
            }
        }

        public Task RefreshAsync()
        {
            lock (sync)
            {
                if (refreshTask == null || refreshTask.IsCompleted)
                    refreshTask = LoadSavesAsync();

                return refreshTask;
            }
        }

        public void Save(SaveData save)
        {
            if (!TryNormalize(save, out var normalized))
                return;

            List<SaveData> snapshot;
            lock (sync)
            {
                var updated = new List<SaveData>(saves.Count + 1);
                bool replaced = false;

                for (int i = 0; i < saves.Count; i++)
                {
                    var existing = saves[i];
                    if (!replaced && existing != null && existing.PlayerName == normalized.PlayerName)
                    {
                        updated.Add(normalized);
                        replaced = true;
                    }
                    else
                    {
                        updated.Add(existing);
                    }
                }

                if (!replaced)
                    updated.Add(normalized);

                saves = updated;
                snapshot = new List<SaveData>(updated);
            }

            Interlocked.Increment(ref version);
            _ = SaveSavesAsync(snapshot);
        }

        private async Task LoadSavesAsync()
        {
            var loaded = await ReadSavesAsync().ConfigureAwait(false);
            lock (sync)
            {
                saves = loaded ?? new List<SaveData>();
            }
            Interlocked.Increment(ref version);
        }

        private async Task<List<SaveData>> ReadSavesAsync()
        {
            if (!File.Exists(savePath))
                return new List<SaveData>();

            string json;
            await ioSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                json = await File.ReadAllTextAsync(savePath).ConfigureAwait(false);
            }
            catch
            {
                return new List<SaveData>();
            }
            finally
            {
                ioSemaphore.Release();
            }

            if (string.IsNullOrWhiteSpace(json))
                return new List<SaveData>();

            try
            {
                var loaded = JsonSerializer.Deserialize<List<SaveData>>(json) ?? new List<SaveData>();
                RemoveInvalid(loaded);
                return loaded;
            }
            catch
            {
                return new List<SaveData>();
            }
        }

        private async Task SaveSavesAsync(List<SaveData> snapshot)
        {
            string json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });

            await ioSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await File.WriteAllTextAsync(savePath, json).ConfigureAwait(false);
            }
            catch
            {
                // ignore write failures
            }
            finally
            {
                ioSemaphore.Release();
            }
        }

        private static bool TryNormalize(SaveData save, out SaveData normalized)
        {
            normalized = null;
            if (save == null)
                return false;

            string name = save.PlayerName?.Trim();
            if (!IsValidName(name))
                return false;

            normalized = new SaveData(name, save.CurrentFaseIndex, save.IsReturning);
            return true;
        }

        private static void RemoveInvalid(List<SaveData> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var entry = list[i];
                if (entry == null || !IsValidName(entry.PlayerName))
                    list.RemoveAt(i);
            }
        }

        private static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return name.Trim().Length > 3;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            ioSemaphore.Dispose();
            disposed = true;
        }
    }
}
