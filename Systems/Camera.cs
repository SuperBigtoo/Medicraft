using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using Microsoft.Xna.Framework.Input;

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
        private const float MaxZoom = 1.15f;
        private float targetZoom = 1.15f;
        private float zoomSpeed = 0.25f;

        public Camera(Viewport viewport)
        {
            viewportWidth = viewport.Width;
            viewportHeight = viewport.Height;
            zoom = 1.15f;
        }

        public void Update(float deltaSeconds)
        {
            SetPosition(Singleton.Instance.cameraPosition + Singleton.Instance.addingCameraPos);

            if (PlayerManager.Instance.GetPlayer().IsMoving)
            {
                targetZoom -= zoomSpeed * deltaSeconds;
            }
            else
            {
                targetZoom += zoomSpeed * deltaSeconds;

                if (targetZoom > MaxZoom) targetZoom = 1.15f;
            }

            // Clamp the zoom within the defined range.
            zoom = MathHelper.Clamp(targetZoom, MinZoom, MaxZoom);
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

        public Matrix GetTransform()
        {
            transform = Matrix.CreateTranslation(-position.X, -position.Y, 0)
                    * Matrix.CreateScale(zoom, zoom, 1);
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
