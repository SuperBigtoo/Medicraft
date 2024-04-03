using GeonBit.UI;
using Medicraft.Screens;
using Medicraft.Screens.chapter_1;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;

namespace Medicraft.Systems.Managers
{
    public class ScreenManager
    {
        private static ScreenManager instance;

        private SpriteBatch _spriteBatch;
        private Screen _curScreen, _prevScreen;

        public Game Game { private set; get; }
        public GraphicsDevice GraphicsDevice { private set; get; }
        public GraphicsDeviceManager GraphicsDeviceManager { private set; get; }
        public GameWindow Window { private set; get; }
        public static Camera Camera { private set; get; }
     
        public enum GameScreen
        {
            TestScreen,
            SplashScreen,
            MainMenuScreen,
            Map1
        }
        public GameScreen CurrentScreen { private set; get; }
        public GameScreen ScreenTransitioningTo { private set; get; }

        public enum LoadMapAction
        {
            NewGame,
            LoadSave,
            Test_to_map_1,
            map_1_to_Test
        }     
        public LoadMapAction CurrentLoadMapAction { set; get; }
        public string CurrentMap { set; get; }

        // For Transition Screen
        public bool IsTransitioning { private set; get; } = false;
        public bool IsScreenLoaded { private set; get; } = false;

        private readonly float _delayAllowPauseMenuTime = 0.5f;
        private float _delayAllowPauseMenuTimer = 0f;

        private readonly float _transitionTime = 1.5f;  // Total transition time in seconds
        private readonly float _pauseTime = 0.5f;       // Pause time in seconds
        private float _transitionTimer;
        private float _transitionRadius;
        private const float _transitionSpeed = 30f;

        private ScreenManager()
        {
            _curScreen = new SplashScreen();
            CurrentScreen = GameScreen.SplashScreen;
        }

        public void Initialize(Game game, GraphicsDeviceManager graphicsDeviceManager)
        {
            Game = game;
            GraphicsDeviceManager = graphicsDeviceManager;
            GraphicsDevice = game.GraphicsDevice;      
            Window = game.Window;
            Camera = new Camera(GraphicsDevice.Viewport);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public void LoadContent()
        {
            _curScreen?.LoadContent();
        }

        public void UnloadContent()
        {
            _curScreen?.UnloadContent();
        }

        public void Dispose()
        {
            _curScreen?.Dispose();
        }

        public void TranstisionToScreen(GameScreen gameScreen)
        {
            switch (gameScreen)
            {
                case GameScreen.TestScreen:
                    ScreenTransitioningTo = GameScreen.TestScreen;
                    break;

                case GameScreen.SplashScreen:
                    ScreenTransitioningTo = GameScreen.SplashScreen;
                    break;

                case GameScreen.MainMenuScreen:
                    ScreenTransitioningTo = GameScreen.MainMenuScreen;
                    break;

                case GameScreen.Map1:
                    ScreenTransitioningTo = GameScreen.Map1;
                    break;
            }

            IsTransitioning = true;
            IsScreenLoaded = false;
            _transitionTimer = 0f;
            _transitionRadius = -0.01f;
        }

        private void LoadScreen()
        {
            // Reset companion summon
            PlayerManager.Instance.IsCompanionSummoned = false;

            _prevScreen = _curScreen;
            _prevScreen?.UnloadContent();
            _prevScreen = null;

            switch (ScreenTransitioningTo)
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

                case GameScreen.MainMenuScreen:
                    CurrentScreen = GameScreen.MainMenuScreen;
                    _curScreen = new MainMenuScreen();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.Map1:
                    CurrentScreen = GameScreen.Map1;
                    _curScreen = new Map1();
                    _curScreen.LoadContent();
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            GameGlobals.Instance.IsGameActive = Game.IsActive;

            // Only receive input if Game is Active
            if (Game.IsActive)
                PlayerManager.UpdateGameController();

            // Update UI
            if (CurrentScreen != GameScreen.SplashScreen)
                UserInterface.Active.Update(gameTime);

            if (!GameGlobals.Instance.IsGamePause)
                _prevScreen?.Update(gameTime);
                _curScreen?.Update(gameTime);

            // Current GUI Panel
            switch (GUIManager.Instance.CurrentGUI)
            {
                case GUIManager.PlayScreen:
                    // play screen ui
                    if (!GameGlobals.Instance.IsRefreshPlayScreenUI)
                    {
                        GUIManager.Instance.RefreshHotbar();
                        // Quest list
                        GUIManager.Instance.UpdateAfterChangeGUI();

                        GameGlobals.Instance.IsRefreshPlayScreenUI = true;

                        _delayAllowPauseMenuTimer = 0f;
                    }
                    else
                    {
                        if (_delayAllowPauseMenuTimer < _delayAllowPauseMenuTime)
                        {
                            _delayAllowPauseMenuTimer += deltaSeconds;
                        }
                        else GameGlobals.Instance.IsPauseMenuAllowed = true;
                    }
                    break;

                case GUIManager.InventoryPanel:
                    // Inventory
                    if (!GameGlobals.Instance.IsOpenInventoryPanel)
                    {
                        GUIManager.Instance.RefreshInvenrotyItem(false);
                        GUIManager.Instance.UpdateAfterChangeGUI();

                        GameGlobals.Instance.IsPauseMenuAllowed = false;
                        GameGlobals.Instance.IsOpenInventoryPanel = true;
                    }
                    break;

                case GUIManager.CraftingPanel:
                    // Crafting
                    if (!GameGlobals.Instance.IsOpenCraftingPanel)
                    {
                        GUIManager.Instance.RefreshCraftableItem(GUIManager.Instance.CurrentCraftingList);
                        GUIManager.Instance.UpdateAfterChangeGUI();

                        GameGlobals.Instance.IsPauseMenuAllowed = false;
                        GameGlobals.Instance.IsOpenCraftingPanel = true;
                    }
                    break;

                case GUIManager.InspectPanel:
                    // Inspecting Character
                    if (!GameGlobals.Instance.IsOpenInspectPanel)
                    {
                        GUIManager.Instance.RefreshInspectCharacterDisplay();
                        GUIManager.Instance.UpdateAfterChangeGUI();

                        GameGlobals.Instance.IsPauseMenuAllowed = false;
                        GameGlobals.Instance.IsOpenInspectPanel = true;
                    }
                    break;

                case GUIManager.MainMenu:
                    if (!GameGlobals.Instance.IsOpenMainMenu)
                    {
                        GUIManager.Instance.RefreshMainMenu();
                        GUIManager.Instance.UpdateAfterChangeGUI();

                        GameGlobals.Instance.IsOpenMainMenu = true;
                    }
                    break;

                case GUIManager.PauseMenu:
                    if (!GameGlobals.Instance.IsOpenPauseMenu)
                    {
                        GUIManager.Instance.UpdateAfterChangeGUI();

                        GameGlobals.Instance.IsOpenPauseMenu = true;
                    }
                    break;
            }

            UpdateTransitionScreen(deltaSeconds);
        }

        public void UpdateTransitionScreen(float deltaSeconds)
        {
            if (IsTransitioning)
            {
                _transitionTimer += deltaSeconds;

                if (_transitionTimer <= _transitionTime)
                {
                    // Transition in progress
                    _transitionRadius += deltaSeconds * _transitionSpeed;
                }
                else if (_transitionTimer >= _transitionTime && _transitionTimer <= _transitionTime + _pauseTime)
                {
                    // Pause after reaching full size and load new screen                
                    if (!IsScreenLoaded)
                    {
                        LoadScreen();
                        IsScreenLoaded = true;
                    }
                }
                else if (_transitionTimer > _transitionTime + _pauseTime)
                {
                    // Transition back in progress
                    _transitionRadius -= deltaSeconds * _transitionSpeed;
                }

                if (_transitionTimer >= (_transitionTime * 2) + _pauseTime)
                {
                    IsTransitioning = false;
                }
            }
        }

        public void Draw()
        {
            // Draw UI
            UserInterface.Active.Draw(_spriteBatch);       

            if (CurrentScreen == GameScreen.SplashScreen || CurrentScreen == GameScreen.MainMenuScreen)
            {
                GraphicsDevice.Clear(Color.White);

                _spriteBatch.Begin
                (
                    SpriteSortMode.Deferred,
                    samplerState: SamplerState.LinearClamp,
                    blendState: BlendState.AlphaBlend,
                    depthStencilState: DepthStencilState.None,
                    rasterizerState: RasterizerState.CullCounterClockwise,
                    transformMatrix: Camera.GetTransform(
                        GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
                );
            }
            else
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
            }

            // Draw on Screen here
            _curScreen.Draw(_spriteBatch);
            _spriteBatch.End();

            // OpenGUI Panel such as Invenotory, Character Inspect and Crafting Panel
            if (GameGlobals.Instance.IsOpenGUI)
            {
                _spriteBatch.Begin
                (
                    SpriteSortMode.Deferred,
                    samplerState: SamplerState.LinearClamp,
                    blendState: BlendState.AlphaBlend,
                    depthStencilState: DepthStencilState.None,
                    rasterizerState: RasterizerState.CullCounterClockwise,
                    transformMatrix: Camera.GetTransform(
                        GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
                );
                // Draw Background
                DrawBackgound(_spriteBatch, Color.Black, 0.75f);
                _spriteBatch.End();
            }

            // Draw render UI
            if (CurrentScreen != GameScreen.SplashScreen)
                UserInterface.Active.DrawMainRenderTarget(_spriteBatch);

            // Draw Selected Item on Hotbar
            if (CurrentScreen != GameScreen.SplashScreen || CurrentScreen != GameScreen.MainMenuScreen)
            {
                _spriteBatch.Begin
                (
                    SpriteSortMode.Deferred,
                    samplerState: SamplerState.LinearClamp,
                    blendState: BlendState.AlphaBlend,
                    depthStencilState: DepthStencilState.None,
                    rasterizerState: RasterizerState.CullCounterClockwise,
                    transformMatrix: Camera.GetTransform(
                        GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
                );

                HUDSystem.DrawOnTopUI(_spriteBatch);

                _spriteBatch.End();
            }

            // For transition game screen
            DrawTransitionScreen(_spriteBatch);
        }

        public void DrawTransitionScreen(SpriteBatch spriteBatch)
        {
            if (IsTransitioning)
            {
                var texture = GameGlobals.Instance.GetGuiTexture(GameGlobals.GuiTextureName.transition_texture);

                var position = Camera.GetViewportCenter(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

                // Draw the black circle
                spriteBatch.Begin
                (
                    SpriteSortMode.Immediate,
                    samplerState: SamplerState.LinearClamp,
                    blendState: BlendState.AlphaBlend,
                    depthStencilState: DepthStencilState.None,
                    rasterizerState: RasterizerState.CullCounterClockwise,
                    transformMatrix: Camera.GetTransform(
                        GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
                );          

                if (_transitionTimer < _transitionTime)
                {
                    // Transition in progress
                    spriteBatch.Draw(texture
                        , position - new Vector2((texture.Width * _transitionRadius / 2), (texture.Height * _transitionRadius / 2))
                        , null, Color.Black, 0f, Vector2.Zero, _transitionRadius, SpriteEffects.None, 0f);
                }
                else if (_transitionTimer >= _transitionTime && _transitionTimer <= _transitionTime + _pauseTime)
                {
                    // Pause after reaching full size
                    spriteBatch.Draw(texture
                        , position - new Vector2((texture.Width * _transitionRadius / 2), (texture.Height * _transitionRadius / 2))
                        , null, Color.Black, 0f, Vector2.Zero, _transitionRadius, SpriteEffects.None, 0f);
                }
                else if (_transitionTimer > _transitionTime + _pauseTime)
                {
                    // Transition back in progress
                    spriteBatch.Draw(texture
                        , position - new Vector2((texture.Width * _transitionRadius / 2), (texture.Height * _transitionRadius / 2))
                        , null, Color.Black, 0f, Vector2.Zero, _transitionRadius, SpriteEffects.None, 0f);
                }

                spriteBatch.End();
            }
        }

        public static void DrawBackgound(SpriteBatch spriteBatch, Color color, float transparency)
        {
            var screenOffSet = new Vector2(
                (GameGlobals.Instance.DefaultAdapterViewport.X - GameGlobals.Instance.GameScreen.X) / 6, 0);

            var position = Camera.GetViewportPosition() - screenOffSet;

            spriteBatch.FillRectangle(
                position.X, position.Y,
                GameGlobals.Instance.DefaultAdapterViewport.X,
                GameGlobals.Instance.DefaultAdapterViewport.Y,
                color * transparency);
        }

        public static LoadMapAction GetLoadMapAction(string zoneName)
        {
            if (Enum.TryParse(zoneName, true, out LoadMapAction enumValue))
                return enumValue;

            return LoadMapAction.LoadSave;
        }

        public GameScreen GetPlayScreenByLoadMapAction()
        {
            GameScreen gameScreen = CurrentScreen;

            switch (CurrentLoadMapAction)
            {
                case LoadMapAction.Test_to_map_1:
                    gameScreen = GameScreen.Map1;
                    break;

                case LoadMapAction.map_1_to_Test:
                    gameScreen = GameScreen.TestScreen;
                    break;
            }

            return gameScreen;
        }

        public static void StartGame(bool isNewGame)
        {
            GameGlobals.Instance.IsMainBGEnding = true;
            PlayerManager.Instance.Initialize(isNewGame);
            GameGlobals.Instance.InitialCameraPos = GameGlobals.Instance.GameScreenCenter;
            Instance.TranstisionToScreen(GameScreen.TestScreen);  // Base on current map
        }

        public static void ToggleFullScreen()
        {
            if (GameGlobals.Instance.IsFullScreen)
            {
                Instance.GraphicsDeviceManager.IsFullScreen = true;
                Instance.GraphicsDeviceManager.ApplyChanges();
            }
            else
            {
                Instance.GraphicsDeviceManager.IsFullScreen = false;
                Instance.GraphicsDeviceManager.ApplyChanges();

                Instance.GraphicsDeviceManager.IsFullScreen = false;
                Instance.GraphicsDeviceManager.ApplyChanges();
            }
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
