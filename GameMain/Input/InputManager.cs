using GameDuMouse.GameMain.Utils;
using SharpDX.DirectInput;
using XnaInput = Microsoft.Xna.Framework.Input;

namespace GameDuMouse.GameMain.Input
{
    /// <summary>
    /// Centralized input manager (keyboard, mouse, DirectInput).
    /// </summary>
    public sealed class InputManager
    {
        private readonly DirectInputController directInput;

        public InputManager()
        {
            directInput = new DirectInputController();
        }

        public XnaInput.KeyboardState Keyboard => XnaInput.Keyboard.GetState();
        public XnaInput.MouseState Mouse => XnaInput.Mouse.GetState();

        public JoystickState GetJoystickState()
        {
            return directInput.GetState();
        }

        public void Dispose()
        {
            directInput?.Dispose();
        }
    }
}
