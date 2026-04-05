using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using GameDuMouse.GameMain.Entities;
using GameDuMouse.GameMain.UI;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Rendering;
using GameDuMouse.GameMain.Managers;

namespace GameDuMouse.GameMain.Fases
{

    public class Fase01 : IFase
    {
        private Game game;
        private Texture2D debugTexture;

        // Obstáculos e plataformas da primeira parte
        private Rectangle groundCollider, groundCollider2;
        private Rectangle saltLid, upStove, platform1, platform2;
        private Obstacle hotPan, venom1, venom2, venom3, venom4, venom5, venom6;
        private Cheese cheese;

        // Segunda parte da fase
        private ReturnStage returnStage;
        private RatsBurrow burrow;
        private VictoryScreen victoryScreen;

        public bool IsReturning { get; private set; } = false;
        public bool hasWon = false;

        // expose a couple of helpers so the game can restore state when loading
        public void SetReturning(bool returning)
        {
            IsReturning = returning;
        }

        public Vector2 GetSpawnPosition(bool returning)
        {
            // se estiver voltando, aparece exatamente onde estava o queijo
            if (returning)
                return new Vector2(cheese.Bounds.X, cheese.Bounds.Y);

            // posição inicial padrão (igual ao ResetPlayer)
            int screenHeight = game.GraphicsDevice != null ? game.GraphicsDevice.Viewport.Height : 0;
            return new Vector2(210, LayoutConfig.GetPlayerSpawnY(screenHeight));
        }

        // Propriedades públicas para o Player acessar
        public List<Rectangle> GroundColliders { get; private set; }
        public List<Rectangle> Platforms { get; private set; }
        public List<Obstacle> Obstacles1 { get; private set; }
        public List<Obstacle> Obstacles2 { get; private set; }
        public List<Rectangle> WallColliders { get; private set; }

        public Fase01(Game game)
        {
            this.game = game;
            Initialize();

            // nota: o jogo irá manter o índice da fase ao iniciar/recuperar saves
            // este trecho foi removido porque a propriedade possui setter privado.
        }

        private void Initialize()
        {
            int screenHeight = game.GraphicsDevice != null ? game.GraphicsDevice.Viewport.Height : 0;
            int groundY = LayoutConfig.GetGroundY(screenHeight);
            int lowObstacleY = groundY - 70;
            int platformY = groundY - 109;
            int upperPlatformY = groundY - 160;
            int stoveY = groundY - 227;
            int panY = groundY - 150;

            groundCollider = new Rectangle(0, groundY, 2600, LayoutConfig.GroundThickness);
            groundCollider2 = new Rectangle(3000, groundY, 2700, LayoutConfig.GroundThickness);
            saltLid = new Rectangle(1155, platformY, 95, LayoutConfig.GroundThickness);
            upStove = new Rectangle(1310, stoveY, 95, LayoutConfig.GroundThickness);
            platform1 = new Rectangle(3300, platformY, 190, LayoutConfig.GroundThickness);
            platform2 = new Rectangle(3650, upperPlatformY, 190, LayoutConfig.GroundThickness);

            hotPan = new Obstacle(game, 1330, panY, 200, 150, "square");
            venom1 = new Obstacle(game, 3425, lowObstacleY, 80, 70, "triangle");
            venom2 = new Obstacle(game, 3510, lowObstacleY, 80, 70, "triangle");
            venom3 = new Obstacle(game, 3600, lowObstacleY, 80, 70, "triangle");
            venom4 = new Obstacle(game, 3930, lowObstacleY, 80, 70, "triangle");
            venom5 = new Obstacle(game, 4565, lowObstacleY, 80, 70, "triangle");
            venom6 = new Obstacle(game, 4715, lowObstacleY, 80, 70, "triangle");

            cheese = new Cheese(game, 5500, lowObstacleY, 80, 70);

            returnStage = new ReturnStage(game);

            // Toca do rato (aparece só no retorno)
            burrow = new RatsBurrow(game, 100, lowObstacleY, 80, 70);
            victoryScreen = new VictoryScreen(game);

            var cache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            debugTexture = cache != null ? cache.Pixel : debugTexture;

            // Inicializa listas para o Player
            GroundColliders = new List<Rectangle> { groundCollider, groundCollider2 };
            Platforms = new List<Rectangle> { saltLid, upStove, platform1, platform2 };
            Obstacles1 = new List<Obstacle> { hotPan, venom1, venom2, venom3, venom4, venom5, venom6 };
            Obstacles2 = new List<Obstacle> { /* knifeTrap, panTrap se expostos pelo ReturnStage */ };
            WallColliders = new List<Rectangle>();
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            victoryScreen.LoadContent(content);
        }

        public void Update(Player player)
        {
            if (!IsReturning)
            {
                if (cheese.CollidesWith(player.Collider))
                {
                    System.Console.WriteLine("O ratinho pegou o queijo! Agora começa o retorno.");
                    IsReturning = true;

                    // gravar progresso automaticamente assim que o queijo for pego
                    
                    var flow = game.Services.GetService(typeof(IGameFlow)) as IGameFlow;
                    flow?.SaveProgress(true);
                }
            }
            else if (!hasWon)
            {
                returnStage.Update(player);

                // Verifica se o player encostou na toca
                if (burrow.CollidesWith(player.Collider))
                {
                    hasWon = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Player player, RenderContext renderContext)
        {
            if (!IsReturning)
            {
                if (debugTexture != null)
                {
                    DrawRect(debugTexture, spriteBatch, renderContext, groundCollider, Color.Red * 0.4f);
                    DrawRect(debugTexture, spriteBatch, renderContext, groundCollider2, Color.Red * 0.4f);
                    DrawRect(debugTexture, spriteBatch, renderContext, saltLid, Color.Blue * 0.4f);
                    DrawRect(debugTexture, spriteBatch, renderContext, upStove, Color.Blue * 0.4f);
                    DrawRect(debugTexture, spriteBatch, renderContext, platform1, Color.Blue * 0.4f);
                    DrawRect(debugTexture, spriteBatch, renderContext, platform2, Color.Blue * 0.4f);
                }

                DrawObstacle(spriteBatch, renderContext, hotPan);
                DrawObstacle(spriteBatch, renderContext, venom1);
                DrawObstacle(spriteBatch, renderContext, venom2);
                DrawObstacle(spriteBatch, renderContext, venom3);
                DrawObstacle(spriteBatch, renderContext, venom4);
                DrawObstacle(spriteBatch, renderContext, venom5);
                DrawObstacle(spriteBatch, renderContext, venom6);

                DrawCheese(spriteBatch, renderContext, cheese);
                player.Draw(game.Services.GetService<GameTime>());
            }
            else if (!hasWon)
            {
                returnStage.Draw(spriteBatch, renderContext);
                DrawBurrow(spriteBatch, renderContext, burrow);
                player.Draw(game.Services.GetService<GameTime>());
            }
            else
            {
                // Tela de vitória
                victoryScreen.Draw(spriteBatch);
            }
        }
        
        public bool HasWon => hasWon;

        private static void DrawRect(Texture2D texture, SpriteBatch spriteBatch, RenderContext renderContext, Rectangle rect, Color color)
        {
            if (renderContext.IsVisible(rect))
                spriteBatch.Draw(texture, rect, color);
        }

        private static void DrawObstacle(SpriteBatch spriteBatch, RenderContext renderContext, Obstacle obstacle)
        {
            if (obstacle != null && renderContext.IsVisible(obstacle.Bounds))
                obstacle.Draw(spriteBatch);
        }

        private static void DrawCheese(SpriteBatch spriteBatch, RenderContext renderContext, Cheese targetCheese)
        {
            if (targetCheese != null && renderContext.IsVisible(targetCheese.Bounds))
                targetCheese.Draw(spriteBatch);
        }

        private static void DrawBurrow(SpriteBatch spriteBatch, RenderContext renderContext, RatsBurrow targetBurrow)
        {
            if (targetBurrow != null && renderContext.IsVisible(targetBurrow.Bounds))
                targetBurrow.Draw(spriteBatch);
        }
    }
}
