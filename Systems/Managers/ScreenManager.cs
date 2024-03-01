using Medicraft.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Medicraft.Systems.Managers
{
    public class ScreenManager
    {
        private static ScreenManager instance;

        private SpriteBatch _spriteBatch;
        private Screen _curScreen;

        public Game Game { private set; get; }
        public GraphicsDevice GraphicsDevice { private set; get; }
        public GameWindow Window { private set; get; }
        public Camera Camera { private set; get; }
        public GameScreen CurrentScreen { private set; get; }
        public enum GameScreen
        {
            TestScreen,
            SplashScreen,
            MainMenuScreen,
            PlayScreen
        }

        public enum LoadMapAction
        {
            NewGame,
            LoadGameSave
        }

        public ScreenManager()
        {
            _curScreen = new SplashScreen();
            CurrentScreen = GameScreen.SplashScreen;
        }

        public void Initialize(Game game)
        {
            Game = game;
            GraphicsDevice = game.GraphicsDevice;
            Window = game.Window;
            Camera = new Camera(GraphicsDevice.Viewport);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public void LoadContent()
        {           
            _curScreen.LoadContent();
        }

        public void UnloadContent()
        {
            _curScreen?.UnloadContent();
        }

        public void Dispose()
        {
            _curScreen?.Dispose();
        }

        public void LoadScreen(GameScreen gameScreen)
        {
            _curScreen?.UnloadContent();

            switch (gameScreen)
            {
                case GameScreen.TestScreen:
                    _curScreen = new TestScreen();
                    CurrentScreen = GameScreen.TestScreen;
                    _curScreen.LoadContent();
                    break;

                case GameScreen.SplashScreen:
                    _curScreen = new SplashScreen();
                    CurrentScreen = GameScreen.SplashScreen;
                    _curScreen.LoadContent();
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            GameGlobals.Instance.IsGameActive = Game.IsActive;

            if (GameGlobals.Instance.IsGameActive)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Game.Exit();
            }

            _curScreen.Update(gameTime);
        }

        public void Draw()
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin
            (
                SpriteSortMode.BackToFront,
                samplerState: SamplerState.PointClamp,
                blendState: BlendState.AlphaBlend,
                transformMatrix: Camera.GetTransform(
                    GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
            );

            _curScreen.Draw(_spriteBatch);

            _spriteBatch.End();
        }

        public static ScreenManager Instance
        {
            get
            {
                instance ??= new ScreenManager();
                return instance;
            }
        }
    }
}
