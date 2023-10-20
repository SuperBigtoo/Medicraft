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
        private readonly Singleton _singleton;

        public GameMain()
        {
            _singleton = Singleton.Instance;
            _entityManager = EntityManager.Instance;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = (int)_singleton.gameScreen.X;
            _graphics.PreferredBackBufferHeight = (int)_singleton.gameScreen.Y;
            _graphics.SynchronizeWithVerticalRetrace = true;
            Window.Position = new Point((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2) - (_graphics.PreferredBackBufferWidth / 2)
                , (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2) - (_graphics.PreferredBackBufferHeight / 2));
            _graphics.ToggleFullScreen();
            _graphics.ApplyChanges();

            //Camera = new Camera(GraphicsDevice.Viewport);

            // Load GameSave
            var gameSave = JsonFileManager.LoadFlie("data/stats.json");
            if (gameSave.Count != 0)
            {
                foreach (var save in gameSave)
                {
                    _singleton.gameSave.Add(save);
                }
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            ScreenManager.Instance.LoadContent(Content, GraphicsDevice, Window);
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
            Singleton.Instance.IsGameActive = IsActive;

            if (IsActive)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();
            }

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