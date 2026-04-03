using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Rendering;
using GameDuMouse.GameMain.Fases;
using GameDuMouse.GameMain.Components;
using GameDuMouse.GameMain.Systems;

namespace GameDuMouse.GameMain.Entities
{
    public class Player
    {
        private readonly Game game;
        private Texture2D debugTexture;
        private float groundY;

        private PlayerBody body;
        private PlayerInputSystem inputSystem;
        private PlayerPhysicsSystem physicsSystem;
        private PlayerMovementSystem movementSystem;
        private PlayerCollisionSystem collisionSystem;
        private PlayerAnimationSystem animationSystem;

        public Rectangle Collider => body != null ? body.Collider : Rectangle.Empty;
        private Rectangle FootPlayer => body != null ? body.FootCollider : Rectangle.Empty;

        public Player(Game game)
        {
            this.game = game;
            Initialize();
            inputSystem = new PlayerInputSystem(game);
            physicsSystem = new PlayerPhysicsSystem();
            movementSystem = new PlayerMovementSystem();
            collisionSystem = new PlayerCollisionSystem();
            animationSystem = new PlayerAnimationSystem();
        }

        public Vector2 GetPosition() => new Vector2(Collider.X, 300);

        public void Initialize()
        {
            groundY = 400;
            body = new PlayerBody();
            body.Position = new Vector2(210, groundY);
            body.IsGrounded = true;
        }

        public void LoadContent(ContentManager content)
        {
            var cache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            debugTexture = cache != null ? cache.Pixel : debugTexture;

            animationSystem.LoadContent(game, groundY);
            animationSystem.SetPosition(body.Position);
        }

        public void ResetPlayer()
        {
            if (body == null)
                return;

            body.Reset(new Vector2(210, 300));
            animationSystem.SetPosition(body.Position);
        }

        public void SetPosition(Vector2 position)
        {
            if (body == null)
                return;

            body.Position = position;
            animationSystem.SetPosition(position);
        }

        public void SetFacing(bool faceRight)
        {
            animationSystem?.SetFacing(faceRight);
        }

        public bool FacingRight => animationSystem != null && animationSystem.FacingRight;

        public void Update(GameTime gameTime, IFase fase)
        {
            if (fase == null || fase.HasWon || body == null)
                return;

            var input = inputSystem.ReadInput();

            physicsSystem.ApplyGravityAndCollisions(body, fase);
            physicsSystem.ApplyJump(body, input.JumpPressed);
            movementSystem.Apply(body, input, fase, animationSystem);

            animationSystem.SetPosition(body.Position);
            animationSystem.Update(gameTime);

            if (collisionSystem.ShouldReset(body, fase))
                ResetPlayer();
        }

        public void Draw(GameTime gameTime)
        {
            animationSystem.Draw(gameTime);

            var spriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));
            if (spriteBatch == null || debugTexture == null)
                return;

            spriteBatch.Draw(debugTexture, Collider, Color.Yellow * 0.5f);
            spriteBatch.Draw(debugTexture, FootPlayer, Color.Blue * 0.5f);
        }
    }
}
