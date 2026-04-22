using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Rendering;
using GameDuMouse.GameMain.Core;

namespace GameDuMouse.GameMain.Entities
{
    public enum ObstacleShape
    {
        Circle,
        Triangle,
        Square
    }

    public class Obstacle
    {
        private Texture2D texture;
        private Vector2 position;
        private int width, height;
        private ObstacleShape shape;
        private Game game;
        private string customTextureName; // For custom obstacles loaded from .json
        private bool isMovable;
        private MovementType movementType;
        private bool isLooping;
        private float movementDistanceStart;
        private float movementDistanceEnd;
        private float movementSpeed;
        private float movementProgress;
        private float movementDirection = 1f;
        private Vector2 initialPosition;
        private bool requiresTrigger;
        private bool triggerActivated;
        private float triggerDistance;
        private bool colliderEnabled = true;

        public Rectangle Bounds => new Rectangle((int)position.X, (int)position.Y, width, height);

        // Para círculo
        public Vector2 Center => new Vector2(position.X + width / 2, position.Y + height / 2);
        public float Radius => Math.Min(width, height) / 2f;

        // Para triângulo
        private Vector2[] triangleVertices;

        public Obstacle(Game game, int x, int y, int width, int height, string shapeStr)
        {
            this.game = game;
            this.position = new Vector2(x, y);
            this.initialPosition = this.position;
            this.width = width;
            this.height = height;
            this.shape = Enum.TryParse(shapeStr, true, out ObstacleShape parsedShape) ? parsedShape : ObstacleShape.Square;

            var cache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
            texture = cache != null ? cache.Pixel : texture;

            if (shape == ObstacleShape.Triangle)
            {
                // Triângulo apontando para cima
                triangleVertices = new Vector2[]
                {
                    new Vector2(x + width / 2, y),        // topo
                    new Vector2(x, y + height),           // canto inferior esquerdo
                    new Vector2(x + width, y + height)    // canto inferior direito
                };
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture == null)
                return;
            
            // For custom textures, draw with white color to preserve original image colors
            // For default shapes, apply colored tinting
            Color color = !string.IsNullOrWhiteSpace(customTextureName) ? Color.White : (shape switch
            {
                ObstacleShape.Circle => Color.Red * 0.6f,
                ObstacleShape.Triangle => Color.Orange * 0.6f,
                ObstacleShape.Square => Color.Purple * 0.6f,
                _ => Color.Gray * 0.6f
            });

            spriteBatch.Draw(texture, Bounds, color);
        }

        public void ConfigureMovement(bool movable, MovementType type, bool looping, int distanceStart, int distanceEnd, float speed, bool waitForTrigger = false, float activationDistance = 0f)
        {
            isMovable = movable;
            movementType = type;
            isLooping = looping;
            movementDistanceStart = Math.Max(0, distanceStart);
            movementDistanceEnd = Math.Max(0, distanceEnd);
            movementSpeed = Math.Max(0f, speed);
            movementProgress = -movementDistanceStart;
            movementDirection = 1f;
            requiresTrigger = waitForTrigger;
            triggerActivated = !waitForTrigger;
            triggerDistance = Math.Max(0f, activationDistance);
            colliderEnabled = true;
            ApplyMovementPosition();
        }

        public void Update(float deltaSeconds)
        {
            float range = movementDistanceStart + movementDistanceEnd;
            if (!isMovable || movementType == MovementType.None || range <= 0f || movementSpeed <= 0f || deltaSeconds <= 0f)
                return;
            if (requiresTrigger && !triggerActivated)
                return;

            float delta = movementSpeed * deltaSeconds * movementDirection;
            movementProgress += delta;

            if (isLooping)
            {
                if (movementProgress >= movementDistanceEnd)
                {
                    movementProgress = movementDistanceEnd;
                    movementDirection = -1f;
                }
                else if (movementProgress <= -movementDistanceStart)
                {
                    movementProgress = -movementDistanceStart;
                    movementDirection = 1f;
                }
            }
            else
            {
                if (movementProgress > movementDistanceEnd)
                    movementProgress = movementDistanceEnd;
                if (movementProgress < -movementDistanceStart)
                    movementProgress = -movementDistanceStart;

                // One-shot movement reached the end: disable collisions after the path ends.
                if (movementProgress >= movementDistanceEnd)
                {
                    movementSpeed = 0f;
                    colliderEnabled = false;
                }
            }

            ApplyMovementPosition();
        }

        public void TryActivateByDistance(Rectangle otherBounds)
        {
            if (!requiresTrigger || triggerActivated)
                return;

            if (DistanceBetweenRectangles(Bounds, otherBounds) <= triggerDistance)
                triggerActivated = true;
        }

        public void TryActivateByHorizontalDistance(Rectangle otherBounds)
        {
            if (!requiresTrigger || triggerActivated)
                return;

            if (HorizontalGapBetweenRectangles(Bounds, otherBounds) <= triggerDistance)
                triggerActivated = true;
        }

        private void ApplyMovementPosition()
        {
            Vector2 nextPosition = initialPosition;
            if (movementType == MovementType.Horizontal)
                nextPosition.X += movementProgress;
            else if (movementType == MovementType.Vertical)
                nextPosition.Y += movementProgress;

            SetPosition(nextPosition);
        }

        public bool CollidesWith(Rectangle playerCollider)
        {
            if (!colliderEnabled)
                return false;

            switch (shape)
            {
                case ObstacleShape.Circle:
                    return CircleIntersects(playerCollider);
                case ObstacleShape.Square:
                    return Bounds.Intersects(playerCollider);
                case ObstacleShape.Triangle:
                    return TriangleIntersects(playerCollider);
                default:
                    return false;
            }
        }

        private bool CircleIntersects(Rectangle rect)
        {
            float closestX = Math.Clamp(Center.X, rect.Left, rect.Right);
            float closestY = Math.Clamp(Center.Y, rect.Top, rect.Bottom);

            float distanceX = Center.X - closestX;
            float distanceY = Center.Y - closestY;

            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            return distanceSquared < (Radius * Radius);
        }

        private bool TriangleIntersects(Rectangle rect)
        {
            // Verifica se algum canto do player está dentro do triângulo
            Vector2[] playerCorners =
            {
                new Vector2(rect.Left, rect.Top),
                new Vector2(rect.Right, rect.Top),
                new Vector2(rect.Left, rect.Bottom),
                new Vector2(rect.Right, rect.Bottom)
            };

            foreach (var corner in playerCorners)
            {
                if (PointInTriangle(corner, triangleVertices[0], triangleVertices[1], triangleVertices[2]))
                    return true;
            }

            return false;
        }

        private bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            // Algoritmo de barycentric coordinates
            float denominator = ((b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y));
            float alpha = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / denominator;
            float beta = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / denominator;
            float gamma = 1.0f - alpha - beta;

            return alpha >= 0 && beta >= 0 && gamma >= 0;
        }

        public void LoadCustomTexture(string textureName)
        {
            if (string.IsNullOrWhiteSpace(textureName) || textureName == "Square")
                return;
            
            // Try to load from CustomObstacleService
            var customObstacleService = game.Services.GetService(typeof(GameDuMouse.GameMain.Services.CustomObstacleService)) as GameDuMouse.GameMain.Services.CustomObstacleService;
            if (customObstacleService != null)
            {
                var obstacle = customObstacleService.GetByName(textureName);
                if (obstacle != null && !string.IsNullOrWhiteSpace(obstacle.ImagePath))
                {
                    var assetManager = game.Services.GetService(typeof(GameDuMouse.GameMain.Services.AssetManager)) as GameDuMouse.GameMain.Services.AssetManager;
                    if (assetManager != null)
                    {
                        var customTexture = assetManager.LoadTextureFromFile(obstacle.ImagePath);
                        if (customTexture != null)
                        {
                            texture = customTexture;
                            customTextureName = textureName;
                            return;
                        }
                    }
                }
            }

            // Keep default obstacle tint if the custom texture cannot be loaded.
            customTextureName = null;
        }

        private void SetPosition(Vector2 newPosition)
        {
            position = newPosition;

            if (shape == ObstacleShape.Triangle)
            {
                triangleVertices = new Vector2[]
                {
                    new Vector2(position.X + width / 2f, position.Y),
                    new Vector2(position.X, position.Y + height),
                    new Vector2(position.X + width, position.Y + height)
                };
            }
        }

        private static float DistanceBetweenRectangles(Rectangle a, Rectangle b)
        {
            int dx = 0;
            if (a.Right < b.Left)
                dx = b.Left - a.Right;
            else if (b.Right < a.Left)
                dx = a.Left - b.Right;

            int dy = 0;
            if (a.Bottom < b.Top)
                dy = b.Top - a.Bottom;
            else if (b.Bottom < a.Top)
                dy = a.Top - b.Bottom;

            return (float)Math.Sqrt((dx * dx) + (dy * dy));
        }

        private static int HorizontalGapBetweenRectangles(Rectangle a, Rectangle b)
        {
            if (a.Right < b.Left)
                return b.Left - a.Right;
            if (b.Right < a.Left)
                return a.Left - b.Right;
            return 0;
        }
    }
}

