using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Core;

namespace GameDuMouse.GameMain.UI
{
    public class VictoryScreen
    {
        private Game game;
        private Texture2D victoryImage;
        private Vector2 position;
        private GraphicsDevice graphics;
        private int screenWidth;
        private int screenHeight;
        private Rectangle screenSize;

        private VictoryMenu victoryMenu;

        public VictoryScreen(Game game)
        {
            this.game = game;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            victoryImage = content.Load<Texture2D>("vitoriaImage");
            position = new Vector2(-130, -10);

            graphics = game.GraphicsDevice;
            screenWidth = graphics.Viewport.Width;
            screenHeight = graphics.Viewport.Height;
            screenSize = new Rectangle(0, -20, screenWidth, screenHeight);

            victoryMenu = new VictoryMenu(game);
            victoryMenu.LoadContent(content);
        }

        public void Update(StateManager stateManager, LevelManager levelManager)
        {
            victoryMenu.Update(stateManager, levelManager);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(victoryImage, position, screenSize, Color.White);
            victoryMenu.Draw(spriteBatch);
        }
    }
}