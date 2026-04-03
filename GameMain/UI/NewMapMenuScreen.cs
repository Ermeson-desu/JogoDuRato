using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.UI.Components;
using GameDuMouse.GameMain.Input;
using GameDuMouse.GameMain.Rendering;

namespace GameDuMouse.GameMain.UI
{
    public class NewMapMenuScreen
    {
        private readonly Game game;
        private SpriteFont font;
        private BackButton backButton;
        private UiButton createButton;
        private UiButton exportButton;
        private UiButton deleteButton;
        private UiList mapList;
        private TextEntryModal textEntry;
        private Texture2D pixel;
        private List<string> maps = new List<string>();
        private InputManager inputManager;
        private TextureCache textureCache;
        private bool isOptionsOpen;
        private int optionsTargetIndex = -1;
        private string statusMessage;
        private Vector2 listPosition;
        private int listLineHeight;
        private int listWidth;
        private Rectangle optionsAnchorRect;

        private KeyboardState previousKeyboard;
        private MouseState previousMouse;
        private bool ignoreNextInput;

        public NewMapMenuScreen(Game game)
        {
            this.game = game;
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;
            textureCache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            previousKeyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            previousMouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            ignoreNextInput = true;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font, inputManager);

            pixel = textureCache != null ? textureCache.Pixel : pixel;

            createButton = new UiButton(new Rectangle(300, 120, 240, 40), "Create New Map");
            listPosition = new Vector2(300, 200);
            listLineHeight = 50;
            listWidth = 240;
            mapList = new UiList(listPosition, listLineHeight, listWidth);
            textEntry = new TextEntryModal(game);
            textEntry.LoadContent(game.GraphicsDevice);
            exportButton = new UiButton(new Rectangle(0, 0, 160, 36), "Exportar");
            deleteButton = new UiButton(new Rectangle(0, 0, 160, 36), "Excluir");

            ReloadMaps();
        }

        public void ResetInput()
        {
            previousKeyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            previousMouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            ignoreNextInput = true;
            backButton?.ResetInput();
            isOptionsOpen = false;
            optionsTargetIndex = -1;
            statusMessage = null;
            ReloadMaps();
        }

        public void Update(StateManager stateManager)
        {
            backButton?.Update(stateManager);
            if (stateManager.CurrentState != GameState.NewMapMenu)
                return;

            if (ignoreNextInput)
            {
                var k = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
                var m = inputManager != null ? inputManager.Mouse : Mouse.GetState();
                previousKeyboard = k;
                previousMouse = m;
                if (m.LeftButton == ButtonState.Released && !k.IsKeyDown(Keys.Enter))
                    ignoreNextInput = false;
                return;
            }

            if (textEntry.IsOpen)
            {
                textEntry.Update();
                string confirmed = textEntry.ConsumeConfirmedText();
                if (!string.IsNullOrWhiteSpace(confirmed))
                {
                    MapListManager.AddMap(confirmed);
                    ReloadMaps();
                    textEntry.Close();
                }

                if (textEntry.ConsumeCanceled())
                    textEntry.Close();

                return;
            }

            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();

            if (isOptionsOpen)
            {
                HandleOptionsPopup(mouse);
                previousKeyboard = keyboard;
                previousMouse = mouse;
                return;
            }

            if (createButton.Update(mouse, previousMouse))
            {
                textEntry.Open();
                previousKeyboard = keyboard;
                previousMouse = mouse;
                return;
            }

            if (TryOpenOptionsAtMouse(mouse))
            {
                previousKeyboard = keyboard;
                previousMouse = mouse;
                return;
            }

            if (mapList.HandleInput(keyboard, previousKeyboard, mouse, previousMouse, out int activatedIndex))
            {
                if (activatedIndex >= 0 && activatedIndex < maps.Count)
                {
                    var selectedMap = maps[activatedIndex];
                    MapListManager.SetCurrentMap(selectedMap);
                    if (game is GameDuMouse.GameMain.Core.Game1 g1)
                        g1.PrepareMapEditing(selectedMap);
                    stateManager.ChangeState(GameState.Mapping);
                }
            }

            previousKeyboard = keyboard;
            previousMouse = mouse;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, "Select a map:", new Vector2(300, 80), Color.White);
            createButton?.Draw(spriteBatch, font, pixel, Color.DarkSlateGray, Color.White);
            backButton?.Draw(spriteBatch);

            if (maps.Count == 0)
            {
                spriteBatch.DrawString(font, "No maps found!", new Vector2(300, 200), Color.Red);
            }
            else
            {
                mapList.Draw(spriteBatch, font, Color.White, Color.Yellow);
                DrawOptionsButtonsForMaps(spriteBatch);
            }

            if (!string.IsNullOrWhiteSpace(statusMessage))
                spriteBatch.DrawString(font, statusMessage, new Vector2(300, 160), Color.Yellow);

            textEntry?.Draw(spriteBatch, font);
            DrawOptionsPopup(spriteBatch);
        }

        private void ReloadMaps()
        {
            maps = MapListManager.LoadAllMaps();
            mapList.SetItems(maps);
            if (maps.Count == 0)
            {
                isOptionsOpen = false;
                optionsTargetIndex = -1;
            }
        }

        private void DrawOptionsButtonsForMaps(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < maps.Count; i++)
            {
                var rect = GetOptionsButtonRectForIndex(i);
                spriteBatch.Draw(pixel, rect, Color.DarkSlateGray);
                DrawOptionsIcon(spriteBatch, rect, Color.White);
            }
        }

        private void DrawOptionsIcon(SpriteBatch spriteBatch, Rectangle bounds, Color color)
        {
            int lineHeight = 3;
            int padding = 8;
            int spacing = 6;
            int totalHeight = lineHeight * 3 + spacing * 2;
            int startY = bounds.Y + (bounds.Height - totalHeight) / 2;
            for (int i = 0; i < 3; i++)
            {
                var line = new Rectangle(bounds.X + padding, startY + i * (lineHeight + spacing), bounds.Width - padding * 2, lineHeight);
                spriteBatch.Draw(pixel, line, color);
            }
        }

        private void DrawOptionsPopup(SpriteBatch spriteBatch)
        {
            if (!isOptionsOpen)
                return;

            UpdateOptionsPopupBounds();

            var panel = new Rectangle(optionsAnchorRect.X - 170, optionsAnchorRect.Bottom + 8, 180, 110);
            spriteBatch.Draw(pixel, panel, Color.DarkSlateGray);

            exportButton.Draw(spriteBatch, font, pixel, Color.Gray, Color.White);
            deleteButton.Draw(spriteBatch, font, pixel, Color.Firebrick, Color.White);
        }

        private void HandleOptionsPopup(MouseState mouse)
        {
            UpdateOptionsPopupBounds();

            if (exportButton.Update(mouse, previousMouse))
            {
                ExportSelectedMap();
                isOptionsOpen = false;
                return;
            }

            if (deleteButton.Update(mouse, previousMouse))
            {
                DeleteSelectedMap();
                isOptionsOpen = false;
                return;
            }

            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                var panel = new Rectangle(optionsAnchorRect.X - 170, optionsAnchorRect.Bottom + 8, 180, 110);
                if (!panel.Contains(mouse.Position) && !optionsAnchorRect.Contains(mouse.Position))
                    isOptionsOpen = false;
            }
        }

        private void UpdateOptionsPopupBounds()
        {
            int panelX = optionsAnchorRect.X - 170;
            int panelY = optionsAnchorRect.Bottom + 8;
            exportButton.SetBounds(new Rectangle(panelX + 10, panelY + 10, 160, 36));
            deleteButton.SetBounds(new Rectangle(panelX + 10, panelY + 56, 160, 36));
        }

        private void ExportSelectedMap()
        {
            if (optionsTargetIndex < 0 || optionsTargetIndex >= maps.Count)
                return;

            string mapName = maps[optionsTargetIndex];
            MapListManager.ExportMap(mapName);
            statusMessage = $"Mapa exportado: {mapName}";
        }

        private void DeleteSelectedMap()
        {
            if (optionsTargetIndex < 0 || optionsTargetIndex >= maps.Count)
                return;

            string mapName = maps[optionsTargetIndex];
            MapListManager.DeleteMap(mapName);
            statusMessage = $"Mapa excluido: {mapName}";
            ReloadMaps();
        }

        private Rectangle GetOptionsButtonRectForIndex(int index)
        {
            int size = 28;
            int x = (int)listPosition.X + listWidth + 10;
            int y = (int)listPosition.Y + index * listLineHeight + (listLineHeight - size) / 2 - 20;
            return new Rectangle(x, y, size, size);
        }

        private bool TryOpenOptionsAtMouse(MouseState mouse)
        {
            if (maps.Count == 0)
                return false;

            if (mouse.LeftButton != ButtonState.Pressed || previousMouse.LeftButton != ButtonState.Released)
                return false;

            for (int i = 0; i < maps.Count; i++)
            {
                var rect = GetOptionsButtonRectForIndex(i);
                if (rect.Contains(mouse.Position))
                {
                    optionsTargetIndex = i;
                    optionsAnchorRect = rect;
                    isOptionsOpen = true;
                    return true;
                }
            }

            return false;
        }
    }
}
