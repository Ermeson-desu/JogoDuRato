using GameDuMouse.GameMain.Components;
using GameDuMouse.GameMain.Fases;

namespace GameDuMouse.GameMain.Systems
{
    public sealed class PlayerCollisionSystem
    {
        public bool ShouldReset(PlayerBody body, IFase fase)
        {
            if (body == null || fase == null)
                return false;

            bool collided = false;

            if (!fase.IsReturning)
            {
                var obstacles = fase.Obstacles1;
                if (obstacles != null)
                {
                    foreach (var obstacle in obstacles)
                    {
                        if (obstacle.CollidesWith(body.Collider))
                        {
                            collided = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                var obstacles = fase.Obstacles2;
                if (obstacles != null)
                {
                    foreach (var obstacle in obstacles)
                    {
                        if (obstacle.CollidesWith(body.Collider))
                        {
                            collided = true;
                            break;
                        }
                    }
                }
            }

            if (body.Position.Y > 1500)
                return true;

            return collided;
        }
    }
}
