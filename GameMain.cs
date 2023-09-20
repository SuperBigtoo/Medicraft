using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;


namespace Medicraft
{
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private readonly Camera _camera;
        private readonly EntityManager _entityManager;
        private readonly Singleton _singleton;

        public GameMain()
        {
            _singleton = Singleton.Instance;
            _entityManager = EntityManager.Instance;
            _graphics = new GraphicsDeviceManager(this);
            _camera = new Camera(GraphicsDevice.Viewport);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = (int)_singleton.gameScreen.X;
            _graphics.PreferredBackBufferHeight = (int)_singleton.gameScreen.Y;
            _graphics.SynchronizeWithVerticalRetrace = false;
            Window.Position = new Point((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2) - (_graphics.PreferredBackBufferWidth / 2)
                , (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2) - (_graphics.PreferredBackBufferHeight / 2));
            _graphics.ToggleFullScreen();
            _graphics.ApplyChanges();
                        
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            ScreenManager.Instance.LoadContent(Content, _camera);
        }

        protected override void UnloadContent()
        {
            ScreenManager.Instance.UnloadContent();
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(transformMatrix: _camera.GetTransform());
            ScreenManager.Instance.Draw(_spriteBatch);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}