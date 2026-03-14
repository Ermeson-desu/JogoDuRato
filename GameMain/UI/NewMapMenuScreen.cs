using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.UI.Components;

namespace GameDuMouse.GameMain.UI
{
    public class NewMapMenuScreen
    {
        private readonly Game game;
        private SpriteFont font;
        private BackButton backButton;
        private UiButton createButton;
        private UiList mapList;
        private TextEntryModal textEntry;
        private Texture2D pixel;
        private List<string> maps = new List<string>();

        private KeyboardState previousKeyboard;
        private MouseState previousMouse;
        private bool ignoreNextInput;

        public NewMapMenuScreen(Game game)
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

            createButton = new UiButton(new Rectangle(300, 120, 240, 40), "Create New Map");
            mapList = new UiList(new Vector2(300, 200), 50, 240);
            textEntry = new TextEntryModal(game);
            textEntry.LoadContent(game.GraphicsDevice);

            ReloadMaps();
        }

        public void ResetInput()
        {
            previousKeyboard = Keyboard.GetState();
            previousMouse = Mouse.GetState();
            ignoreNextInput = true;
            backButton?.ResetInput();
            ReloadMaps();
        }

        public void Update(StateManager stateManager)
        {
            backButton?.Update(stateManager);
            if (stateManager.CurrentState != GameState.NewMapMenu)
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

            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();

            if (createButton.Update(mouse, previousMouse))
            {
                textEntry.Open();
                previousKeyboard = keyboard;
                previousMouse = mouse;
                return;
            }

            if (mapList.HandleInput(keyboard, previousKeyboard, mouse, previousMouse, out int activatedIndex))
            {
                if (activatedIndex >= 0 && activatedIndex < maps.Count)
                    stateManager.ChangeState(GameState.Mapping);
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
            }

            textEntry?.Draw(spriteBatch, font);
        }

        private void ReloadMaps()
        {
            maps = MapListManager.LoadAllMaps();
            mapList.SetItems(maps);
        }
    }
}
