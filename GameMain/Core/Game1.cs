using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Entities;
using GameDuMouse.GameMain.Fases;
using GameDuMouse.GameMain.UI;
using GameDuMouse.GameMain.Input;

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
        private bool useDynamicBackground;
        private bool startedFromChapterSelection;
        private int currentCustomMapIndex = -1;

        private StateManager stateManager;
        private GameState previousGameState; // usado para detectar mudança de estado
        private MenuScreen menuScreen;
        private VictoryScreen victoryScreen;
        private LoadScreen loadScreen;
        private ChapterSelectScreen chapterSelectScreen;
        private NewMapMenuScreen newMapMenuScreen;
        private CreateMappingScreen mappingScreen;
        private MapTestScreen mapTestScreen;
        private InputManager inputManager;

        // player/name state used for saving mid–game
        public string CurrentPlayerName { get; private set; }
        public int CurrentFaseIndex { get; private set; } = -1;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // create mapping screen early so it's never null; content will be
            // loaded later in LoadContent.
            mappingScreen = new CreateMappingScreen(this);
        }

        protected override void Initialize()
        {
            camera = new Camera();
            stateManager = new StateManager();
            previousGameState = stateManager.CurrentState;
            inputManager = new InputManager();
            Services.AddService(typeof(InputManager), inputManager);
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
            // Chapter Select Screen
            chapterSelectScreen = new ChapterSelectScreen(this);
            chapterSelectScreen.LoadContent(Content);
            // New Map Menu Screen
            newMapMenuScreen = new NewMapMenuScreen(this);
            newMapMenuScreen.LoadContent(Content);
            // Mapping / Map Editor
            mappingScreen = new CreateMappingScreen(this);
            mappingScreen.LoadContent(Content);
            // Map Test Screen
            mapTestScreen = new MapTestScreen(this);
            mapTestScreen.LoadContent(Content);

        }

        protected override void Update(GameTime gameTime)
        {
            if (inputManager != null && inputManager.Keyboard.IsKeyDown(Keys.Escape))
                Exit();

            // Detecta transição entre estados e dispara callbacks auxiliares
            if (stateManager.CurrentState != previousGameState)
            {
                if (stateManager.CurrentState == GameState.Load)
                    loadScreen.ResetInput();
                if (stateManager.CurrentState == GameState.Menu)
                    menuScreen.ResetInput();
                if (stateManager.CurrentState == GameState.Mapping)
                    mappingScreen?.ResetInput();
                if (stateManager.CurrentState == GameState.MappingTest)
                {
                    mapTestScreen?.ResetInput();
                    mapTestScreen?.StartTest(GameDuMouse.GameMain.Core.MapListManager.CurrentMapName);
                }
                if (stateManager.CurrentState == GameState.NewMapMenu)
                    newMapMenuScreen?.ResetInput();
                if (stateManager.CurrentState == GameState.ChapterSelect)
                    chapterSelectScreen?.ResetInput();
                if (stateManager.CurrentState == GameState.PreGame)
                    preGameScreen.ResetInput();

                previousGameState = stateManager.CurrentState;
            }

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
                case GameState.ChapterSelect:
                    chapterSelectScreen?.Update(stateManager);
                    break;

                case GameState.Mapping:
                    if (mappingScreen != null)
                        mappingScreen.Update(stateManager);
                    break;

                case GameState.MappingTest:
                    mapTestScreen?.Update(stateManager, gameTime);
                    break;

                case GameState.NewMapMenu:
                    newMapMenuScreen?.Update(stateManager);
                    break;

                case GameState.Settings:
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
                CurrentPlayerName = playerName.Trim();
                CurrentFaseIndex = 0;

                levelManager = new LevelManager(this);

                var exportedMap = GameDuMouse.GameMain.Core.MapListManager.ExportedMapName;
                var dynamicFase = !string.IsNullOrWhiteSpace(exportedMap)
                    ? PhaseFactory.CreateDynamicFase(this, exportedMap)
                    : null;

                if (dynamicFase != null)
                {
                    levelManager.AddFase(dynamicFase);
                    useDynamicBackground = true;
                    currentCustomMapIndex = ResolveCustomMapIndex(exportedMap);
                }
                else
                {
                    levelManager.AddFase(PhaseFactory.CreateFase(this, 0));
                    useDynamicBackground = false;
                    currentCustomMapIndex = -1;
                }

                levelManager.LoadContent(Content);

                // once we're actually in play, clear any history so that "back"
                // doesn't accidentally drop the player back into PreGame/Load etc.
                stateManager.ResetHistory();
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
            // mantém jogador e fase atuais para futuros saves
            CurrentPlayerName = save.PlayerName?.Trim();
            CurrentFaseIndex = save.CurrentFaseIndex;
            startedFromChapterSelection = false;
            currentCustomMapIndex = -1;

            // recria a fase que estava em andamento
            levelManager = new LevelManager(this);
            var fase = PhaseFactory.CreateFase(this, save.CurrentFaseIndex) ?? PhaseFactory.CreateFase(this, 0);
            levelManager.AddFase(fase);
            levelManager.LoadContent(Content);
            useDynamicBackground = false;

            // aplica situação de retorno e posiciona o player
            if (levelManager.CurrentFase != null)
            {
                levelManager.CurrentFase.SetReturning(save.IsReturning);
                player1.ResetPlayer();
                Vector2 spawn = levelManager.CurrentFase.GetSpawnPosition(save.IsReturning);
                player1.SetPosition(spawn);
                // se for retorno, o sprite deve olhar para esquerda (oposto do início)
                player1.SetFacing(!save.IsReturning);
            }

            stateManager.ResetHistory();
            stateManager.ChangeState(GameState.Playing);
        }

        /// <summary>
        /// Cria/atualiza o save atual usando o nome previamente configurado através de
        /// StartNewGame/LoadSave. A flag <paramref name="isReturning"/> é mantida
        /// para que, ao recarregar, o ratinho saiba se deve aparecer na posição do
        /// queijo ou na inicial.
        /// </summary>
        public void SaveProgress(bool isReturning)
        {
            if (startedFromChapterSelection)
                return;

            if (string.IsNullOrWhiteSpace(CurrentPlayerName) || CurrentFaseIndex < 0)
                return;

            var save = new SaveData
            {
                PlayerName = CurrentPlayerName,
                CurrentFaseIndex = CurrentFaseIndex,
                IsReturning = isReturning
            };

            SaveManager.SaveGame(save);
        }

        public void PrepareMapEditing(string mapName)
        {
            mappingScreen?.LoadMapForEditing(mapName);
        }

        public void SetChapterSelection(bool value)
        {
            startedFromChapterSelection = value;
        }

        public bool TryAdvanceToNextFase()
        {
            if (startedFromChapterSelection || levelManager == null)
                return false;

            bool advanced = levelManager.TryAdvanceToNextFase();
            if (advanced)
            {
                CurrentFaseIndex++;
                useDynamicBackground = false;
                currentCustomMapIndex = -1;
            }

            if (advanced)
                return true;

            // No more native fases: try exported/custom maps
            string nextCustom = GetNextCustomMapName();
            if (string.IsNullOrWhiteSpace(nextCustom))
                return false;

            var customFase = PhaseFactory.CreateDynamicFase(this, nextCustom);
            if (customFase == null)
                return false;

            levelManager.SwitchToNextFase(customFase);
            useDynamicBackground = true;
            return true;
        }

        private string GetNextCustomMapName()
        {
            var maps = GameDuMouse.GameMain.Core.MapListManager.LoadAllMaps();
            if (maps == null || maps.Count == 0)
                return null;

            int nextIndex = currentCustomMapIndex + 1;
            if (nextIndex < 0 || nextIndex >= maps.Count)
                return null;

            currentCustomMapIndex = nextIndex;
            return maps[nextIndex];
        }

        private int ResolveCustomMapIndex(string mapName)
        {
            if (string.IsNullOrWhiteSpace(mapName))
                return -1;

            var maps = GameDuMouse.GameMain.Core.MapListManager.LoadAllMaps();
            return maps.IndexOf(mapName.Trim());
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Matrix? transform = null;
            if (stateManager.CurrentState == GameState.Playing)
                transform = camera.Transform;
            else if (stateManager.CurrentState == GameState.MappingTest)
                transform = mapTestScreen?.CameraTransform;

            spriteBatch.Begin(transformMatrix: transform);

            switch (stateManager.CurrentState)
            {
                case GameState.Menu:
                    menuScreen.Draw(spriteBatch);
                    break;

                case GameState.Playing:
                    if (!useDynamicBackground)
                        background1.Draw(spriteBatch);
                    levelManager.Draw(spriteBatch, player1);
                    break;

                case GameState.Settings:
                    spriteBatch.DrawString(Content.Load<SpriteFont>("Font/Arial"), "Settings Screen", new Vector2(300, 200), Color.White);
                    break;

                case GameState.Mapping:
                    if (mappingScreen != null)
                        mappingScreen.Draw(spriteBatch);
                    break;

                case GameState.MappingTest:
                    mapTestScreen?.Draw(spriteBatch);
                    break;

                case GameState.NewMapMenu:
                    newMapMenuScreen?.Draw(spriteBatch);
                    break;
                
                case GameState.PreGame:
                    preGameScreen.Draw(spriteBatch);
                    break;

                case GameState.Load:
                    loadScreen.Draw(spriteBatch);
                    break;
                case GameState.ChapterSelect:
                    chapterSelectScreen?.Draw(spriteBatch);
                    break;


                case GameState.Victory:
                    victoryScreen.Draw(spriteBatch);
                    break;
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                inputManager?.Dispose();
            base.Dispose(disposing);
        }
    }
}
