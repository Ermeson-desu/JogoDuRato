using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDuMouse
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
            texture = new Texture2D(game.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.Brown }); // marrom para representar a toca
            bounds = new Rectangle(x, y, width, height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, bounds, Color.Brown);
        }

        public bool CollidesWith(Rectangle playerCollider)
        {
            return bounds.Intersects(playerCollider);
        }
    }
}