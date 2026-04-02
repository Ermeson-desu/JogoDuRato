using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Input;
using System.Collections.Generic;

namespace GameDuMouse.GameMain.UI
{
    public class LoadScreen
    {
        private SpriteFont font;
        private List<SaveData> saves;
        private int selectedIndex = 0;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private InputManager inputManager;

        private Game game;
        private BackButton backButton;
        // flag used when we just entered the screen to avoid carrying over a mouse
        // or keyboard press from the previous screen (e.g. clicking "Load" on
        // the main menu).  We reset this whenever the state changes in Game1.
        private bool ignoreNextInput;

        public LoadScreen(Game game)
        {
            this.game = game;
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;
            saves = SaveManager.LoadAllSaves();

            // make sure the first update won't treat whatever input happened during
            // initialization as a selection
            previousMouseState = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            previousKeyboardState = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            ignoreNextInput = true;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font, inputManager);
        }
        public void Update(StateManager stateManager)
        {
            // first let the back button do its thing; if it changes state we'll
            // return early to avoid processing other input on the old screen.
            backButton?.Update(stateManager);
            if (stateManager.CurrentState != GameState.Load)
                return;

            // 🔑 sempre recarrega a lista
            saves = SaveManager.LoadAllSaves();

            if (saves.Count == 0)
                return;

            // if we just arrived to the screen clear any lingering input
            if (ignoreNextInput)
            {
                var k = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
                var m = inputManager != null ? inputManager.Mouse : Mouse.GetState();
                previousKeyboardState = k;
                previousMouseState = m;
                // wait until user releases the button/keys before accepting input
                if (m.LeftButton == ButtonState.Released && !k.IsKeyDown(Keys.Enter))
                    ignoreNextInput = false;
                return;
            }

            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            var state = inputManager?.GetJoystickState();

            if (IsKeyPressed(Keys.Enter, keyboard))
                ConfirmSelection(stateManager);

            if (IsKeyPressed(Keys.Down, keyboard))
                selectedIndex = (selectedIndex + 1) % saves.Count;

            if (IsKeyPressed(Keys.Up, keyboard))
                selectedIndex = (selectedIndex - 1 + saves.Count) % saves.Count;

            // Mouse
            if (mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < saves.Count; i++)
                {
                    Rectangle optionBounds = new Rectangle(300, 200 + i * 50, 200, 40);
                    if (optionBounds.Contains(mouse.Position))
                    {
                        selectedIndex = i;
                        LoadSelectedSave(stateManager);
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

        private void LoadSelectedSave(StateManager stateManager)
        {
            if (saves.Count == 0)
                return;

            var save = saves[selectedIndex];
            var game1 = (Game1)game;

            game1.LoadSave(save);
            stateManager.ChangeState(GameState.Playing);
        }

        private void ConfirmSelection(StateManager stateManager)
        {
            if (saves.Count == 0) return;

            var save = saves[selectedIndex];
            var game1 = (Game1)game;

            game1.LoadSave(save);
            stateManager.ChangeState(GameState.Playing);
        }

        /// <summary>
        /// Called by <see cref="Game1"/> when the state machine transitions to
        /// <see cref="GameState.Load"/>.  Resets the internal mouse/keyboard
        /// state to avoid processing the click that caused the transition.
        /// </summary>
        public void ResetInput()
        {
            previousKeyboardState = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            previousMouseState = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            ignoreNextInput = true;
            backButton?.ResetInput();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, "Selecione seu save:", new Vector2(100, 100), Color.White);
            backButton?.Draw(spriteBatch);

            if (saves.Count == 0)
            {
                spriteBatch.DrawString(font, "Nenhum save encontrado!", new Vector2(300, 200), Color.Red);
                return;
            }

            for (int i = 0; i < saves.Count; i++)
            {
                Color color = (i == selectedIndex) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(font, saves[i].PlayerName, new Vector2(300, 200 + i * 50), color);
            }
        }
    }
}
