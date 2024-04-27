using GeonBit.UI;
using Medicraft.Screens;
using Medicraft.Screens.chapter_1;
using Medicraft.Screens.chapter_2;
using Medicraft.Screens.chapter_3;
using Medicraft.Screens.noahs_home;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Linq;

namespace Medicraft.Systems.Managers
{
    public class ScreenManager
    {
        private static ScreenManager instance;

        private SpriteBatch _spriteBatch;
        private Screen _curScreen, _prevScreen;
        private static GraphicsDevice _graphicsDevice;

        public Game Game { private set; get; }
        public GraphicsDevice GraphicsDevice { private set; get; }
        public GraphicsDeviceManager GraphicsDeviceManager { private set; get; }
        public GameWindow Window { private set; get; }
        public static Camera Camera { private set; get; }
     
        public enum GameScreen
        {
            None,
            TestScreen,
            StartMiniGame,
            SplashScreen,
            MainMenuScreen,
            NoahHome,
            NoahRoom,
            Map1,
            BattleZone1,
            Dungeon1,
            Map2,
            BattleZone2,
            Dungeon2,
            Map3,
            BattleZone3,
            Dungeon3
        }
        public GameScreen CurrentScreen { private set; get; }
        public GameScreen ScreenTransitioningTo { private set; get; }

        public enum EntranceZoneName
        {
            None,
            NewGame,
            LoadSave,
            warp_to_noah_home,
            noah_room_to_noah_home,
            noah_home_to_battlezone_1,
            noah_home_to_map_1,
            noah_home_to_noah_room,
            warp_to_map_1,
            map_1_to_noah_home,
            map_1_to_battlezone_1,
            battlezone_1_to_dungeon_1,
            battlezone_1_to_noah_home,
            battlezone_1_to_map_1,
            dungeon_1_to_map_2,
            dungeon_1_to_battlezone_1,
            warp_to_map_2,
            map_2_to_dungeon_1,
            map_2_to_battlezone_2,
            battlezone_2_to_map_2,
            battlezone_2_to_dungeon_2,
            dungeon_2_to_battlezone_2,
            dungeon_2_to_map_3,
            warp_to_map_3,
            map_3_to_dungeon_2,
            map_3_to_battlezone_3,
            battlezone_3_to_map_3,
            battlezone_3_to_dungeon_3,
            dungeon_3_to_battlezone_3
        }     
        public EntranceZoneName LoadMapByEntranceZone { set; get; }
        public string CurrentMap { set; get; }

        // For Transition Screen
        public bool IsTransitioning { private set; get; } = false;
        public bool DoActionAtMidTransition { set; get; } = false;
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
            _graphicsDevice = game.GraphicsDevice;
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

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            GameGlobals.Instance.IsGameActive = Game.IsActive;

            // Only receive input if Game is Active
            if (Game.IsActive)
            {
                PlayerManager.UpdateGameController(gameTime);
            }
            else
            {
                GameGlobals.Instance.CurMouse = new MouseState();
                GameGlobals.Instance.CurKeyboard = new KeyboardState();
            }

            // Update UI
            if (CurrentScreen != GameScreen.SplashScreen)
                UserInterface.Active.Update(gameTime);

            // Update Screen
            if (!GameGlobals.Instance.IsGamePause)
                _prevScreen?.Update(gameTime);
                _curScreen?.Update(gameTime);

            // Current GUI Panel
            switch (UIManager.Instance.CurrentUI)
            {
                case UIManager.PlayScreen:
                    // play screen ui
                    if (!GameGlobals.Instance.IsRefreshPlayScreenUI)
                    {
                        UIManager.Instance.RefreshHotbar();
                        // Quest list
                        UIManager.Instance.UpdateAfterChangeGUI();
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

                case UIManager.InventoryPanel:
                    // Inventory
                    if (!GameGlobals.Instance.IsOpenInventoryPanel)
                    {
                        UIManager.Instance.RefreshInvenrotyItem(false);
                        UIManager.Instance.UpdateAfterChangeGUI();
                        GameGlobals.Instance.IsPauseMenuAllowed = false;
                        GameGlobals.Instance.IsOpenInventoryPanel = true;
                    }
                    break;

                case UIManager.CraftingPanel:
                    // Crafting
                    if (!GameGlobals.Instance.IsOpenCraftingPanel)
                    {
                        UIManager.Instance.RefreshCraftableItem(UIManager.Instance.CurrentCraftingList);
                        UIManager.Instance.UpdateAfterChangeGUI();
                        GameGlobals.Instance.IsPauseMenuAllowed = false;
                        GameGlobals.Instance.IsOpenCraftingPanel = true;
                    }
                    break;

                case UIManager.InspectPanel:
                    // Inspecting Character
                    if (!GameGlobals.Instance.IsOpenInspectPanel)
                    {
                        UIManager.Instance.RefreshInspectCharacterDisplay();
                        UIManager.Instance.UpdateAfterChangeGUI();
                        GameGlobals.Instance.IsPauseMenuAllowed = false;
                        GameGlobals.Instance.IsOpenInspectPanel = true;
                    }
                    break;

                case UIManager.MainMenu:
                    if (!GameGlobals.Instance.IsOpenMainMenu)
                    {
                        UIManager.Instance.RefreshMainMenu();
                        UIManager.Instance.UpdateAfterChangeGUI();
                        GameGlobals.Instance.IsPauseMenuAllowed = false;
                        GameGlobals.Instance.IsOpenMainMenu = true;
                    }
                    break;

                case UIManager.PauseMenu:
                    if (!GameGlobals.Instance.IsOpenPauseMenu)
                    {
                        UIManager.Instance.RefreshPauseMenu();
                        UIManager.Instance.UpdateAfterChangeGUI();
                        GameGlobals.Instance.IsOpenPauseMenu = true;
                    }
                    break;

                case UIManager.SaveMenu:
                    if (!GameGlobals.Instance.IsOpenSaveMenuPanel)
                    {
                        UIManager.Instance.RefreshSaveMenu();
                        UIManager.Instance.UpdateAfterChangeGUI();
                        GameGlobals.Instance.IsPauseMenuAllowed = false;
                        GameGlobals.Instance.IsOpenSaveMenuPanel = true;
                    }
                    break;

                case UIManager.TradingPanel:
                    if (!GameGlobals.Instance.IsOpenTradingPanel)
                    {
                        UIManager.Instance.RefreshTradingItem("Buy Item");
                        UIManager.Instance.UpdateAfterChangeGUI();
                        GameGlobals.Instance.IsPauseMenuAllowed = false;
                        GameGlobals.Instance.IsOpenTradingPanel = true;
                    }
                    break;

                case UIManager.WarpPointPanel:
                    if (!GameGlobals.Instance.IsOpenWarpPointPanel)
                    {
                        UIManager.Instance.RefreshWarpPointUI();
                        UIManager.Instance.UpdateAfterChangeGUI();
                        GameGlobals.Instance.IsPauseMenuAllowed = false;
                        GameGlobals.Instance.IsOpenWarpPointPanel = true;
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
                    // Set IsTransitionAtMid
                    DoActionAtMidTransition = true;

                    // Pause after reaching full size and load new screen                
                    if (!IsScreenLoaded)
                    {
                        LoadScreen();
                        IsScreenLoaded = true;
                    }
                    else
                    {
                        if (_curScreen == null) return;

                        if (_curScreen is not PlayScreen playScreen) return;

                        playScreen.startingBG = true;
                        GameGlobals.Instance.IsMainBGEnding = false;
                        if (GameGlobals.Instance.CurrentMapMusics.Count != 0)
                        {
                            var mapBGMusic = GameGlobals.Instance.CurrentMapMusics.FirstOrDefault();
                            GameGlobals.PlayBackgroundMusic(mapBGMusic.Song, true, 1f);
                        }
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

                    if (ScreenTransitioningTo == GameScreen.MainMenuScreen)
                    {
                        EntityManager.Instance.ClearEntity();
                        ObjectManager.Instance.ClearGameObject();
                        QuestManager.Instance.QuestList.Clear();
                        InventoryManager.Instance.InventoryBag.Clear();
                        UIManager.Instance.ClearHotbar();
                        PlayerManager.Instance.Clear();                    
                        StatusEffectManager.Instance.StatusEffects.Clear();
                        StatusEffectManager.Instance.EffectsToRemove.Clear();
                        GameGlobals.Instance.InitGameSave();
                    }
                    else
                    {
                        // Invoke the event to notify listeners
                        OnTransitionScreen(new TransitionScreenEventArgs(ScreenTransitioningTo));
                    }
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
                // Draw Background
                DrawBackgound(_spriteBatch, Color.Black, 0.75f, true);
            }

            // BG for Dead Screen
            if (PlayerManager.Instance.IsPlayerDead)
            {
                DrawBackgound(_spriteBatch, Color.Red, 0.30f, true);
            }

            // Draw render UI
            if (CurrentScreen != GameScreen.SplashScreen)
                UserInterface.Active.DrawMainRenderTarget(_spriteBatch);

            // Draw Selected Item on Hotbar
            if (CurrentScreen != GameScreen.SplashScreen || CurrentScreen != GameScreen.MainMenuScreen)
            {          
                if (!UIManager.Instance.IsShowDialogUI && !PlayerManager.Instance.IsPlayerDead)
                    HUDSystem.DrawOnTopUI(_spriteBatch, _graphicsDevice);
            }

            // For transition game screen
            DrawTransitionScreen(_spriteBatch);
        }

        public void DrawTransitionScreen(SpriteBatch spriteBatch)
        {
            if (IsTransitioning)
            {
                var texture = GameGlobals.GetGuiTexture(GameGlobals.GuiTextureName.transition_texture);

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

        public static void DrawBackgound(SpriteBatch spriteBatch, Color color, float transparency, bool callEndspriteBatch)
        {
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                samplerState: SamplerState.LinearClamp,
                blendState: BlendState.AlphaBlend,
                depthStencilState: DepthStencilState.None,
                rasterizerState: RasterizerState.CullCounterClockwise,
                transformMatrix: Camera.GetTransform(
                    _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height));

            var screenOffSet = new Vector2(
                (GameGlobals.Instance.DefaultAdapterViewport.X - GameGlobals.Instance.GameScreen.X) / 6, 0);

            var position = Camera.GetViewportPosition() - screenOffSet;

            spriteBatch.FillRectangle(
                position.X, position.Y,
                GameGlobals.Instance.DefaultAdapterViewport.X,
                GameGlobals.Instance.DefaultAdapterViewport.Y,
                color * transparency);


            if (callEndspriteBatch)
                spriteBatch.End();
        }

        public static EntranceZoneName GetLoadMapAction(string zoneName)
        {
            if (Enum.TryParse(zoneName, true, out EntranceZoneName enumValue))
                return enumValue;

            return EntranceZoneName.LoadSave;
        }
            
        public GameScreen GetPlayScreenByLoadMapAction()
        {
            GameScreen gameScreen = CurrentScreen;

            switch (LoadMapByEntranceZone)
            {
                case EntranceZoneName.warp_to_noah_home:
                case EntranceZoneName.battlezone_1_to_noah_home:
                case EntranceZoneName.noah_room_to_noah_home:
                case EntranceZoneName.map_1_to_noah_home:
                    // NoahHome
                    gameScreen = GameScreen.NoahHome;
                    break;

                case EntranceZoneName.noah_home_to_noah_room:
                    // NoahRoom
                    gameScreen = GameScreen.NoahRoom;
                    break;

                case EntranceZoneName.warp_to_map_1:
                case EntranceZoneName.noah_home_to_map_1:
                case EntranceZoneName.battlezone_1_to_map_1:
                    // Map1
                    gameScreen = GameScreen.Map1;
                    break;

                case EntranceZoneName.noah_home_to_battlezone_1:
                case EntranceZoneName.map_1_to_battlezone_1:
                case EntranceZoneName.dungeon_1_to_battlezone_1:
                    // Battle Zone 1
                    gameScreen = GameScreen.BattleZone1;
                    break;

                case EntranceZoneName.battlezone_1_to_dungeon_1:
                case EntranceZoneName.map_2_to_dungeon_1:
                    // Dungeon 1
                    gameScreen = GameScreen.Dungeon1;
                    break;

                case EntranceZoneName.warp_to_map_2:
                case EntranceZoneName.dungeon_1_to_map_2:
                case EntranceZoneName.battlezone_2_to_map_2:
                    // Map2
                    gameScreen = GameScreen.Map2;
                    break;

                case EntranceZoneName.map_2_to_battlezone_2:
                case EntranceZoneName.dungeon_2_to_battlezone_2:
                    // Battle Zone 2
                    gameScreen = GameScreen.BattleZone2;
                    break;

                case EntranceZoneName.battlezone_2_to_dungeon_2:
                case EntranceZoneName.map_3_to_dungeon_2:
                    // Dungeon 2
                    gameScreen = GameScreen.Dungeon2;
                    break;

                case EntranceZoneName.warp_to_map_3:
                case EntranceZoneName.dungeon_2_to_map_3:
                case EntranceZoneName.battlezone_3_to_map_3:
                    // Map3
                    gameScreen = GameScreen.Map3;
                    break;

                case EntranceZoneName.map_3_to_battlezone_3:
                case EntranceZoneName.dungeon_3_to_battlezone_3:
                    // Battle Zone 3
                    gameScreen = GameScreen.BattleZone3;
                    break;

                case EntranceZoneName.battlezone_3_to_dungeon_3:
                    // Dungeon 3
                    gameScreen = GameScreen.Dungeon3;
                    break;
            }

            return gameScreen;
        }

        public void TransitionToScreen(GameScreen gameScreen)
        {
            switch (gameScreen)
            {
                case GameScreen.None:
                    ScreenTransitioningTo = GameScreen.None;
                    break;

                case GameScreen.StartMiniGame:
                    ScreenTransitioningTo = GameScreen.StartMiniGame;
                    break;

                case GameScreen.TestScreen:
                    ScreenTransitioningTo = GameScreen.TestScreen;
                    break;

                case GameScreen.SplashScreen:
                    ScreenTransitioningTo = GameScreen.SplashScreen;
                    break;

                case GameScreen.MainMenuScreen:
                    ScreenTransitioningTo = GameScreen.MainMenuScreen;
                    break;

                case GameScreen.NoahHome:
                    ScreenTransitioningTo = GameScreen.NoahHome;
                    break;

                case GameScreen.NoahRoom:
                    ScreenTransitioningTo = GameScreen.NoahRoom;
                    break;

                case GameScreen.Map1:
                    ScreenTransitioningTo = GameScreen.Map1;
                    break;

                case GameScreen.BattleZone1:
                    ScreenTransitioningTo = GameScreen.BattleZone1;
                    break;

                case GameScreen.Dungeon1:
                    ScreenTransitioningTo = GameScreen.Dungeon1;
                    break;

                case GameScreen.Map2:
                    ScreenTransitioningTo = GameScreen.Map2;
                    break;

                case GameScreen.BattleZone2:
                    ScreenTransitioningTo = GameScreen.BattleZone2;
                    break;

                case GameScreen.Dungeon2:
                    ScreenTransitioningTo = GameScreen.Dungeon2;
                    break;

                case GameScreen.Map3:
                    ScreenTransitioningTo = GameScreen.Map3;
                    break;

                case GameScreen.BattleZone3:
                    ScreenTransitioningTo = GameScreen.BattleZone3;
                    break;

                case GameScreen.Dungeon3:
                    ScreenTransitioningTo = GameScreen.Dungeon3;
                    break;
            }

            EntityManager.Instance.ClearAggroMobs();
            GameGlobals.Instance.IsMainBGEnding = true;
            IsTransitioning = true;
            DoActionAtMidTransition = false;
            IsScreenLoaded = gameScreen == GameScreen.None || gameScreen == GameScreen.StartMiniGame;
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

                case GameScreen.NoahHome:
                    CurrentScreen = GameScreen.NoahHome;
                    _curScreen = new NoahHome();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.NoahRoom:
                    CurrentScreen = GameScreen.NoahRoom;
                    _curScreen = new NoahRoom();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.Map1:
                    CurrentScreen = GameScreen.Map1;
                    _curScreen = new Map1();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.BattleZone1:
                    CurrentScreen = GameScreen.BattleZone1;
                    _curScreen = new BattleZone1();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.Dungeon1:
                    CurrentScreen = GameScreen.Dungeon1;
                    _curScreen = new Dungeon1();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.Map2:
                    CurrentScreen = GameScreen.Map2;
                    _curScreen = new Map2();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.BattleZone2:
                    CurrentScreen = GameScreen.BattleZone2;
                    _curScreen = new BattleZone2();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.Dungeon2:
                    CurrentScreen = GameScreen.Dungeon2;
                    _curScreen = new Dungeon2();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.Map3:
                    CurrentScreen = GameScreen.Map3;
                    _curScreen = new Map3();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.BattleZone3:
                    CurrentScreen = GameScreen.BattleZone3;
                    _curScreen = new BattleZone3();
                    _curScreen.LoadContent();
                    break;

                case GameScreen.Dungeon3:
                    CurrentScreen = GameScreen.Dungeon3;
                    _curScreen = new Dungeon3();
                    _curScreen.LoadContent();
                    break;
            }
        }

        public static void StartGame(bool isNewGame)
        {
            PlayerManager.Instance.Initialize(isNewGame);
            GameGlobals.Instance.InitialCameraPos = GameGlobals.Instance.GameScreenCenter;

            switch (PlayerManager.Instance.Player.PlayerData.CurrentMap)
            {
                case "Test":
                    Instance.TransitionToScreen(GameScreen.TestScreen);
                    break;

                case "noah_home":
                    Instance.TransitionToScreen(GameScreen.NoahHome);
                    break;

                case "noah_room":
                    Instance.TransitionToScreen(GameScreen.NoahRoom);
                    break;

                case "map_1":
                    Instance.TransitionToScreen(GameScreen.Map1);
                    break;

                case "battlezone_1":
                    Instance.TransitionToScreen(GameScreen.BattleZone1);
                    break;

                case "dungeon_1":
                    Instance.TransitionToScreen(GameScreen.Dungeon1);
                    break;

                case "map_2":
                    Instance.TransitionToScreen(GameScreen.Map2);
                    break;

                case "battlezone_2":
                    Instance.TransitionToScreen(GameScreen.BattleZone2);
                    break;

                case "dungeon_2":
                    Instance.TransitionToScreen(GameScreen.Dungeon2);
                    break;

                case "map_3":
                    Instance.TransitionToScreen(GameScreen.Map3);
                    break;

                case "battlezone_3":
                    Instance.TransitionToScreen(GameScreen.BattleZone3);
                    break;

                case "dungeon_3":
                    Instance.TransitionToScreen(GameScreen.Dungeon3);
                    break;
            }
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

        // Define a delegate for the event handler
        public delegate void ScreenEventHandler(object sender, EventArgs e);

        // Define an event based on the delegate
        public event ScreenEventHandler EventHandler;

        // Method to raise the event
        public virtual void OnTransitionScreen(TransitionScreenEventArgs e)
        {
            EventHandler?.Invoke(this, e);
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

    public class TransitionScreenEventArgs(ScreenManager.GameScreen gameScreen) : EventArgs
    {
        public ScreenManager.GameScreen GameScreen { get; } = gameScreen;
    }
}
