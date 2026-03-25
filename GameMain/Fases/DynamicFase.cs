using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Entities;

namespace GameDuMouse.GameMain.Fases
{
    public class DynamicFase : IFase
    {
        private readonly Game game;
        private readonly MapData data;
        private Texture2D pixel;
        private List<Texture2D> backgroundTextures = new List<Texture2D>();
        private Cheese cheese;
        private Rectangle burrowBounds;

        public bool IsReturning { get; private set; } = false;
        public bool HasWon { get; private set; } = false;

        public List<Rectangle> GroundColliders { get; private set; } = new List<Rectangle>();
        public List<Rectangle> Platforms { get; private set; } = new List<Rectangle>();
        public List<Rectangle> WallColliders { get; private set; } = new List<Rectangle>();
        public List<Obstacle> Obstacles1 { get; private set; } = new List<Obstacle>();
        public List<Obstacle> Obstacles2 { get; private set; } = new List<Obstacle>();

        public DynamicFase(Game game, MapData data)
        {
            this.game = game;
            this.data = data;
            BuildFromData();
        }

        private void BuildFromData()
        {
            GroundColliders.Clear();
            Platforms.Clear();
            WallColliders.Clear();
            Obstacles1.Clear();
            Obstacles2.Clear();

            foreach (var collider in data.Colliders)
            {
                var r = new Rectangle(collider.Bounds.X, collider.Bounds.Y, collider.Bounds.Width, collider.Bounds.Height);
                if (collider.Type == "Ground")
                    GroundColliders.Add(r);
                else if (collider.Type == "Platform")
                    Platforms.Add(r);
                else if (collider.Type == "Wall")
                    WallColliders.Add(r);
            }

            foreach (var obstacle in data.Obstacles)
            {
                var b = obstacle.Bounds;
                Obstacles1.Add(new Obstacle(game, b.X, b.Y, b.Width, b.Height, "Square"));
            }

            if (data.ObstaclesReturn != null)
            {
                foreach (var obstacle in data.ObstaclesReturn)
                {
                    var b = obstacle.Bounds;
                    Obstacles2.Add(new Obstacle(game, b.X, b.Y, b.Width, b.Height, "Square"));
                }
            }

            if (data.CheeseBounds != null)
            {
                var c = data.CheeseBounds;
                cheese = new Cheese(game, c.X, c.Y, c.Width, c.Height);
            }

            if (data.BurrowBounds != null)
            {
                var b = data.BurrowBounds;
                burrowBounds = new Rectangle(b.X, b.Y, b.Width, b.Height);
            }
            else
            {
                burrowBounds = new Rectangle(100, 330, 80, 70);
            }
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            pixel = new Texture2D(game.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            backgroundTextures.Clear();
            if (data.IsCustomBackgroundLoaded && data.BackgroundLayers.Count > 0)
            {
                foreach (var layer in data.BackgroundLayers)
                {
                    Texture2D tex = null;
                    if (!string.IsNullOrWhiteSpace(layer.ImagePath) && File.Exists(layer.ImagePath))
                        tex = Texture2D.FromFile(game.GraphicsDevice, layer.ImagePath);
                    backgroundTextures.Add(tex);
                }
            }
        }

        public void Update(Player player)
        {
            if (!IsReturning && cheese != null && cheese.CollidesWith(player.Collider))
            {
                IsReturning = true;
            }
            else if (IsReturning && !HasWon)
            {
                if (burrowBounds.Intersects(player.Collider))
                    HasWon = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            DrawBackground(spriteBatch);

            if (!IsReturning)
            {
                foreach (var obstacle in Obstacles1)
                    obstacle.Draw(spriteBatch);
                cheese?.Draw(spriteBatch);
            }
            else
            {
                foreach (var obstacle in Obstacles2)
                    obstacle.Draw(spriteBatch);
                spriteBatch.Draw(pixel, burrowBounds, Color.SandyBrown * 0.6f);
            }

            player.Draw(game.Services.GetService<GameTime>());
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            if (data.IsCustomBackgroundLoaded && data.BackgroundLayers.Count > 0)
            {
                for (int i = 0; i < data.BackgroundLayers.Count; i++)
                {
                    var layer = data.BackgroundLayers[i];
                    var tex = i < backgroundTextures.Count ? backgroundTextures[i] : null;
                    if (tex != null)
                        spriteBatch.Draw(tex, new Vector2(layer.StartX, 0), Color.White);
                }
            }
            else
            {
                int width = data.PhaseWidth > 0 ? data.PhaseWidth : 600;
                int height = data.ScreenHeight > 0 ? data.ScreenHeight : 480;
                spriteBatch.Draw(pixel, new Rectangle(0, 0, width, height), Color.CornflowerBlue);
            }
        }

        public void SetReturning(bool returning)
        {
            IsReturning = returning;
        }

        public Vector2 GetSpawnPosition(bool returning)
        {
            if (data.Objects != null)
            {
                foreach (var obj in data.Objects)
                {
                    if (obj != null && obj.Name == "Spawn")
                        return new Vector2(obj.Position.X, obj.Position.Y);
                }
            }

            return new Vector2(210, 300);
        }
    }
}
