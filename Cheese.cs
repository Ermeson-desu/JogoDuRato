using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDuMouse
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
            texture = new Texture2D(game.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.Yellow }); // quadrado amarelo
            bounds = new Rectangle(x, y, width, height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, bounds, Color.Yellow);
        }

        public bool CollidesWith(Rectangle playerCollider)
        {
            return bounds.Intersects(playerCollider);
        }
    }
}