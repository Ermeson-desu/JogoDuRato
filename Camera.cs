using Microsoft.Xna.Framework;


namespace GameDuMouse
{
    public class Camera
    {
        public Matrix Transform { get; set; }
        public Vector2 Position { get; set; }

        public void Follow(Vector2 targetPosition)
        {
            Position = targetPosition;
            Transform = Matrix.CreateTranslation(
                new Vector3(-Position.X + 300, -Position.Y + 300,0)
            );
        }
    }
}