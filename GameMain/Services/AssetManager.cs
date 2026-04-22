using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GameDuMouse.GameMain.Services
{
    /// <summary>
    /// Central cache for textures loaded from file and content.
    /// </summary>
    public sealed class AssetManager : IDisposable
    {
        private readonly ContentManager content;
        private readonly GraphicsDevice graphicsDevice;
        private readonly Dictionary<string, Texture2D> fileTextureCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private bool disposed;

        public AssetManager(ContentManager content, GraphicsDevice graphicsDevice)
        {
            this.content = content;
            this.graphicsDevice = graphicsDevice;
        }

        public T LoadContent<T>(string assetName)
        {
            return content.Load<T>(assetName);
        }

        public Texture2D LoadTextureFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            string fullPath = Path.GetFullPath(path);
            if (fileTextureCache.TryGetValue(fullPath, out var cached))
                return cached;

            if (!File.Exists(fullPath))
                return null;

            try
            {
                var tex = Texture2D.FromFile(graphicsDevice, fullPath);
                fileTextureCache[fullPath] = tex;
                return tex;
            }
            catch
            {
                return null;
            }
        }

        public void UnloadTextureFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            string fullPath = Path.GetFullPath(path);
            if (!fileTextureCache.TryGetValue(fullPath, out var texture))
                return;

            fileTextureCache.Remove(fullPath);
            texture?.Dispose();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            foreach (var kv in fileTextureCache)
                kv.Value?.Dispose();
            fileTextureCache.Clear();
            disposed = true;
        }
    }
}
