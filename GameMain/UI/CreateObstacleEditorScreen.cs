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
        private CheckboxState horizontalCheckbox;
        private CheckboxState verticalCheckbox;
        private CheckboxState loopingCheckbox;

        // Preview rectangle for obstacle
        private Rectangle obstaclePreviewBounds;

        // Movement animation
        private float movementOffset = 0f; // Offset from center position
        private float movementDirection = 1f; // 1 for forward, -1 for backward
        private const float MoveSpeed = 50f; // pixels per second

        // Size adjustment buttons
        private Rectangle widthDecreaseBtn;
        private Rectangle widthIncreaseBtn;
        private Rectangle heightDecreaseBtn;
        private Rectangle heightIncreaseBtn;

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
            UpdateSizeControls();
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
            UpdateSizeControls();
        }

        private void UpdateCheckboxes()
        {
            int checkboxSize = 20;
            int checkboxX = Margin + 20;
            int checkboxY1 = 200;
            int checkboxY2 = 240;
            int movementCheckboxY = 280;
            int loopingCheckboxY = 320;

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

            // Movement type checkboxes (only visible if movable)
            horizontalCheckbox = new CheckboxState
            {
                Bounds = new Rectangle(checkboxX + 30, movementCheckboxY, checkboxSize, checkboxSize),
                IsChecked = obstacleData.MovementType == MovementType.Horizontal,
                Label = "Horizontal"
            };

            verticalCheckbox = new CheckboxState
            {
                Bounds = new Rectangle(checkboxX + 30, movementCheckboxY + 35, checkboxSize, checkboxSize),
                IsChecked = obstacleData.MovementType == MovementType.Vertical,
                Label = "Vertical"
            };

            // Looping checkbox (only visible if movement is selected)
            loopingCheckbox = new CheckboxState
            {
                Bounds = new Rectangle(checkboxX + 30, loopingCheckboxY, checkboxSize, checkboxSize),
                IsChecked = obstacleData.IsLooping,
                Label = "Volta"
            };
        }

        private void UpdateSizeControls()
        {
            int btnSize = 25;
            int spacing = 5;
            int startX = Margin + 20;
            int startY = 120;
            int btnWidthDisplay = 50;

            // Width controls: [−] [ 50 ] [+]
            widthDecreaseBtn = new Rectangle(startX, startY, btnSize, btnSize);
            widthIncreaseBtn = new Rectangle(startX + btnSize + btnWidthDisplay + spacing, startY, btnSize, btnSize);

            // Height controls: [−] [ 50 ] [+]
            int heightStartY = startY + btnSize + 20;
            heightDecreaseBtn = new Rectangle(startX, heightStartY, btnSize, btnSize);
            heightIncreaseBtn = new Rectangle(startX + btnSize + btnWidthDisplay + spacing, heightStartY, btnSize, btnSize);
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

            UpdateSizeControls();
        }

        public void ResetInput()
        {
            backButton?.ResetInput();
            previousMouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            previousKeyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();
        }

        public void Update(StateManager stateManager)
        {
            Update(null, stateManager);
        }

        public void Update(GameTime gameTime, StateManager stateManager)
        {
            backButton?.Update(stateManager);
            if (stateManager.CurrentState != GameState.CreateObstacle)
                return;

            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();

            // Handle checkbox clicks
            HandleCheckboxClick(mouse);

            // Handle size button clicks
            HandleSizeButtonClick(mouse);

            // Handle width/height adjustments with arrow keys or mouse wheel
            HandleSizeAdjustment(mouse, keyboard);

            // Update movement animation
            if (gameTime != null && obstacleData.IsMovable && obstacleData.MovementType != MovementType.None)
            {
                UpdateMovement(gameTime);
            }

            saveButton?.Update(mouse, previousMouse);

            previousMouse = mouse;
            previousKeyboard = keyboard;
        }

        private void UpdateMovement(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float distance = MoveSpeed * elapsed * movementDirection;
            movementOffset += distance;

            if (obstacleData.IsLooping)
            {
                // Bounce back and forth
                if (Math.Abs(movementOffset) >= obstacleData.MovementDistance)
                {
                    movementDirection *= -1f;
                    movementOffset = MathHelper.Clamp(movementOffset, -obstacleData.MovementDistance, obstacleData.MovementDistance);
                }
            }
            else
            {
                // Only go one direction
                movementOffset = Math.Min(movementOffset, obstacleData.MovementDistance);
            }
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
                    
                    if (obstacleData.IsMovable)
                    {
                        // Set default to Horizontal when enabling movement
                        obstacleData.MovementType = MovementType.Horizontal;
                        obstacleData.IsLooping = true;
                        movementOffset = 0f; // Reset movement animation
                        movementDirection = 1f;
                    }
                    else
                    {
                        obstacleData.MovementType = MovementType.None;
                        movementOffset = 0f; // Reset movement animation
                        movementDirection = 1f;
                    }
                    
                    UpdateCheckboxes();
                }

                // Check horizontal checkbox (only if movable)
                if (obstacleData.IsMovable && horizontalCheckbox.Bounds.Contains(mouse.Position))
                {
                    obstacleData.MovementType = obstacleData.MovementType == MovementType.Horizontal 
                        ? MovementType.None 
                        : MovementType.Horizontal;
                    horizontalCheckbox.IsChecked = obstacleData.MovementType == MovementType.Horizontal;
                    movementOffset = 0f; // Reset animation
                    movementDirection = 1f;
                }

                // Check vertical checkbox (only if movable)
                if (obstacleData.IsMovable && verticalCheckbox.Bounds.Contains(mouse.Position))
                {
                    obstacleData.MovementType = obstacleData.MovementType == MovementType.Vertical 
                        ? MovementType.None 
                        : MovementType.Vertical;
                    verticalCheckbox.IsChecked = obstacleData.MovementType == MovementType.Vertical;
                    movementOffset = 0f; // Reset animation
                    movementDirection = 1f;
                }

                // Check looping checkbox (only if movement is selected)
                if (obstacleData.MovementType != MovementType.None && loopingCheckbox.Bounds.Contains(mouse.Position))
                {
                    obstacleData.IsLooping = !obstacleData.IsLooping;
                    loopingCheckbox.IsChecked = obstacleData.IsLooping;
                }
            }
        }

        private void HandleSizeButtonClick(MouseState mouse)
        {
            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                int sizeStep = 10;

                // Width decrease button
                if (widthDecreaseBtn.Contains(mouse.Position))
                {
                    obstacleData.Width = Math.Max(20, obstacleData.Width - sizeStep);
                    UpdatePreviewBounds();
                }

                // Width increase button
                if (widthIncreaseBtn.Contains(mouse.Position))
                {
                    obstacleData.Width = Math.Min(300, obstacleData.Width + sizeStep);
                    UpdatePreviewBounds();
                }

                // Height decrease button
                if (heightDecreaseBtn.Contains(mouse.Position))
                {
                    obstacleData.Height = Math.Max(20, obstacleData.Height - sizeStep);
                    UpdatePreviewBounds();
                }

                // Height increase button
                if (heightIncreaseBtn.Contains(mouse.Position))
                {
                    obstacleData.Height = Math.Min(300, obstacleData.Height + sizeStep);
                    UpdatePreviewBounds();
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

            // Draw size controls
            DrawSizeControls(spriteBatch);

            // Checkboxes
            DrawCheckbox(spriteBatch, mortalCheckbox);
            DrawCheckbox(spriteBatch, movableCheckbox);

            // Movement options (only if movable)
            if (obstacleData.IsMovable)
            {
                spriteBatch.DrawString(font, "Movimento:", new Vector2(Margin + 30, 275), Color.LightGray);
                DrawCheckbox(spriteBatch, horizontalCheckbox);
                DrawCheckbox(spriteBatch, verticalCheckbox);

                // Looping option (only if movement type is selected)
                if (obstacleData.MovementType != MovementType.None)
                {
                    spriteBatch.DrawString(font, "Comportamento:", new Vector2(Margin + 30, 315), Color.LightGray);
                    DrawCheckbox(spriteBatch, loopingCheckbox);
                }
            }

            // Size display
            var sizeText = $"W: {obstacleData.Width}  H: {obstacleData.Height}";
            spriteBatch.DrawString(font, sizeText, new Vector2(Margin, 260), Color.White);

            // Save button
            saveButton?.Draw(spriteBatch, font, pixel, Color.DarkSlateGray, Color.White);
        }

        private void DrawSizeControls(SpriteBatch spriteBatch)
        {
            int spacing = 5;

            // Width label and controls
            spriteBatch.DrawString(font, "Largura:", new Vector2(Margin, 120), Color.LightGray);
            
            // Width buttons: [-] [value] [+]
            DrawButton(spriteBatch, widthDecreaseBtn, "-", Color.Firebrick);
            var widthValuePos = new Vector2(
                widthDecreaseBtn.X + widthDecreaseBtn.Width + spacing,
                widthDecreaseBtn.Y + (widthDecreaseBtn.Height - font.LineSpacing) / 2
            );
            spriteBatch.DrawString(font, obstacleData.Width.ToString(), widthValuePos, Color.Yellow);
            DrawButton(spriteBatch, widthIncreaseBtn, "+", Color.Green);

            // Height label and controls
            spriteBatch.DrawString(font, "Altura:", new Vector2(Margin, heightDecreaseBtn.Y - 25), Color.LightGray);
            
            // Height buttons: [-] [value] [+]
            DrawButton(spriteBatch, heightDecreaseBtn, "-", Color.Firebrick);
            var heightValuePos = new Vector2(
                heightDecreaseBtn.X + heightDecreaseBtn.Width + spacing,
                heightDecreaseBtn.Y + (heightDecreaseBtn.Height - font.LineSpacing) / 2
            );
            spriteBatch.DrawString(font, obstacleData.Height.ToString(), heightValuePos, Color.Yellow);
            DrawButton(spriteBatch, heightIncreaseBtn, "+", Color.Green);
        }

        private void DrawButton(SpriteBatch spriteBatch, Rectangle bounds, string text, Color backgroundColor)
        {
            // Draw button background
            spriteBatch.Draw(pixel, bounds, backgroundColor);
            
            // Draw button text
            var textSize = font.MeasureString(text);
            var textPos = new Vector2(
                bounds.X + (bounds.Width - textSize.X) / 2f,
                bounds.Y + (bounds.Height - textSize.Y) / 2f
            );
            spriteBatch.DrawString(font, text, textPos, Color.White);
            
            // Draw button border
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - 1, bounds.Y, 1, bounds.Height), Color.White);
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
            // Draw movement path first (behind the obstacle)
            if (obstacleData.IsMovable && obstacleData.MovementType != MovementType.None)
            {
                DrawMovementPath(spriteBatch);
            }

            // Calculate position with movement offset
            Rectangle animatedObstacleBounds = obstaclePreviewBounds;
            if (obstacleData.IsMovable && obstacleData.MovementType != MovementType.None)
            {
                if (obstacleData.MovementType == MovementType.Horizontal)
                {
                    animatedObstacleBounds.X += (int)movementOffset;
                }
                else if (obstacleData.MovementType == MovementType.Vertical)
                {
                    animatedObstacleBounds.Y += (int)movementOffset;
                }
            }

            // Draw obstacle preview rectangle
            Color obstacleColor = obstacleData.IsMortal ? Color.Red : Color.Green;
            float colorAlpha = 0.7f;

            spriteBatch.Draw(pixel, animatedObstacleBounds, obstacleColor * colorAlpha);

            // Draw border
            DrawRectangleBorder(spriteBatch, animatedObstacleBounds, 2, Color.White);

            // Draw label
            var labelText = obstacleData.IsMortal ? "Mortal" : "Plataforma";
            var labelSize = font.MeasureString(labelText);
            spriteBatch.DrawString(font, labelText,
                new Vector2(
                    animatedObstacleBounds.Center.X - labelSize.X / 2,
                    animatedObstacleBounds.Center.Y - labelSize.Y / 2),
                Color.White);
        }

        private void DrawMovementPath(SpriteBatch spriteBatch)
        {
            int lineThickness = 2;
            int checkboxSize = 15;

            if (obstacleData.MovementType == MovementType.Horizontal)
            {
                // Draw horizontal line
                int startX = obstaclePreviewBounds.X - obstacleData.MovementDistance;
                int endX = obstaclePreviewBounds.X + obstacleData.MovementDistance;
                int centerY = obstaclePreviewBounds.Center.Y;

                var lineRect = new Rectangle(startX, centerY - lineThickness / 2, endX - startX, lineThickness);
                spriteBatch.Draw(pixel, lineRect, Color.Yellow * 0.5f);

                // Draw endpoints
                DrawMovementEndpoint(spriteBatch, new Point(startX, centerY), checkboxSize);
                DrawMovementEndpoint(spriteBatch, new Point(endX, centerY), checkboxSize);
            }
            else if (obstacleData.MovementType == MovementType.Vertical)
            {
                // Draw vertical line
                int startY = obstaclePreviewBounds.Y - obstacleData.MovementDistance;
                int endY = obstaclePreviewBounds.Y + obstacleData.MovementDistance;
                int centerX = obstaclePreviewBounds.Center.X;

                var lineRect = new Rectangle(centerX - lineThickness / 2, startY, lineThickness, endY - startY);
                spriteBatch.Draw(pixel, lineRect, Color.Yellow * 0.5f);

                // Draw endpoints
                DrawMovementEndpoint(spriteBatch, new Point(centerX, startY), checkboxSize);
                DrawMovementEndpoint(spriteBatch, new Point(centerX, endY), checkboxSize);
            }
        }

        private void DrawMovementEndpoint(SpriteBatch spriteBatch, Point position, int size)
        {
            var rect = new Rectangle(position.X - size / 2, position.Y - size / 2, size, size);
            
            // Draw circle/endpoint
            spriteBatch.Draw(pixel, rect, Color.Cyan * 0.7f);
            
            // Draw border
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), Color.White);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), Color.White);

            // Draw looping indicator
            if (obstacleData.IsLooping)
            {
                // Draw a small check mark on the endpoint
                int markSize = 2;
                spriteBatch.Draw(pixel, new Rectangle(rect.X + 3, rect.Y + rect.Height / 2 - 2, 3, markSize), Color.Lime);
                spriteBatch.Draw(pixel, new Rectangle(rect.X + 6, rect.Y + rect.Height / 2 - 4, 2, 4), Color.Lime);
            }
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
