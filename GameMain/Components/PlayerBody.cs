using Microsoft.Xna.Framework;

namespace GameDuMouse.GameMain.Components
{
    public sealed class PlayerBody
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public bool IsGrounded;

        public float Gravity = 1f;
        public float JumpStrength = -18f;

        public int ColliderWidth = 100;
        public int ColliderHeight = 110;
        public int RunColliderHeight = 55;

        public Rectangle RightBarrier = new Rectangle(5700, 1, 10, 500);
        public Rectangle LeftBarrier = new Rectangle(10, 1, 10, 500);

        public Rectangle? ManualCollider;
        public Rectangle? ManualFootCollider;

        public Rectangle Collider
        {
            get
            {
                if (ManualCollider.HasValue)
                    return ManualCollider.Value;

                return new Rectangle((int)Position.X, (int)Position.Y, ColliderWidth, ColliderHeight);
            }
        }

        public Rectangle FootCollider
        {
            get
            {
                if (ManualFootCollider.HasValue)
                    return ManualFootCollider.Value;

                return new Rectangle((int)Position.X + 15, (int)Position.Y + ColliderHeight, ColliderWidth - 30, 5);
            }
        }

        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            IsGrounded = true;
            ManualCollider = null;
            ManualFootCollider = null;
        }
    }
}
