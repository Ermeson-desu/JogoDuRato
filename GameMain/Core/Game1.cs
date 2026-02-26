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
        private PreGameScreen preGameScreen;

        private Camera camera;
        private Player player1;
        private Background background1;
        private LevelManager levelManager;

        private StateManager stateManager;
        private MenuScreen menuScreen;
        private VictoryScreen victoryScreen;
        private LoadScreen loadScreen;

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

            // Pre-Game
            preGameScreen = new PreGameScreen(this);
            preGameScreen.LoadContent(Content);

            // Victory Screen
            victoryScreen = new VictoryScreen(this);
            victoryScreen.LoadContent(Content);

            // Load Screen
            loadScreen = new LoadScreen(this);
            loadScreen.LoadContent(Content);

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

                    // Se a fase terminou, muda para Victory
                    if (levelManager.CurrentFase.HasWon)
                        stateManager.ChangeState(GameState.Victory);
                    break;

                case GameState.Victory:
                    victoryScreen.Update(stateManager, levelManager);
                    break;
                
                case GameState.Load:
                    loadScreen.Update(stateManager);
                    break;

                case GameState.Settings:
                    // lógica futura
                    break;

                case GameState.Mapping:
                    // lógica futura
                    break;

                case GameState.PreGame: 
                preGameScreen.Update(stateManager);
                break;

                case GameState.Exit:
                    Exit();
                    break;
            }

            base.Update(gameTime);
        }

        public void StartNewGame(string playerName)
        {
            if (!string.IsNullOrEmpty(playerName) && playerName.Length > 3)
            {
                levelManager = new LevelManager(this);
                levelManager.AddFase(PhaseFactory.CreateFase(this, 0));
                levelManager.LoadContent(Content);

                stateManager.ChangeState(GameState.Playing);
            }
            else
            {
                // Se o nome for inválido, permanece na tela PreGame
                stateManager.ChangeState(GameState.PreGame);
            }

        }

        public void LoadSave(SaveData save)
        {
            levelManager = new LevelManager(this);
            levelManager.AddFase(PhaseFactory.CreateFase(this, save.CurrentFaseIndex));
            levelManager.LoadContent(Content);

            // Aqui você pode restaurar o estado "IsReturning" dentro da fase
            // Exemplo: levelManager.CurrentFase.SetReturning(save.IsReturning);

            stateManager.ChangeState(GameState.Playing);
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
                    spriteBatch.DrawString(Content.Load<SpriteFont>("Font/Arial"), "Settings Screen", new Vector2(300, 200), Color.White);
                    break;

                case GameState.Mapping:
                    spriteBatch.DrawString(Content.Load<SpriteFont>("Font/Arial"), "Map Editor", new Vector2(300, 200), Color.White);
                    break;
                
                case GameState.PreGame:
                    preGameScreen.Draw(spriteBatch);
                    break;

                case GameState.Load:
                    loadScreen.Draw(spriteBatch);
                    break;


                case GameState.Victory:
                    victoryScreen.Draw(spriteBatch);
                    break;
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}