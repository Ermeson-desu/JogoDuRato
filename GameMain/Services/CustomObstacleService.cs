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
    /// Manages custom obstacle library with JSON persistence.
    /// </summary>
    public sealed class CustomObstacleService : IDisposable
    {
        private readonly string customObstaclesPath = Path.Combine("Content", "customObstacles.json");
        private readonly object sync = new object();
        private readonly SemaphoreSlim ioSemaphore = new SemaphoreSlim(1, 1);
        private List<CustomObstacleEntry> customObstacles = new List<CustomObstacleEntry>();
        private Task refreshTask;
        private int version;
        private bool disposed;

        public CustomObstacleService()
        {
            LoadObstaclesSync();
        }

        public int Version => Volatile.Read(ref version);

        public List<CustomObstacleEntry> GetObstaclesSnapshot()
        {
            lock (sync)
            {
                return new List<CustomObstacleEntry>(customObstacles);
            }
        }

        public Task RefreshAsync()
        {
            lock (sync)
            {
                if (refreshTask == null || refreshTask.IsCompleted)
                    refreshTask = LoadObstaclesAsync();

                return refreshTask;
            }
        }

        public void SaveObstacle(ObstacleEditorData obstacle)
        {
            if (obstacle == null || string.IsNullOrWhiteSpace(obstacle.Name))
                return;

            var entry = new CustomObstacleEntry
            {
                Id = DateTime.UtcNow.Ticks.ToString(),
                Name = obstacle.Name,
                Width = obstacle.Width,
                Height = obstacle.Height,
                IsMortal = obstacle.IsMortal,
                IsMovable = obstacle.IsMovable,
                MovementType = (int)obstacle.MovementType,
                IsLooping = obstacle.IsLooping,
                MovementDistance = obstacle.MovementDistance,
                MovementSpeed = obstacle.MovementSpeed,
                ImagePath = obstacle.ImagePath,
                CreatedAtUtc = DateTime.UtcNow.ToString("o")
            };

            List<CustomObstacleEntry> snapshot;
            lock (sync)
            {
                var updated = new List<CustomObstacleEntry>(customObstacles.Count + 1);
                bool replaced = false;

                for (int i = 0; i < customObstacles.Count; i++)
                {
                    var existing = customObstacles[i];
                    if (!replaced && existing != null && existing.Name == entry.Name)
                    {
                        updated.Add(entry);
                        replaced = true;
                    }
                    else
                    {
                        updated.Add(existing);
                    }
                }

                if (!replaced)
                    updated.Add(entry);

                customObstacles = updated;
                snapshot = new List<CustomObstacleEntry>(updated);
            }

            Interlocked.Increment(ref version);
            _ = SaveObstaclesAsync(snapshot);
        }

        public CustomObstacleEntry GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            lock (sync)
            {
                for (int i = 0; i < customObstacles.Count; i++)
                {
                    var obstacle = customObstacles[i];
                    if (obstacle != null && obstacle.Name == name)
                        return obstacle;
                }
            }

            return null;
        }

        public void DeleteObstacle(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            List<CustomObstacleEntry> snapshot;
            lock (sync)
            {
                var updated = new List<CustomObstacleEntry>();
                bool found = false;

                for (int i = 0; i < customObstacles.Count; i++)
                {
                    var obstacle = customObstacles[i];
                    if (obstacle != null && obstacle.Name == name)
                    {
                        found = true;
                        continue;
                    }

                    updated.Add(obstacle);
                }

                if (!found)
                    return;

                customObstacles = updated;
                snapshot = new List<CustomObstacleEntry>(updated);
            }

            Interlocked.Increment(ref version);
            _ = SaveObstaclesAsync(snapshot);
        }

        private void LoadObstaclesSync()
        {
            try
            {
                if (!File.Exists(customObstaclesPath))
                {
                    lock (sync)
                    {
                        customObstacles = new List<CustomObstacleEntry>();
                    }
                    return;
                }

                string json = File.ReadAllText(customObstaclesPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loaded = JsonSerializer.Deserialize<List<CustomObstacleEntry>>(json, options) ?? new List<CustomObstacleEntry>();

                lock (sync)
                {
                    customObstacles = loaded;
                }

                Interlocked.Increment(ref version);
            }
            catch
            {
                lock (sync)
                {
                    customObstacles = new List<CustomObstacleEntry>();
                }
            }
        }

        private async Task LoadObstaclesAsync()
        {
            try
            {
                await ioSemaphore.WaitAsync();

                if (!File.Exists(customObstaclesPath))
                {
                    lock (sync)
                    {
                        customObstacles = new List<CustomObstacleEntry>();
                    }
                    return;
                }

                string json = await File.ReadAllTextAsync(customObstaclesPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loaded = JsonSerializer.Deserialize<List<CustomObstacleEntry>>(json, options) ?? new List<CustomObstacleEntry>();

                lock (sync)
                {
                    customObstacles = loaded;
                }

                Interlocked.Increment(ref version);
            }
            catch
            {
                lock (sync)
                {
                    customObstacles = new List<CustomObstacleEntry>();
                }
            }
            finally
            {
                ioSemaphore.Release();
            }
        }

        private async Task SaveObstaclesAsync(List<CustomObstacleEntry> obstacles)
        {
            try
            {
                await ioSemaphore.WaitAsync();

                string dir = Path.GetDirectoryName(customObstaclesPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(obstacles, options);
                await File.WriteAllTextAsync(customObstaclesPath, json);
            }
            catch
            {
                // Silently fail on save errors
            }
            finally
            {
                ioSemaphore.Release();
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            ioSemaphore?.Dispose();
        }
    }

    /// <summary>
    /// Represents a saved custom obstacle entry
    /// </summary>
    public class CustomObstacleEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsMortal { get; set; }
        public bool IsMovable { get; set; }
        public int MovementType { get; set; }
        public bool IsLooping { get; set; }
        public int MovementDistance { get; set; }
        public float MovementSpeed { get; set; }
        public string ImagePath { get; set; }
        public string CreatedAtUtc { get; set; }
    }
}
