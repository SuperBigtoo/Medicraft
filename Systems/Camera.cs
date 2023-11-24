using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace Medicraft.Systems
{
    public class Camera
    {
        private Matrix transform;
        private Vector2 position;
        private float zoom;
        private readonly float viewportWidth;
        private readonly float viewportHeight;
        private const float MinZoom = 1.0f;
        private const float MaxZoom = 1.0f;
        private float targetZoom = 1.0f;
        private float zoomSpeed = 0.25f;

        public Camera(Viewport viewport)
        {
            viewportWidth = viewport.Width;
            viewportHeight = viewport.Height;
            zoom = 1.0f;
        }

        public void Update(float deltaSeconds)
        {
            // if cutscene is false do dis
            SetPosition(GameGlobals.Instance.InitialCameraPos + GameGlobals.Instance.AddingCameraPos);

            //if (PlayerManager.Instance.Player.IsMoving)
            //{
            //    targetZoom -= zoomSpeed * deltaSeconds;
            //}
            //else
            //{
            //    targetZoom += zoomSpeed * deltaSeconds;

            //    if (targetZoom > MaxZoom) targetZoom = 1.0f;
            //}

            //// Clamp the zoom within the defined range.
            //zoom = MathHelper.Clamp(targetZoom, MinZoom, MaxZoom);
        }

        public void SetPosition(Vector2 targetPosition)
        {
            position.X = targetPosition.X - viewportWidth / (2 * zoom);
            position.Y = targetPosition.Y - viewportHeight / (2 * zoom);
        }

        public void SetZoom(float zoom, float zoomMin, float zoomMax)
        {
            this.zoom = MathHelper.Clamp(zoom, zoomMin, zoomMax);
        }

        public Matrix GetTransform(int screenWidth, int screenHeight)
        {
            float scaleX = (float)screenWidth / viewportWidth;
            float scaleY = (float)screenHeight / viewportHeight;
            float scale = MathHelper.Min(scaleX, scaleY);

            // Calculate the letterbox bars (if any)
            int barsWidth = screenWidth - (int)(viewportWidth * scale);
            int barsHeight = screenHeight - (int)(viewportHeight * scale);

            transform = Matrix.CreateTranslation(-position.X, -position.Y, 0)
                * Matrix.CreateScale(zoom, zoom, 1)
                * Matrix.CreateTranslation(barsWidth / 2, barsHeight / 2, 0);

            return transform;
        }

        public float GetZoom()
        {
            return zoom;
        }

        public Vector2 GetPosition()
        {
            return position;
        }
    }
}
