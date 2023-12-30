using Medicraft.Data;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Medicraft
{
    public class GameMain : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        //private Camera Camera;

        private readonly EntityManager _entityManager;
        private readonly GameGlobals _singleton;

        public GameMain()
        {
            _singleton = GameGlobals.Instance;
            _entityManager = EntityManager.Instance;
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
            //_graphics.ToggleFullScreen();
            _graphics.ApplyChanges();

            // Load GameSave
            var gameSave = JsonFileManager.LoadFlie("data/stats.json");
            if (gameSave.Count != 0)
            {
                foreach (var save in gameSave)
                {
                    _singleton.GameSave.Add(save);
                }
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            ScreenManager.Instance.LoadContent(this, Content);
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