using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Medicraft.Systems;

namespace Medicraft.Screens
{
    public class Screen
    {
        protected ContentManager Content;
        protected GraphicsDevice GraphicsDevice;
        protected GameWindow Window;

        public virtual void LoadContent()
        {
            Content = new ContentManager(ScreenManager.Instance.Content.ServiceProvider, "Content");
            GraphicsDevice = ScreenManager.Instance.GraphicsDevice;
            Window = ScreenManager.Instance.Window;
        }

        public virtual void UnloadContent()
        {
            Content.Unload();
        }

        public virtual void Dispose() { }

        public virtual void Update(GameTime gameTime) { }

        public virtual void Draw(SpriteBatch spriteBatch) { }
    }
}
