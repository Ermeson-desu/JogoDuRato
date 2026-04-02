using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Input;
using System.Text;

namespace GameDuMouse.GameMain.UI
{
    public class PreGameScreen
    {
        private SpriteFont font;
        private StringBuilder playerName;
        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private InputManager inputManager;
        private Game game;
        private BackButton backButton;

        // Teclado virtual QWERTY
        private string[] qwertyRows = {
            "QWERTYUIOP",
            "ASDFGHJKL",
            "ZXCVBNM"
        };
        private int selectedRow = 0;
        private int selectedCol = 0;

        public PreGameScreen(Game game)
        {
            this.game = game;
            playerName = new StringBuilder();
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;

            previousKeyboardState = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            previousMouseState = inputManager != null ? inputManager.Mouse : Mouse.GetState();
        }

        public void ResetInput()
        {
            previousKeyboardState = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            previousMouseState = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            // not using ignore flag here because PreGame has more complex mouse interactions,
            // but we at least reset states so earlier clicks don't trigger buttons accidentally.
            backButton?.ResetInput();
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font, inputManager);
        }

        public void Update(StateManager stateManager)
        {
            // handle shared back-navigation (click or backspace when name empty)
            backButton?.Update(stateManager, ignoreBackKey: playerName.Length > 0);

            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            var state = inputManager?.GetJoystickState();

            // --- Input via teclado físico ---
            if (IsKeyPressed(Keys.Enter, keyboard))
            {
                // só salva/começa se tiver nome válido
                string name = playerName.ToString();
                if (!string.IsNullOrWhiteSpace(name) && name.Length > 3)
                {
                    var save = new SaveData
                    {
                        PlayerName = name,
                        CurrentFaseIndex = 0,
                        IsReturning = false
                    };

                    SaveManager.SaveGame(save); // 🔑 grava o save inicial
                    ((Game1)game).StartNewGame(name);
                }
            }

            else if (IsKeyPressed(Keys.Back, keyboard) && playerName.Length > 0)
            {
                playerName.Remove(playerName.Length - 1, 1);
            }
            else if (IsKeyPressed(Keys.Space, keyboard))
            {
                playerName.Append(" ");
            }
            else
            {
                foreach (Keys key in keyboard.GetPressedKeys())
                {
                    if (!previousKeyboardState.IsKeyDown(key))
                    {
                        string k = key.ToString();
                        if (k.Length == 1)
                            playerName.Append(k);
                    }
                }
            }

            previousKeyboardState = keyboard;
            // --- Input via mouse (clicando no teclado virtual) ---
            if (mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                for (int r = 0; r < qwertyRows.Length; r++)
                {
                    for (int c = 0; c < qwertyRows[r].Length; c++)
                    {
                        Rectangle keyBounds = new Rectangle(100 + c * 40, 300 + r * 50, 35, 35);
                        if (keyBounds.Contains(mouse.Position))
                        {
                            playerName.Append(qwertyRows[r][c]);
                        }
                    }
                }

                // Botões extras
                Rectangle spaceBtn = new Rectangle(100, 500, 80, 40);
                Rectangle backBtn = new Rectangle(200, 500, 80, 40);
                Rectangle okBtn = new Rectangle(300, 500, 80, 40);
                Rectangle cancelBtn = new Rectangle(400, 500, 80, 40);

                if (spaceBtn.Contains(mouse.Position)) playerName.Append(" ");
                if (backBtn.Contains(mouse.Position) && playerName.Length > 0) playerName.Remove(playerName.Length - 1, 1);
                if (okBtn.Contains(mouse.Position))
                {
                    string name = playerName.ToString();
                    if (!string.IsNullOrWhiteSpace(name) && name.Length > 3)
                    {
                        var save = new SaveData
                        {
                            PlayerName = name,
                            CurrentFaseIndex = 0,
                            IsReturning = false
                        };

                        SaveManager.SaveGame(save);
                        ((Game1)game).StartNewGame(name);
                    }
                }
                if (cancelBtn.Contains(mouse.Position)) stateManager.GoBack();
            }

            // --- Input via controle genérico ---
            if (state != null)
            {
                if (state.PointOfViewControllers.Length > 0)
                {
                    int pov = state.PointOfViewControllers[0];
                    if (pov == 0) selectedRow = Math.Max(0, selectedRow - 1); // cima
                    if (pov == 18000) selectedRow = Math.Min(qwertyRows.Length - 1, selectedRow + 1); // baixo
                    if (pov == 27000) selectedCol = Math.Max(0, selectedCol - 1); // esquerda
                    if (pov == 9000) selectedCol = Math.Min(qwertyRows[selectedRow].Length - 1, selectedCol + 1); // direita
                }

                // Botão X → confirma letra
                if (state.Buttons[2])
                    playerName.Append(qwertyRows[selectedRow][selectedCol]);

                // Botão O → espaço
                if (state.Buttons[1])
                    playerName.Append(" ");

                // Botão quadrado → apagar
                if (state.Buttons[0] && playerName.Length > 0)
                    playerName.Remove(playerName.Length - 1, 1);

                // Botão triângulo → voltar à tela anterior (menu neste caso)
                if (state.Buttons[3])
                    stateManager.GoBack();
            }

            previousKeyboardState = keyboard;
            previousMouseState = mouse;
        }

        private bool IsKeyPressed(Keys key, KeyboardState current)
        {
            return current.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, "Digite seu nome:", new Vector2(100, 100), Color.White);
            backButton?.Draw(spriteBatch);
            spriteBatch.DrawString(font, playerName.ToString(), new Vector2(100, 150), Color.Yellow);

            // Teclado virtual
            for (int r = 0; r < qwertyRows.Length; r++)
            {
                for (int c = 0; c < qwertyRows[r].Length; c++)
                {
                    string letter = qwertyRows[r][c].ToString();
                    Color color = (r == selectedRow && c == selectedCol) ? Color.Yellow : Color.White;
                    spriteBatch.DrawString(font, letter, new Vector2(100 + c * 40, 300 + r * 50), color);
                }
            }

            if (string.IsNullOrEmpty(playerName.ToString()) || playerName.Length <= 3)
            {
                spriteBatch.DrawString(font, "Nome deve ter mais de 3 caracteres!", new Vector2(100, 200), Color.Red);
            }

            // Botões extras
            spriteBatch.DrawString(font, "[SPACE]", new Vector2(100, 500), Color.White);
            spriteBatch.DrawString(font, "[BACK]", new Vector2(200, 500), Color.White);
            spriteBatch.DrawString(font, "[OK]", new Vector2(300, 500), Color.White);
            spriteBatch.DrawString(font, "[CANCEL]", new Vector2(400, 500), Color.White);
        }
    }
}
