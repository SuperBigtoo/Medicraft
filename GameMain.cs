using Medicraft.Systems;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Medicraft
{
    public class GameMain : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly GameGlobals _singleton;

        public GameMain()
        {
            _singleton = GameGlobals.Instance;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = (int)_singleton.GameScreen.X;
            _graphics.PreferredBackBufferHeight = (int)_singleton.GameScreen.Y;
            _graphics.SynchronizeWithVerticalRetrace = true;
            //Window.Position = new Point((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2) - (_graphics.PreferredBackBufferWidth / 2)
            //    , (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2) - (_graphics.PreferredBackBufferHeight / 2));
            _graphics.ToggleFullScreen();
            _graphics.ApplyChanges();

            Content = new ContentManager(Content.ServiceProvider, "Content");

            GameGlobals.Instance.Initialize(Content);

            ScreenManager.Instance.Initialize(this, _graphics);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            GameGlobals.Instance.LoadContent();

            ScreenManager.Instance.LoadContent();
        }

        protected override void UnloadContent()
        {
            ScreenManager.Instance.UnloadContent();
        }

        protected override void Dispose(bool disposing)
        {
            ScreenManager.Instance.Dispose();

            base.Dispose(disposing);
        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            GameGlobals.Instance.Update(gameTime);

            ScreenManager.Instance.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here          
            ScreenManager.Instance.Draw(); 

            base.Draw(gameTime);
        }
    }
}