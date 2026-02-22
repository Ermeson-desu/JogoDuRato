using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDuMouse.GameMain.UI
{
    public class VictoryScreen
    {
        private Game game;
        private Texture2D victoryImage;
        Vector2 position;


        GraphicsDevice graphics;
        int screenWidth;
        int screenHeight;
        Rectangle screenSize;


        public VictoryScreen(Game game)
        {
            this.game = game;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            // Fonte provisória (adicione uma SpriteFont chamada "DefaultFont" no Content)
            victoryImage = content.Load<Texture2D>("vitoriaImage");
            position = new Vector2(-130,-10);

            graphics = game.GraphicsDevice;
            screenWidth = graphics.Viewport.Width;
            screenHeight = graphics.Viewport.Height;
            screenSize = new Rectangle(0, -20, screenWidth, screenHeight);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            
            spriteBatch.Draw(victoryImage, position , screenSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}