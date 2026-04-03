using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameDuMouse.GameMain.Core;

namespace GameDuMouse.GameMain.Services
{
    /// <summary>
    /// Handles map persistence and cached access for map data.
    /// </summary>
    public sealed class MapService : IDisposable
    {
        private readonly string mapPath = Path.Combine("Content", "maps.json");
        private readonly string exportedMapPath = Path.Combine("Content", "exported_map.txt");
        private readonly object sync = new object();
        private readonly SemaphoreSlim ioSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim exportSemaphore = new SemaphoreSlim(1, 1);
        private List<MapData> maps = new List<MapData>();
        private string currentMapName;
        private string exportedMapName;
        private Task refreshTask;
        private Task exportLoadTask;
        private int version;
        private bool disposed;

        public MapService()
        {
            _ = RefreshAsync();
            _ = LoadExportedAsync();
        }

        public string CurrentMapName => currentMapName;
        public string ExportedMapName => exportedMapName;
        public int Version => Volatile.Read(ref version);

        public void SetCurrentMap(string name)
        {
            currentMapName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        }

        public void ExportMap(string name)
        {
            exportedMapName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            Interlocked.Increment(ref version);
            _ = WriteExportedAsync(exportedMapName);
        }

        public void ClearExportedMap()
        {
            exportedMapName = null;
            Interlocked.Increment(ref version);
            _ = WriteExportedAsync(null);
        }

        public List<MapData> GetMapsSnapshot()
        {
            lock (sync)
            {
                return new List<MapData>(maps);
            }
        }

        public List<string> GetMapNamesSnapshot()
        {
            var result = new List<string>();
            var snapshot = GetMapsSnapshot();
            for (int i = 0; i < snapshot.Count; i++)
            {
                var map = snapshot[i];
                if (map == null || string.IsNullOrWhiteSpace(map.MapName))
                    continue;

                string trimmed = map.MapName.Trim();
                if (trimmed.Length >= 4)
                    result.Add(trimmed);
            }
            return result;
        }

        public MapData GetByName(string mapName)
        {
            if (string.IsNullOrWhiteSpace(mapName))
                return null;

            string trimmed = mapName.Trim();
            lock (sync)
            {
                for (int i = 0; i < maps.Count; i++)
                {
                    var map = maps[i];
                    if (map != null && map.MapName == trimmed)
                        return map;
                }
            }
            return null;
        }

        public void AddMap(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            string trimmed = name.Trim();
            if (trimmed.Length < 4)
                return;

            lock (sync)
            {
                var updated = new List<MapData>(maps.Count + 1);
                bool exists = false;
                for (int i = 0; i < maps.Count; i++)
                {
                    var map = maps[i];
                    updated.Add(map);
                    if (map != null && map.MapName == trimmed)
                        exists = true;
                }

                if (exists)
                {
                    maps = updated;
                    return;
                }

                updated.Add(new MapData { MapName = trimmed, CreatedAtUtc = DateTime.UtcNow.ToString("o") });
                maps = updated;
            }

            Interlocked.Increment(ref version);
            _ = SaveMapsAsync();
        }

        public void SaveMap(MapData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.MapName))
                return;

            string trimmed = data.MapName.Trim();
            data.MapName = trimmed;

            lock (sync)
            {
                var updated = new List<MapData>(maps.Count + 1);
                bool replaced = false;
                for (int i = 0; i < maps.Count; i++)
                {
                    var map = maps[i];
                    if (!replaced && map != null && map.MapName == trimmed)
                    {
                        updated.Add(data);
                        replaced = true;
                    }
                    else
                    {
                        updated.Add(map);
                    }
                }

                if (!replaced)
                    updated.Add(data);

                maps = updated;
            }

            Interlocked.Increment(ref version);
            _ = SaveMapsAsync();
        }

        public void DeleteMap(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            string trimmed = name.Trim();
            bool shouldClearExported = false;

            lock (sync)
            {
                var updated = new List<MapData>();
                for (int i = 0; i < maps.Count; i++)
                {
                    var map = maps[i];
                    if (map != null && map.MapName == trimmed)
                        continue;

                    updated.Add(map);
                }

                maps = updated;

                if (currentMapName == trimmed)
                    currentMapName = null;

                if (exportedMapName == trimmed)
                {
                    exportedMapName = null;
                    shouldClearExported = true;
                }
            }

            Interlocked.Increment(ref version);
            _ = SaveMapsAsync();

            if (shouldClearExported)
                _ = WriteExportedAsync(null);
        }

        public Task RefreshAsync()
        {
            lock (sync)
            {
                if (refreshTask == null || refreshTask.IsCompleted)
                    refreshTask = LoadMapsAsync();

                return refreshTask;
            }
        }

        private async Task LoadMapsAsync()
        {
            var loaded = await ReadMapsAsync().ConfigureAwait(false);
            lock (sync)
            {
                maps = loaded ?? new List<MapData>();
            }
            Interlocked.Increment(ref version);
        }

        private async Task<List<MapData>> ReadMapsAsync()
        {
            if (!File.Exists(mapPath))
                return new List<MapData>();

            string json;
            await ioSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                json = await File.ReadAllTextAsync(mapPath).ConfigureAwait(false);
            }
            catch
            {
                return new List<MapData>();
            }
            finally
            {
                ioSemaphore.Release();
            }

            if (string.IsNullOrWhiteSpace(json))
                return new List<MapData>();

            try
            {
                var maps = JsonSerializer.Deserialize<List<MapData>>(json);
                if (maps != null)
                    return maps;
            }
            catch (JsonException)
            {
                // ignore and try legacy format
            }
            catch
            {
                return new List<MapData>();
            }

            try
            {
                var names = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                var converted = new List<MapData>();
                for (int i = 0; i < names.Count; i++)
                {
                    var name = names[i];
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    converted.Add(new MapData { MapName = name.Trim() });
                }
                return converted;
            }
            catch
            {
                return new List<MapData>();
            }
        }

        private async Task SaveMapsAsync()
        {
            var snapshot = GetMapsSnapshot();
            string json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });

            await ioSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await File.WriteAllTextAsync(mapPath, json).ConfigureAwait(false);
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

        private Task LoadExportedAsync()
        {
            lock (sync)
            {
                if (exportLoadTask == null || exportLoadTask.IsCompleted)
                    exportLoadTask = LoadExportedInternalAsync();

                return exportLoadTask;
            }
        }

        private async Task LoadExportedInternalAsync()
        {
            if (!File.Exists(exportedMapPath))
                return;

            await exportSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                string text = await File.ReadAllTextAsync(exportedMapPath).ConfigureAwait(false);
                exportedMapName = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
                Interlocked.Increment(ref version);
            }
            catch
            {
                // ignore read failures
            }
            finally
            {
                exportSemaphore.Release();
            }
        }

        private async Task WriteExportedAsync(string name)
        {
            await exportSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    if (File.Exists(exportedMapPath))
                        File.Delete(exportedMapPath);
                }
                else
                {
                    await File.WriteAllTextAsync(exportedMapPath, name).ConfigureAwait(false);
                }
            }
            catch
            {
                // ignore write failures
            }
            finally
            {
                exportSemaphore.Release();
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            ioSemaphore.Dispose();
            exportSemaphore.Dispose();
            disposed = true;
        }
    }
}
