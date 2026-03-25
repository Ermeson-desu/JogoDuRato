using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;

namespace GameDuMouse.GameMain.UI
{
    public class ChapterSelectScreen
    {
        private readonly Game game;
        private SpriteFont font;
        private Texture2D pixel;
        private BackButton backButton;

        private readonly List<ChapterEntry> chapters = new List<ChapterEntry>();
        private int selectedIndex;

        private KeyboardState previousKeyboard;
        private MouseState previousMouse;
        private bool ignoreNextInput;

        private const int ItemHeight = 60;
        private const int ThumbSize = 30;
        private readonly Vector2 listPosition = new Vector2(300, 180);

        private class ChapterEntry
        {
            public string DisplayName;
            public string MapName;
            public bool IsCustom;
            public Texture2D Thumbnail;
        }

        public ChapterSelectScreen(Game game)
        {
            this.game = game;
            previousKeyboard = Keyboard.GetState();
            previousMouse = Mouse.GetState();
            ignoreNextInput = true;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font);

            pixel = new Texture2D(game.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            ReloadChapters();
        }

        public void ResetInput()
        {
            previousKeyboard = Keyboard.GetState();
            previousMouse = Mouse.GetState();
            ignoreNextInput = true;
            backButton?.ResetInput();
            ReloadChapters();
        }

        public void Update(StateManager stateManager)
        {
            backButton?.Update(stateManager);
            if (stateManager.CurrentState != GameState.ChapterSelect)
                return;

            if (ignoreNextInput)
            {
                var k = Keyboard.GetState();
                var m = Mouse.GetState();
                previousKeyboard = k;
                previousMouse = m;
                if (m.LeftButton == ButtonState.Released && !k.IsKeyDown(Keys.Enter))
                    ignoreNextInput = false;
                return;
            }

            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();

            if (IsKeyPressed(Keys.Down, keyboard))
                selectedIndex = (selectedIndex + 1) % chapters.Count;

            if (IsKeyPressed(Keys.Up, keyboard))
                selectedIndex = (selectedIndex - 1 + chapters.Count) % chapters.Count;

            if (IsKeyPressed(Keys.Enter, keyboard))
                ActivateSelected(stateManager);

            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < chapters.Count; i++)
                {
                    var rowRect = GetRowRect(i);
                    if (rowRect.Contains(mouse.Position))
                    {
                        selectedIndex = i;
                        ActivateSelected(stateManager);
                        break;
                    }
                }
            }

            previousKeyboard = keyboard;
            previousMouse = mouse;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, "Select a chapter:", new Vector2(300, 120), Color.White);
            backButton?.Draw(spriteBatch);

            if (chapters.Count == 0)
            {
                spriteBatch.DrawString(font, "No chapters found!", new Vector2(300, 200), Color.Red);
                return;
            }

            for (int i = 0; i < chapters.Count; i++)
            {
                var rowRect = GetRowRect(i);
                var thumbRect = new Rectangle(rowRect.X, rowRect.Y + (rowRect.Height - ThumbSize) / 2, ThumbSize, ThumbSize);

                if (i == selectedIndex)
                    spriteBatch.Draw(pixel, rowRect, Color.DarkSlateGray * 0.6f);

                DrawThumbnail(spriteBatch, chapters[i], thumbRect);
                spriteBatch.DrawString(font, chapters[i].DisplayName, new Vector2(thumbRect.Right + 12, rowRect.Y + 16), Color.White);
            }
        }

        private void DrawThumbnail(SpriteBatch spriteBatch, ChapterEntry entry, Rectangle rect)
        {
            spriteBatch.Draw(pixel, rect, Color.Black * 0.6f);

            if (entry.Thumbnail != null)
            {
                spriteBatch.Draw(entry.Thumbnail, rect, Color.White);
            }
            else
            {
                var inner = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                spriteBatch.Draw(pixel, inner, Color.Gray * 0.6f);
            }
        }

        private Rectangle GetRowRect(int index)
        {
            return new Rectangle((int)listPosition.X, (int)listPosition.Y + index * ItemHeight, 360, ItemHeight);
        }

        private bool IsKeyPressed(Keys key, KeyboardState current)
        {
            return current.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
        }

        private void ActivateSelected(StateManager stateManager)
        {
            if (selectedIndex < 0 || selectedIndex >= chapters.Count)
                return;

            var selected = chapters[selectedIndex];
            if (selected.IsCustom)
                MapListManager.ExportMap(selected.MapName);
            else
                MapListManager.ClearExportedMap();

            if (game is GameDuMouse.GameMain.Core.Game1 g1)
                g1.SetChapterSelection(true);

            stateManager.ChangeState(GameState.PreGame);
        }

        private void ReloadChapters()
        {
            chapters.Clear();

            // Built-in chapters
            chapters.Add(new ChapterEntry
            {
                DisplayName = "Fase 1",
                MapName = "Fase 1",
                IsCustom = false,
                Thumbnail = LoadBuiltInThumbnail()
            });

            // Exported custom chapter (if any)
            var exported = MapListManager.ExportedMapName;
            if (!string.IsNullOrWhiteSpace(exported))
            {
                var data = MapDataManager.LoadByName(exported);
                chapters.Add(new ChapterEntry
                {
                    DisplayName = exported,
                    MapName = exported,
                    IsCustom = true,
                    Thumbnail = LoadCustomThumbnail(data)
                });
            }

            if (selectedIndex >= chapters.Count)
                selectedIndex = 0;
        }

        private Texture2D LoadBuiltInThumbnail()
        {
            try
            {
                return game.Content.Load<Texture2D>("scenario/background_image(01)");
            }
            catch
            {
                return null;
            }
        }

        private Texture2D LoadCustomThumbnail(MapData data)
        {
            if (data == null || data.BackgroundLayers == null || data.BackgroundLayers.Count == 0)
                return null;

            string path = data.BackgroundLayers[0].ImagePath;
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            try
            {
                return Texture2D.FromFile(game.GraphicsDevice, path);
            }
            catch
            {
                return null;
            }
        }
    }
}
