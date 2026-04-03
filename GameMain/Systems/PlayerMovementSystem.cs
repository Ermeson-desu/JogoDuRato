using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Components;
using GameDuMouse.GameMain.Fases;
using GameDuMouse.GameMain.Animations;

namespace GameDuMouse.GameMain.Systems
{
    public sealed class PlayerMovementSystem
    {
        public void Apply(PlayerBody body, PlayerInputCommand input, IFase fase, PlayerAnimationSystem animation)
        {
            if (body == null || animation == null)
                return;

            if (input.MoveDirection > 0)
            {
                animation.SetFacing(true);
                if (animation.CurrentState != PlayerState.Running &&
                    animation.CurrentState != PlayerState.TransitionToRun)
                {
                    animation.StartTransition(PlayerState.TransitionToRun, 80);
                }

                if (animation.CurrentState == PlayerState.Running)
                {
                    body.ManualCollider = new Rectangle((int)body.Position.X + 50,
                        (int)body.Position.Y + body.RunColliderHeight,
                        body.ColliderWidth, body.RunColliderHeight);

                    body.ManualFootCollider = new Rectangle((int)body.Position.X + 65,
                        (int)body.Position.Y + body.ColliderHeight,
                        body.ColliderWidth - 30, 5);

                    MoveRight(body, fase);
                }
            }
            else if (input.MoveDirection < 0)
            {
                animation.SetFacing(false);
                if (animation.CurrentState != PlayerState.Running &&
                    animation.CurrentState != PlayerState.TransitionToRun)
                {
                    animation.StartTransition(PlayerState.TransitionToRun, 80);
                }

                if (animation.CurrentState == PlayerState.Running)
                {
                    body.ManualCollider = new Rectangle((int)body.Position.X,
                        (int)body.Position.Y + body.RunColliderHeight,
                        body.ColliderWidth, body.RunColliderHeight);

                    body.ManualFootCollider = new Rectangle((int)body.Position.X + 15,
                        (int)body.Position.Y + body.ColliderHeight,
                        body.ColliderWidth - 30, 5);

                    MoveLeft(body, fase);
                }
            }
            else
            {
                if (animation.CurrentState != PlayerState.Idle &&
                    animation.CurrentState != PlayerState.TransitionToIdle)
                {
                    body.ManualCollider = null;
                    body.ManualFootCollider = null;
                    animation.StartTransition(PlayerState.TransitionToIdle, 80);
                }
            }
        }

        private void MoveRight(PlayerBody body, IFase fase)
        {
            var position = body.Position;
            var nextPosition = position + new Vector2(10, 0);
            var futureCollider = new Rectangle((int)nextPosition.X, (int)nextPosition.Y, body.ColliderWidth, body.ColliderHeight);

            if (CollidesWithWalls(fase, body, futureCollider))
                return;

            body.Position = nextPosition;
        }

        private void MoveLeft(PlayerBody body, IFase fase)
        {
            var position = body.Position;
            var nextPosition = position + new Vector2(-10, 0);
            var futureCollider = new Rectangle((int)nextPosition.X, (int)nextPosition.Y, body.ColliderWidth, body.ColliderHeight);

            if (CollidesWithLeftBarrier(fase, body, futureCollider))
                return;

            body.Position = nextPosition;
        }

        private bool CollidesWithWalls(IFase fase, PlayerBody body, Rectangle futureCollider)
        {
            var walls = fase?.WallColliders;
            if (walls != null && walls.Count > 0)
            {
                foreach (var wall in walls)
                {
                    if (futureCollider.Intersects(wall))
                        return true;
                }
                return false;
            }

            return futureCollider.Intersects(body.RightBarrier);
        }

        private bool CollidesWithLeftBarrier(IFase fase, PlayerBody body, Rectangle futureCollider)
        {
            var walls = fase?.WallColliders;
            if (walls != null && walls.Count > 0)
            {
                foreach (var wall in walls)
                {
                    if (futureCollider.Intersects(wall))
                        return true;
                }
                return false;
            }

            return futureCollider.Intersects(body.LeftBarrier);
        }
    }
}
