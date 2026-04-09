using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Input;
using GameDuMouse.GameMain.UI.Components;

namespace GameDuMouse.GameMain.UI
{
    public class CreateObstacleScreen
    {
        private Game game;
        private SpriteFont font;
        private BackButton backButton;
        private InputManager inputManager;

        public CreateObstacleScreen(Game game)
        {
            this.game = game;
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;
        }

        public void ResetInput()
        {
            backButton?.ResetInput();
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font, inputManager);
        }

        public void Update(StateManager stateManager)
        {
            backButton?.Update(stateManager);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string title = "Criar Novo Obstáculo";
            var titleSize = font.MeasureString(title);
            var titlePos = new Vector2((game.GraphicsDevice.Viewport.Width - titleSize.X) / 2f, 50);
            spriteBatch.DrawString(font, title, titlePos, Color.White);

            backButton?.Draw(spriteBatch);
        }
    }
}
