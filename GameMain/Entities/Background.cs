using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameDuMouse.GameMain.Entities
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

        public float GetTotalWidth()
        {
            if (background2 != null)
                return (initRender2 + background2.Width) - initRender1;
            if (background != null)
                return (initRender1 + background.Width) - initRender1;
            return 0;
        }

        public float GetBackgroundStartX()
        {
            return initRender1;
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, backgroundPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(background2, backgroundPosition2, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        public void Draw(SpriteBatch spriteBatch, float cameraOffsetX)
        {
            Vector2 adjustedPos1 = new Vector2(initRender1 - cameraOffsetX, yPosition);
            Vector2 adjustedPos2 = new Vector2(initRender2 - cameraOffsetX, yPosition);
            spriteBatch.Draw(background, adjustedPos1, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(background2, adjustedPos2, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

    }
}