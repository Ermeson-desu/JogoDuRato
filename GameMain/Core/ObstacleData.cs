using System;

namespace GameDuMouse.GameMain.Core
{
    /// <summary>
    /// Represents editable obstacle properties for the obstacle editor
    /// </summary>
    public class ObstacleEditorData
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        
        /// <summary>
        /// If true, the obstacle is mortal (kills the player).
        /// If false, it's a platform (allows the player to stand on it).
        /// </summary>
        public bool IsMortal { get; set; }
        
        /// <summary>
        /// If true, the obstacle can move/be dynamic.
        /// </summary>
        public bool IsMovable { get; set; }

        public ObstacleEditorData()
        {
            Name = "New Obstacle";
            Width = 50;
            Height = 50;
            IsMortal = true;
            IsMovable = false;
        }

        public ObstacleEditorData Clone()
        {
            return new ObstacleEditorData
            {
                Name = this.Name,
                Width = this.Width,
                Height = this.Height,
                IsMortal = this.IsMortal,
                IsMovable = this.IsMovable
            };
        }
    }
}
