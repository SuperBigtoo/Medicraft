using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Medicraft.Systems;
using Medicraft.Systems.TilemapRenderer;
using static Medicraft.Systems.Managers.ScreenManager;

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
        protected ContentManager Content;

        protected HUDSystem HudSystem;
        protected TilemapOrthogonalRender TileMapRender;

        protected GameScreen ScreenName;

        public virtual void LoadContent()
        {
            Content = new ContentManager(GameGlobals.Instance.Content.ServiceProvider, "Content");
        }

        public virtual void UnloadContent()
        {
            Content?.Unload();
        }

        public virtual void Dispose()
        {
            Content?.Dispose();
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
