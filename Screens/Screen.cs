using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Medicraft.Systems;
using Medicraft.Systems.Managers;

namespace Medicraft.Screens
{
    public interface IScreen
    {
        void LoadContent();
        void UnloadContent();
        void Dispose();
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }

    public abstract class Screen : IScreen
    {
        protected ContentManager _content;

        public virtual void LoadContent()
        {
            _content = new ContentManager(GameGlobals.Instance.Content.ServiceProvider, "Content");
        }

        public virtual void UnloadContent()
        {
            // Clear List Entity, GameObject and MusicBG
            EntityManager.Instance.ClearEntity();
            ObjectManager.Instance.ClearGameObject();
            GameGlobals.Instance.CurrentMapMusics.Clear();

            _content?.Unload();
        }

        public virtual void Dispose()
        {
            _content?.Dispose();
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
