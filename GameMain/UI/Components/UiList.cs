using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDuMouse.GameMain.UI.Components
{
    public class UiList
    {
        private List<string> items = new List<string>();
        private Vector2 position;
        private int lineHeight;
        private int width;

        public int SelectedIndex { get; private set; } = 0;

        public UiList(Vector2 position, int lineHeight, int width)
        {
            this.position = position;
            this.lineHeight = lineHeight;
            this.width = width;
        }

        public void SetItems(List<string> newItems)
        {
            items = newItems ?? new List<string>();
            if (items.Count == 0)
                SelectedIndex = 0;
            else
                SelectedIndex = MathHelper.Clamp(SelectedIndex, 0, items.Count - 1);
        }

        public bool HandleInput(KeyboardState keyboard, KeyboardState previousKeyboard, MouseState mouse, MouseState previousMouse, out int activatedIndex)
        {
            activatedIndex = -1;

            if (items.Count == 0)
                return false;

            if (IsKeyPressed(Keys.Down, keyboard, previousKeyboard))
                SelectedIndex = (SelectedIndex + 1) % items.Count;

            if (IsKeyPressed(Keys.Up, keyboard, previousKeyboard))
                SelectedIndex = (SelectedIndex - 1 + items.Count) % items.Count;

            if (IsKeyPressed(Keys.Enter, keyboard, previousKeyboard))
            {
                activatedIndex = SelectedIndex;
                return true;
            }

            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    Rectangle itemBounds = new Rectangle(
                        (int)position.X,
                        (int)position.Y + i * lineHeight,
                        width,
                        lineHeight
                    );

                    if (itemBounds.Contains(mouse.Position))
                    {
                        SelectedIndex = i;
                        activatedIndex = i;
                        return true;
                    }
                }
            }

            return false;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Color normalColor, Color selectedColor)
        {
            for (int i = 0; i < items.Count; i++)
            {
                Color color = (i == SelectedIndex) ? selectedColor : normalColor;
                spriteBatch.DrawString(font, items[i], new Vector2(position.X, position.Y + i * lineHeight), color);
            }
        }

        private bool IsKeyPressed(Keys key, KeyboardState current, KeyboardState previous)
        {
            return current.IsKeyDown(key) && !previous.IsKeyDown(key);
        }
    }
}
