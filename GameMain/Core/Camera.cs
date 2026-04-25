using System;
using Microsoft.Xna.Framework;


namespace GameDuMouse.GameMain.Core
{
    public class Camera
    {
        public Matrix Transform { get; set; }
        public Vector2 Position { get; set; }
        
        public Vector2 MaxPosition { get; set; } = new Vector2(5200, 290);
        public Vector2 ViewOffset { get; set; } = new Vector2(300f, 300f);
        private Vector2 targetPos;
        private const float LerpSpeed = 0.15f; 
        private bool hasLockedY;
        private float lockedY;

        public bool LockVertical { get; set; } = true;

        public void ResetVerticalLock(float? targetY = null)
        {
            if (targetY.HasValue)
            {
                lockedY = Math.Max(0, Math.Min(targetY.Value, MaxPosition.Y));
                hasLockedY = true;
                Position = new Vector2(Position.X, lockedY);
                return;
            }

            hasLockedY = false;
        }
        
        public void Follow(Vector2 targetPosition)
        {
            float clampedX = Math.Max(0, Math.Min(targetPosition.X, MaxPosition.X));
            float clampedY = Math.Max(0, Math.Min(targetPosition.Y, MaxPosition.Y));
            float horizontalTarget = ResolveHorizontalTarget(clampedX);

            if (LockVertical)
            {
                if (!hasLockedY)
                {
                    lockedY = clampedY;
                    hasLockedY = true;
                }

                targetPos = new Vector2(horizontalTarget, lockedY);
                float nextX = MathHelper.Lerp(Position.X, targetPos.X, LerpSpeed);
                Position = new Vector2(nextX, lockedY);
            }
            else
            {
                targetPos = new Vector2(horizontalTarget, clampedY);
                Position = Vector2.Lerp(Position, targetPos, LerpSpeed);
            }
            
            Transform = Matrix.CreateTranslation(
                new Vector3(-Position.X + ViewOffset.X, -Position.Y + ViewOffset.Y, 0)
            );
        }

        private float ResolveHorizontalTarget(float playerX)
        {
            float centerX = Math.Max(300, ViewOffset.X);

            // Keep camera fixed at the start until player reaches screen center.
            if (playerX <= centerX)
                return 300f;

            // Keep camera fixed at the end when player passes the symmetric center zone.
            float endLockThreshold = MaxPosition.X - centerX - 250f;
            if (playerX >= endLockThreshold)
                return MaxPosition.X - 500f;

            return playerX;
        }
    }
}
