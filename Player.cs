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
        private const int tailOfiset = 44;

        private Texture2D debugTexture;

        private Animation idleAnime, runAnime, trans_run, trans_idle;
        private AnimationController animationController;
        private KeyboardState previousKeyboardState, keyboardState;
        private Game game;
        private Vector2 velocity;
        private float groundY, gravity, jumpStrength;
        private bool isGrounded;
        private bool canJump = true; 
        private int widthColliderPlayer,heightColliderPlayer, heightPlayerRun;
        private Rectangle saltLid, upStove,platform1, platform2;
        private Rectangle groundCollider, groundCollider2, rightBarrerCollider, leftBarrerCollider;
        private Rectangle? manualCollider,footManualCollider;
        private Rectangle Collider
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
            set
            {
                manualCollider = value;
            }
        }
        private Rectangle FootPlayer
        {
            get
            {
                var pos = animationController.Position;

                if (footManualCollider.HasValue)
                    return footManualCollider.Value;

                // Reduz a largura dos pés em 30 pixels de cada lado para evitar flutuação nas bordas
                return new Rectangle((int)pos.X + 15, (int)pos.Y + heightColliderPlayer, widthColliderPlayer - 30, 5);
            }
            set
            {
                footManualCollider = value;
            }
        }

        public Player(Game game)
        {
            this.game = game;
            Initialize();
        }
        public Vector2 GetPosition()
        {
            Vector2 positionPlayer = new Vector2(Collider.X, 300);
            return positionPlayer;
        }
        public void Initialize()
        {
            heightPlayerRun = 55;
            groundY = 400;
            animationController = new AnimationController();
            gravity = 1f;
            jumpStrength = -18f;
            groundCollider = new Rectangle(0, (int)groundY, 2600, 5);
            groundCollider2 = new Rectangle(3000, (int)groundY, 2700, 5);
            rightBarrerCollider = new Rectangle(5700, 1, 10, 500);
            leftBarrerCollider = new Rectangle(10, 1, 10, 500);
            saltLid = new Rectangle(1155, 291, 95, 5);
            upStove = new Rectangle(1310,173,95,5);

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

        public void Move()
        {
            var pos = animationController.Position;
            bool SpacePressed = keyboardState.IsKeyDown(Keys.Space);
            bool SpacePressedPrevious = previousKeyboardState.IsKeyDown(Keys.Space);

            // Jump only on fresh Space press AND when grounded
            if (SpacePressed && !SpacePressedPrevious && isGrounded)
            {
                velocity.Y = jumpStrength;
                isGrounded = false;
            }

            if (isGrounded)
            {
                canJump = true;
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
                    Collider = new Rectangle((int)pos.X + 50, (int)pos.Y + heightPlayerRun ,widthColliderPlayer,heightPlayerRun);
                    FootPlayer = new Rectangle((int)pos.X + 50 + 15, (int)pos.Y + heightColliderPlayer, widthColliderPlayer - 30, 5);
                    MoveRight();
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
                    Collider = new Rectangle((int)pos.X, (int)pos.Y + heightPlayerRun, widthColliderPlayer, heightPlayerRun);
                    FootPlayer = new Rectangle((int)pos.X + 15, (int)pos.Y + heightColliderPlayer, widthColliderPlayer - 30, 5);
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

            if (futureCollider.Intersects(rightBarrerCollider))
            {
                return;
            }

            animationController.Position = nextPosition;
            Console.WriteLine(position);
        }
        private void MoveLeft()
        {
            var position = animationController.Position;
            var nextPosition = position + new Vector2(-10, 0);
            var futureCollider = new Rectangle((int)nextPosition.X, (int)nextPosition.Y, widthColliderPlayer, heightColliderPlayer);

            if (futureCollider.Intersects(leftBarrerCollider))
            {
                return;
            }
            

            animationController.Position = nextPosition;
            Console.WriteLine(position);
        }
        private void ApplyPhysics()
        {
            var position = animationController.Position;
            var prevPosition = position;

            velocity.Y += gravity;
            var nextPosition = position + velocity;

            // Se houver um Collider manual, mantenha o offset/size ao calcular o próximo colisor do corpo
            Rectangle nextBodyCollider;
            if (manualCollider.HasValue)
            {
                var offsetX = manualCollider.Value.X - (int)position.X;
                var offsetY = manualCollider.Value.Y - (int)position.Y;
                nextBodyCollider = new Rectangle((int)nextPosition.X + offsetX, (int)nextPosition.Y + offsetY, manualCollider.Value.Width, manualCollider.Value.Height);
            }
            else
            {
                nextBodyCollider = new Rectangle((int)nextPosition.X, (int)nextPosition.Y, widthColliderPlayer, heightColliderPlayer);
            }

            // Função local para construir o colisor dos pés na posição fornecida (prev/next)
            Rectangle FootColliderAt(Vector2 p)
            {
                if (footManualCollider.HasValue)
                {
                    var offsetX = footManualCollider.Value.X - (int)position.X;
                    var offsetY = footManualCollider.Value.Y - (int)position.Y;
                    return new Rectangle((int)p.X + offsetX, (int)p.Y + offsetY, footManualCollider.Value.Width, footManualCollider.Value.Height);
                }

                // padrão reduzido (mesma lógica que a propriedade FootPlayer)
                return new Rectangle((int)p.X + 15, (int)p.Y + heightColliderPlayer, widthColliderPlayer - 30, 5);
            }

            var nextFootCollider = FootColliderAt(nextPosition);
            var prevFootCollider = FootColliderAt(prevPosition);
            var nextFootY = nextFootCollider.Top;
            var prevFootY = prevFootCollider.Top;

            // Altura atual do jogador (considera collider manual quando existente)
            int currentPlayerHeight = manualCollider.HasValue ? manualCollider.Value.Height : heightColliderPlayer;

            if (nextBodyCollider.Intersects(groundCollider))
            {
                if (manualCollider.HasValue)
                {
                    var offsetY = manualCollider.Value.Y - (int)position.Y;
                    position.Y = groundCollider.Top - manualCollider.Value.Height - offsetY;
                }
                else
                {
                    position.Y = groundCollider.Top - heightColliderPlayer;
                }

                velocity.Y = 0;
                isGrounded = true;
            }
            else if (nextBodyCollider.Intersects(groundCollider2))
            {
                if (manualCollider.HasValue)
                {
                    var offsetY = manualCollider.Value.Y - (int)position.Y;
                    position.Y = groundCollider2.Top - manualCollider.Value.Height - offsetY;
                }
                else
                {
                    position.Y = groundCollider2.Top - heightColliderPlayer;
                }

                velocity.Y = 0;
                isGrounded = true;
            }
            else if (prevFootY <= saltLid.Top && nextFootY >= saltLid.Top && nextFootCollider.Intersects(saltLid) && velocity.Y >= 0f)
            {
                if (manualCollider.HasValue)
                {
                    var offsetY = manualCollider.Value.Y - (int)position.Y;
                    position.Y = saltLid.Top - manualCollider.Value.Height - offsetY;
                }
                else
                {
                    position.Y = saltLid.Top - heightColliderPlayer;
                }

                velocity.Y = 0.5f;
                isGrounded = true;
            }
            // Up stove platform
            else if (prevFootY <= upStove.Top && nextFootY >= upStove.Top && nextFootCollider.Intersects(upStove) && velocity.Y >= 0f)
            {
                if (manualCollider.HasValue)
                {
                    var offsetY = manualCollider.Value.Y - (int)position.Y;
                    position.Y = upStove.Top - manualCollider.Value.Height - offsetY;
                }
                else
                {
                    position.Y = upStove.Top - heightColliderPlayer;
                }

                velocity.Y = 0;
                isGrounded = true;   
            }
            else
            {
                position = nextPosition;
                isGrounded = false;
            }

            animationController.Position = position;
        }
        public void Update(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();
            ApplyPhysics();
            Move();
            animationController.Update(gameTime);
            previousKeyboardState = keyboardState;

            // Reset player to initial position if falls beyond Y = 1500
            if (animationController.Position.Y > 1500)
            {
                animationController.Position = new Vector2(210, groundY);
                velocity = Vector2.Zero;
                isGrounded = false;
            }
        }
        public void Draw(GameTime gameTime)
        {
            animationController.Draw(gameTime);

            var spriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));

            spriteBatch.Draw(debugTexture, saltLid, Color.Blue * 0.4f);
            spriteBatch.Draw(debugTexture, upStove, Color.Blue * 0.4f);
            // Chão - vermelho translúcido
            spriteBatch.Draw(debugTexture, groundCollider, Color.Red * 0.4f);
            spriteBatch.Draw(debugTexture, groundCollider2, Color.Red * 0.4f);

            // Barreira direita - azul translúcido
            spriteBatch.Draw(debugTexture, rightBarrerCollider, Color.Blue * 0.4f);

            // Barreira esquerda - verde translúcido
            spriteBatch.Draw(debugTexture, leftBarrerCollider, Color.Green * 0.4f);

            // Collider do próprio player - amarelo
            spriteBatch.Draw(debugTexture, Collider, Color.Yellow * 0.5f);
            spriteBatch.Draw(debugTexture, FootPlayer, Color.Blue*0.5f);
        }

    }
}