using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameDuMouse
{
    public class Background
    {
        private Game game;
        private Texture2D background,background2;
        private Vector2 backgroundPosition, backgroundPosition2;

        public Background(Game game)
        {
            this.game = game;
            Initialize();
        }
        public void Initialize()
        {
            backgroundPosition = new Vector2(-150, -100);
            backgroundPosition2 = new Vector2(3250, -100);

        }
        public void LoadContent(ContentManager content)
        {
            background = content.Load<Texture2D>("scenario/background_image(01)");
            background2 = content.Load<Texture2D>("scenario/background_image(02)");
        }
        public void ScrollLeft()
        {

            //Movimento da primeira imagem do background
            backgroundPosition += new Vector2(-10, 0);
            //Lógica de repetição de imagem
            if (backgroundPosition.X <= -890)
            {
                backgroundPosition = new Vector2(890, 0);
            }

            //movimento da segunda imagem background 
            backgroundPosition2 += new Vector2(-10, 0);
            //lógica de repetição de imagem
            if (backgroundPosition2.X <= -890)
            {
                backgroundPosition2 = new Vector2(890, 0);
            }

        }
        public void ScrollRight()
        {
            //Movimento da primeira imagem do background
            backgroundPosition += new Vector2(10, 0);
            //Lógica de repetição de imagem
            if (backgroundPosition.X >= 890)
            {
                backgroundPosition = new Vector2(-890, 0);
            }

            //movimento da segunda imagem background 
            backgroundPosition2 += new Vector2(10, 0);
            //lógica de repetição de imagem
            if (backgroundPosition2.X >= 890)
            {
                backgroundPosition2 = new Vector2(-890, 0);
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, backgroundPosition, null ,Color.White, 0f, Vector2.Zero, 1f,SpriteEffects.None, 0f );
            spriteBatch.Draw(background2, backgroundPosition2, null ,Color.White, 0f, Vector2.Zero, 1f,SpriteEffects.None, 0f );
        }

    }
}