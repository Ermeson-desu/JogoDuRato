using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Helper;

namespace GameDuMouse
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        private Texture2D scenario;
        private Vector2 scenarioPosition, scenarioPosition2 ;
        private Player player1;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            scenarioPosition = new Vector2(0, 0);
            scenarioPosition2 = new Vector2(900, 0);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);
            scenario = Content.Load<Texture2D>("scenario/cenario_game");

            player1 = new Player(this);
            player1.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            player1.Move(gameTime);
            player1.Update(gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                //Movimento da primeira imagem do background
                scenarioPosition += new Vector2(-10, 0);
                //Lógica de repetição de imagem
                if (scenarioPosition.X <= -890)
                {
                    scenarioPosition = new Vector2(890, 0);
                }

                //movimento da segunda imagem background 
                scenarioPosition2 += new Vector2(-10, 0);
                //lógica de repetição de imagem
                if (scenarioPosition2.X <= -890)
                {
                    scenarioPosition2 = new Vector2(890, 0);
                }
                Console.WriteLine($"Posição do Background: 1- {scenarioPosition} 2- {scenarioPosition2}");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                    //Movimento da primeira imagem do background
                    scenarioPosition += new Vector2(10, 0);
                    //Lógica de repetição de imagem
                    if (scenarioPosition.X >= 890)
                    {
                        scenarioPosition = new Vector2(-890, 0);
                    }

                    //movimento da segunda imagem background 
                    scenarioPosition2 += new Vector2(10, 0);
                    //lógica de repetição de imagem
                    if (scenarioPosition2.X >= 890)
                    {
                        scenarioPosition2 = new Vector2(-890, 0);
                    }
                    Console.WriteLine($"Posição do Background: 1- {scenarioPosition} 2- {scenarioPosition2}");
            }
            

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(scenario ,scenarioPosition, null ,Color.White, 0f, Vector2.Zero, 3f,SpriteEffects.None, 0f );
            spriteBatch.Draw(scenario ,scenarioPosition2, null ,Color.White, 0f, Vector2.Zero, 3f,SpriteEffects.None, 0f );
            player1.Draw(gameTime);
            spriteBatch.End();
            
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
