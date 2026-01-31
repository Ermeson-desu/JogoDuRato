using System;
using Microsoft.Xna.Framework;


namespace GameDuMouse
{
    public class Camera
    {
        public Matrix Transform { get; set; }
        public Vector2 Position { get; set; }
        
        public Vector2 MaxPosition { get; set; } = new Vector2(5200, 290);
        private Vector2 targetPos;
        private const float LerpSpeed = 0.15f; 
        
        public void Follow(Vector2 targetPosition)
        {
            targetPos = new Vector2(
                Math.Max(0, Math.Min(targetPosition.X, MaxPosition.X)),
                Math.Max(0, Math.Min(targetPosition.Y, MaxPosition.Y))
            );
            
            Position = Vector2.Lerp(Position, targetPos, LerpSpeed);
            
            Transform = Matrix.CreateTranslation(
                new Vector3(-Position.X + 300, -Position.Y + 300, 0)
            );
        }
    }
}