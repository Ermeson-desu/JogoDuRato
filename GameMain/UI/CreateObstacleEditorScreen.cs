using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Input;
using GameDuMouse.GameMain.Rendering;
using GameDuMouse.GameMain.UI.Components;

namespace GameDuMouse.GameMain.UI
{
    /// <summary>
    /// Screen for editing obstacle properties with a split layout:
    /// Left side (1/4): Property settings with checkboxes
    /// Right side (3/4): Visual preview/editing area
    /// </summary>
    public class CreateObstacleEditorScreen
    {
        private const int Margin = 10;
        private const float PanelAlpha = 0.7f;

        private Game game;
        private SpriteFont font;
        private BackButton backButton;
        private UiActionButton saveButton;
        private InputManager inputManager;
        private TextureCache textureCache;
        private Texture2D pixel;

        private int panelWidth => game.GraphicsDevice.Viewport.Width / 4; // 1/4 of screen
        private int screenWidth;
        private int screenHeight;

        // Obstacle data being edited
        private ObstacleEditorData obstacleData;
        private ObstacleEditorData originalData; // Keep original for cancel detection

        // Checkbox states and rendering
        private CheckboxState mortalCheckbox;
        private CheckboxState movableCheckbox;

        // Preview rectangle for obstacle
        private Rectangle obstaclePreviewBounds;

        // Input tracking
        private MouseState previousMouse;
        private KeyboardState previousKeyboard;

        // Callback when obstacle is saved
        private Action<ObstacleEditorData> onSaveCallback;

        private struct CheckboxState
        {
            public Rectangle Bounds;
            public bool IsChecked;
            public string Label;
        }

        public CreateObstacleEditorScreen(Game game)
        {
            this.game = game;
            inputManager = game.Services.GetService(typeof(InputManager)) as InputManager;
            textureCache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            previousMouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            previousKeyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font, inputManager);
            pixel = textureCache != null ? textureCache.Pixel : pixel;

            screenWidth = game.GraphicsDevice.Viewport.Width;
            screenHeight = game.GraphicsDevice.Viewport.Height;

            // Initialize save button
            var saveButtonBounds = new Rectangle(Margin, screenHeight - 60, panelWidth - Margin * 2, 40);
            saveButton = new UiActionButton(saveButtonBounds, "Salvar", SaveObstacle);

            // Initialize obstacle data
            obstacleData = new ObstacleEditorData();
            originalData = obstacleData.Clone();

            UpdateCheckboxes();
            UpdatePreviewBounds();
        }

        public void SetObstacleData(ObstacleEditorData data, Action<ObstacleEditorData> saveCallback)
        {
            if (data != null)
            {
                obstacleData = data.Clone();
                originalData = data.Clone();
            }
            else
            {
                obstacleData = new ObstacleEditorData();
                originalData = obstacleData.Clone();
            }

            onSaveCallback = saveCallback;
            UpdateCheckboxes();
            UpdatePreviewBounds();
        }

        private void UpdateCheckboxes()
        {
            int checkboxSize = 20;
            int checkboxX = Margin + 20;
            int checkboxY1 = 150;
            int checkboxY2 = 200;

            mortalCheckbox = new CheckboxState
            {
                Bounds = new Rectangle(checkboxX, checkboxY1, checkboxSize, checkboxSize),
                IsChecked = obstacleData.IsMortal,
                Label = "Mortal"
            };

            movableCheckbox = new CheckboxState
            {
                Bounds = new Rectangle(checkboxX, checkboxY2, checkboxSize, checkboxSize),
                IsChecked = obstacleData.IsMovable,
                Label = "Movel"
            };
        }

        private void UpdatePreviewBounds()
        {
            // Center the obstacle preview in the right 3/4 of the screen
            int previewAreaX = panelWidth;
            int previewAreaW = screenWidth - panelWidth;
            int previewAreaH = screenHeight;

            int centerX = previewAreaX + previewAreaW / 2;
            int centerY = previewAreaH / 2;

            obstaclePreviewBounds = new Rectangle(
                centerX - obstacleData.Width / 2,
                centerY - obstacleData.Height / 2,
                obstacleData.Width,
                obstacleData.Height
            );
        }

        public void ResetInput()
        {
            backButton?.ResetInput();
            previousMouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            previousKeyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
        }

        public void Update(StateManager stateManager)
        {
            backButton?.Update(stateManager);
            if (stateManager.CurrentState != GameState.CreateObstacle)
                return;

            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();

            // Handle checkbox clicks
            HandleCheckboxClick(mouse);

            // Handle width/height adjustments with arrow keys or mouse wheel
            HandleSizeAdjustment(mouse, keyboard);

            saveButton?.Update(mouse, previousMouse);

            previousMouse = mouse;
            previousKeyboard = keyboard;
        }

        private void HandleCheckboxClick(MouseState mouse)
        {
            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                // Check mortal checkbox
                if (mortalCheckbox.Bounds.Contains(mouse.Position))
                {
                    obstacleData.IsMortal = !obstacleData.IsMortal;
                    mortalCheckbox.IsChecked = obstacleData.IsMortal;
                }

                // Check movable checkbox
                if (movableCheckbox.Bounds.Contains(mouse.Position))
                {
                    obstacleData.IsMovable = !obstacleData.IsMovable;
                    movableCheckbox.IsChecked = obstacleData.IsMovable;
                }
            }
        }

        private void HandleSizeAdjustment(MouseState mouse, KeyboardState keyboard)
        {
            int sizeStep = 5;

            // Adjust width
            if (IsKeyPressed(Keys.Left, keyboard))
                obstacleData.Width = Math.Max(20, obstacleData.Width - sizeStep);
            if (IsKeyPressed(Keys.Right, keyboard))
                obstacleData.Width = Math.Min(300, obstacleData.Width + sizeStep);

            // Adjust height
            if (IsKeyPressed(Keys.Up, keyboard))
                obstacleData.Height = Math.Max(20, obstacleData.Height - sizeStep);
            if (IsKeyPressed(Keys.Down, keyboard))
                obstacleData.Height = Math.Min(300, obstacleData.Height + sizeStep);

            UpdatePreviewBounds();
        }

        private bool IsKeyPressed(Keys key, KeyboardState current)
        {
            return current.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
        }

        private void SaveObstacle()
        {
            onSaveCallback?.Invoke(obstacleData);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawPanel(spriteBatch);
            DrawPreviewArea(spriteBatch);
            DrawObstaclePreview(spriteBatch);
            backButton?.Draw(spriteBatch);
        }

        private void DrawPanel(SpriteBatch spriteBatch)
        {
            // Draw left panel background
            var panelRect = new Rectangle(0, 0, panelWidth, screenHeight);
            spriteBatch.Draw(pixel, panelRect, Color.DarkSlateGray * PanelAlpha);

            // Title
            spriteBatch.DrawString(font, "Editar Obstaculo", new Vector2(Margin, Margin + 20), Color.White);

            // Instructions
            spriteBatch.DrawString(font, "Setas para ajustar", new Vector2(Margin, 100), Color.LightGray);
            spriteBatch.DrawString(font, "tamanho", new Vector2(Margin, 118), Color.LightGray);

            // Checkboxes
            DrawCheckbox(spriteBatch, mortalCheckbox);
            DrawCheckbox(spriteBatch, movableCheckbox);

            // Size display
            var sizeText = $"W: {obstacleData.Width}  H: {obstacleData.Height}";
            spriteBatch.DrawString(font, sizeText, new Vector2(Margin, 260), Color.White);

            // Save button
            saveButton?.Draw(spriteBatch, font, pixel, Color.DarkSlateGray, Color.White);
        }

        private void DrawCheckbox(SpriteBatch spriteBatch, CheckboxState checkbox)
        {
            // Draw checkbox border
            spriteBatch.Draw(pixel, checkbox.Bounds, Color.White);

            // Draw checkbox fill if checked
            if (checkbox.IsChecked)
            {
                var innerRect = new Rectangle(
                    checkbox.Bounds.X + 3,
                    checkbox.Bounds.Y + 3,
                    checkbox.Bounds.Width - 6,
                    checkbox.Bounds.Height - 6
                );
                spriteBatch.Draw(pixel, innerRect, Color.LimeGreen);

                // Draw X mark
                DrawX(spriteBatch, checkbox.Bounds);
            }

            // Draw label
            spriteBatch.DrawString(font, checkbox.Label, 
                new Vector2(checkbox.Bounds.Right + 15, checkbox.Bounds.Y + 3), 
                Color.White);
        }

        private void DrawX(SpriteBatch spriteBatch, Rectangle bounds)
        {
            int x = bounds.X;
            int y = bounds.Y;
            int w = bounds.Width;
            int h = bounds.Height;
            int thickness = 2;

            // Draw top-left to bottom-right diagonal
            DrawLine(spriteBatch, new Point(x + 3, y + 3), new Point(x + w - 3, y + h - 3), thickness);

            // Draw top-right to bottom-left diagonal
            DrawLine(spriteBatch, new Point(x + w - 3, y + 3), new Point(x + 3, y + h - 3), thickness);
        }

        private void DrawLine(SpriteBatch spriteBatch, Point p1, Point p2, int thickness)
        {
            float distance = Vector2.Distance(new Vector2(p1.X, p1.Y), new Vector2(p2.X, p2.Y));
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            spriteBatch.Draw(pixel,
                new Rectangle(p1.X, p1.Y, (int)distance, thickness),
                null,
                Color.LimeGreen,
                angle,
                Vector2.Zero,
                SpriteEffects.None,
                0);
        }

        private void DrawPreviewArea(SpriteBatch spriteBatch)
        {
            // Draw right preview area background
            var previewRect = new Rectangle(panelWidth, 0, screenWidth - panelWidth, screenHeight);
            spriteBatch.Draw(pixel, previewRect, Color.CornflowerBlue * 0.3f);

            // Draw grid lines or instructions
            spriteBatch.DrawString(font, "Preview", 
                new Vector2(screenWidth - 150, 20), 
                Color.White);
        }

        private void DrawObstaclePreview(SpriteBatch spriteBatch)
        {
            // Draw obstacle preview rectangle
            Color obstacleColor = obstacleData.IsMortal ? Color.Red : Color.Green;
            float colorAlpha = 0.7f;

            spriteBatch.Draw(pixel, obstaclePreviewBounds, obstacleColor * colorAlpha);

            // Draw border
            DrawRectangleBorder(spriteBatch, obstaclePreviewBounds, 2, Color.White);

            // Draw label
            var labelText = obstacleData.IsMortal ? "Mortal" : "Plataforma";
            var labelSize = font.MeasureString(labelText);
            spriteBatch.DrawString(font, labelText,
                new Vector2(
                    obstaclePreviewBounds.Center.X - labelSize.X / 2,
                    obstaclePreviewBounds.Center.Y - labelSize.Y / 2),
                Color.White);
        }

        private void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle bounds, int thickness, Color color)
        {
            // Top
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
            // Left
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
            // Right
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
        }
    }
}
