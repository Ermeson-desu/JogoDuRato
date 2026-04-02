using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Input;

namespace GameDuMouse.GameMain.UI
{
    /// <summary>
    /// Reusable UI component that renders a small "back" arrow in the upper
    /// left corner and handles mouse / keyboard input (Backspace) for
    /// navigating to the previous state.
    /// </summary>
    public class BackButton
    {
        private readonly Rectangle bounds = new Rectangle(10, 10, 48, 24);
        private SpriteFont font;
        private readonly InputManager inputManager;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;

        public BackButton(SpriteFont font, InputManager inputManager = null)
        {
            this.font = font;
            this.inputManager = inputManager;
            previousKeyboardState = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            previousMouseState = inputManager != null ? inputManager.Mouse : Mouse.GetState();
        }

        /// <summary>
        /// Should be called when the owning screen is activated so that old
        /// input doesn't trigger an immediate back.
        /// </summary>
        public void ResetInput()
        {
            previousKeyboardState = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            previousMouseState = inputManager != null ? inputManager.Mouse : Mouse.GetState();
        }

        /// <summary>
        /// Update the back button; if the user clicks the arrow or presses
        /// Backspace the <see cref="stateManager"/> will be asked to
        /// navigate backwards.
        /// </summary>
        /// <param name="ignoreBackKey">
        /// When <c>true</c> the Backspace key will not trigger navigation.  This
        /// is useful on screens that use Backspace for something else (for
        /// example the name entry in <see cref="PreGameScreen"/>).</param>
        public void Update(StateManager stateManager, bool ignoreBackKey = false)
        {
            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();

            if (!ignoreBackKey && IsKeyPressed(Keys.Back, keyboard))
            {
                stateManager.GoBack();
            }

            if (mouse.LeftButton == ButtonState.Pressed &&
                previousMouseState.LeftButton == ButtonState.Released)
            {
                if (bounds.Contains(mouse.Position))
                {
                    stateManager.GoBack();
                }
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
            // simple text arrow but could be replaced with a texture later
            spriteBatch.DrawString(font, "<--", new Vector2(bounds.X, bounds.Y), Color.White);
        }
    }
}
