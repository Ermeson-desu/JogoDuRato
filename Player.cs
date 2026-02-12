using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Helper;

namespace GameDuMouse
{
    public class Player
    {
        private Texture2D debugTexture;
        private Animation idleAnime, runAnime, trans_run, trans_idle;
        private AnimationController animationController;
        private KeyboardState previousKeyboardState, keyboardState;
        private Game game;
        private Vector2 velocity;
        private float groundY, gravity, jumpStrength;
        private bool previousJumpButton = false;
        private bool isGrounded;
        private int widthColliderPlayer, heightColliderPlayer, heightPlayerRun;
        private Rectangle rightBarrerCollider, leftBarrerCollider;
        private Rectangle? manualCollider, footManualCollider;
        private DirectInputController directController;

        public Rectangle Collider
        {
            get
            {
                widthColliderPlayer = 100;
                heightColliderPlayer = 110;
                var pos = animationController.Position;
                if (manualCollider.HasValue)
                    return manualCollider.Value;

                return new Rectangle((int)pos.X, (int)pos.Y, widthColliderPlayer, heightColliderPlayer);
            }
            set { manualCollider = value; }
        }

        private Rectangle FootPlayer
        {
            get
            {
                var pos = animationController.Position;
                if (footManualCollider.HasValue)
                    return footManualCollider.Value;

                return new Rectangle((int)pos.X + 15, (int)pos.Y + heightColliderPlayer, widthColliderPlayer - 30, 5);
            }
            set { footManualCollider = value; }
        }

        public Player(Game game)
        {
            this.game = game;
            Initialize();
            directController = new DirectInputController();
        }

        public Vector2 GetPosition() => new Vector2(Collider.X, 300);

        public void Initialize()
        {
            heightPlayerRun = 55;
            groundY = 400;
            animationController = new AnimationController();
            gravity = 1f;
            jumpStrength = -18f;
            rightBarrerCollider = new Rectangle(5700, 1, 10, 500);
            leftBarrerCollider = new Rectangle(10, 1, 10, 500);
        }

        public void LoadContent(ContentManager content)
        {
            debugTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            debugTexture.SetData(new[] { Color.White });

            idleAnime = new Animation(game, 170f);
            idleAnime.AddSprite("Idle/Idle01", "Idle/Idle02", "Idle/Idle03",
                                "Idle/Idle04", "Idle/Idle05", "Idle/Idle06");
            idleAnime.Position = new Vector2(210, groundY);

            runAnime = new Animation(game, 50f);
            runAnime.AddSprite("run/run01", "run/run02", "run/run03",
                                "run/run04", "run/run05", "run/run06", "run/run07");
            runAnime.Position = new Vector2(210, groundY);

            trans_idle = new Animation(game, 50f);
            trans_idle.AddSprite("Trans/trans05", "Trans/trans04", "Trans/trans03",
                                 "Trans/trans02", "Trans/trans01");
            trans_idle.Position = new Vector2(210, groundY);

            trans_run = new Animation(game, 50f);
            trans_run.AddSprite("Trans/trans01", "Trans/trans02", "Trans/trans03",
                                 "Trans/trans04", "Trans/trans05", "Trans/trans06");
            trans_run.Position = new Vector2(210, groundY);

            animationController.AddAnimation(PlayerState.Idle, idleAnime);
            animationController.AddAnimation(PlayerState.Running, runAnime);
            animationController.AddAnimation(PlayerState.TransitionToRun, trans_run);
            animationController.AddAnimation(PlayerState.TransitionToIdle, trans_idle);
        }

        private void HandleInput()
        {
            var keyboard = Keyboard.GetState();
            var state = directController.GetState();

            // --- PULO ---
            bool jumpPressed = keyboard.IsKeyDown(Keys.Space);
            bool jumpPressedPrevious = previousKeyboardState.IsKeyDown(Keys.Space);

            bool gamepadJump = state != null && state.Buttons[2]; // ajuste índice conforme seu controle
            bool gamepadJumpPrevious = previousJumpButton;
            previousJumpButton = gamepadJump;

            if (((jumpPressed && !jumpPressedPrevious) || (gamepadJump && !gamepadJumpPrevious)) && isGrounded)
            {
                velocity.Y = jumpStrength;
                isGrounded = false;
            }

            // --- MOVIMENTO ---
            bool moveRight = keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right);
            bool moveLeft  = keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left);

            if (state != null)
            {
                if (state.X > 50000) moveRight = true;
                if (state.X < 10000) moveLeft = true;

                if (state.PointOfViewControllers.Length > 0)
                {
                    int pov = state.PointOfViewControllers[0];
                    if (pov == 9000) moveRight = true;
                    if (pov == 27000) moveLeft = true;
                }
            }

            if (moveRight)
            {
                animationController.Effects = SpriteEffects.None;
                if (animationController.CurrentState != PlayerState.Running &&
                    animationController.CurrentState != PlayerState.TransitionToRun)
                {
                    animationController.StartTransition(PlayerState.TransitionToRun, 80);
                }
                if (animationController.CurrentState == PlayerState.Running)
                {
                    Collider = new Rectangle((int)animationController.Position.X + 50,
                                             (int)animationController.Position.Y + heightPlayerRun,
                                             widthColliderPlayer, heightPlayerRun);
                    FootPlayer = new Rectangle((int)animationController.Position.X + 65,
                                               (int)animationController.Position.Y + heightColliderPlayer,
                                               widthColliderPlayer - 30, 5);
                    MoveRight();
                }
            }
            else if (moveLeft)
            {
                animationController.Effects = SpriteEffects.FlipHorizontally;
                if (animationController.CurrentState != PlayerState.Running &&
                    animationController.CurrentState != PlayerState.TransitionToRun)
                {
                    animationController.StartTransition(PlayerState.TransitionToRun, 80);
                }
                if (animationController.CurrentState == PlayerState.Running)
                {
                    Collider = new Rectangle((int)animationController.Position.X,
                                             (int)animationController.Position.Y + heightPlayerRun,
                                             widthColliderPlayer, heightPlayerRun);
                    FootPlayer = new Rectangle((int)animationController.Position.X + 15,
                                               (int)animationController.Position.Y + heightColliderPlayer,
                                               widthColliderPlayer - 30, 5);
                    MoveLeft();
                }
            }
            else
            {
                if (animationController.CurrentState != PlayerState.Idle &&
                    animationController.CurrentState != PlayerState.TransitionToIdle)
                {
                    manualCollider = null;
                    footManualCollider = null;
                    animationController.StartTransition(PlayerState.TransitionToIdle, 80);
                }
            }
        }

        private void MoveRight()
        {
            var position = animationController.Position;
            var nextPosition = position + new Vector2(10, 0);
            var futureCollider = new Rectangle((int)nextPosition.X, (int)nextPosition.Y, widthColliderPlayer, heightColliderPlayer);

            if (futureCollider.Intersects(rightBarrerCollider)) return;
            animationController.Position = nextPosition;
        }

        private void MoveLeft()
        {
            var position = animationController.Position;
            var nextPosition = position + new Vector2(-10, 0);
            var futureCollider = new Rectangle((int)nextPosition.X, (int)nextPosition.Y, widthColliderPlayer, heightColliderPlayer);

            if (futureCollider.Intersects(leftBarrerCollider)) return;
            animationController.Position = nextPosition;
        }

        private void ApplyPhysics(Fase01 fase)
        {
            var position = animationController.Position;
            var prevPosition = position;

            velocity.Y += gravity;
            var nextPosition = position + velocity;

            // Se houver um Collider manual, mantém offset/size
            Rectangle nextBodyCollider;
            if (manualCollider.HasValue)
            {
                var offsetX = manualCollider.Value.X - (int)position.X;
                var offsetY = manualCollider.Value.Y - (int)position.Y;
                nextBodyCollider = new Rectangle((int)nextPosition.X + offsetX, (int)nextPosition.Y + offsetY,
                                                manualCollider.Value.Width, manualCollider.Value.Height);
            }
            else
            {
                nextBodyCollider = new Rectangle((int)nextPosition.X, (int)nextPosition.Y,
                                                widthColliderPlayer, heightColliderPlayer);
            }

            // Função local para construir colisor dos pés
            Rectangle FootColliderAt(Vector2 p)
            {
                if (footManualCollider.HasValue)
                {
                    var offsetX = footManualCollider.Value.X - (int)position.X;
                    var offsetY = footManualCollider.Value.Y - (int)position.Y;
                    return new Rectangle((int)p.X + offsetX, (int)p.Y + offsetY,
                                        footManualCollider.Value.Width, footManualCollider.Value.Height);
                }
                return new Rectangle((int)p.X + 15, (int)p.Y + heightColliderPlayer, widthColliderPlayer - 30, 5);
            }

            var nextFootCollider = FootColliderAt(nextPosition);
            var prevFootCollider = FootColliderAt(prevPosition);
            var nextFootY = nextFootCollider.Top;
            var prevFootY = prevFootCollider.Top;

            bool collided = false;

            // --- Chão ---
            foreach (var ground in fase.GroundColliders)
            {
                if (nextBodyCollider.Intersects(ground))
                {
                    position.Y = ground.Top - heightColliderPlayer;
                    velocity.Y = 0;
                    isGrounded = true;
                    collided = true;
                    break;
                }
            }

            // --- Plataformas ---
            if (!collided)
            {
                foreach (var platform in fase.Platforms)
                {
                    if (prevFootY <= platform.Top && nextFootY >= platform.Top &&
                        nextFootCollider.Intersects(platform) && velocity.Y >= 0f)
                    {
                        position.Y = platform.Top - heightColliderPlayer;
                        velocity.Y = 0;
                        isGrounded = true;
                        collided = true;
                        break;
                    }
                }
            }

            // --- Se não colidiu, continua caindo ---
            if (!collided)
            {
                position = nextPosition;
                isGrounded = false;
            }

            animationController.Position = position;
        }
        public void ResetPlayer()
        {
            animationController.Position = new Vector2(210, 300);
            velocity = Vector2.Zero;
            isGrounded = true;
        }

        public void Update(GameTime gameTime, Fase01 fase)
        {
            keyboardState = Keyboard.GetState();
            ApplyPhysics(fase);
            HandleInput();
            animationController.Update(gameTime);
            previousKeyboardState = keyboardState;

            bool collideWithObstacle = false;
            foreach (var obstacle in fase.Obstacles)
            {
                if(obstacle.CollidesWith(Collider))
                {
                    collideWithObstacle = true;
                    break;
                }
            }

            if (animationController.Position.Y > 1500 
              ||collideWithObstacle )
                ResetPlayer();
        }

        public void Draw(GameTime gameTime)
        {
            animationController.Draw(gameTime);

            var spriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));

            spriteBatch.Draw(debugTexture, Collider, Color.Yellow * 0.5f);
            spriteBatch.Draw(debugTexture, FootPlayer, Color.Blue * 0.5f);
        }
    }
}