using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Helper;

namespace GameDuMouse
{
    public class Player
    {
        private Animation idleAnime;
        private Game game;
        private Vector2 wallRight, wallLeft;
        private float personY;
        public Player(Game game)
        {
            this.game = game;
        }
        public void Initialize()
        {
            personY = 270;
            wallRight = new Vector2(200, personY);
            wallLeft = new Vector2(-130, personY);
        }

        public void LoadContent(ContentManager content)
        {
            idleAnime = new Animation(game, 170f);
            idleAnime.AddSprite("Idle/Idle01", "Idle/Idle02", "Idle/Idle03",
                                "Idle/Idle04", "Idle/Idle05", "Idle/Idle06");
            idleAnime.Position = new Vector2(100, personY);
        }

        public void Move(GameTime gameTime)
        {
            var KeyboardState = Keyboard.GetState();
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                idleAnime.Position += new Vector2(10, 0);
                idleAnime.Effects = SpriteEffects.None;
                if (idleAnime.Position.X >= wallRight.X)
                {
                    idleAnime.Position = wallRight;
                }

            }
            else if (KeyboardState.IsKeyDown(Keys.A))
            {
                idleAnime.Position += new Vector2(-10, 0);
                idleAnime.Effects = SpriteEffects.FlipHorizontally;
                if (idleAnime.Position.X <= wallLeft.X)
                {
                    idleAnime.Position = wallLeft;
                }

            }
        }

        public void Update(GameTime gameTime)
        {
            idleAnime.Update(gameTime);
        }
        public void Draw(GameTime gameTime)
        {
            idleAnime.Draw(gameTime);
        }

    }
}