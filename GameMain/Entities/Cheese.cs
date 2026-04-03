using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Rendering;

namespace GameDuMouse.GameMain.Entities
{
    public class Cheese
    {
        private Texture2D texture;
        private Rectangle bounds;
        private Game game;

        public Rectangle Bounds => bounds;

        public Cheese(Game game, int x, int y, int width, int height)
        {
            this.game = game;
            var cache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            texture = cache != null ? cache.Pixel : texture;
            bounds = new Rectangle(x, y, width, height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
                spriteBatch.Draw(texture, bounds, Color.Yellow);
        }

        public bool CollidesWith(Rectangle playerCollider)
        {
            return bounds.Intersects(playerCollider);
        }
    }
}
