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
        private Animation idleAnime;
        private Texture2D scenario;
        private Vector2 wallLeft, wallRight, scenarioPosition, scenarioPosition2 ;
        private float personX, personY;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            personX = 100;
            personY = 270;
            wallRight = new Vector2(200, personY);
            wallLeft = new Vector2(-130,personY);
            scenarioPosition = new Vector2(0, 0);
            scenarioPosition2 = new Vector2(900, 0);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            idleAnime = new Animation(this, 170f);
            idleAnime.AddSprite("Idle/Idle01", "Idle/Idle02", "Idle/Idle03",
                                "Idle/Idle04", "Idle/Idle05", "Idle/Idle06");
            idleAnime.Position = new Vector2(personX, personY);

            scenario = Content.Load<Texture2D>("scenario/cenario_game");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                idleAnime.Position += new Vector2(10, 0);
                idleAnime.Effects = SpriteEffects.None;
                if (idleAnime.Position.X >= wallRight.X)
                {
                    idleAnime.Position = wallRight;

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
                Console.WriteLine($"Posição do personagem: {idleAnime.Position}");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                //movimento do personagem
                idleAnime.Position += new Vector2(-10, 0);
                idleAnime.Effects = SpriteEffects.FlipHorizontally;

                //lógica para limitar a posição do personagem
                if (idleAnime.Position.X <= wallLeft.X)
                {
                    //limitador de posição do personagem
                    idleAnime.Position = wallLeft;

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
                Console.WriteLine($"Posição do personagem: {idleAnime.Position}");
            }
            idleAnime.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(scenario ,scenarioPosition, null ,Color.White, 0f, Vector2.Zero, 3f,SpriteEffects.None, 0f );
            spriteBatch.Draw(scenario ,scenarioPosition2, null ,Color.White, 0f, Vector2.Zero, 3f,SpriteEffects.None, 0f );
            idleAnime.Draw(gameTime);
            spriteBatch.End();
            
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
