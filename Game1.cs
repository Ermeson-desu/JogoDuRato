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
        private LevelManager levelManager;

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

            // 🔑 Gerenciador de fases
            levelManager = new LevelManager(this);
            levelManager.AddFase(new Fase01(this));
           // levelManager.AddFase(new Fase02(this)); // basta adicionar aqui
            // Se quiser mais fases, só adicionar: levelManager.AddFase(new Fase03(this));

            levelManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Atualiza player e fase atual
            player1.Update(gameTime, levelManager.CurrentFase);
            levelManager.Update(player1);

            camera.Follow(player1.GetPosition());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(transformMatrix: camera.Transform);

            background1.Draw(spriteBatch);
            levelManager.Draw(spriteBatch, player1);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}