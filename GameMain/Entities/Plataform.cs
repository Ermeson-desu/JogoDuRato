using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Rendering;

namespace GameDuMouse.GameMain.Entities
{
    /// <summary>
    /// Simple platform entity used by the map editor.
    /// </summary>
    public sealed class Plataform
    {
        private readonly Texture2D texture;
        private Rectangle bounds;

        public Rectangle Bounds => bounds;

        public Plataform(Game game, Rectangle bounds)
        {
            var cache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            texture = cache != null ? cache.Pixel : null;
            this.bounds = bounds;
        }

        public void SetBounds(Rectangle newBounds)
        {
            bounds = newBounds;
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (texture != null)
                spriteBatch.Draw(texture, bounds, color);
        }

        public bool Contains(Point point)
        {
            return bounds.Contains(point);
        }
    }
}
