using System.Collections.Generic;

namespace GameDuMouse.GameMain.Core
{
    public class MapData
    {
        public string MapName { get; set; }
        public int PhaseWidth { get; set; }
        public int ScreenHeight { get; set; }
        public bool IsCustomBackgroundLoaded { get; set; }
        public string DefaultBackgroundColor { get; set; }
        public int DefaultBackgroundWidth { get; set; }
        public int DefaultBackgroundHeight { get; set; }
        public string CreatedAtUtc { get; set; }

        public List<BackgroundLayerData> BackgroundLayers { get; set; } = new List<BackgroundLayerData>();
        public List<ColliderData> Colliders { get; set; } = new List<ColliderData>();
        public List<ObstacleData> Obstacles { get; set; } = new List<ObstacleData>();
        public List<ObjectData> Objects { get; set; } = new List<ObjectData>();
    }

    public class BackgroundLayerData
    {
        public string ImagePath { get; set; }
        public int StartX { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ColliderData
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public RectangleData Bounds { get; set; }
    }

    public class ObstacleData
    {
        public string Name { get; set; }
        public int TextureIndex { get; set; }
        public string TextureName { get; set; }
        public RectangleData Bounds { get; set; }
    }

    public class ObjectData
    {
        public string Name { get; set; }
        public Vector2Data Position { get; set; }
    }

    public class RectangleData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Vector2Data
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
