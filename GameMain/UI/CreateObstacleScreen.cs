using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Input;
using GameDuMouse.GameMain.Rendering;
using GameDuMouse.GameMain.UI.Components;

namespace GameDuMouse.GameMain.UI
{
    public class CreateObstacleScreen
    {
        private const int Margin = 10;
        private const int ButtonSpacing = 70;
        private const int ButtonWidth = 250;
        private const int ButtonHeight = 50;

        private Game game;
        private SpriteFont font;
        private BackButton backButton;
        private UiActionButton newObstacleButton;
        private InputManager inputManager;
        private Texture2D pixel;
        private TextureCache textureCache;
        private MouseState previousMouse;
        private bool requestOpenEditor; // Flag to trigger state change

        public CreateObstacleScreen(Game game)
        {
            this.game = game;
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;
            textureCache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            previousMouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
        }

        public void ResetInput()
        {
            backButton?.ResetInput();
            previousMouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            requestOpenEditor = false;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font, inputManager);
            pixel = textureCache != null ? textureCache.Pixel : pixel;

            int screenWidth = game.GraphicsDevice.Viewport.Width;
            int screenHeight = game.GraphicsDevice.Viewport.Height;

            // Create "New Obstacle" button centered
            var buttonRect = new Rectangle(
                (screenWidth - ButtonWidth) / 2,
                (screenHeight - ButtonHeight) / 2,
                ButtonWidth,
                ButtonHeight
            );
            newObstacleButton = new UiActionButton(buttonRect, "New Obstacle", () => requestOpenEditor = true);
        }

        public void Update(StateManager stateManager)
        {
            backButton?.Update(stateManager);
            
            if (stateManager.CurrentState != GameState.CreatingObstacle)
                return;

            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            newObstacleButton?.Update(mouse, previousMouse);

            // Change state if button was clicked
            if (requestOpenEditor)
            {
                stateManager.ChangeState(GameState.CreateObstacle);
                requestOpenEditor = false;
            }

            previousMouse = mouse;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (font == null)
                return;

            string title = "Criar Obstaculo";
            var titleSize = font.MeasureString(title);
            var titlePos = new Vector2((game.GraphicsDevice.Viewport.Width - titleSize.X) / 2f, 50);
            spriteBatch.DrawString(font, title, titlePos, Color.White);

            newObstacleButton?.Draw(spriteBatch, font, pixel, Color.DarkSlateGray, Color.White);
            backButton?.Draw(spriteBatch);
        }
    }
}
