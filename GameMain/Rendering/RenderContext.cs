using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Core;

namespace GameDuMouse.GameMain.Rendering
{
    public readonly struct RenderContext
    {
        public const int DefaultCullPadding = 128;

        public Rectangle ViewBounds { get; }
        public Rectangle CullingBounds { get; }

        private RenderContext(Rectangle viewBounds, Rectangle cullingBounds)
        {
            ViewBounds = viewBounds;
            CullingBounds = cullingBounds;
        }

        public static RenderContext FromCamera(Camera camera, Viewport viewport, int padding = DefaultCullPadding)
        {
            int width = Math.Max(1, viewport.Width);
            int height = Math.Max(1, viewport.Height);

            float camX = camera != null ? camera.Position.X : 0f;
            float camY = camera != null ? camera.Position.Y : 0f;

            Vector2 offset = camera != null ? camera.ViewOffset : new Vector2(width / 2f, height / 2f);

            int left = (int)(camX - offset.X);
            int top = (int)(camY - offset.Y);

            var view = new Rectangle(left, top, width, height);
            var cull = view;
            cull.Inflate(padding, padding);

            return new RenderContext(view, cull);
        }

        public bool IsVisible(Rectangle bounds)
        {
            return CullingBounds.Intersects(bounds);
        }
    }
}
