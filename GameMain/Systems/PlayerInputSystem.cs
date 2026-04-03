using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Input;

namespace GameDuMouse.GameMain.Systems
{
    public struct PlayerInputCommand
    {
        public int MoveDirection;
        public bool JumpPressed;
    }

    public sealed class PlayerInputSystem
    {
        private readonly InputManager inputManager;
        private KeyboardState previousKeyboard;
        private bool previousJumpButton;

        public PlayerInputSystem(Microsoft.Xna.Framework.Game game)
        {
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;
            previousKeyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
        }

        public PlayerInputCommand ReadInput()
        {
            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            var state = inputManager?.GetJoystickState();

            bool jumpPressed = keyboard.IsKeyDown(Keys.Space) && !previousKeyboard.IsKeyDown(Keys.Space);
            bool gamepadJump = state != null && state.Buttons[2];
            bool gamepadJumpPressed = gamepadJump && !previousJumpButton;
            previousJumpButton = gamepadJump;

            bool moveRight = keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right);
            bool moveLeft = keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left);

            if (state != null)
            {
                if (state.X > 50000) moveRight = true;
                if (state.X < 10000) moveLeft = true;

                if (state.PointOfViewControllers.Length > 0)
                {
                    int pov = state.PointOfViewControllers[0];
                    if (pov == 9000) moveRight = true;
                    if (pov == 27000) moveLeft = true;
                }
            }

            int move = 0;
            if (moveRight && !moveLeft)
                move = 1;
            else if (moveLeft && !moveRight)
                move = -1;

            previousKeyboard = keyboard;

            return new PlayerInputCommand
            {
                MoveDirection = move,
                JumpPressed = jumpPressed || gamepadJumpPressed
            };
        }
    }
}
