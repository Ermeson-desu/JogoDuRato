using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDuMouse
{
    public class ReturnStage
    {
        private Game game;
        private Texture2D debugTexture;

        // Novo cenário
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
            // chão
            groundCollider = new Rectangle(0, 400, 2500, 5);
            groundCollider2 = new Rectangle(3000, 400, 2700, 5);

            // plataformas diferentes
            platformA = new Rectangle(1200, 300, 150, 5);
            platformB = new Rectangle(1800, 250, 200, 5);
            platformC = new Rectangle(2400, 200, 200, 5);

            // obstáculos novos
            knifeTrap = new Obstacle(game, 1500, 350, 80, 80, "triangle");
            panTrap = new Obstacle(game, 2000, 350, 100, 100, "circle");

            // queijo no final
            cheese = new Cheese(game, 2800, 350, 50, 50);

            debugTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            debugTexture.SetData(new[] { Color.White });
        }

        public void Update(Player player)
        {
            // Se o player encostar no queijo, podemos sinalizar vitória
            if (cheese.CollidesWith(player.Collider))
            {
                // Aqui você pode colocar lógica de "fim da fase"
                System.Console.WriteLine("O ratinho pegou o queijo!");
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // chão
            spriteBatch.Draw(debugTexture, groundCollider, Color.Red * 0.4f);
            spriteBatch.Draw(debugTexture, groundCollider2, Color.Red * 0.4f);

            // plataformas
            spriteBatch.Draw(debugTexture, platformA, Color.Blue * 0.4f);
            spriteBatch.Draw(debugTexture, platformB, Color.Blue * 0.4f);
            spriteBatch.Draw(debugTexture, platformC, Color.Blue * 0.4f);

            // obstáculos
            knifeTrap.Draw(spriteBatch);
            panTrap.Draw(spriteBatch);

            // queijo
            cheese.Draw(spriteBatch);
        }
    }
}