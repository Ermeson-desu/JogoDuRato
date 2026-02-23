using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Entities;
using GameDuMouse.GameMain.Fases;
using GameDuMouse.GameMain.UI;

namespace GameDuMouse.GameMain.Core
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;

        private Camera camera;
        private Player player1;
        private Background background1;
        private LevelManager levelManager;

        private StateManager stateManager;
        private MenuScreen menuScreen;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            camera = new Camera();
            stateManager = new StateManager();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            // Menu
            menuScreen = new MenuScreen(this);
            menuScreen.LoadContent(Content);

            // Background
            background1 = new Background(this);
            background1.LoadContent(Content);

            // Player
            player1 = new Player(this);
            player1.LoadContent(Content);

            // Fases
            levelManager = new LevelManager(this);
            levelManager.AddFase(new Fase01(this));
            // levelManager.AddFase(new Fase02(this)); // basta adicionar aqui
            levelManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (stateManager.CurrentState)
            {
                case GameState.Menu:
                    menuScreen.Update(stateManager);
                    break;

                case GameState.Playing:
                    player1.Update(gameTime, levelManager.CurrentFase);
                    levelManager.Update(player1);
                    camera.Follow(player1.GetPosition());
                    break;

                case GameState.Settings:
                    // lógica de settings futura
                    break;

                case GameState.Mapping:
                    // lógica de criação de mapas futura
                    break;

                case GameState.Exit:
                    Exit();
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(transformMatrix: (stateManager.CurrentState == GameState.Playing ? camera.Transform : null));

            switch (stateManager.CurrentState)
            {
                case GameState.Menu:
                    menuScreen.Draw(spriteBatch);
                    break;

                case GameState.Playing:
                    background1.Draw(spriteBatch);
                    levelManager.Draw(spriteBatch, player1);
                    break;

                case GameState.Settings:
                    spriteBatch.DrawString(Content.Load<SpriteFont>("Arial"), "Settings Screen", new Vector2(300, 200), Color.White);
                    break;

                case GameState.Mapping:
                    spriteBatch.DrawString(Content.Load<SpriteFont>("Arial"), "Map Editor", new Vector2(300, 200), Color.White);
                    break;
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}