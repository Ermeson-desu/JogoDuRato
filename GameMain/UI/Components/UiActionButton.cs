using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameDuMouse.GameMain.UI.Components
{
    public class UiActionButton
    {
        public Rectangle Bounds { get; private set; }
        public string Text { get; private set; }
        public Action OnClick { get; private set; }

        public UiActionButton(Rectangle bounds, string text, Action onClick)
        {
            Bounds = bounds;
            Text = text;
            OnClick = onClick;
        }

        public void SetText(string text)
        {
            Text = text;
        }

        public void SetBounds(Rectangle bounds)
        {
            Bounds = bounds;
        }

        public void SetAction(Action onClick)
        {
            OnClick = onClick;
        }

        public void Update(MouseState mouse, MouseState previousMouse)
        {
            bool clicked = mouse.LeftButton == ButtonState.Pressed &&
                           previousMouse.LeftButton == ButtonState.Released &&
                           Bounds.Contains(mouse.Position);
            if (clicked)
                OnClick?.Invoke();
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Texture2D pixel, Color background, Color textColor)
        {
            spriteBatch.Draw(pixel, Bounds, background);
            var textSize = font.MeasureString(Text);
            var textPos = new Vector2(
                Bounds.X + (Bounds.Width - textSize.X) / 2f,
                Bounds.Y + (Bounds.Height - textSize.Y) / 2f
            );
            spriteBatch.DrawString(font, Text, textPos, textColor);
        }
    }
}
