using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameDuMouse
{
    public class Background
    {
        private Game game;
        private Texture2D background;
        private Vector2 backgroundPosition, backgroundPosition2;

        public Background(Game game)
        {
            this.game = game;
            Initialize();
        }
        public void Initialize()
        {
            backgroundPosition = new Vector2(0, 0);
            backgroundPosition2 = new Vector2(900, 0);

        }
        public void LoadContent(ContentManager content)
        {
            background = content.Load<Texture2D>("scenario/cenario_game");
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
            Console.WriteLine($"Posição do Background: 1- {backgroundPosition} 2- {backgroundPosition2}");
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
            Console.WriteLine($"Posição do Background: 1- {backgroundPosition} 2- {backgroundPosition2}");
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, backgroundPosition, null ,Color.White, 0f, Vector2.Zero, 3f,SpriteEffects.None, 0f );
            spriteBatch.Draw(background, backgroundPosition2, null ,Color.White, 0f, Vector2.Zero, 3f,SpriteEffects.None, 0f );
        }

    }
}