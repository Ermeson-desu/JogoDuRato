using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Helper;

namespace GameDuMouse
{
    public class Player
    {
        private Animation idleAnime, runAnime, player, trans_run, trans_idle;
        private Game game;
        private Vector2 wallRight, wallLeft;
        private float personY;
        private bool positionInit = false;
        public Player(Game game)
        {
            this.game = game;
            Initialize();
        }
        public void Initialize()
        {
            personY = 270;
            wallRight = new Vector2(200, personY);
            wallLeft = new Vector2(0, personY);
        }

        public void LoadContent(ContentManager content)
        {
            idleAnime = new Animation(game, 170f);
            idleAnime.AddSprite("Idle/Idle01", "Idle/Idle02", "Idle/Idle03",
                                "Idle/Idle04", "Idle/Idle05", "Idle/Idle06");
            idleAnime.Position = new Vector2(100, personY);

            runAnime = new Animation(game, 50f);
            runAnime.AddSprite("run/run01", "run/run02", "run/run03",
                                "run/run04", "run/run05", "run/run06", "run/run07");
            runAnime.Position = new Vector2(100, personY);

            trans_idle = new Animation(game, 50f);
            trans_idle.AddSprite("Trans/trans05", "Trans/trans04","Trans/trans03",
                                 "Trans/trans02","Trans/trans01");
            trans_idle.Position = new Vector2(100, personY);

            trans_run = new Animation(game, 50f);
            trans_run.AddSprite("Trans/trans01", "Trans/trans02","Trans/trans03",
                                 "Trans/trans04","Trans/trans05","Trans/trans06");
            trans_run.Position = new Vector2(100, personY);
            
            player = idleAnime;
        }

        public void Move(GameTime gameTime, Background background)
        {

            var KeyboardState = Keyboard.GetState();
            idleAnime.Position = player.Position;
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                MoveRight(background);
                positionInit = false;
            }
            else if (KeyboardState.IsKeyDown(Keys.A))
            {
                MoveLeft(background);
                positionInit = true;
            }
            else if (positionInit == false)
            {
                player = idleAnime;
                player.Effects = SpriteEffects.None;
            }
            else if (positionInit == true)
            {
                player = idleAnime;
                player.Effects = SpriteEffects.FlipHorizontally;
            }
        }
        private void MoveRight(Background background)
        {
            player = runAnime;
            player.Position += new Vector2(10, 0);
            player.Effects = SpriteEffects.None;
            if (player.Position.X >= wallRight.X)
            {
                player.Position = wallRight;
                background.ScrollLeft();
            }
            Console.WriteLine($"Posição do player: {player.Position}");
        }
        private void MoveLeft(Background background)
        {
            player = runAnime;
            player.Position += new Vector2(-10, 0);
            player.Effects = SpriteEffects.FlipHorizontally;
            if (player.Position.X <= wallLeft.X)
            {
                player.Position = wallLeft;
                background.ScrollRight();
            }
            Console.WriteLine($"Posição do player: {player.Position}");
        }

        public void Update(GameTime gameTime)
        {
            player.Update(gameTime);
        }
        public void Draw(GameTime gameTime)
        {
            player.Draw(gameTime);
        }

    }
}