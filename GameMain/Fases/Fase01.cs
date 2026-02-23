using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using GameDuMouse.GameMain.Entities;
using GameDuMouse.GameMain.UI;


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

        // Propriedades públicas para o Player acessar
        public List<Rectangle> GroundColliders { get; private set; }
        public List<Rectangle> Platforms { get; private set; }
        public List<Obstacle> Obstacles1 { get; private set; }
        public List<Obstacle> Obstacles2 { get; private set; }

        public Fase01(Game game)
        {
            this.game = game;
            Initialize();
        }

        private void Initialize()
        {
            groundCollider = new Rectangle(0, 400, 2600, 5);
            groundCollider2 = new Rectangle(3000, 400, 2700, 5);
            saltLid = new Rectangle(1155, 291, 95, 5);
            upStove = new Rectangle(1310, 173, 95, 5);
            platform1 = new Rectangle(3300, 291, 190, 5);
            platform2 = new Rectangle(3650, 240, 190, 5);

            hotPan = new Obstacle(game, 1330, 250, 200, 150, "square");
            venom1 = new Obstacle(game, 3425, 330, 80, 70, "triangle");
            venom2 = new Obstacle(game, 3510, 330, 80, 70, "triangle");
            venom3 = new Obstacle(game, 3600, 330, 80, 70, "triangle");
            venom4 = new Obstacle(game, 3930, 330, 80, 70, "triangle");
            venom5 = new Obstacle(game, 4565, 330, 80, 70, "triangle");
            venom6 = new Obstacle(game, 4715, 330, 80, 70, "triangle");

            cheese = new Cheese(game, 5500, 330, 80, 70);

            returnStage = new ReturnStage(game);

            // Toca do rato (aparece só no retorno)
            burrow = new RatsBurrow(game, 100, 330, 80, 70);
            victoryScreen = new VictoryScreen(game);

            debugTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            debugTexture.SetData(new[] { Color.White });

            // Inicializa listas para o Player
            GroundColliders = new List<Rectangle> { groundCollider, groundCollider2 };
            Platforms = new List<Rectangle> { saltLid, upStove, platform1, platform2 };
            Obstacles1 = new List<Obstacle> { hotPan, venom1, venom2, venom3, venom4, venom5, venom6 };
            Obstacles2 = new List<Obstacle> { /* knifeTrap, panTrap se expostos pelo ReturnStage */ };
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

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            if (!IsReturning)
            {
                spriteBatch.Draw(debugTexture, groundCollider, Color.Red * 0.4f);
                spriteBatch.Draw(debugTexture, groundCollider2, Color.Red * 0.4f);
                spriteBatch.Draw(debugTexture, saltLid, Color.Blue * 0.4f);
                spriteBatch.Draw(debugTexture, upStove, Color.Blue * 0.4f);
                spriteBatch.Draw(debugTexture, platform1, Color.Blue * 0.4f);
                spriteBatch.Draw(debugTexture, platform2, Color.Blue * 0.4f);

                hotPan.Draw(spriteBatch);
                venom1.Draw(spriteBatch);
                venom2.Draw(spriteBatch);
                venom3.Draw(spriteBatch);
                venom4.Draw(spriteBatch);
                venom5.Draw(spriteBatch);
                venom6.Draw(spriteBatch);

                cheese.Draw(spriteBatch);
                player.Draw(game.Services.GetService<GameTime>());
            }
            else if (!hasWon)
            {
                returnStage.Draw(spriteBatch);
                burrow.Draw(spriteBatch);
                player.Draw(game.Services.GetService<GameTime>());
            }
            else
            {
                // Tela de vitória
                victoryScreen.Draw(spriteBatch);
            }
        }
        
        public bool HasWon => hasWon;
    }
}