using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Animations;

namespace GameDuMouse.GameMain.Systems
{
    public sealed class PlayerAnimationSystem
    {
        private readonly AnimationController controller = new AnimationController();
        private Animation idleAnime;
        private Animation runAnime;
        private Animation transRun;
        private Animation transIdle;

        public PlayerState CurrentState => controller.CurrentState;

        public void LoadContent(Microsoft.Xna.Framework.Game game, float groundY)
        {
            idleAnime = new Animation(game, 170f);
            idleAnime.AddSprite("Idle/Idle01", "Idle/Idle02", "Idle/Idle03",
                                "Idle/Idle04", "Idle/Idle05", "Idle/Idle06");
            idleAnime.Position = new Vector2(210, groundY);

            runAnime = new Animation(game, 50f);
            runAnime.AddSprite("run/run01", "run/run02", "run/run03",
                                "run/run04", "run/run05", "run/run06", "run/run07");
            runAnime.Position = new Vector2(210, groundY);

            transIdle = new Animation(game, 50f);
            transIdle.AddSprite("Trans/trans05", "Trans/trans04", "Trans/trans03",
                                 "Trans/trans02", "Trans/trans01");
            transIdle.Position = new Vector2(210, groundY);

            transRun = new Animation(game, 50f);
            transRun.AddSprite("Trans/trans01", "Trans/trans02", "Trans/trans03",
                                 "Trans/trans04", "Trans/trans05", "Trans/trans06");
            transRun.Position = new Vector2(210, groundY);

            controller.AddAnimation(PlayerState.Idle, idleAnime);
            controller.AddAnimation(PlayerState.Running, runAnime);
            controller.AddAnimation(PlayerState.TransitionToRun, transRun);
            controller.AddAnimation(PlayerState.TransitionToIdle, transIdle);
        }

        public void Update(GameTime gameTime)
        {
            controller.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            controller.Draw(gameTime);
        }

        public void SetPosition(Vector2 position)
        {
            controller.Position = position;
        }

        public Vector2 GetPosition()
        {
            return controller.Position;
        }

        public void StartTransition(PlayerState state, double duration)
        {
            controller.StartTransition(state, duration);
        }

        public void SetFacing(bool faceRight)
        {
            controller.Effects = faceRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        }

        public bool FacingRight => controller.Effects == SpriteEffects.None;
    }
}
