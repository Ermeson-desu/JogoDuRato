using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Utils;

namespace GameDuMouse.GameMain.UI
{
    public class MenuScreen
    {
        private SpriteFont font;
        private string[] options = { "New Game", "Load", "Settings", "Create Mapping", "Exit" };
        private int selectedIndex = 0;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private DirectInputController directController;
        private Game game;

        public MenuScreen(Game game)
        {
            this.game = game;
            directController = new DirectInputController();
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial"); 
        }

        public void Update(StateManager stateManager)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();
            var state = directController.GetState();

            // --- Navegação teclado (edge detection) ---
            if (IsKeyPressed(Keys.Down, keyboard) || IsKeyPressed(Keys.S, keyboard))
                selectedIndex = (selectedIndex + 1) % options.Length;

            if (IsKeyPressed(Keys.Up, keyboard) || IsKeyPressed(Keys.W, keyboard))
                selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;

            // --- Navegação controle genérico ---
            if (state != null)
            {
                // Exemplo simplificado: POV hat
                if (state.PointOfViewControllers.Length > 0)
                {
                    int pov = state.PointOfViewControllers[0];
                    if (pov == 9000) selectedIndex = (selectedIndex + 1) % options.Length; // baixo
                    if (pov == 27000) selectedIndex = (selectedIndex - 1 + options.Length) % options.Length; // cima
                }
            }

            // --- Seleção com Enter ou botão do controle ---
            if (IsKeyPressed(Keys.Enter, keyboard) || (state != null && state.Buttons[0]))
                ExecuteOption(stateManager);

            // --- Seleção com mouse ---
            if (mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    Rectangle optionBounds = new Rectangle(300, 200 + i * 50, 200, 40);
                    if (optionBounds.Contains(mouse.Position))
                    {
                        selectedIndex = i;
                        ExecuteOption(stateManager);
                    }
                }
            }

            previousKeyboardState = keyboard;
            previousMouseState = mouse;
        }

        private bool IsKeyPressed(Keys key, KeyboardState current)
        {
            return current.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }

        private void ExecuteOption(StateManager stateManager)
        {
            switch (options[selectedIndex])
            {
                case "New Game":
                    stateManager.ChangeState(GameState.PreGame);
                    break;
                case "Load":
                    stateManager.ChangeState(GameState.Load);
                    break;
                case "Settings":
                    stateManager.ChangeState(GameState.Settings);
                    break;
                case "Create Mapping":
                    stateManager.ChangeState(GameState.Mapping);
                    break;
                case "Exit":
                    stateManager.ChangeState(GameState.Exit);
                    game.Exit();
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < options.Length; i++)
            {
                Color color = (i == selectedIndex) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(font, options[i], new Vector2(300, 200 + i * 50), color);
            }
        }
    }
}