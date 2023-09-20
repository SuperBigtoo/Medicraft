using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Medicraft.Systems;

namespace Medicraft.Screens
{
    public class Screen
    {
        protected ContentManager Content;
        protected Camera Camera;

        public virtual void LoadContent(Camera camera)
        {
            Content = new ContentManager(ScreenManager.Instance.Content.ServiceProvider, "Content");
            Camera = camera;
        }

        public virtual void UnloadContent()
        {
            Content.Unload();
        }

        public virtual void Update(GameTime gameTime) { }

        public virtual void Draw(SpriteBatch spriteBatch) { }
    }
}
