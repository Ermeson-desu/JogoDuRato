using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Input;
using GameDuMouse.GameMain.Rendering;
using GameDuMouse.GameMain.Services;
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
        private UiActionButton addImageButton;
        private InputManager inputManager;
        private TextureCache textureCache;
        private EditorService editorService;
        private AssetManager assetManager;
        private CustomObstacleService customObstacleService;
        private Texture2D pixel;
        private Texture2D obstacleImage; // Loaded image texture

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

        // Panel scroll
        private float panelScroll = 0f; // Scroll offset for the left panel
        private const float ScrollSpeed = 0.5f;

        // Size adjustment buttons
        private Rectangle widthDecreaseBtn;
        private Rectangle widthIncreaseBtn;
        private Rectangle heightDecreaseBtn;
        private Rectangle heightIncreaseBtn;

        // Speed adjustment buttons
        private Rectangle speedDecreaseBtn;
        private Rectangle speedIncreaseBtn;

        // Obstacle name input field
        private Rectangle nameInputBounds;
        private string editingName = "New Obstacle";
        private bool isNameInputFocused = false;
        private double nameInputBlinkTime = 0;

        // Status message display
        private string saveStatusMessage = "";
        private System.DateTime saveStatusExpiresAtUtc = System.DateTime.UtcNow;

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
            editorService = game.Services.GetService(typeof(EditorService)) as EditorService;
            assetManager = game.Services.GetService(typeof(AssetManager)) as AssetManager;
            customObstacleService = game.Services.GetService(typeof(CustomObstacleService)) as CustomObstacleService;
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

            // Initialize add image button
            var addImageButtonBounds = new Rectangle(Margin, screenHeight - 110, panelWidth - Margin * 2, 40);
            addImageButton = new UiActionButton(addImageButtonBounds, "Adicionar Imagem", AddImage);

            // Initialize obstacle data
            obstacleData = new ObstacleEditorData();
            originalData = obstacleData.Clone();

            // Initialize name editing
            editingName = obstacleData.Name;
            isNameInputFocused = false;

            UpdateCheckboxes();
            UpdatePreviewBounds();
            UpdateSizeControls();
            
            // Initialize movement animation to start from beginning
            movementOffset = -obstacleData.MovementDistance;
            movementDirection = 1f;
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
            
            // Reset movement animation to start from beginning
            movementOffset = -obstacleData.MovementDistance;
            movementDirection = 1f;

            // Initialize name editing
            editingName = string.IsNullOrWhiteSpace(obstacleData.Name) ? "New Obstacle" : obstacleData.Name;
            isNameInputFocused = false;

            // Load image if it exists
            if (!string.IsNullOrWhiteSpace(obstacleData.ImagePath) && assetManager != null)
            {
                obstacleImage = assetManager.LoadTextureFromFile(obstacleData.ImagePath);
            }
            else
            {
                obstacleImage = null;
            }
        }

        private void UpdateCheckboxes()
        {
            int checkboxSize = 20;
            int checkboxX = Margin + 20;
            int checkboxY1 = 250;
            int checkboxY2 = 290;
            int movementCheckboxY = 350;
            int loopingCheckboxY = 450;

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

            // Name input field (positioned above size controls)
            nameInputBounds = new Rectangle(startX, startY - 50, panelWidth - Margin - startX, 25);

            // Width controls: [−] [ 50 ] [+]
            widthDecreaseBtn = new Rectangle(startX, startY, btnSize, btnSize);
            widthIncreaseBtn = new Rectangle(startX + btnSize + btnWidthDisplay + spacing, startY, btnSize, btnSize);

            // Height controls: [−] [ 50 ] [+]
            int heightStartY = startY + btnSize + 20;
            heightDecreaseBtn = new Rectangle(startX, heightStartY, btnSize, btnSize);
            heightIncreaseBtn = new Rectangle(startX + btnSize + btnWidthDisplay + spacing, heightStartY, btnSize, btnSize);

            // Speed controls (only if movable) [−] [ 50 ] [+]
            int speedStartY = heightStartY + btnSize + 20;
            speedDecreaseBtn = new Rectangle(startX, speedStartY, btnSize, btnSize);
            speedIncreaseBtn = new Rectangle(startX + btnSize + btnWidthDisplay + spacing, speedStartY, btnSize, btnSize);
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
            backButton?.Update(stateManager, ignoreBackKey: isNameInputFocused);
            if (stateManager.CurrentState != GameState.CreateObstacle)
                return;

            var mouse = inputManager != null ? inputManager.Mouse : Mouse.GetState();
            var keyboard = inputManager != null ? inputManager.Keyboard : Keyboard.GetState();

            // Handle image import from EditorService
            HandleImageImport();

            // Handle name input
            HandleNameInput(mouse, keyboard, gameTime);

            // Handle panel scroll
            HandlePanelScroll(mouse);

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

            addImageButton?.Update(mouse, previousMouse);
            saveButton?.Update(mouse, previousMouse);

            previousMouse = mouse;
            previousKeyboard = keyboard;
        }

        private void UpdateMovement(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float distance = obstacleData.MovementSpeed * elapsed * movementDirection;
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
                // Only interact with left panel
                if (mouse.X >= panelWidth)
                    return;

                // Check mortal checkbox
                if (CheckboxContainsPoint(mortalCheckbox.Bounds, mouse.Position))
                {
                    obstacleData.IsMortal = !obstacleData.IsMortal;
                    mortalCheckbox.IsChecked = obstacleData.IsMortal;
                }

                // Check movable checkbox
                if (CheckboxContainsPoint(movableCheckbox.Bounds, mouse.Position))
                {
                    obstacleData.IsMovable = !obstacleData.IsMovable;
                    movableCheckbox.IsChecked = obstacleData.IsMovable;
                    
                    if (obstacleData.IsMovable)
                    {
                        // Set default to Horizontal when enabling movement
                        obstacleData.MovementType = MovementType.Horizontal;
                        obstacleData.IsLooping = true;
                        movementOffset = -obstacleData.MovementDistance; // Start from beginning
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
                if (obstacleData.IsMovable && CheckboxContainsPoint(horizontalCheckbox.Bounds, mouse.Position))
                {
                    obstacleData.MovementType = obstacleData.MovementType == MovementType.Horizontal 
                        ? MovementType.None 
                        : MovementType.Horizontal;
                    horizontalCheckbox.IsChecked = obstacleData.MovementType == MovementType.Horizontal;
                    movementOffset = -obstacleData.MovementDistance; // Start from beginning
                    movementDirection = 1f;
                }

                // Check vertical checkbox (only if movable)
                if (obstacleData.IsMovable && CheckboxContainsPoint(verticalCheckbox.Bounds, mouse.Position))
                {
                    obstacleData.MovementType = obstacleData.MovementType == MovementType.Vertical 
                        ? MovementType.None 
                        : MovementType.Vertical;
                    verticalCheckbox.IsChecked = obstacleData.MovementType == MovementType.Vertical;
                    movementOffset = -obstacleData.MovementDistance; // Start from beginning
                    movementDirection = 1f;
                }

                // Check looping checkbox (only if movement is selected)
                if (obstacleData.MovementType != MovementType.None && CheckboxContainsPoint(loopingCheckbox.Bounds, mouse.Position))
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
                // Only interact with left panel
                if (mouse.X >= panelWidth)
                    return;

                int sizeStep = 10;

                // Width decrease button
                if (ButtonContainsPoint(widthDecreaseBtn, mouse.Position))
                {
                    obstacleData.Width = Math.Max(20, obstacleData.Width - sizeStep);
                    UpdatePreviewBounds();
                }

                // Width increase button
                if (ButtonContainsPoint(widthIncreaseBtn, mouse.Position))
                {
                    obstacleData.Width = Math.Min(300, obstacleData.Width + sizeStep);
                    UpdatePreviewBounds();
                }

                // Height decrease button
                if (ButtonContainsPoint(heightDecreaseBtn, mouse.Position))
                {
                    obstacleData.Height = Math.Max(20, obstacleData.Height - sizeStep);
                    UpdatePreviewBounds();
                }

                // Height increase button
                if (ButtonContainsPoint(heightIncreaseBtn, mouse.Position))
                {
                    obstacleData.Height = Math.Min(300, obstacleData.Height + sizeStep);
                    UpdatePreviewBounds();
                }

                // Speed decrease button (only if movable)
                if (obstacleData.IsMovable && ButtonContainsPoint(speedDecreaseBtn, mouse.Position))
                {
                    obstacleData.MovementSpeed = Math.Max(10f, obstacleData.MovementSpeed - 10f);
                }

                // Speed increase button (only if movable)
                if (obstacleData.IsMovable && ButtonContainsPoint(speedIncreaseBtn, mouse.Position))
                {
                    obstacleData.MovementSpeed = Math.Min(500f, obstacleData.MovementSpeed + 10f);
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

        private void HandlePanelScroll(MouseState mouse)
        {
            // Only scroll if mouse is over the left panel
            if (mouse.X >= panelWidth)
                return;

            int wheelDelta = mouse.ScrollWheelValue - previousMouse.ScrollWheelValue;
            if (wheelDelta == 0)
                return;

            panelScroll -= wheelDelta * ScrollSpeed;

            // Clamp scroll to valid range
            // Maximum content height is approximately 450 pixels
            float maxScroll = Math.Max(0, 450 - screenHeight + 100);
            panelScroll = MathHelper.Clamp(panelScroll, 0, maxScroll);
        }

        private bool CheckboxContainsPoint(Rectangle bounds, Point point)
        {
            return bounds.Contains(new Point(point.X, (int)(point.Y + panelScroll)));
        }

        private bool ButtonContainsPoint(Rectangle bounds, Point point)
        {
            return bounds.Contains(new Point(point.X, (int)(point.Y + panelScroll)));
        }

        private bool IsKeyPressed(Keys key, KeyboardState current)
        {
            return current.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
        }

        private void SaveObstacle()
        {
            if (customObstacleService != null)
            {
                // Save to custom obstacle library
                customObstacleService.SaveObstacle(obstacleData);
                saveStatusMessage = "Obstacle saved!";
                saveStatusExpiresAtUtc = System.DateTime.UtcNow.AddSeconds(2);
            }

            onSaveCallback?.Invoke(obstacleData);
        }

        private void AddImage()
        {
            if (editorService != null)
            {
                editorService.BeginPickImage();
            }
        }

        private void HandleNameInput(MouseState mouse, KeyboardState keyboard, GameTime gameTime)
        {
            // Handle click on name input field
            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                if (mouse.X >= panelWidth)
                    return;

                var adjustedNameInputBounds = new Rectangle(
                    nameInputBounds.X,
                    (int)(nameInputBounds.Y - panelScroll),
                    nameInputBounds.Width,
                    nameInputBounds.Height
                );

                if (adjustedNameInputBounds.Contains(mouse.Position))
                {
                    isNameInputFocused = true;
                    nameInputBlinkTime = 0;
                    return;
                }
                else
                {
                    isNameInputFocused = false;
                }
            }

            // Handle text input when focused
            if (isNameInputFocused)
            {
                // Update blink time for cursor
                if (gameTime != null)
                {
                    nameInputBlinkTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (nameInputBlinkTime > 500)
                        nameInputBlinkTime = 0;
                }

                // Handle backspace
                if (IsKeyPressed(Keys.Back, keyboard) && editingName.Length > 0)
                {
                    editingName = editingName.Substring(0, editingName.Length - 1);
                    obstacleData.Name = editingName;
                }

                // Handle delete
                if (IsKeyPressed(Keys.Delete, keyboard))
                {
                    editingName = "";
                    obstacleData.Name = editingName;
                }

                // Handle text input (letters, numbers, spaces, and some symbols)
                foreach (Keys key in keyboard.GetPressedKeys())
                {
                    if (!previousKeyboard.IsKeyDown(key))
                    {
                        char? character = null;

                        // Letters
                        if (key >= Keys.A && key <= Keys.Z)
                        {
                            character = (char)('A' + (int)key - (int)Keys.A);
                            if (keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift))
                                character = char.ToUpper(character.Value);
                            else
                                character = char.ToLower(character.Value);
                        }
                        // Numbers
                        else if (key >= Keys.D0 && key <= Keys.D9)
                        {
                            character = (char)('0' + (int)key - (int)Keys.D0);
                        }
                        // Space
                        else if (key == Keys.Space)
                        {
                            character = ' ';
                        }
                        // Common symbols
                        else if (key == Keys.OemMinus)
                        {
                            character = '-';
                        }

                        if (character.HasValue && editingName.Length < 30)
                        {
                            editingName += character.Value;
                            obstacleData.Name = editingName;
                        }
                    }
                }

                // Handle Enter to confirm
                if (IsKeyPressed(Keys.Enter, keyboard))
                {
                    isNameInputFocused = false;
                    if (string.IsNullOrWhiteSpace(editingName))
                    {
                        editingName = "New Obstacle";
                        obstacleData.Name = editingName;
                    }
                }
            }
            else
            {
                // Sync editing name with obstacle data when not focused
                editingName = string.IsNullOrWhiteSpace(obstacleData.Name) ? "New Obstacle" : obstacleData.Name;
            }
        }

        private void HandleImageImport()
        {
            if (editorService == null)
                return;

            // Check if user picked an image
            if (editorService.TryConsumePickedImage(out string pickedPath))
            {
                editorService.BeginImportImage(pickedPath);
            }

            // Check if image was imported
            if (editorService.TryConsumeImportedImage(out string importedPath))
            {
                obstacleData.ImagePath = importedPath;
                
                if (assetManager != null)
                {
                    obstacleImage = assetManager.LoadTextureFromFile(importedPath);
                }
            }
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

            // Set up scissor rect to clip content to panel area
            var previousScissorRect = game.GraphicsDevice.ScissorRectangle;
            game.GraphicsDevice.ScissorRectangle = panelRect;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, new RasterizerState() { ScissorTestEnable = true });

            // Title (not scrolled)
            spriteBatch.DrawString(font, "Editar Obstaculo", new Vector2(Margin, Margin + 20 - panelScroll), Color.White);

            // Draw size controls with scroll
            DrawSizeControlsScrolled(spriteBatch);

            // Checkboxes with scroll
            DrawCheckboxScrolled(spriteBatch, mortalCheckbox);
            DrawCheckboxScrolled(spriteBatch, movableCheckbox);

            // Movement options (only if movable)
            if (obstacleData.IsMovable)
            {
                spriteBatch.DrawString(font, "Movimento:", new Vector2(Margin + 30, 320 - panelScroll), Color.LightGray);
                DrawCheckboxScrolled(spriteBatch, horizontalCheckbox);
                DrawCheckboxScrolled(spriteBatch, verticalCheckbox);

                // Looping option (only if movement type is selected)
                if (obstacleData.MovementType != MovementType.None)
                {
                    spriteBatch.DrawString(font, "Comportamento:", new Vector2(Margin + 30, 420 - panelScroll), Color.LightGray);
                    DrawCheckboxScrolled(spriteBatch, loopingCheckbox);
                }
            }

            // Size display with scroll
            //var sizeText = $"W: {obstacleData.Width}  H: {obstacleData.Height}";
            //spriteBatch.DrawString(font, sizeText, new Vector2(Margin, 260 - panelScroll), Color.White);

            spriteBatch.End();
            spriteBatch.Begin();

            // Restore scissor rect
            game.GraphicsDevice.ScissorRectangle = previousScissorRect;

            // Status message (if visible)
            if (!string.IsNullOrWhiteSpace(saveStatusMessage) && System.DateTime.UtcNow < saveStatusExpiresAtUtc)
            {
                spriteBatch.DrawString(font, saveStatusMessage, new Vector2(Margin, screenHeight - 135), Color.LimeGreen);
            }

            // Add image button (not scrolled, stays at bottom)
            addImageButton?.Draw(spriteBatch, font, pixel, Color.DarkSlateGray, Color.White);

            // Save button (not scrolled, stays at bottom)
            saveButton?.Draw(spriteBatch, font, pixel, Color.DarkSlateGray, Color.White);
        }

        private void DrawCheckboxScrolled(SpriteBatch spriteBatch, CheckboxState checkbox)
        {
            var scrolledBounds = new Rectangle(
                checkbox.Bounds.X,
                (int)(checkbox.Bounds.Y - panelScroll),
                checkbox.Bounds.Width,
                checkbox.Bounds.Height
            );

            // Draw checkbox border
            spriteBatch.Draw(pixel, scrolledBounds, Color.White);

            // Draw checkbox fill if checked
            if (checkbox.IsChecked)
            {
                var innerRect = new Rectangle(
                    scrolledBounds.X + 3,
                    scrolledBounds.Y + 3,
                    scrolledBounds.Width - 6,
                    scrolledBounds.Height - 6
                );
                spriteBatch.Draw(pixel, innerRect, Color.LimeGreen);

                // Draw X mark
                DrawX(spriteBatch, scrolledBounds);
            }

            // Draw label
            spriteBatch.DrawString(font, checkbox.Label, 
                new Vector2(scrolledBounds.Right + 15, scrolledBounds.Y + 3), 
                Color.White);
        }

        private void DrawSizeControlsScrolled(SpriteBatch spriteBatch)
        {
            int spacing = 5;

            // Draw name input field
            DrawNameInput(spriteBatch);

            // Width label and controls
            spriteBatch.DrawString(font, "Largura:", new Vector2(Margin, 90 - panelScroll), Color.LightGray);
            
            // Width buttons: [-] [value] [+]
            var widthDecreaseBtnScrolled = new Rectangle(widthDecreaseBtn.X, (int)(widthDecreaseBtn.Y - panelScroll), widthDecreaseBtn.Width, widthDecreaseBtn.Height);
            var widthIncreaseBtnScrolled = new Rectangle(widthIncreaseBtn.X, (int)(widthIncreaseBtn.Y - panelScroll), widthIncreaseBtn.Width, widthIncreaseBtn.Height);
            
            DrawButton(spriteBatch, widthDecreaseBtnScrolled, "-", Color.Firebrick);
            var widthValuePos = new Vector2(
                widthDecreaseBtnScrolled.X + widthDecreaseBtnScrolled.Width + spacing,
                widthDecreaseBtnScrolled.Y + (widthDecreaseBtnScrolled.Height - font.LineSpacing) / 2
            );
            spriteBatch.DrawString(font, obstacleData.Width.ToString(), widthValuePos, Color.Yellow);
            DrawButton(spriteBatch, widthIncreaseBtnScrolled, "+", Color.Green);

            // Height label and controls
            spriteBatch.DrawString(font, "Altura:", new Vector2(Margin, heightDecreaseBtn.Y - panelScroll - 25), Color.LightGray);
            
            // Height buttons: [-] [value] [+]
            var heightDecreaseBtnScrolled = new Rectangle(heightDecreaseBtn.X, (int)(heightDecreaseBtn.Y - panelScroll), heightDecreaseBtn.Width, heightDecreaseBtn.Height);
            var heightIncreaseBtnScrolled = new Rectangle(heightIncreaseBtn.X, (int)(heightIncreaseBtn.Y - panelScroll), heightIncreaseBtn.Width, heightIncreaseBtn.Height);
            
            DrawButton(spriteBatch, heightDecreaseBtnScrolled, "-", Color.Firebrick);
            var heightValuePos = new Vector2(
                heightDecreaseBtnScrolled.X + heightDecreaseBtnScrolled.Width + spacing,
                heightDecreaseBtnScrolled.Y + (heightDecreaseBtnScrolled.Height - font.LineSpacing) / 2
            );
            spriteBatch.DrawString(font, obstacleData.Height.ToString(), heightValuePos, Color.Yellow);
            DrawButton(spriteBatch, heightIncreaseBtnScrolled, "+", Color.Green);

            // Speed label and controls (only if movable)
            if (obstacleData.IsMovable)
            {
                spriteBatch.DrawString(font, "Velocidade:", new Vector2(Margin, speedDecreaseBtn.Y - panelScroll - 25), Color.LightGray);
                
                // Speed buttons: [-] [value] [+]
                var speedDecreaseBtnScrolled = new Rectangle(speedDecreaseBtn.X, (int)(speedDecreaseBtn.Y - panelScroll), speedDecreaseBtn.Width, speedDecreaseBtn.Height);
                var speedIncreaseBtnScrolled = new Rectangle(speedIncreaseBtn.X, (int)(speedIncreaseBtn.Y - panelScroll), speedIncreaseBtn.Width, speedIncreaseBtn.Height);
                
                DrawButton(spriteBatch, speedDecreaseBtnScrolled, "-", Color.Firebrick);
                var speedValuePos = new Vector2(
                    speedDecreaseBtnScrolled.X + speedDecreaseBtnScrolled.Width + spacing,
                    speedDecreaseBtnScrolled.Y + (speedDecreaseBtnScrolled.Height - font.LineSpacing) / 2
                );
                spriteBatch.DrawString(font, ((int)obstacleData.MovementSpeed).ToString(), speedValuePos, Color.Yellow);
                DrawButton(spriteBatch, speedIncreaseBtnScrolled, "+", Color.Green);
            }
        }

        private void DrawNameInput(SpriteBatch spriteBatch)
        {
            // Label
            spriteBatch.DrawString(font, "Nome:", new Vector2(Margin, nameInputBounds.Y - 20 - panelScroll), Color.LightGray);

            // Draw input field background
            var scrolledNameInputBounds = new Rectangle(
                nameInputBounds.X,
                (int)(nameInputBounds.Y - panelScroll),
                nameInputBounds.Width,
                nameInputBounds.Height
            );

            // Draw background
            Color backgroundColor = isNameInputFocused ? Color.DarkCyan : Color.DarkSlateGray;
            spriteBatch.Draw(pixel, scrolledNameInputBounds, backgroundColor);

            // Draw border
            Color borderColor = isNameInputFocused ? Color.Cyan : Color.White;
            spriteBatch.Draw(pixel, new Rectangle(scrolledNameInputBounds.X, scrolledNameInputBounds.Y, scrolledNameInputBounds.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(scrolledNameInputBounds.X, scrolledNameInputBounds.Bottom - 2, scrolledNameInputBounds.Width, 2), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(scrolledNameInputBounds.X, scrolledNameInputBounds.Y, 2, scrolledNameInputBounds.Height), borderColor);
            spriteBatch.Draw(pixel, new Rectangle(scrolledNameInputBounds.Right - 2, scrolledNameInputBounds.Y, 2, scrolledNameInputBounds.Height), borderColor);

            // Draw text
            string displayText = string.IsNullOrWhiteSpace(editingName) ? "New Obstacle" : editingName;
            var textSize = font.MeasureString(displayText);
            var textPos = new Vector2(
                scrolledNameInputBounds.X + 5,
                scrolledNameInputBounds.Y + (scrolledNameInputBounds.Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(font, displayText, textPos, Color.White);

            // Draw cursor if focused
            if (isNameInputFocused && nameInputBlinkTime < 250)
            {
                var cursorX = textPos.X + textSize.X + 3;
                var cursorRect = new Rectangle(
                    (int)cursorX,
                    scrolledNameInputBounds.Y + 3,
                    2,
                    scrolledNameInputBounds.Height - 6
                );
                spriteBatch.Draw(pixel, cursorRect, Color.Cyan);
            }
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

            // Draw image if available, otherwise draw colored rectangle
            if (obstacleImage != null)
            {
                // Draw the image scaled to fit the obstacle bounds
                spriteBatch.Draw(obstacleImage, animatedObstacleBounds, Color.White);
            }
            else
            {
                // Draw obstacle preview rectangle
                Color obstacleColor = obstacleData.IsMortal ? Color.Red : Color.Green;
                float colorAlpha = 0.7f;

                spriteBatch.Draw(pixel, animatedObstacleBounds, obstacleColor * colorAlpha);
            }

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
