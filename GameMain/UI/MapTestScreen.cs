using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Entities;
using GameDuMouse.GameMain.Fases;
using GameDuMouse.GameMain.Input;

namespace GameDuMouse.GameMain.UI
{
    public class MapTestScreen
    {
        private readonly Game game;
        private SpriteFont font;
        private BackButton backButton;
        private Player player;
        private LevelManager levelManager;
        private Camera camera;
        private string statusMessage;
        private InputManager inputManager;

        public Matrix CameraTransform => camera != null ? camera.Transform : Matrix.Identity;

        public MapTestScreen(Game game)
        {
            this.game = game;
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font, inputManager);

            player = new Player(game);
            player.LoadContent(content);

            camera = new Camera();
        }

        public void ResetInput()
        {
            backButton?.ResetInput();
        }

        public void StartTest(string mapName)
        {
            statusMessage = null;
            levelManager = new LevelManager(game);

            var data = MapDataManager.LoadByName(mapName);
            if (data == null)
            {
                statusMessage = "Mapa nao encontrado para teste.";
                return;
            }

            var fase = PhaseFactory.CreateDynamicFase(game, mapName);
            if (fase == null)
            {
                statusMessage = "Falha ao carregar o mapa.";
                return;
            }

            levelManager.AddFase(fase);
            levelManager.LoadContent(game.Content);

            player.ResetPlayer();
            player.SetPosition(fase.GetSpawnPosition(false));

            if (data.PhaseWidth > 0)
                camera.MaxPosition = new Vector2(data.PhaseWidth, 290);
        }

        public void Update(StateManager stateManager, GameTime gameTime)
        {
            backButton?.Update(stateManager);
            if (stateManager.CurrentState != GameState.MappingTest)
                return;

            if (levelManager == null || levelManager.CurrentFase == null)
                return;

            player.Update(gameTime, levelManager.CurrentFase);
            levelManager.Update(player);
            camera.Follow(player.GetPosition());
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                spriteBatch.DrawString(font, statusMessage, new Vector2(300, 200), Color.Red);
                return;
            }

            levelManager?.Draw(spriteBatch, player);
            backButton?.Draw(spriteBatch);
        }
    }
}
