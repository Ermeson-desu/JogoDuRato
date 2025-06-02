using System.Drawing.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Helper;

namespace GameDuMouse
{
    public class Player
    {
        private Animation idleAnime, runAnime, trans_run, trans_idle;
        private AnimationController animationController;
        private KeyboardState previousKeyboardState,keyboardState ;
        private Game game;
        private Vector2 wallRight, wallLeft, velocity;
        private float groundY, gravity, jumpStrength;
        private bool isGrounded;

        public Player(Game game)
        {
            this.game = game;
            Initialize();
        }
        public void Initialize()
        {
            groundY = 270;
            wallRight = new Vector2(200, groundY);
            wallLeft = new Vector2(0, groundY);
            animationController = new AnimationController();
            gravity = 0.5f;
            jumpStrength = -10f;
        }

        public void LoadContent(ContentManager content)
        {
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
            
            if (keyboardState.IsKeyDown(Keys.D))
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
                else if (keyboardState.IsKeyDown(Keys.A))
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
            position += new Vector2(10, 0);

            if (position.X >= wallRight.X)
            {
                position = wallRight;
                background.ScrollLeft();
            }

            animationController.Position = position;
        }
        private void MoveLeft(Background background)
        {
            var position = animationController.Position;
            position += new Vector2(-10, 0);

            if (position.X <= wallLeft.X)
            {
                position = wallLeft;
                background.ScrollRight();
            }

            animationController.Position = position;
        }
        private void ApplyPhysics()
        {
            velocity.Y += gravity;
            var position = animationController.Position;
            position += velocity;
            if (position.Y >= groundY)
            {
                position.Y = groundY;
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
        }

    }
}