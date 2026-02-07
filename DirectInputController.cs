using SharpDX.DirectInput;
using System;

namespace GameDuMouse
{
    public class DirectInputController
    {
        private DirectInput directInput;
        private Joystick joystick;

        public DirectInputController()
        {
            directInput = new DirectInput();

            // pega o primeiro controle conectado
            var devices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            if (devices.Count > 0)
            {
                joystick = new Joystick(directInput, devices[0].InstanceGuid);
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
                Console.WriteLine("Controle DirectInput conectado: " + devices[0].ProductName);
            }
            else
            {
                Console.WriteLine("Nenhum controle DirectInput encontrado.");
            }
        }

        public JoystickState GetState()
        {
            if (joystick == null) return null;
            joystick.Poll();
            return joystick.GetCurrentState();
        }
    }
}