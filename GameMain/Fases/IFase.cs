using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Entities;

namespace GameDuMouse.GameMain.Fases
{
    using Microsoft.Xna.Framework;

    public interface IFase
    {
        void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content);
        void Update(Player player);
        void Draw(SpriteBatch spriteBatch, Player player);
        bool HasWon { get; }

        bool IsReturning { get; }  
        void SetReturning(bool returning);
        Vector2 GetSpawnPosition(bool returning);

        List<Rectangle> GroundColliders { get; }
        List<Rectangle> Platforms { get; }
        List<Obstacle> Obstacles1 { get; }
        List<Obstacle> Obstacles2 { get; }
        List<Rectangle> WallColliders { get; }

    }
}
