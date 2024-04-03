using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Medicraft.Systems.Managers;

namespace Medicraft.Systems
{
    public class Camera(Viewport viewport)
    {
        private Matrix _transform;
        private Vector2 _position;
        private float _zoom = 1.0f;
        private readonly float _viewportWidth = viewport.Width;
        private readonly float _viewportHeight = viewport.Height;

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

        public static void ResetCameraPosition(bool isPlayerPos)
        {
            // Adjust HUD and camera position
            GameGlobals.Instance.TopLeftCornerPos = isPlayerPos ? (PlayerManager.Instance.Player.Position - GameGlobals.Instance.GameScreenCenter) : Vector2.Zero;
            GameGlobals.Instance.InitialCameraPos = isPlayerPos ? PlayerManager.Instance.Player.Position : GameGlobals.Instance.GameScreenCenter;
            GameGlobals.Instance.AddingCameraPos = Vector2.Zero;
        }

        public void SetPosition(Vector2 targetPosition)
        {
            _position.X = targetPosition.X - _viewportWidth / (2 * _zoom);
            _position.Y = targetPosition.Y - _viewportHeight / (2 * _zoom);
        }

        public void SetZoom(float zoom, float zoomMin, float zoomMax)
        {
            _zoom = MathHelper.Clamp(zoom, zoomMin, zoomMax);
        }

        public Matrix GetTransform(int screenWidth, int screenHeight)
        {
            float scaleX = screenWidth / _viewportWidth;
            float scaleY = screenHeight / _viewportHeight;
            float scale = MathHelper.Min(scaleX, scaleY);

            // Calculate the letterbox bars (if any)
            int barsWidth = screenWidth - (int)(_viewportWidth * scale);
            int barsHeight = screenHeight - (int)(_viewportHeight * scale);

            _transform = Matrix.CreateTranslation(-_position.X, -_position.Y, 0)
                * Matrix.CreateScale(_zoom, _zoom, 1)
                * Matrix.CreateTranslation(barsWidth / 2, barsHeight / 2, 0);

            return _transform;
        }

        public Vector2 GetViewportCenter(int screenWidth, int screenHeight)
        {
            float scaleX = screenWidth / _viewportWidth;
            float scaleY = screenHeight / _viewportHeight;
            float scale = MathHelper.Min(scaleX, scaleY);

            int barsWidth = screenWidth - (int)(_viewportWidth * scale);
            int barsHeight = screenHeight - (int)(_viewportHeight * scale);

            // Calculate the viewport rectangle after scaling
            Rectangle viewportRect = new Rectangle(
                barsWidth / 2, barsHeight / 2,
                (int)(_viewportWidth * scale), (int)(_viewportHeight * scale));

            // Calculate the center point of the scaled viewport rectangle
            Vector2 viewportCenter = new Vector2(
                viewportRect.X + viewportRect.Width / 2,
                viewportRect.Y + viewportRect.Height / 2);

            // Get the inverse of the transformation matrix
            Matrix inverseTransform = Matrix.Invert(GetTransform(screenWidth, screenHeight));

            // Transform the center point back to the original coordinate system
            Vector2 originalCenter = Vector2.Transform(viewportCenter, inverseTransform);

            return originalCenter;
        }

        public float GetZoom()
        {
            return _zoom;
        }

        public Vector2 GetViewportPosition()
        {
            return _position;
        }
    }
}
