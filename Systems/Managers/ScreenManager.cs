using GeonBit.UI.Entities;
using Medicraft.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Medicraft.Systems.Managers
{
    public class ScreenManager
    {
        private static ScreenManager instance;

        private SpriteBatch _spriteBatch;
        private Screen _curScreen;

        public Game Game { private set; get; }
        public GraphicsDevice GraphicsDevice { private set; get; }
        public GraphicsDeviceManager GraphicsDeviceManager { private set; get; }
        public GameWindow Window { private set; get; }
        public Camera Camera { private set; get; }
     
        public enum GameScreen
        {
            TestScreen,
            SplashScreen,
            MainMenuScreen,
            PlayScreen
        }
        public GameScreen CurrentScreen { private set; get; }

        public string CurrentMap { set; get; }
        public enum LoadMapAction
        {
            NewGame,
            LoadGameSave
        }
        public LoadMapAction CurrentLoadMapAction { set; get; }

        public ScreenManager()
        {
            _curScreen = new SplashScreen();
            CurrentScreen = GameScreen.SplashScreen;
        }

        public void Initialize(Game game, GraphicsDeviceManager graphicsDeviceManager)
        {
            Game = game;
            GraphicsDevice = game.GraphicsDevice;
            GraphicsDeviceManager = graphicsDeviceManager;
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
                if (!GameGlobals.Instance.IsGamePause)
                {
                    _curScreen.Update(gameTime);
                }
                else
                {
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                        Game.Exit();

                    InventoryManager.Instance.Update(gameTime);
                }

                PlayerManager.UpdateGameController(gameTime);
            }                     
        }

        public void Draw()
        {
            GraphicsDevice.Clear(Color.Black);

            if (GameGlobals.Instance.IsGameActive) 
            {
                if (GameGlobals.Instance.IsOpenInventory) InventoryManager.Instance.DrawUI(_spriteBatch);

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

                if (GameGlobals.Instance.IsOpenInventory)
                {
                    _spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        samplerState: SamplerState.LinearClamp,
                        blendState: BlendState.AlphaBlend,
                        depthStencilState: DepthStencilState.None,
                        rasterizerState: RasterizerState.CullCounterClockwise,
                        transformMatrix: Camera.GetTransform(
                            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
                    );
                    // Draw Background
                    DrawBackgound(_spriteBatch, Color.Black, 0.4f);
                    _spriteBatch.End();

                    if (!GameGlobals.Instance.IsInventoryRefreshed)
                    {
                        InventoryManager.Instance.RefreshInvenrotyItemDisplay(false);

                        GameGlobals.Instance.IsInventoryRefreshed = true;
                    }

                    InventoryManager.Instance.DrawMainRenderTarget(_spriteBatch);
                }
                else GameGlobals.Instance.IsInventoryRefreshed = false;
            }        
        }

        public static void DrawBackgound(SpriteBatch spriteBatch, Color color, float transparency)
        {
            var topLeftCorner = GameGlobals.Instance.TopLeftCornerPosition;

            spriteBatch.FillRectangle(topLeftCorner.X, topLeftCorner.Y
                , Instance.GraphicsDevice.Viewport.Width
                , Instance.GraphicsDevice.Viewport.Height
                , color * transparency);
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
