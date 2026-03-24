using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Utils;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.UI.Components;
using System.Runtime.InteropServices;

namespace GameDuMouse.GameMain.UI
{
    public class CreateMappingScreen
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;

        // Constants to replace magic numbers
        private const int PanelFraction = 4;
        private const int PaletteItemHeight = 60;
        private const int Margin = 10;
        private const int ScrollPadding = 30;
        private const int TextureSize = 32;
        private const float ScrollSpeed = 0.5f;
        private const float LeftScrollSpeed = 0.01f;
        private const float ColliderAlpha = 0.6f;
        private const float PlacedAlpha = 0.4f;
        private const float PanelAlpha = 0.7f;
        private const int BackgroundShift = -600;
        private const int BackgroundDrawOffsetX = 200;
        private const int RightWallX = 590;
        private const int TextOffsetY = 8;
        private const int RightWallAdjust = 0;
        private const int WallThickness = 10;
        private const int CeilingHeight = 10;
        private const int BackgroundDeleteButtonSize = 28;

        private Game game;
        private SpriteFont font;
        private BackButton backButton;
        private UiActionButton saveButton;
        private UiActionButton previewButton;
        private Texture2D defaultBackgroundTexture;
        private Texture2D pixel;

        private List<Texture2D> obstacleTextures = new List<Texture2D>();
        private List<string> obstacleTextureNames = new List<string>();
        private List<PlacedObstacle> placed = new List<PlacedObstacle>();

        private int panelWidth => game.GraphicsDevice.Viewport.Width / PanelFraction;
        private int screenWidth;
        private int screenHeight;

        private float leftScroll = 0f;
        private int selectedPaletteIndex = -1;
        private float mapCameraOffsetX = 0f;

        private MouseState previousMouse;
        private KeyboardState previousKeyboard;
        private DirectInputController directController;

        private PlacedObstacle dragging;
        private Point dragOffset;

        // CreateMappingScreen's own ground colliders
        private Rectangle groundCollider;
        private List<Rectangle> GroundColliders;

        // Import button (world coordinates)
        private Point importButtonWorldPos;
        private int importButtonSize = 50;
        private Texture2D customBackground;
        private int phaseWidth = 590; // default
        private string pendingImagePath = null;
        private bool isDialogOpen = false;
        private bool isCustomBackgroundLoaded = false; // Track if we've loaded a custom background
        private string saveStatusMessage = "";
        private bool requestPreview = false;
        private bool isConfirmingBackgroundDelete = false;
        private int pendingDeleteLayerIndex = -1;
        private List<int> pendingDeleteObstacleIndices = new List<int>();
        private UiButton confirmYesButton;
        private UiButton confirmNoButton;

        // Multiple backgrounds support
        private struct BackgroundLayer
        {
            public Texture2D texture;
            public int startX; // Position where this background starts
            public string sourcePath;
        }
        private List<BackgroundLayer> backgroundLayers = new List<BackgroundLayer>();

        private class PlacedObstacle
        {
            public Texture2D Texture;
            public Rectangle Bounds;
            public int PaletteIndex;
            public string TextureName;
        }

        public CreateMappingScreen(Game game)
        {
            this.game = game;
            directController = new DirectInputController();
            previousMouse = Mouse.GetState();
            previousKeyboard = Keyboard.GetState();
            
            // Initialize CreateMappingScreen's own ground colliders
            groundCollider = new Rectangle(0, 400, 600, 5);
            //groundCollider2 = new Rectangle(3000, 400, 2700, 5);
            GroundColliders = new List<Rectangle> { groundCollider };
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Arial");
            backButton = new BackButton(font);
            pixel = new Texture2D(game.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            
            screenWidth = game.GraphicsDevice.Viewport.Width;
            screenHeight = game.GraphicsDevice.Viewport.Height;

            // Initialize camera offset
            mapCameraOffsetX = 0;

            // Create default background texture 
            defaultBackgroundTexture = CreateSolidTexture(Color.CornflowerBlue, 500, screenHeight);

            // Try to load available obstacle textures; fall back to colored placeholders
            try
            {
                var t1 = content.Load<Texture2D>("Content/Windows/JOGO_DO_RATO");
                obstacleTextures.Add(t1);
                obstacleTextureNames.Add("Content/Windows/JOGO_DO_RATO");
            }
            catch
            {
                // create simple colored placeholders
                obstacleTextures.Add(CreateSolidTexture(Color.SandyBrown));
                obstacleTextures.Add(CreateSolidTexture(Color.DarkGray));
                obstacleTextures.Add(CreateSolidTexture(Color.Olive));
                obstacleTextureNames.Add("Placeholder_SandyBrown");
                obstacleTextureNames.Add("Placeholder_DarkGray");
                obstacleTextureNames.Add("Placeholder_Olive");
            }

            // Initialize colliders based on default phaseWidth
            UpdateCollidersForBackground();

            UpdateImportButtonPosition();

            var saveButtonBounds = new Rectangle(Margin, screenHeight - 60, panelWidth - Margin * 2, 40);
            saveButton = new UiActionButton(saveButtonBounds, "Salvar Mapa", SaveCurrentMap);
            var previewButtonBounds = new Rectangle(Margin, screenHeight - 110, panelWidth - Margin * 2, 40);
            previewButton = new UiActionButton(previewButtonBounds, "Visualizar", () => requestPreview = true);

            confirmYesButton = new UiButton(new Rectangle(0, 0, 120, 40), "SIM");
            confirmNoButton = new UiButton(new Rectangle(0, 0, 120, 40), "NAO");
        }

        public void LoadMapForEditing(string mapName)
        {
            // reset editor state
            dragging = null;
            selectedPaletteIndex = -1;
            leftScroll = 0f;
            mapCameraOffsetX = 0f;
            placed.Clear();
            backgroundLayers.Clear();
            customBackground = null;
            isCustomBackgroundLoaded = false;
            pendingImagePath = null;
            saveStatusMessage = "";
            requestPreview = false;

            var data = MapDataManager.LoadByName(mapName);
            if (data == null)
            {
                phaseWidth = RightWallX;
                UpdateCollidersForBackground();
                UpdateImportButtonPosition();
                return;
            }

            phaseWidth = data.PhaseWidth > 0 ? data.PhaseWidth : RightWallX;

            if (data.IsCustomBackgroundLoaded && data.BackgroundLayers != null && data.BackgroundLayers.Count > 0)
            {
                foreach (var layer in data.BackgroundLayers)
                {
                    Texture2D tex = null;
                    if (!string.IsNullOrWhiteSpace(layer.ImagePath) && System.IO.File.Exists(layer.ImagePath))
                        tex = Texture2D.FromFile(game.GraphicsDevice, layer.ImagePath);

                    if (tex != null)
                    {
                        if (customBackground == null)
                            customBackground = tex;

                        backgroundLayers.Add(new BackgroundLayer
                        {
                            texture = tex,
                            startX = layer.StartX,
                            sourcePath = layer.ImagePath
                        });
                    }
                }

                if (backgroundLayers.Count > 0)
                    isCustomBackgroundLoaded = true;
            }

            if (phaseWidth <= 0 && data.BackgroundLayers != null && data.BackgroundLayers.Count > 0)
            {
                int maxRight = 0;
                foreach (var layer in data.BackgroundLayers)
                {
                    int right = layer.StartX + layer.Width;
                    if (right > maxRight)
                        maxRight = right;
                }
                if (maxRight > 0)
                    phaseWidth = maxRight;
            }

            GroundColliders.Clear();
            if (data.Colliders != null)
            {
                foreach (var collider in data.Colliders)
                {
                    if (collider?.Type == "Ground")
                    {
                        var b = collider.Bounds;
                        GroundColliders.Add(new Rectangle(b.X, b.Y, b.Width, b.Height));
                    }
                }
            }

            if (GroundColliders.Count == 0)
                UpdateCollidersForBackground();

            if (data.Obstacles != null)
            {
                foreach (var obstacle in data.Obstacles)
                {
                    if (obstacle?.Bounds == null)
                        continue;

                    int texIndex = obstacle.TextureIndex;
                    if (texIndex < 0 || texIndex >= obstacleTextures.Count)
                        texIndex = obstacleTextureNames.IndexOf(obstacle.TextureName);
                    if (texIndex < 0 || texIndex >= obstacleTextures.Count)
                        texIndex = 0;

                    placed.Add(new PlacedObstacle
                    {
                        Texture = obstacleTextures[texIndex],
                        Bounds = new Rectangle(obstacle.Bounds.X, obstacle.Bounds.Y, obstacle.Bounds.Width, obstacle.Bounds.Height),
                        PaletteIndex = texIndex,
                        TextureName = texIndex >= 0 && texIndex < obstacleTextureNames.Count
                            ? obstacleTextureNames[texIndex]
                            : "Unknown"
                    });
                }
            }

            UpdateImportButtonPosition();
        }

        private void UpdateCollidersForBackground()
        {
            // Update groundCollider width to match background width
            groundCollider = new Rectangle(0, 400, phaseWidth, 5);
            GroundColliders = new List<Rectangle> { groundCollider };
        }

        private Texture2D CreateSolidTexture(Color c)
        {
            var tx = new Texture2D(game.GraphicsDevice, TextureSize, TextureSize);
            var data = new Color[TextureSize * TextureSize];
            for (int i = 0; i < data.Length; i++) data[i] = c;
            tx.SetData(data);
            return tx;
        }

        private Texture2D CreateSolidTexture(Color c, int width, int height)
        {
            var tx = new Texture2D(game.GraphicsDevice, width, height);
            var data = new Color[width * height];
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

            if (isConfirmingBackgroundDelete)
            {
                HandleBackgroundDeleteConfirmation(mouse);
                previousMouse = mouse;
                previousKeyboard = keyboard;
                return;
            }

            if (HandleBackgroundDeleteButtons(mouse))
            {
                previousMouse = mouse;
                previousKeyboard = keyboard;
                return;
            }

            // Check import button click
            var buttonScreenPos = WorldToScreen(importButtonWorldPos);
            var importButtonScreenRect = new Rectangle(buttonScreenPos.X, buttonScreenPos.Y, importButtonSize, importButtonSize);
            if (mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && previousMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && importButtonScreenRect.Contains(mouse.Position) && !isDialogOpen)
            {
                System.Console.WriteLine("Import button clicked!");
                // launch external dialog via PowerShell
                OpenFileDialogAsync();
            }

            // Check if there's a pending image to load
            if (pendingImagePath != null)
            {
                System.Console.WriteLine("Loading pending image: " + pendingImagePath);
                try
                {
                    Texture2D newBackground = Texture2D.FromFile(game.GraphicsDevice, pendingImagePath);
                    
                    if (!isCustomBackgroundLoaded)
                    {
                        // First custom background: replace the default
                        System.Console.WriteLine("Loading first custom background");
                        customBackground = newBackground;
                        backgroundLayers.Clear();
                        backgroundLayers.Add(new BackgroundLayer { texture = newBackground, startX = 0, sourcePath = pendingImagePath });
                        phaseWidth = newBackground.Width;
                        isCustomBackgroundLoaded = true;
                        mapCameraOffsetX = 0; // Reset camera to start of image
                    }
                    else
                    {
                        // Already have a custom background: add new layer at the end
                        System.Console.WriteLine("Adding additional background layer");
                        int newBackgroundStartX = phaseWidth;
                        backgroundLayers.Add(new BackgroundLayer { texture = newBackground, startX = newBackgroundStartX, sourcePath = pendingImagePath });
                        phaseWidth += newBackground.Width;
                    }
                    
                    // Update colliders to match new total background width
                    UpdateCollidersForBackground();
                    
                    // Recalculate button position based on new total background width
                    UpdateImportButtonPosition();
                    
                    System.Console.WriteLine("Background loaded successfully, total width: " + phaseWidth);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Error loading image: " + ex.Message);
                }
                pendingImagePath = null; // Clear pending
            }

            HandleScroll(mouse);
            HandlePaletteSelection(mouse);
            HandlePlacingAndDragging(mouse);
            HandleDelete(mouse, keyboard);
            saveButton?.Update(mouse, previousMouse);
            previewButton?.Update(mouse, previousMouse);

            if (requestPreview)
            {
                SaveCurrentMap();
                stateManager.ChangeState(GameState.MappingTest);
                requestPreview = false;
                return;
            }

            previousMouse = mouse;
            previousKeyboard = keyboard;
        }

        private void HandleScroll(MouseState mouse)
        {
            int wheelDelta = mouse.ScrollWheelValue - previousMouse.ScrollWheelValue;
            if (wheelDelta != 0)
            {
                if (mouse.X >= panelWidth)
                {
                    mapCameraOffsetX += wheelDelta * ScrollSpeed;
                    int mapDisplayWidth = screenWidth - panelWidth;
                    float backgroundWidth = customBackground != null ? phaseWidth : phaseWidth;
                    float minOffset = 0;
                    float maxOffset = Math.Max(minOffset, backgroundWidth - mapDisplayWidth + 80);
                    mapCameraOffsetX = MathHelper.Clamp(mapCameraOffsetX, minOffset, maxOffset);
                }
                else
                {
                    leftScroll -= wheelDelta * LeftScrollSpeed;
                    leftScroll = MathHelper.Clamp(leftScroll, 0, Math.Max(0, (obstacleTextures.Count * PaletteItemHeight) - screenHeight + ScrollPadding));
                }
            }
        }

        private void HandlePaletteSelection(MouseState mouse)
        {
            if (mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed 
            && previousMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released 
            && mouse.X < panelWidth)
            {
                int index = (int)((mouse.Y + leftScroll - Margin) / PaletteItemHeight);
                if (index >= 0 && index < obstacleTextures.Count)
                    selectedPaletteIndex = index;
            }
        }

        // convert a point from screen coordinates to map/world coordinates
        private Point ScreenToWorld(Point screen)
        {
            int worldX = (int)(screen.X - panelWidth + mapCameraOffsetX);
            return new Point(worldX, screen.Y);
        }

        // convert a point in world coords back to screen for drawing/interactions
        private Point WorldToScreen(Point world)
        {
            int screenX = (int)(world.X - mapCameraOffsetX + panelWidth);
            return new Point(screenX, world.Y);
        }

        private void HandlePlacingAndDragging(MouseState mouse)
        {
            if (mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && previousMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                if (mouse.X >= panelWidth)      
                {
                    // convert the click to world coords for hit testing
                    var worldMouse = ScreenToWorld(mouse.Position);

                    // check if clicking an existing placed obstacle
                    for (int i = placed.Count - 1; i >= 0; i--)
                    {
                        if (placed[i].Bounds.Contains(worldMouse))
                        {
                            dragging = placed[i];
                            dragOffset = new Point(worldMouse.X - dragging.Bounds.X, worldMouse.Y - dragging.Bounds.Y);
                            break;
                        }
                    }

                    if (dragging == null && selectedPaletteIndex >= 0)
                    {
                        var tex = obstacleTextures[selectedPaletteIndex];
                        // create bounds in world coords centered under the mouse
                        var topLeft = new Point(worldMouse.X - tex.Width / 2, worldMouse.Y - tex.Height / 2);
                        var rect = new Rectangle(topLeft, new Point(tex.Width, tex.Height));
                        placed.Add(new PlacedObstacle
                        {
                            Texture = tex,
                            Bounds = rect,
                            PaletteIndex = selectedPaletteIndex,
                            TextureName = selectedPaletteIndex >= 0 && selectedPaletteIndex < obstacleTextureNames.Count
                                ? obstacleTextureNames[selectedPaletteIndex]
                                : "Unknown"
                        });
                    }
                }
            }

            // dragging
            if (mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && dragging != null)
            {
                var worldMouse = ScreenToWorld(mouse.Position);
                dragging.Bounds = new Rectangle(worldMouse.X - dragOffset.X, worldMouse.Y - dragOffset.Y, dragging.Bounds.Width, dragging.Bounds.Height);
            }

            // release drag
            if (mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && previousMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                dragging = null;
            }
        }

        private void HandleDelete(MouseState mouse, KeyboardState keyboard)
        {
            // delete selected object with Delete key
            if (IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Delete, keyboard))
            {
                var worldMouse = ScreenToWorld(mouse.Position);
                // remove last placed or any that contain mouse
                for (int i = placed.Count - 1; i >= 0; i--)
                {
                    if (placed[i].Bounds.Contains(worldMouse))
                    {
                        placed.RemoveAt(i);
                        break;
                    }
                }
            }

            // delete obstacle with right mouse button
            if (mouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && previousMouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released && mouse.X >= panelWidth)
            {
                var worldMouse = ScreenToWorld(mouse.Position);
                // remove obstacle under mouse cursor
                for (int i = placed.Count - 1; i >= 0; i--)
                {
                    if (placed[i].Bounds.Contains(worldMouse))
                    {
                        placed.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private bool HandleBackgroundDeleteButtons(MouseState mouse)
        {
            if (mouse.LeftButton != Microsoft.Xna.Framework.Input.ButtonState.Pressed ||
                previousMouse.LeftButton != Microsoft.Xna.Framework.Input.ButtonState.Released)
                return false;

            if (!isCustomBackgroundLoaded || backgroundLayers.Count == 0)
                return false;

            if (mouse.X < panelWidth)
                return false;

            for (int i = backgroundLayers.Count - 1; i >= 0; i--)
            {
                var rect = GetBackgroundDeleteButtonScreenRect(backgroundLayers[i]);
                if (rect.Contains(mouse.Position))
                {
                    StartBackgroundDelete(i);
                    return true;
                }
            }

            return false;
        }

        private Rectangle GetBackgroundDeleteButtonScreenRect(BackgroundLayer layer)
        {
            // Top-left corner of the layer, near the ceiling
            int worldX = layer.startX + 8;
            int worldY = CeilingHeight + 6;

            int screenX = worldX - (int)mapCameraOffsetX + BackgroundDrawOffsetX;
            int screenY = worldY;

            return new Rectangle(screenX, screenY, BackgroundDeleteButtonSize, BackgroundDeleteButtonSize);
        }

        private void StartBackgroundDelete(int layerIndex)
        {
            if (layerIndex < 0 || layerIndex >= backgroundLayers.Count)
                return;

            var layerRect = GetLayerWorldRect(backgroundLayers[layerIndex]);
            pendingDeleteObstacleIndices = GetObstaclesInLayer(layerRect);

            if (pendingDeleteObstacleIndices.Count == 0)
            {
                RemoveBackgroundLayerAt(layerIndex);
                return;
            }

            isConfirmingBackgroundDelete = true;
            pendingDeleteLayerIndex = layerIndex;
        }

        private void HandleBackgroundDeleteConfirmation(MouseState mouse)
        {
            UpdateConfirmButtonBounds();

            if (confirmYesButton.Update(mouse, previousMouse))
            {
                RemovePendingObstacles();
                RemoveBackgroundLayerAt(pendingDeleteLayerIndex);
                CloseBackgroundDeleteConfirmation();
                return;
            }

            if (confirmNoButton.Update(mouse, previousMouse))
            {
                RemoveBackgroundLayerAt(pendingDeleteLayerIndex);
                CloseBackgroundDeleteConfirmation();
            }
        }

        private void CloseBackgroundDeleteConfirmation()
        {
            isConfirmingBackgroundDelete = false;
            pendingDeleteLayerIndex = -1;
            pendingDeleteObstacleIndices.Clear();
        }

        private void RemovePendingObstacles()
        {
            if (pendingDeleteObstacleIndices == null || pendingDeleteObstacleIndices.Count == 0)
                return;

            pendingDeleteObstacleIndices.Sort();
            for (int i = pendingDeleteObstacleIndices.Count - 1; i >= 0; i--)
            {
                int index = pendingDeleteObstacleIndices[i];
                if (index >= 0 && index < placed.Count)
                    placed.RemoveAt(index);
            }
        }

        private Rectangle GetLayerWorldRect(BackgroundLayer layer)
        {
            return new Rectangle(layer.startX, 0, layer.texture.Width, screenHeight);
        }

        private List<int> GetObstaclesInLayer(Rectangle layerRect)
        {
            var indices = new List<int>();
            for (int i = 0; i < placed.Count; i++)
            {
                if (layerRect.Contains(placed[i].Bounds))
                    indices.Add(i);
            }
            return indices;
        }

        private void RemoveBackgroundLayerAt(int index)
        {
            if (index < 0 || index >= backgroundLayers.Count)
                return;

            var toRemove = backgroundLayers[index];
            backgroundLayers.RemoveAt(index);
            toRemove.texture?.Dispose();

            if (backgroundLayers.Count == 0)
            {
                isCustomBackgroundLoaded = false;
                customBackground = null;
                phaseWidth = RightWallX;
                UpdateCollidersForBackground();
                UpdateImportButtonPosition();
                mapCameraOffsetX = 0;
                return;
            }

            int currentX = 0;
            for (int i = 0; i < backgroundLayers.Count; i++)
            {
                var layer = backgroundLayers[i];
                layer.startX = currentX;
                backgroundLayers[i] = layer;
                currentX += layer.texture.Width;
            }

            phaseWidth = currentX;
            customBackground = backgroundLayers[0].texture;
            isCustomBackgroundLoaded = true;
            UpdateCollidersForBackground();
            UpdateImportButtonPosition();

            int mapDisplayWidth = screenWidth - panelWidth;
            float minOffset = 0;
            float maxOffset = Math.Max(minOffset, phaseWidth - mapDisplayWidth + 80);
            mapCameraOffsetX = MathHelper.Clamp(mapCameraOffsetX, minOffset, maxOffset);
        }

        private bool IsKeyPressed(Microsoft.Xna.Framework.Input.Keys key, KeyboardState current)
        {
            return current.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
        }

        private void OpenFileDialogAsync()
        {
            System.Console.WriteLine("Launching PowerShell file dialog...");
            isDialogOpen = true;

            // build PowerShell command script
            string psScript = @"Add-Type -AssemblyName System.Windows.Forms; 
                                $ofd = New-Object System.Windows.Forms.OpenFileDialog; 
                                $ofd.Filter = 'Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg'; 
                                if($ofd.ShowDialog() -eq 'OK'){ Write-Output $ofd.FileName }";

            var psi = new System.Diagnostics.ProcessStartInfo("powershell", "-NoProfile -Command " + psScript)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var proc = new System.Diagnostics.Process { StartInfo = psi, EnableRaisingEvents = true };
            proc.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    pendingImagePath = e.Data.Trim();
                    System.Console.WriteLine("PowerShell returned path: " + pendingImagePath);
                }
            };
            proc.Exited += (s, e) =>
            {
                isDialogOpen = false;
            };

            proc.Start();
            proc.BeginOutputReadLine();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawColliders(spriteBatch);
            DrawPanel(spriteBatch);
            DrawPalette(spriteBatch);
            DrawPlacedObstacles(spriteBatch);
            DrawImportButton(spriteBatch);
            backButton?.Draw(spriteBatch);
            DrawBackgroundDeleteConfirmation(spriteBatch);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            // Set up scissor rect to clip background to map area
            var mapArea = new Rectangle(panelWidth, 0, screenWidth - panelWidth, screenHeight);
            var previousScissorRect = game.GraphicsDevice.ScissorRectangle;
            game.GraphicsDevice.ScissorRectangle = mapArea;

            // Draw background with camera offset
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, new RasterizerState() { ScissorTestEnable = true });
            
            if (isCustomBackgroundLoaded && backgroundLayers.Count > 0)
            {
                // Draw multiple custom backgrounds in sequence
                foreach (var layer in backgroundLayers)
                {
                    spriteBatch.Draw(layer.texture, new Vector2(layer.startX - mapCameraOffsetX + BackgroundDrawOffsetX, 0), Color.White);
                }

                // Draw delete buttons centered on each background layer
                foreach (var layer in backgroundLayers)
                {
                    var rect = GetBackgroundDeleteButtonScreenRect(layer);
                    if (rect.Right < panelWidth || rect.Left > screenWidth)
                        continue;

                    spriteBatch.Draw(pixel, rect, Color.Black * 0.6f);
                    DrawTrashIcon(spriteBatch, rect, Color.White);
                }
            }
            else
            {
                // Draw default background (600px wide)
                spriteBatch.Draw(defaultBackgroundTexture, new Vector2(-mapCameraOffsetX, 0), Color.White);
            }
            
            spriteBatch.End();
            spriteBatch.Begin();

            // Restore scissor rect
            game.GraphicsDevice.ScissorRectangle = previousScissorRect;
        }

        private void DrawPanel(SpriteBatch spriteBatch)
        {
            
            // background panel
            var panelRect = new Rectangle(0, 0, panelWidth, screenHeight);
            Texture2D panelBg = CreateSolidTexture(Color.DarkSlateGray * PanelAlpha);
            spriteBatch.Draw(panelBg, panelRect, Color.White);

            // Show dialog status
            if (isDialogOpen)
            {
                Vector2 statusPos = new Vector2(Margin, screenHeight / 2 + Margin);
                spriteBatch.DrawString(font, "Opening file dialog...", statusPos, Color.Yellow);
            }

            // left panel
            spriteBatch.DrawString(font, "Obstaculos", new Vector2(Margin, Margin), Color.White);

            previewButton?.Draw(spriteBatch, font, pixel, Color.DarkSlateGray, Color.White);
            saveButton?.Draw(spriteBatch, font, pixel, Color.DarkSlateGray, Color.White);
            if (!string.IsNullOrWhiteSpace(saveStatusMessage))
                spriteBatch.DrawString(font, saveStatusMessage, new Vector2(Margin, screenHeight - 90), Color.Yellow);
        }

        private void DrawBackgroundDeleteConfirmation(SpriteBatch spriteBatch)
        {
            if (!isConfirmingBackgroundDelete)
                return;

            UpdateConfirmButtonBounds();

            var overlay = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(pixel, overlay, Color.Black * 0.6f);

            int dialogWidth = 460;
            int dialogHeight = 160;
            var dialogRect = new Rectangle(
                (screenWidth - dialogWidth) / 2,
                (screenHeight - dialogHeight) / 2,
                dialogWidth,
                dialogHeight);

            spriteBatch.Draw(pixel, dialogRect, Color.DarkSlateGray);

            string msg = "Deseja remover os obstaculos tambem?";
            var msgSize = font.MeasureString(msg);
            var msgPos = new Vector2(
                dialogRect.X + (dialogRect.Width - msgSize.X) / 2f,
                dialogRect.Y + 20);
            spriteBatch.DrawString(font, msg, msgPos, Color.White);

            confirmYesButton.Draw(spriteBatch, font, pixel, Color.ForestGreen, Color.White);
            confirmNoButton.Draw(spriteBatch, font, pixel, Color.Firebrick, Color.White);
        }

        private void UpdateConfirmButtonBounds()
        {
            int dialogWidth = 460;
            int dialogHeight = 160;
            int dialogX = (screenWidth - dialogWidth) / 2;
            int dialogY = (screenHeight - dialogHeight) / 2;

            int buttonWidth = 120;
            int buttonHeight = 40;
            int gap = 20;
            int totalButtonsWidth = buttonWidth * 2 + gap;
            int buttonsX = dialogX + (dialogWidth - totalButtonsWidth) / 2;
            int buttonsY = dialogY + dialogHeight - 60;

            confirmYesButton.SetBounds(new Rectangle(buttonsX, buttonsY, buttonWidth, buttonHeight));
            confirmNoButton.SetBounds(new Rectangle(buttonsX + buttonWidth + gap, buttonsY, buttonWidth, buttonHeight));
        }

        private void DrawTrashIcon(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            int thickness = Math.Max(1, rect.Width / 10);
            int padding = Math.Max(2, rect.Width / 6);

            int lidHeight = Math.Max(2, rect.Height / 6);
            int bodyTop = rect.Y + padding + lidHeight;
            int bodyBottom = rect.Bottom - padding;
            int bodyLeft = rect.X + padding;
            int bodyRight = rect.Right - padding;

            // Lid
            var lidRect = new Rectangle(bodyLeft, rect.Y + padding, bodyRight - bodyLeft, lidHeight);
            spriteBatch.Draw(pixel, lidRect, color);

            // Handle
            int handleWidth = Math.Max(2, lidRect.Width / 3);
            int handleX = lidRect.X + (lidRect.Width - handleWidth) / 2;
            var handleRect = new Rectangle(handleX, lidRect.Y - Math.Max(1, lidHeight / 2), handleWidth, Math.Max(1, lidHeight / 2));
            spriteBatch.Draw(pixel, handleRect, color);

            // Body outline
            var left = new Rectangle(bodyLeft, bodyTop, thickness, bodyBottom - bodyTop);
            var right = new Rectangle(bodyRight - thickness, bodyTop, thickness, bodyBottom - bodyTop);
            var bottom = new Rectangle(bodyLeft, bodyBottom - thickness, bodyRight - bodyLeft, thickness);
            spriteBatch.Draw(pixel, left, color);
            spriteBatch.Draw(pixel, right, color);
            spriteBatch.Draw(pixel, bottom, color);

            // Inner lines
            int lineCount = 2;
            int gap = (bodyRight - bodyLeft) / (lineCount + 1);
            for (int i = 1; i <= lineCount; i++)
            {
                int x = bodyLeft + i * gap;
                var line = new Rectangle(x - thickness / 2, bodyTop + thickness, thickness, bodyBottom - bodyTop - thickness * 2);
                spriteBatch.Draw(pixel, line, color);
            }
        }

        private void DrawImportButton(SpriteBatch spriteBatch)
        {
            // Import button centered between colliders (follows camera offset)
            var buttonScreenPos = WorldToScreen(importButtonWorldPos);
            var buttonScreenRect = new Rectangle(buttonScreenPos.X, buttonScreenPos.Y, importButtonSize, importButtonSize);
            
            // Only draw if it's visible in the editing area
            if (buttonScreenRect.X >= panelWidth && buttonScreenRect.X < screenWidth)
            {
                Texture2D buttonBg = CreateSolidTexture(Color.Gray);
                spriteBatch.Draw(buttonBg, buttonScreenRect, Color.White);
                spriteBatch.DrawString(font, "+", new Vector2(buttonScreenRect.X + buttonScreenRect.Width / 2 - 5, buttonScreenRect.Y + buttonScreenRect.Height / 2 - 8), Color.White);
            }
        }

        private void DrawPalette(SpriteBatch spriteBatch)
        {
            // palette items
            for (int i = 0; i < obstacleTextures.Count; i++)
            {
                int y = Margin + i * PaletteItemHeight - (int)leftScroll + Margin;
                var thumb = obstacleTextures[i];
                spriteBatch.Draw(thumb, new Vector2(Margin, y), Color.White);
                Color c = (i == selectedPaletteIndex) ? Color.Yellow : Color.White;
                spriteBatch.DrawString(font, "Item " + (i + 1), new Vector2(50, y + TextOffsetY), c);
            }
        }

        private void DrawColliders(SpriteBatch spriteBatch)
        {
            int rightWallX = customBackground != null ? phaseWidth : RightWallX;

            // Draw map boundary colliders (left/right walls + ceiling)
            var leftWall = new Rectangle((int)(panelWidth - mapCameraOffsetX), 0, WallThickness, screenHeight - 75);
            // Only apply RightWallAdjust when there's no custom background
            int rightWallAdjust = customBackground != null ? 0 : RightWallAdjust;
            var rightWall = new Rectangle((int)(panelWidth + rightWallX - mapCameraOffsetX + rightWallAdjust ), 0, WallThickness, screenHeight - 75);
            var ceiling = new Rectangle((int)(panelWidth - mapCameraOffsetX), 0, rightWallX, CeilingHeight);
            var colliderTex = CreateSolidTexture(Color.Red * ColliderAlpha);
            spriteBatch.Draw(colliderTex, leftWall, Color.White);
            spriteBatch.Draw(colliderTex, rightWall, Color.White);
            spriteBatch.Draw(colliderTex, ceiling, Color.White);

            // Draw CreateMappingScreen's own ground colliders
            foreach (var gc in GroundColliders)
            {
                var r = new Rectangle(
                    (int)(panelWidth + gc.X - mapCameraOffsetX),
                    gc.Y,
                    gc.Width,
                    gc.Height);
                spriteBatch.Draw(colliderTex, r, Color.White);
            }
        }

        private void DrawPlacedObstacles(SpriteBatch spriteBatch)
        {
            // draw placed obstacles converting from world coords to screen coords
            foreach (var p in placed)
            {
                var topLeftScreen = WorldToScreen(p.Bounds.Location);
                var adjustedPos = new Vector2(topLeftScreen.X, topLeftScreen.Y);
                spriteBatch.Draw(p.Texture, adjustedPos, Color.White);
                var col = CreateSolidTexture(Color.Red * PlacedAlpha);
                var adjustedBounds = new Rectangle(topLeftScreen.X, topLeftScreen.Y, p.Bounds.Width, p.Bounds.Height);
                spriteBatch.Draw(col, adjustedBounds, Color.White);
            }
        }

        private void UpdateImportButtonPosition()
        {
            int centerWorldY = (screenHeight / 2) - (importButtonSize / 2) - 30;

            if (!isCustomBackgroundLoaded || backgroundLayers.Count == 0)
            {
                int leftWallX = 0;
                int rightWallX = RightWallX;
                int colliderAreaWidth = rightWallX - leftWallX;
                int centerWorldX = leftWallX + colliderAreaWidth / 2 - importButtonSize / 2;
                importButtonWorldPos = new Point(centerWorldX, centerWorldY);
            }
            else
            {
                importButtonWorldPos = new Point(phaseWidth + 20, centerWorldY);
            }
        }

        private void SaveCurrentMap()
        {
            string mapName = MapListManager.CurrentMapName;
            if (string.IsNullOrWhiteSpace(mapName))
                mapName = "Mapa_Sem_Nome";

            int rightWallX = customBackground != null ? phaseWidth : RightWallX;

            var data = new MapData
            {
                MapName = mapName,
                PhaseWidth = phaseWidth,
                ScreenHeight = screenHeight,
                IsCustomBackgroundLoaded = isCustomBackgroundLoaded,
                DefaultBackgroundColor = "CornflowerBlue",
                DefaultBackgroundWidth = defaultBackgroundTexture?.Width ?? 0,
                DefaultBackgroundHeight = defaultBackgroundTexture?.Height ?? 0,
                CreatedAtUtc = System.DateTime.UtcNow.ToString("o")
            };

            if (isCustomBackgroundLoaded && backgroundLayers.Count > 0)
            {
                foreach (var layer in backgroundLayers)
                {
                    data.BackgroundLayers.Add(new BackgroundLayerData
                    {
                        ImagePath = layer.sourcePath,
                        StartX = layer.startX,
                        Width = layer.texture.Width,
                        Height = layer.texture.Height
                    });
                }
            }

            data.Colliders.Add(new ColliderData
            {
                Name = "LeftWall",
                Type = "Wall",
                Bounds = new RectangleData { X = 0, Y = 0, Width = WallThickness, Height = screenHeight - 75 }
            });
            data.Colliders.Add(new ColliderData
            {
                Name = "RightWall",
                Type = "Wall",
                Bounds = new RectangleData { X = rightWallX, Y = 0, Width = WallThickness, Height = screenHeight - 75 }
            });
            data.Colliders.Add(new ColliderData
            {
                Name = "Ceiling",
                Type = "Ceiling",
                Bounds = new RectangleData { X = 0, Y = 0, Width = rightWallX, Height = CeilingHeight }
            });

            for (int i = 0; i < GroundColliders.Count; i++)
            {
                var gc = GroundColliders[i];
                data.Colliders.Add(new ColliderData
                {
                    Name = $"Ground_{i}",
                    Type = "Ground",
                    Bounds = new RectangleData { X = gc.X, Y = gc.Y, Width = gc.Width, Height = gc.Height }
                });
            }

            for (int i = 0; i < placed.Count; i++)
            {
                var p = placed[i];
                data.Obstacles.Add(new ObstacleData
                {
                    Name = $"Obstacle_{i}",
                    TextureIndex = p.PaletteIndex,
                    TextureName = p.TextureName,
                    Bounds = new RectangleData { X = p.Bounds.X, Y = p.Bounds.Y, Width = p.Bounds.Width, Height = p.Bounds.Height }
                });
            }

            MapDataManager.SaveMap(data);
            saveStatusMessage = $"Mapa salvo: {mapName}";
        }
    }
}
