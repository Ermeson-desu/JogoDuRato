using Microsoft.Xna.Framework;
using GameDuMouse.GameMain.Components;
using GameDuMouse.GameMain.Fases;

namespace GameDuMouse.GameMain.Systems
{
    public sealed class PlayerPhysicsSystem
    {
        public void ApplyGravityAndCollisions(PlayerBody body, IFase fase)
        {
            if (body == null)
                return;

            var position = body.Position;
            var prevPosition = position;

            body.Velocity = new Vector2(body.Velocity.X, body.Velocity.Y + body.Gravity);
            var nextPosition = position + body.Velocity;

            Rectangle nextBodyCollider;
            if (body.ManualCollider.HasValue)
            {
                var offsetX = body.ManualCollider.Value.X - (int)position.X;
                var offsetY = body.ManualCollider.Value.Y - (int)position.Y;
                nextBodyCollider = new Rectangle((int)nextPosition.X + offsetX, (int)nextPosition.Y + offsetY,
                    body.ManualCollider.Value.Width, body.ManualCollider.Value.Height);
            }
            else
            {
                nextBodyCollider = new Rectangle((int)nextPosition.X, (int)nextPosition.Y,
                    body.ColliderWidth, body.ColliderHeight);
            }

            Rectangle FootColliderAt(Vector2 p)
            {
                if (body.ManualFootCollider.HasValue)
                {
                    var offsetX = body.ManualFootCollider.Value.X - (int)position.X;
                    var offsetY = body.ManualFootCollider.Value.Y - (int)position.Y;
                    return new Rectangle((int)p.X + offsetX, (int)p.Y + offsetY,
                        body.ManualFootCollider.Value.Width, body.ManualFootCollider.Value.Height);
                }

                return new Rectangle((int)p.X + 15, (int)p.Y + body.ColliderHeight, body.ColliderWidth - 30, 5);
            }

            var nextFootCollider = FootColliderAt(nextPosition);
            var prevFootCollider = FootColliderAt(prevPosition);
            var nextFootY = nextFootCollider.Top;
            var prevFootY = prevFootCollider.Top;

            bool collided = false;

            var grounds = fase?.GroundColliders;
            if (grounds != null)
            {
                foreach (var ground in grounds)
                {
                    if (nextBodyCollider.Intersects(ground))
                    {
                        position.Y = ground.Top - body.ColliderHeight;
                        body.Velocity = new Vector2(body.Velocity.X, 0f);
                        body.IsGrounded = true;
                        collided = true;
                        break;
                    }
                }
            }

            if (!collided)
            {
                var platforms = fase?.Platforms;
                if (platforms != null)
                {
                    foreach (var platform in platforms)
                    {
                        if (prevFootY <= platform.Top && nextFootY >= platform.Top &&
                            nextFootCollider.Intersects(platform) && body.Velocity.Y >= 0f)
                        {
                            position.Y = platform.Top - body.ColliderHeight;
                            body.Velocity = new Vector2(body.Velocity.X, 0f);
                            body.IsGrounded = true;
                            collided = true;
                            break;
                        }
                    }
                }
            }

            if (!collided)
            {
                position = nextPosition;
                body.IsGrounded = false;
            }

            body.Position = position;
        }

        public void ApplyJump(PlayerBody body, bool jumpPressed)
        {
            if (body == null)
                return;

            if (jumpPressed && body.IsGrounded)
            {
                body.Velocity = new Vector2(body.Velocity.X, body.JumpStrength);
                body.IsGrounded = false;
            }
        }
    }
}
