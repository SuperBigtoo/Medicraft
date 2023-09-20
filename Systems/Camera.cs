using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Medicraft.Systems
{
    public class Camera
    {
        private Matrix transform;
        private Vector2 position;
        private float viewportWidth;
        private float viewportHeight;

        public Camera(Viewport viewport)
        {
            viewportWidth = viewport.Width;
            viewportHeight = viewport.Height;
        }

        public void SetPosition(Vector2 targetPosition)
        {
            position.X = targetPosition.X - viewportWidth / 2;
            position.Y = targetPosition.Y - viewportHeight / 2;
        }

        public Matrix GetTransform()
        {
            transform = Matrix.CreateTranslation(-position.X, -position.Y, 0);
            return transform;
        }
    }
}
