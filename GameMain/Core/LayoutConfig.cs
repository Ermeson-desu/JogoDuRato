using System;

namespace GameDuMouse.GameMain.Core
{
    public static class LayoutConfig
    {
        public const int EditorPanelFraction = 4;
        public const int EditorWallThickness = 10;
        public const int EditorCeilingHeight = 10;
        public const int EditorBackgroundDeleteButtonSize = 28;
        public const int EditorRightWallAdjust = 0;
        public const int EditorFloorPadding = 75;
        public const int EditorGroundThickness = 5;
        public const int EditorMinPhaseWidth = 590;

        public const int GroundOffsetFromBottom = 80;
        public const int PlayerSpawnOffsetFromGround = 100;
        public const int GroundThickness = 5;

        public static int GetGroundY(int screenHeight)
        {
            if (screenHeight <= 0)
                return 400;

            int groundY = screenHeight - GroundOffsetFromBottom;
            return Math.Max(groundY, 0);
        }

        public static int GetPlayerSpawnY(int screenHeight)
        {
            int groundY = GetGroundY(screenHeight);
            int spawnY = groundY - PlayerSpawnOffsetFromGround;
            return spawnY > 0 ? spawnY : groundY;
        }

        public static int GetDefaultPhaseWidth(int screenWidth, int panelWidth)
        {
            int width = screenWidth - panelWidth;
            if (width <= 0)
                return EditorMinPhaseWidth;

            return Math.Max(width, EditorMinPhaseWidth);
        }
    }
}
