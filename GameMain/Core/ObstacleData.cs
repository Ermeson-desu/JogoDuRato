using System;

namespace GameDuMouse.GameMain.Core
{
    public enum MovementType
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2
    }

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

        /// <summary>
        /// Type of movement when IsMovable is true.
        /// </summary>
        public MovementType MovementType { get; set; }

        /// <summary>
        /// If true, the obstacle goes back and forth.
        /// If false, it only goes to the end and stays there.
        /// </summary>
        public bool IsLooping { get; set; }

        /// <summary>
        /// Distance of movement in pixels.
        /// </summary>
        public int MovementDistance { get; set; }

        public ObstacleEditorData()
        {
            Name = "New Obstacle";
            Width = 50;
            Height = 50;
            IsMortal = true;
            IsMovable = false;
            MovementType = MovementType.None;
            IsLooping = true;
            MovementDistance = 100;
        }

        public ObstacleEditorData Clone()
        {
            return new ObstacleEditorData
            {
                Name = this.Name,
                Width = this.Width,
                Height = this.Height,
                IsMortal = this.IsMortal,
                IsMovable = this.IsMovable,
                MovementType = this.MovementType,
                IsLooping = this.IsLooping,
                MovementDistance = this.MovementDistance
            };
        }
    }
}
