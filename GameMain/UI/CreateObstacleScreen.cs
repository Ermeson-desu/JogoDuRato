using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Input;
using GameDuMouse.GameMain.UI.Components;

namespace GameDuMouse.GameMain.UI
{
    public class CreateObstacleScreen
    {
        private const int Margin = 10;

        private Game game;
        private SpriteFont font;
        private BackButton backButton;
        private Texture2D pixel;
        private InputManager inputManager;

        private MouseState previousMouse;
        private KeyboardState previousKeyboard;

        private int screenWidth;
        private int screenHeight;

        public CreateObstacleScreen(Game game)
        {
            this.game = game;
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;
            previousMouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            previousKeyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
        }

        public void ResetInput()
        {
            previousMouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            previousKeyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font, inputManager);
            
            screenWidth = game.GraphicsDevice.Viewport.Width;
            screenHeight = game.GraphicsDevice.Viewport.Height;

            var textureCache = game.Services.GetService(typeof(Rendering.TextureCache)) as Rendering.TextureCache;
            pixel = textureCache != null ? textureCache.Pixel : pixel;
        }

        public void Update(StateManager stateManager)
        {
            // Back button: if it changes the state we should bail out
            backButton?.Update(stateManager);
            if (stateManager.CurrentState != GameState.CreatingObstacle)
                return;

            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();

            // TODO: Implementar lógica de criação de obstáculo

            previousMouse = mouse;
            previousKeyboard = keyboard;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Background
            spriteBatch.Draw(pixel, new Rectangle(0, 0, screenWidth, screenHeight), Color.DarkSlateGray);

            // Title
            string title = "Criar Novo Obstáculo";
            var titleSize = font.MeasureString(title);
            var titlePos = new Vector2((screenWidth - titleSize.X) / 2f, Margin + 20);
            spriteBatch.DrawString(font, title, titlePos, Color.White);

            // Placeholder message
            string placeholder = "Tela de criação de obstáculos (Em desenvolvimento)";
            var placeholderSize = font.MeasureString(placeholder);
            var placeholderPos = new Vector2((screenWidth - placeholderSize.X) / 2f, screenHeight / 2 - placeholderSize.Y / 2);
            spriteBatch.DrawString(font, placeholder, placeholderPos, Color.Yellow);

            // Draw back button
            backButton?.Draw(spriteBatch);
        }
    }
}
