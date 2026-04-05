using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Entities;
using GameDuMouse.GameMain.Rendering;

namespace GameDuMouse.GameMain.Fases
{
    public class ReturnStage
    {
        private Game game;
        private Texture2D debugTexture;

        // Novo cenario
        private Rectangle groundCollider, groundCollider2;
        private Rectangle platformA, platformB, platformC;
        private Obstacle knifeTrap, panTrap;
        private Cheese cheese;

        public ReturnStage(Game game)
        {
            this.game = game;
            Initialize();
        }

        public void Initialize()
        {
            // chao
            groundCollider = new Rectangle(0, 400, 2500, 5);
            groundCollider2 = new Rectangle(3000, 400, 2700, 5);

            // plataformas diferentes
            platformA = new Rectangle(1200, 300, 150, 5);
            platformB = new Rectangle(1800, 250, 200, 5);
            platformC = new Rectangle(2400, 200, 200, 5);

            // obstaculos novos
            knifeTrap = new Obstacle(game, 1500, 350, 80, 80, "triangle");
            panTrap = new Obstacle(game, 2000, 350, 100, 100, "circle");

            // queijo no final
            cheese = new Cheese(game, 2800, 350, 50, 50);

            var cache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            debugTexture = cache != null ? cache.Pixel : debugTexture;
        }

        public void Update(Player player)
        {
            // Se o player encostar no queijo, podemos sinalizar vitoria
            if (cheese.CollidesWith(player.Collider))
            {
                // Aqui voce pode colocar logica de "fim da fase"
                System.Console.WriteLine("O ratinho pegou o queijo!");
            }
        }

        public void Draw(SpriteBatch spriteBatch, RenderContext renderContext)
        {
            // chao
            if (debugTexture != null)
            {
                DrawRect(debugTexture, spriteBatch, renderContext, groundCollider, Color.Red * 0.4f);
                DrawRect(debugTexture, spriteBatch, renderContext, groundCollider2, Color.Red * 0.4f);
            }

            // plataformas
            if (debugTexture != null)
            {
                DrawRect(debugTexture, spriteBatch, renderContext, platformA, Color.Blue * 0.4f);
                DrawRect(debugTexture, spriteBatch, renderContext, platformB, Color.Blue * 0.4f);
                DrawRect(debugTexture, spriteBatch, renderContext, platformC, Color.Blue * 0.4f);
            }

            // obstaculos
            DrawObstacle(spriteBatch, renderContext, knifeTrap);
            DrawObstacle(spriteBatch, renderContext, panTrap);

            // queijo
            DrawCheese(spriteBatch, renderContext, cheese);
        }

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
    }
}
