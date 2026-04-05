using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Entities;
using GameDuMouse.GameMain.Rendering;
using GameDuMouse.GameMain.Services;

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
        private AssetManager assetManager;

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
            assetManager = game.Services.GetService(typeof(AssetManager)) as AssetManager;
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
                switch (collider.Type)
                {
                    case ColliderType.Ground:
                        GroundColliders.Add(r);
                        break;
                    case ColliderType.Platform:
                        Platforms.Add(r);
                        break;
                    case ColliderType.Wall:
                        WallColliders.Add(r);
                        break;
                }
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
            var cache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            pixel = cache != null ? cache.Pixel : pixel;

            backgroundTextures.Clear();
            if (data.IsCustomBackgroundLoaded && data.BackgroundLayers.Count > 0)
            {
                foreach (var layer in data.BackgroundLayers)
                {
                    Texture2D tex = null;
                    if (!string.IsNullOrWhiteSpace(layer.ImagePath))
                        tex = assetManager != null ? assetManager.LoadTextureFromFile(layer.ImagePath) : null;
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

        public void Draw(SpriteBatch spriteBatch, Player player, RenderContext renderContext)
        {
            DrawBackground(spriteBatch, renderContext);

            if (!IsReturning)
            {
                foreach (var obstacle in Obstacles1)
                    DrawObstacle(spriteBatch, renderContext, obstacle);
                DrawCheese(spriteBatch, renderContext, cheese);
            }
            else
            {
                foreach (var obstacle in Obstacles2)
                    DrawObstacle(spriteBatch, renderContext, obstacle);
                DrawBurrow(spriteBatch, renderContext, burrowBounds);
            }

            player.Draw(game.Services.GetService<GameTime>());
        }

        private void DrawBackground(SpriteBatch spriteBatch, RenderContext renderContext)
        {
            if (data.IsCustomBackgroundLoaded && data.BackgroundLayers.Count > 0)
            {
                for (int i = 0; i < data.BackgroundLayers.Count; i++)
                {
                    var layer = data.BackgroundLayers[i];
                    var tex = i < backgroundTextures.Count ? backgroundTextures[i] : null;
                    if (tex != null)
                    {
                        var bounds = new Rectangle((int)layer.StartX, 0, tex.Width, tex.Height);
                        if (renderContext.IsVisible(bounds))
                            spriteBatch.Draw(tex, new Vector2(layer.StartX, 0), Color.White);
                    }
                }
            }
            else
            {
                int width = data.PhaseWidth > 0 ? data.PhaseWidth : 600;
                int height = data.ScreenHeight > 0 ? data.ScreenHeight : 480;
                var bounds = new Rectangle(0, 0, width, height);
                if (renderContext.IsVisible(bounds))
                    spriteBatch.Draw(pixel, bounds, Color.CornflowerBlue);
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
                    if (obj != null && obj.Name == ObjectType.Spawn)
                        return new Vector2(obj.Position.X, obj.Position.Y);
                }
            }

            return new Vector2(210, 300);
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

        private void DrawBurrow(SpriteBatch spriteBatch, RenderContext renderContext, Rectangle burrow)
        {
            if (pixel != null && renderContext.IsVisible(burrow))
                spriteBatch.Draw(pixel, burrow, Color.SandyBrown * 0.6f);
        }
    }
}
