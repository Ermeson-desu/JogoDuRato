using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameDuMouse
{
    public class Background
    {
        private Game game;
        private float initRender1 = -300, initRender2 = 3100, yPosition = -100;
        private Texture2D background, background2;
        private Vector2 backgroundPosition, backgroundPosition2;

        public Background(Game game)
        {
            this.game = game;
            Initialize();
        }
        public void Initialize()
        {
            backgroundPosition = new Vector2(initRender1, yPosition);
            backgroundPosition2 = new Vector2(initRender2, yPosition);

        }
        public void LoadContent(ContentManager content)
        {
            background = content.Load<Texture2D>("scenario/background_image(01)");
            background2 = content.Load<Texture2D>("scenario/background_image(02)");
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, backgroundPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(background2, backgroundPosition2, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

    }
}