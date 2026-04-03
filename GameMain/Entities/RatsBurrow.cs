using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Rendering;

namespace GameDuMouse.GameMain.Entities
{
    public class RatsBurrow
    {
        private Game game;
        private Texture2D texture;
        private Rectangle bounds;

        public Rectangle Bounds => bounds;

        public RatsBurrow(Game game, int x, int y, int width, int height)
        {
            this.game = game;
            var cache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            texture = cache != null ? cache.Pixel : texture;
            bounds = new Rectangle(x, y, width, height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                spriteBatch.Draw(texture, bounds, Color.Brown);
        }

        public bool CollidesWith(Rectangle playerCollider)
        {
            return bounds.Intersects(playerCollider);
        }
    }
}
