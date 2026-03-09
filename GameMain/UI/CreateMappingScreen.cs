using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameDuMouse.GameMain.Utils;
using GameDuMouse.GameMain.Core;
using GameDuMouse.GameMain.Entities;
using GameDuMouse.GameMain.Fases;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

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
        private const int RightWallX = 590;
        private const int TextOffsetY = 8;
        private const int RightWallAdjust = 0;
        private const int WallThickness = 10;
        private const int CeilingHeight = 10;

        private Game game;
        private SpriteFont font;
        private BackButton backButton;
        private Texture2D defaultBackgroundTexture;

        private List<Texture2D> obstacleTextures = new List<Texture2D>();
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
        private Rectangle groundCollider, groundCollider2;
        private List<Rectangle> GroundColliders;

        // Import button (world coordinates)
        private Point importButtonWorldPos;
        private int importButtonSize = 50;
        private Texture2D customBackground;
        private int phaseWidth = 590; // default
        private string pendingImagePath = null;
        private bool isDialogOpen = false;

        private class PlacedObstacle
        {
            public Texture2D Texture;
            public Rectangle Bounds;
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
            }
            catch
            {
                // create simple colored placeholders
                obstacleTextures.Add(CreateSolidTexture(Color.SandyBrown));
                obstacleTextures.Add(CreateSolidTexture(Color.DarkGray));
                obstacleTextures.Add(CreateSolidTexture(Color.Olive));
            }

            // Initialize colliders based on default phaseWidth
            UpdateCollidersForBackground();

            importButtonWorldPos = new Point(550/2, 190);
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
                    customBackground = Texture2D.FromFile(game.GraphicsDevice, pendingImagePath);
                    phaseWidth = customBackground.Width;
                    mapCameraOffsetX = 0; // Reset camera to start of image
                    
                    // Update colliders to match new background width
                    UpdateCollidersForBackground();
                    
                    // Recalculate button position based on new background width
                    int leftWallX = 0;
                    int rightWallX_Calc = phaseWidth;
                    int colliderAreaWidth = rightWallX_Calc - leftWallX;
                    int centerWorldX = leftWallX + colliderAreaWidth / 2 - importButtonSize / 2;
                    int centerWorldY = screenHeight / 2 - importButtonSize / 2;
                    importButtonWorldPos = new Point(rightWallX_Calc + 20, centerWorldY);
                    
                    System.Console.WriteLine("Background loaded successfully, width: " + phaseWidth);
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
            if (mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && previousMouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released && mouse.X < panelWidth)
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
                        placed.Add(new PlacedObstacle { Texture = tex, Bounds = rect });
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
            DrawPanel(spriteBatch);
            DrawColliders(spriteBatch);
            DrawPalette(spriteBatch);
            DrawPlacedObstacles(spriteBatch);
            DrawImportButton(spriteBatch);
            backButton?.Draw(spriteBatch);
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
            
            if (customBackground != null)
            {
                // Draw custom background
                spriteBatch.Draw(customBackground, new Vector2(-(mapCameraOffsetX - 200) , 0), Color.White);
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
            // left panel
            spriteBatch.DrawString(font, "Obstaculos", new Vector2(Margin, Margin), Color.White);
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
    }
}
