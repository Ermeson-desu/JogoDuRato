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
        private KeyboardState previousKeyboardState,keyboardState ;
        private Game game;
        private Vector2 velocity;
        private float groundY, gravity, jumpStrength;
        private bool isGrounded;
        private Rectangle groundCollider,rightBarrerCollider, leftBarrerCollider;
        private Rectangle Collider
        {
            get
            {
                var pos = animationController.Position;
                return new Rectangle((int)pos.X, (int)pos.Y, 100, 110);
            }
        }

        public Player(Game game)
        {
            this.game = game;
            Initialize();
        }
        public void Initialize()
        {
            groundY = 270;
            animationController = new AnimationController();
            gravity = 1f;
            jumpStrength = -15f;
            groundCollider = new Rectangle(0, 400, 800, 50);
            rightBarrerCollider = new Rectangle(400, 1, 10, 500);
            leftBarrerCollider = new Rectangle(200,1, 10,500);
            
        }

        public void LoadContent(ContentManager content)
        {
            debugTexture = new Texture2D(game.GraphicsDevice,1,1);
            debugTexture.SetData(new[]{ Color.White});

            idleAnime = new Animation(game, 170f);
            idleAnime.AddSprite("Idle/Idle01", "Idle/Idle02", "Idle/Idle03",
                                "Idle/Idle04", "Idle/Idle05", "Idle/Idle06");
            idleAnime.Position = new Vector2(100, groundY);

            runAnime = new Animation(game, 50f);
            runAnime.AddSprite("run/run01", "run/run02", "run/run03",
                                "run/run04", "run/run05", "run/run06", "run/run07");
            runAnime.Position = new Vector2(100, groundY);

            trans_idle = new Animation(game, 50f);
            trans_idle.AddSprite("Trans/trans05", "Trans/trans04", "Trans/trans03",
                                 "Trans/trans02", "Trans/trans01");
            trans_idle.Position = new Vector2(100, groundY);

            trans_run = new Animation(game, 50f);
            trans_run.AddSprite("Trans/trans01", "Trans/trans02", "Trans/trans03",
                                 "Trans/trans04", "Trans/trans05", "Trans/trans06");
            trans_run.Position = new Vector2(100, groundY);

            animationController.AddAnimation(PlayerState.Idle, idleAnime);
            animationController.AddAnimation(PlayerState.Running, runAnime);
            animationController.AddAnimation(PlayerState.TransitionToRun, trans_run);
            animationController.AddAnimation(PlayerState.TransitionToIdle, trans_idle);
        }

        public void Move(GameTime gameTime, Background background)
        {
            keyboardState = Keyboard.GetState();
            bool SpacePressed = keyboardState.IsKeyDown(Keys.Space) &&
                                previousKeyboardState.IsKeyUp(Keys.Space);
            
            if (SpacePressed && isGrounded)
            {
                velocity.Y = jumpStrength;
                isGrounded = false;
            }
            
            if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                {
                    animationController.Effects = SpriteEffects.None;

                    if (animationController.CurrentState != PlayerState.Running &&
                       animationController.CurrentState != PlayerState.TransitionToRun)
                    {
                        animationController.StartTransition(PlayerState.TransitionToRun, 80);
                    }
                    if (animationController.CurrentState == PlayerState.Running)
                    {
                        MoveRight(background);
                    }
                }
                else if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                {
                    animationController.Effects = SpriteEffects.FlipHorizontally;

                    if (animationController.CurrentState != PlayerState.Running &&
                        animationController.CurrentState != PlayerState.TransitionToRun)
                    {
                        animationController.StartTransition(PlayerState.TransitionToRun, 80);
                    }

                    if (animationController.CurrentState == PlayerState.Running)
                    {
                        MoveLeft(background);
                    }
                }
                else
                {
                    if (animationController.CurrentState != PlayerState.Idle &&
                       animationController.CurrentState != PlayerState.TransitionToIdle)
                    {
                        animationController.StartTransition(PlayerState.TransitionToIdle, 80);
                    }
                }
        }
        private void MoveRight(Background background)
        {
            var position = animationController.Position;
            var nextPosition = position + new Vector2(10, 0);
            var futureCollider = new Rectangle((int)nextPosition.X,(int)nextPosition.Y,100,110);

            if (futureCollider.Intersects(rightBarrerCollider))
            {
                background.ScrollLeft();
                return;
            }

            animationController.Position = nextPosition;
        }
        private void MoveLeft(Background background)
        {
            var position = animationController.Position;
            var nextPosition = position + new Vector2(-10, 0);
            var futureCollider = new Rectangle((int)nextPosition.X,(int)nextPosition.Y,100,110);

            if (futureCollider.Intersects(leftBarrerCollider))
            {
                background.ScrollRight();
                return;
            }

            animationController.Position = nextPosition;
        }
        private void ApplyPhysics()
        {
            var position = animationController.Position;
            var playerCollider = new Rectangle((int)position.X,(int)position.Y,100,110);
            velocity.Y += gravity;
            position += velocity;
            if (playerCollider.Intersects(groundCollider))
            {
                position.Y = groundCollider.Top - 110;
                velocity.Y = 0;
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
            animationController.Position = position;
        }
        public void Update(GameTime gameTime)
        {
            ApplyPhysics();
            animationController.Update(gameTime);
            previousKeyboardState = keyboardState;
        }
        public void Draw(GameTime gameTime)
        {
            animationController.Draw(gameTime);

            var spriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));

            // Chão - vermelho translúcido
            spriteBatch.Draw(debugTexture, groundCollider, Color.Red * 0.4f);

            // Barreira direita - azul translúcido
            spriteBatch.Draw(debugTexture, rightBarrerCollider, Color.Blue * 0.4f);

            // Barreira esquerda - verde translúcido
            spriteBatch.Draw(debugTexture, leftBarrerCollider, Color.Green * 0.4f);

            // Collider do próprio player - amarelo
            spriteBatch.Draw(debugTexture, Collider, Color.Yellow * 0.5f);
        }

    }
}