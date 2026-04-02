using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Input;

namespace GameDuMouse.GameMain.UI.Components
{
    public class TextEntryModal
    {
        private readonly Game game;
        private readonly string[] qwertyRows =
        {
            "QWERTYUIOP",
            "ASDFGHJKL",
            "ZXCVBNM"
        };

        private Texture2D pixel;
        private StringBuilder textBuffer = new StringBuilder();
        private KeyboardState previousKeyboard;
        private MouseState previousMouse;
        private bool isOpen;
        private InputManager inputManager;

        public bool WasConfirmed { get; private set; }
        public bool WasCanceled { get; private set; }
        public int MinimumLength { get; set; } = 4;

        public TextEntryModal(Game game)
        {
            this.game = game;
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;
        }

        public string CurrentText => textBuffer.ToString();

        public bool IsOpen => isOpen;

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            pixel = new Texture2D(graphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        public void Open()
        {
            isOpen = true;
            WasConfirmed = false;
            WasCanceled = false;
            textBuffer.Clear();
            ResetInput();
        }

        public void Close()
        {
            isOpen = false;
        }

        public void ResetInput()
        {
            previousKeyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            previousMouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
        }

        public void Update()
        {
            if (!isOpen)
                return;

            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();

            if (IsKeyPressed(Keys.Enter, keyboard))
            {
                if (IsValid())
                {
                    WasConfirmed = true;
                }
            }
            else if (IsKeyPressed(Keys.Back, keyboard) && textBuffer.Length > 0)
            {
                textBuffer.Remove(textBuffer.Length - 1, 1);
            }
            else if (IsKeyPressed(Keys.Space, keyboard))
            {
                textBuffer.Append(" ");
            }
            else
            {
                foreach (Keys key in keyboard.GetPressedKeys())
                {
                    if (!previousKeyboard.IsKeyDown(key))
                    {
                        string k = key.ToString();
                        if (k.Length == 1)
                            textBuffer.Append(k);
                    }
                }
            }

            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                for (int r = 0; r < qwertyRows.Length; r++)
                {
                    for (int c = 0; c < qwertyRows[r].Length; c++)
                    {
                        Rectangle keyBounds = new Rectangle(180 + c * 40, 300 + r * 50, 35, 35);
                        if (keyBounds.Contains(mouse.Position))
                            textBuffer.Append(qwertyRows[r][c]);
                    }
                }

                Rectangle spaceBtn = new Rectangle(180, 500, 80, 40);
                Rectangle backBtn = new Rectangle(280, 500, 80, 40);
                Rectangle okBtn = new Rectangle(380, 500, 80, 40);
                Rectangle cancelBtn = new Rectangle(480, 500, 80, 40);

                if (spaceBtn.Contains(mouse.Position)) textBuffer.Append(" ");
                if (backBtn.Contains(mouse.Position) && textBuffer.Length > 0) textBuffer.Remove(textBuffer.Length - 1, 1);
                if (okBtn.Contains(mouse.Position) && IsValid()) WasConfirmed = true;
                if (cancelBtn.Contains(mouse.Position)) WasCanceled = true;
            }

            previousKeyboard = keyboard;
            previousMouse = mouse;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (!isOpen)
                return;

            var viewport = game.GraphicsDevice.Viewport;
            var overlay = new Rectangle(0, 0, viewport.Width, viewport.Height);
            spriteBatch.Draw(pixel, overlay, Color.Black * 0.7f);

            var panel = new Rectangle(120, 80, viewport.Width - 240, viewport.Height - 160);
            spriteBatch.Draw(pixel, panel, Color.DarkSlateGray * 0.95f);

            spriteBatch.DrawString(font, "Enter map name:", new Vector2(160, 120), Color.White);
            spriteBatch.DrawString(font, CurrentText, new Vector2(160, 160), Color.Yellow);

            for (int r = 0; r < qwertyRows.Length; r++)
            {
                for (int c = 0; c < qwertyRows[r].Length; c++)
                {
                    string letter = qwertyRows[r][c].ToString();
                    spriteBatch.DrawString(font, letter, new Vector2(180 + c * 40, 300 + r * 50), Color.White);
                }
            }

            if (!IsValid())
            {
                spriteBatch.DrawString(font, "Name must have more than 3 characters!", new Vector2(160, 200), Color.Red);
            }

            spriteBatch.DrawString(font, "[SPACE]", new Vector2(180, 500), Color.White);
            spriteBatch.DrawString(font, "[BACK]", new Vector2(280, 500), Color.White);
            spriteBatch.DrawString(font, "[OK]", new Vector2(380, 500), Color.White);
            spriteBatch.DrawString(font, "[CANCEL]", new Vector2(480, 500), Color.White);
        }

        public string ConsumeConfirmedText()
        {
            if (!WasConfirmed)
                return null;

            WasConfirmed = false;
            return CurrentText;
        }

        public bool ConsumeCanceled()
        {
            if (!WasCanceled)
                return false;

            WasCanceled = false;
            return true;
        }

        private bool IsKeyPressed(Keys key, KeyboardState current)
        {
            return current.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
        }

        private bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(CurrentText) && CurrentText.Trim().Length >= MinimumLength;
        }
    }
}
