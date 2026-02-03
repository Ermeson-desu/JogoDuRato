using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDuMouse
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
            this.width = width;
            this.height = height;
            this.shape = Enum.TryParse(shapeStr, true, out ObstacleShape parsedShape) ? parsedShape : ObstacleShape.Square;

            texture = new Texture2D(game.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.Red });

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
            Color color = shape switch
            {
                ObstacleShape.Circle => Color.Red * 0.6f,
                ObstacleShape.Triangle => Color.Orange * 0.6f,
                ObstacleShape.Square => Color.Purple * 0.6f,
                _ => Color.Gray * 0.6f
            };

            spriteBatch.Draw(texture, Bounds, color);
        }

        public bool CollidesWith(Rectangle playerCollider)
        {
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
    }
}