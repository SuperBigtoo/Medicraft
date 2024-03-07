using GeonBit.UI;
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

            // Initialize GUI Panels
            GUIManager.Instance.InitializeThemeAndUI(GameGlobals.Instance.BuiltinTheme);
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
                    CurrentScreen = GameScreen.TestScreen;
                    _curScreen = new TestScreen();                  
                    _curScreen.LoadContent();
                    break;

                case GameScreen.SplashScreen:
                    CurrentScreen = GameScreen.SplashScreen;
                    _curScreen = new SplashScreen();                 
                    _curScreen.LoadContent();
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            GameGlobals.Instance.IsGameActive = Game.IsActive;

            if (GameGlobals.Instance.IsGameActive)
            {
                // update UI
                UserInterface.Active.Update(gameTime);                        

                if (!GameGlobals.Instance.IsGamePause)
                {
                    _curScreen.Update(gameTime);                
                }
                else
                {
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                         Game.Exit();
                }

                // Current GUI Panel
                switch (GUIManager.Instance.CurrentGUI)
                {
                    case GUIManager.Hotbar:
                        // Item bar
                        if (!GameGlobals.Instance.IsRefreshHotbar)
                        {
                            GUIManager.Instance.CurrentGUI = GUIManager.Hotbar;
                            GUIManager.Instance.RefreshHotbarDisplay();
                            GUIManager.Instance.UpdateAfterChangeGUI();

                            GameGlobals.Instance.IsRefreshHotbar = true;
                        }
                        break;

                    case GUIManager.InventoryPanel:
                        // Inventory
                        if (!GameGlobals.Instance.IsOpenInventoryPanel)
                        {
                            GUIManager.Instance.CurrentGUI = GUIManager.InventoryPanel;
                            GUIManager.Instance.RefreshInvenrotyItemDisplay(false);
                            GUIManager.Instance.UpdateAfterChangeGUI();

                            GameGlobals.Instance.IsOpenInventoryPanel = true;
                        }
                        break;

                    case GUIManager.CraftingTablePanel:
                        // Crafting
                        if (!GameGlobals.Instance.IsOpenCraftingPanel)
                        {

                        }
                        break;
                }

                PlayerManager.UpdateGameController(gameTime);
            }                     
        }

        public void Draw()
        {
            GraphicsDevice.Clear(Color.Black);

            if (GameGlobals.Instance.IsGameActive) 
            {
                // Draw UI
                UserInterface.Active.Draw(_spriteBatch);

                _spriteBatch.Begin
                (
                    SpriteSortMode.BackToFront,
                    samplerState: SamplerState.PointClamp,
                    blendState: BlendState.AlphaBlend,
                    transformMatrix: Camera.GetTransform(
                        GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
                );

                // Draw on Screen here
                _curScreen.Draw(_spriteBatch);
                _spriteBatch.End();

                // OpenGUI Panel such as Invenotory, Character Inspect and Crafting Panel
                if (GameGlobals.Instance.IsOpenGUI)
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
                    DrawBackgound(_spriteBatch, Color.Black, 0.6f);
                    _spriteBatch.End();                
                }

                // Draw render UI
                if (CurrentScreen == GameScreen.TestScreen || CurrentScreen == GameScreen.PlayScreen)
                    UserInterface.Active.DrawMainRenderTarget(_spriteBatch);

                if (!GameGlobals.Instance.IsGamePause)
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

                    HUDSystem.DrawSelectedSlotItemBar(_spriteBatch);

                    _spriteBatch.End();
                }
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
