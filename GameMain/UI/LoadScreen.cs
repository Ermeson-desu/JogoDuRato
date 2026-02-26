using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Utils;
using System.Collections.Generic;

namespace GameDuMouse.GameMain.UI
{
    public class LoadScreen
    {
        private SpriteFont font;
        private List<SaveData> saves;
        private int selectedIndex = 0;

        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private DirectInputController directController;
        private Game game;

        public LoadScreen(Game game)
        {
            this.game = game;
            directController = new DirectInputController();
            saves = SaveManager.LoadAllSaves();
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
        }
        public void Update(StateManager stateManager)
        {
            // 🔑 sempre recarrega a lista
            saves = SaveManager.LoadAllSaves();

            if (saves.Count == 0)
                return;

            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();
            var state = directController.GetState();

            if (IsKeyPressed(Keys.Down, keyboard))
                selectedIndex = (selectedIndex + 1) % saves.Count;

            if (IsKeyPressed(Keys.Up, keyboard))
                selectedIndex = (selectedIndex - 1 + saves.Count) % saves.Count;

            if (IsKeyPressed(Keys.Enter, keyboard))
                LoadSelectedSave(stateManager);

            // Mouse
            if (mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < saves.Count; i++)
                {
                    Rectangle optionBounds = new Rectangle(300, 200 + i * 50, 200, 40);
                    if (optionBounds.Contains(mouse.Position))
                    {
                        selectedIndex = i;
                        LoadSelectedSave(stateManager);
                    }
                }
            }

            previousKeyboardState = keyboard;
            previousMouseState = mouse;
        }
        private bool IsKeyPressed(Keys key, KeyboardState current)
        {
            return current.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }

        private void LoadSelectedSave(StateManager stateManager)
        {
            if (saves.Count == 0)
                return;

            var save = saves[selectedIndex];
            var game1 = (Game1)game;

            game1.LoadSave(save);
            stateManager.ChangeState(GameState.Playing);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, "Selecione seu save:", new Vector2(100, 100), Color.White);

            if (saves.Count == 0)
            {
                spriteBatch.DrawString(font, "Nenhum save encontrado!", new Vector2(300, 200), Color.Red);
                return;
            }

            for (int i = 0; i < saves.Count; i++)
            {
                Color color = (i == selectedIndex) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(font, saves[i].PlayerName, new Vector2(300, 200 + i * 50), color);
            }
        }
    }
}