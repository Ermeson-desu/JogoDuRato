using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Utils;
using GameDuMouse.GameMain.Core;

namespace GameDuMouse.GameMain.UI
{
    public class CreateMappingScreen
    {
        private Game game;
        private SpriteFont font;
        private BackButton backButton;

        private List<Texture2D> obstacleTextures = new List<Texture2D>();
        private List<PlacedObstacle> placed = new List<PlacedObstacle>();

        private int panelWidth => game.GraphicsDevice.Viewport.Width / 4;
        private int screenWidth;
        private int screenHeight;

        private float leftScroll = 0f;
        private int selectedPaletteIndex = -1;

        private MouseState previousMouse;
        private KeyboardState previousKeyboard;
        private DirectInputController directController;

        private PlacedObstacle dragging;
        private Point dragOffset;

        private class PlacedObstacle
        {
            public Texture2D Texture;
            public Rectangle Bounds;
        }

        public CreateMappingScreen(Game game)
        {
            this.game = game;
            directController = new DirectInputController();
            previousMouse = Mouse.GetState();
            previousKeyboard = Keyboard.GetState();
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font);

            screenWidth = game.GraphicsDevice.Viewport.Width;
            screenHeight = game.GraphicsDevice.Viewport.Height;

            // Try to load available obstacle textures; fall back to colored placeholders
            try
            {
                var t1 = content.Load<Texture2D>("Content/Windows/JOGO_DO_RATO");
                obstacleTextures.Add(t1);
            }
            catch
            {
                // create simple colored placeholders
                obstacleTextures.Add(CreateSolidTexture(Color.SandyBrown));
                obstacleTextures.Add(CreateSolidTexture(Color.DarkGray));
                obstacleTextures.Add(CreateSolidTexture(Color.Olive));
            }
        }

        private Texture2D CreateSolidTexture(Color c)
        {
            var tx = new Texture2D(game.GraphicsDevice, 32, 32);
            var data = new Color[32 * 32];
            for (int i = 0; i < data.Length; i++) data[i] = c;
            tx.SetData(data);
            return tx;
        }

        public void ResetInput()
        {
            previousMouse = Mouse.GetState();
            previousKeyboard = Keyboard.GetState();
            backButton?.ResetInput();
        }

        public void Update(StateManager stateManager)
        {
            // Back button (early): if it changes the state we should bail out
            backButton?.Update(stateManager);
            if (stateManager.CurrentState != GameState.Mapping)
                return;

            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            // Left palette scrolling with mouse wheel
            int wheelDelta = mouse.ScrollWheelValue - previousMouse.ScrollWheelValue;
            if (wheelDelta != 0)
            {
                leftScroll -= wheelDelta * 0.01f;
                leftScroll = MathHelper.Clamp(leftScroll, 0, Math.Max(0, (obstacleTextures.Count * 60) - screenHeight + 20));
            }

            // Palette selection
            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                if (mouse.X < panelWidth)
                {
                    int index = (int)((mouse.Y + leftScroll - 10) / 60);
                    if (index >= 0 && index < obstacleTextures.Count)
                        selectedPaletteIndex = index;
                }
                else
                {
                    // placing or starting drag
                    // check if clicking an existing placed obstacle
                    for (int i = placed.Count - 1; i >= 0; i--)
                    {
                        if (placed[i].Bounds.Contains(mouse.Position))
                        {
                            dragging = placed[i];
                            dragOffset = new Point(mouse.X - dragging.Bounds.X, mouse.Y - dragging.Bounds.Y);
                            break;
                        }
                    }

                    if (dragging == null && selectedPaletteIndex >= 0)
                    {
                        var tex = obstacleTextures[selectedPaletteIndex];
                        var rect = new Rectangle(mouse.X - tex.Width / 2, mouse.Y - tex.Height / 2, tex.Width, tex.Height);
                        placed.Add(new PlacedObstacle { Texture = tex, Bounds = rect });
                    }
                }
            }

            // dragging
            if (mouse.LeftButton == ButtonState.Pressed && dragging != null)
            {
                dragging.Bounds = new Rectangle(mouse.X - dragOffset.X, mouse.Y - dragOffset.Y, dragging.Bounds.Width, dragging.Bounds.Height);
            }

            // release drag
            if (mouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed)
            {
                dragging = null;
            }

            // delete selected object with Delete key
            if (IsKeyPressed(Keys.Delete, keyboard))
            {
                // remove last placed or any that contain mouse
                for (int i = placed.Count - 1; i >= 0; i--)
                {
                    if (placed[i].Bounds.Contains(mouse.Position))
                    {
                        placed.RemoveAt(i);
                        break;
                    }
                }
            }

            previousMouse = mouse;
            previousKeyboard = keyboard;
        }

        private bool IsKeyPressed(Keys key, KeyboardState current)
        {
            return current.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // left panel
            spriteBatch.DrawString(font, "Obstaculos", new Vector2(10, 10), Color.White);
            // background panel
            var panelRect = new Rectangle(0, 0, panelWidth, screenHeight);
            Texture2D panelBg = CreateSolidTexture(Color.DarkSlateGray * 0.7f);
            spriteBatch.Draw(panelBg, panelRect, Color.White);

            // palette items
            for (int i = 0; i < obstacleTextures.Count; i++)
            {
                int y = 10 + i * 60 - (int)leftScroll + 10;
                var thumb = obstacleTextures[i];
                spriteBatch.Draw(thumb, new Vector2(10, y), Color.White);
                Color c = (i == selectedPaletteIndex) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(font, "Item " + (i + 1), new Vector2(50, y + 8), c);
            }

            // right area (map background)
            var mapRect = new Rectangle(panelWidth, 0, screenWidth - panelWidth, screenHeight);
            // simple background
            Texture2D bg = CreateSolidTexture(Color.CornflowerBlue);
            spriteBatch.Draw(bg, mapRect, Color.White);

            // draw placed obstacles
            foreach (var p in placed)
            {
                spriteBatch.Draw(p.Texture, new Vector2(p.Bounds.X, p.Bounds.Y), Color.White);
                // draw collider as semi-transparent rectangle
                var col = CreateSolidTexture(Color.Red * 0.4f);
                spriteBatch.Draw(col, p.Bounds, Color.White);
            }

            // draw map boundary colliders (walls + floor)
            var leftWall = new Rectangle(mapRect.X, mapRect.Y, 10, mapRect.Height);
            var rightWall = new Rectangle(mapRect.X + mapRect.Width - 10, mapRect.Y, 10, mapRect.Height);
            var floor = new Rectangle(mapRect.X, mapRect.Y + mapRect.Height - 40, mapRect.Width, 40);
            var colliderTex = CreateSolidTexture(Color.Red * 0.6f);
            spriteBatch.Draw(colliderTex, leftWall, Color.White);
            spriteBatch.Draw(colliderTex, rightWall, Color.White);
            spriteBatch.Draw(colliderTex, floor, Color.White);

            backButton?.Draw(spriteBatch);
        }
    }
}
