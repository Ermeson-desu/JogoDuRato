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
        private Fase01 fase01;

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

            fase01 = new Fase01(this);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player1.Update(gameTime, fase01);
            fase01.Update(player1);
            camera.Follow(player1.GetPosition());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(transformMatrix: camera.Transform);

            background1.Draw(spriteBatch);
            fase01.Draw(spriteBatch, player1);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}