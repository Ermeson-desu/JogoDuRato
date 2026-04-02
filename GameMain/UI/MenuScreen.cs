using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Input;

namespace GameDuMouse.GameMain.UI
{
    public class MenuScreen
    {
        private SpriteFont font;
        // keep string consistent with NewMapMenu entry point so typo is less likely
        private const string MapOption = "Create Mapping";
        private const string ChapterOption = "Chapters";
        private string[] options = { "New Game", "Load", ChapterOption, MapOption, "Settings", "Exit" };
        private int selectedIndex = 0;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private InputManager inputManager;
        private Game game;

        private bool ignoreNextInput; // usado para não processar clique residual

        public MenuScreen(Game game)
        {
            this.game = game;
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;

            previousKeyboardState = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            previousMouseState = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            ignoreNextInput = true;
        }

        public void ResetInput()
        {
            previousKeyboardState = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            previousMouseState = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            ignoreNextInput = true;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial"); 
        }

        public void Update(StateManager stateManager)
        {
            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            var state = inputManager?.GetJoystickState();

            if (ignoreNextInput)
            {
                previousKeyboardState = keyboard;
                previousMouseState = mouse;
                if (mouse.LeftButton == ButtonState.Released && !keyboard.IsKeyDown(Keys.Enter))
                    ignoreNextInput = false;
                return;
            }

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
                    if (game is GameDuMouse.GameMain.Core.Game1 g1)
                        g1.SetChapterSelection(false);
                    stateManager.ChangeState(GameState.PreGame);
                    break;
                case "Load":
                    if (game is GameDuMouse.GameMain.Core.Game1 g1Load)
                        g1Load.SetChapterSelection(false);
                    stateManager.ChangeState(GameState.Load);
                    break;
                case ChapterOption:
                    stateManager.ChangeState(GameState.ChapterSelect);
                    break;
                case "Settings":
                    stateManager.ChangeState(GameState.Settings);
                    break;
                case MapOption:
                    stateManager.ChangeState(GameState.NewMapMenu);
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
