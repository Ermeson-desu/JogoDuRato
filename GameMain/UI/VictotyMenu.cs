using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Input;
using GameDuMouse.GameMain.Managers;

namespace GameDuMouse.GameMain.UI
{
    public class VictoryMenu
    {
        private SpriteFont font;
        private string[] options = { "Replay", "Menu", "Next" };
        private int selectedIndex = 0;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private InputManager inputManager;
        private IGameFlow gameFlow;
        private Game game;

        public VictoryMenu(Game game)
        {
            this.game = game;
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;
            gameFlow = game.Services.GetService(typeof(IGameFlow)) as IGameFlow;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
        }

        public void Update(StateManager stateManager, LevelManager levelManager)
        {
            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            var state = inputManager?.GetJoystickState();

            // --- Navegação teclado (edge detection) ---
            if (IsKeyPressed(Keys.Right, keyboard) || IsKeyPressed(Keys.D, keyboard))
                selectedIndex = (selectedIndex + 1) % options.Length;

            if (IsKeyPressed(Keys.Left, keyboard) || IsKeyPressed(Keys.A, keyboard))
                selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;

            // --- Navegação controle genérico ---
            if (state != null)
            {
                if (state.PointOfViewControllers.Length > 0)
                {
                    int pov = state.PointOfViewControllers[0];
                    if (pov == 9000) selectedIndex = (selectedIndex + 1) % options.Length; // direita
                    if (pov == 27000) selectedIndex = (selectedIndex - 1 + options.Length) % options.Length; // esquerda
                }
            }

            // --- Seleção com Enter ou botão X do controle ---
            if (IsKeyPressed(Keys.Enter, keyboard) || (state != null && state.Buttons[2]))
                ExecuteOption(stateManager, levelManager);

            // --- Seleção com mouse ---
            if (mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    Rectangle optionBounds = new Rectangle(250 + i * 200, 400, 180, 40);
                    if (optionBounds.Contains(mouse.Position))
                    {
                        selectedIndex = i;
                        ExecuteOption(stateManager, levelManager);
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

        private void ExecuteOption(StateManager stateManager, LevelManager levelManager)
        {
            switch (options[selectedIndex])
            {
                case "Replay":
                    levelManager.ReplayCurrentFase();
                    stateManager.ChangeState(GameState.Playing);
                    break;

                case "Menu":
                    stateManager.ChangeState(GameState.Menu);
                    break;

                case "Next":
                    if (gameFlow != null && gameFlow.TryAdvanceToNextFase())
                        stateManager.ChangeState(GameState.Playing);
                    else
                        stateManager.ChangeState(GameState.Menu);
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < options.Length; i++)
            {
                Color color = (i == selectedIndex) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(font, options[i], new Vector2(250 + i * 200, 400), color);
            }
        }
    }
}
