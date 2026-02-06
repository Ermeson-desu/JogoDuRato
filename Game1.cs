using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDuMouse
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        private Camera camera;
        private Player player1;
        private Background background1;
        private ReturnStage returnStage;
        private Cheese cheese;
        private bool isReturning = false;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            camera = new Camera();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            background1 = new Background(this);
            background1.LoadContent(Content);

            player1 = new Player(this);
            player1.LoadContent(Content);

            cheese = new Cheese(this, 5500,330,80,70);
            returnStage = new ReturnStage(this);
            

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            if (!isReturning)
            {
                player1.Update(gameTime);

            // Se colidir com o queijo da primeira fase
                if (cheese.CollidesWith(player1.Collider))
                {
                    Console.WriteLine("O ratinho pegou o queijo! Agora começa o retorno.");
                    isReturning = true;
                }

                camera.Follow(player1.GetPosition());

            }
            else
            {
                player1.Update(gameTime);
                camera.Follow(player1.GetPosition());
            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(transformMatrix: camera.Transform);

            if (isReturning)
            {
                returnStage.Draw(spriteBatch);
                player1.Draw(gameTime);
            }
            else
            {
                background1.Draw(spriteBatch);
                cheese.Draw(spriteBatch);
                player1.Draw(gameTime);
            }
            
            spriteBatch.End();
        

            base.Draw(gameTime);
        }
    }
}
