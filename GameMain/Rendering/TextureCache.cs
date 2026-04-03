using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDuMouse.GameMain.Rendering
{
    /// <summary>
    /// Cache for small reusable textures (e.g. 1x1 pixel).
    /// Avoid creating textures inside Draw.
    /// </summary>
    public sealed class TextureCache : IDisposable
    {
        private readonly GraphicsDevice graphicsDevice;
        private Texture2D pixel;
        private readonly Dictionary<Color, Texture2D> solidCache = new Dictionary<Color, Texture2D>();
        private bool disposed;

        public TextureCache(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public Texture2D Pixel
        {
            get
            {
                if (pixel == null)
                {
                    pixel = new Texture2D(graphicsDevice, 1, 1);
                    pixel.SetData(new[] { Color.White });
                }
                return pixel;
            }
        }

        public Texture2D GetSolid(Color color)
        {
            if (!solidCache.TryGetValue(color, out var tex))
            {
                tex = new Texture2D(graphicsDevice, 1, 1);
                tex.SetData(new[] { color });
                solidCache[color] = tex;
            }
            return tex;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            foreach (var kv in solidCache)
                kv.Value?.Dispose();
            solidCache.Clear();

            pixel?.Dispose();
            pixel = null;
            disposed = true;
        }
    }
}
