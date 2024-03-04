using Medicraft.Data;
using Medicraft.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;
using System.Linq;

namespace Medicraft.Systems.Managers
{
    public class PlayerManager
    {
        public Player Player { private set; get; }
        public bool IsPlayerDead { private set; get; }

        private static PlayerManager instance;
        private PlayerManager()
        {
            IsPlayerDead = false;
        }

        public void Initialize()
        {
            var initialPlayerData = GameGlobals.Instance.InitialPlayerData;
            var playerSprite = new AnimatedSprite(GameGlobals.Instance.PlayerAnimation);

            if (GameGlobals.Instance.GameSave.Count != 0)
            {
                ScreenManager.Instance.CurrentLoadMapAction = ScreenManager.LoadMapAction.LoadGameSave;

                // Load save game according to selected index. 
                var gameSave = GameGlobals.Instance.GameSave[GameGlobals.Instance.GameSaveIdex];

                // Set Total Playtime
                GameGlobals.Instance.TotalPlayTime = gameSave.TotalPlayTime[0] * 3600 
                                                    + gameSave.TotalPlayTime[1] * 60
                                                    + gameSave.TotalPlayTime[2];

                // Initialize Player's Inventory
                var inventoryData = gameSave.PlayerData.InventoryData;
                InventoryManager.Instance.InitializeInventory(inventoryData);

                // Initial Player
                var basePlayerData = gameSave.PlayerData;

                ScreenManager.Instance.CurrentMap = basePlayerData.CurrentMap;

                Player = new Player(playerSprite, basePlayerData);

                // Adjust HUD and camera positions
                GameGlobals.Instance.TopLeftCornerPosition = Player.Position - GameGlobals.Instance.GameScreenCenter;
                GameGlobals.Instance.InitialCameraPos = Player.Position;
                GameGlobals.Instance.AddingCameraPos = Vector2.Zero;
            }
            else // In case New Game
            {
                ScreenManager.Instance.CurrentLoadMapAction = ScreenManager.LoadMapAction.NewGame;

                // Initial Player
                Player = new Player(playerSprite, initialPlayerData);

                // Initialize camera position
                GameGlobals.Instance.InitialCameraPos = new Vector2((float)initialPlayerData.Position[0]
                    , (float)initialPlayerData.Position[1]);

                // Initialize Player's Inventory
                var inventoryData = initialPlayerData.InventoryData;
                InventoryManager.Instance.InitializeInventory(inventoryData);
            }

            // Other Stuff
            SetPlayerEXPCap(Player.Level);
            GameGlobals.Instance.TopLeftCornerPosition = Player.Position - GameGlobals.Instance.GameScreenCenter;
        }

        public static void UpdateGameController(GameTime gameTime)
        {
            // Key Control
            GameGlobals.Instance.PrevKeyboard = GameGlobals.Instance.CurKeyboard;
            GameGlobals.Instance.CurKeyboard = Keyboard.GetState();
            var keyboardCur = GameGlobals.Instance.CurKeyboard;
            var keyboardPrev = GameGlobals.Instance.PrevKeyboard;

            // Mouse Control
            GameGlobals.Instance.PrevMouse = GameGlobals.Instance.CurMouse;
            GameGlobals.Instance.CurMouse = Mouse.GetState();
            var mouseCur = GameGlobals.Instance.CurMouse;
            var mousePrev = GameGlobals.Instance.PrevMouse;

            // Save Game for Test
            if (keyboardCur.IsKeyUp(Keys.M) && keyboardPrev.IsKeyDown(Keys.M))
            {
                JsonFileManager.SaveGame();
            }

            // Open Inventory
            if (keyboardCur.IsKeyDown(Keys.I) && !GameGlobals.Instance.SwitchOpenInventory)
            {
                // Toggle the IsOpenInventory flag               
                GameGlobals.Instance.IsOpenInventory = !GameGlobals.Instance.IsOpenInventory;

                // Pause PlayScreen
                GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;

                GameGlobals.Instance.SwitchOpenInventory = true;
            }
            else if (keyboardCur.IsKeyUp(Keys.I))
            {
                GameGlobals.Instance.SwitchOpenInventory = false;
            }

            // Select Item Bar Slot
            if (ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.TestScreen
                || ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.PlayScreen)
            {
                if (keyboardCur.IsKeyDown(Keys.D1))
                {
                    GameGlobals.Instance.SelectedItemBarSlot = 0;
                }
                else if (keyboardCur.IsKeyDown(Keys.D2))
                {
                    GameGlobals.Instance.SelectedItemBarSlot = 1;
                }
                else if (keyboardCur.IsKeyDown(Keys.D3))
                {
                    GameGlobals.Instance.SelectedItemBarSlot = 2;
                }
                else if (keyboardCur.IsKeyDown(Keys.D4))
                {
                    GameGlobals.Instance.SelectedItemBarSlot = 3;
                }
                else if (keyboardCur.IsKeyDown(Keys.D5))
                {
                    GameGlobals.Instance.SelectedItemBarSlot = 4;
                }
                else if (keyboardCur.IsKeyDown(Keys.D6))
                {
                    GameGlobals.Instance.SelectedItemBarSlot = 5;
                }
                else if (keyboardCur.IsKeyDown(Keys.D7))
                {
                    GameGlobals.Instance.SelectedItemBarSlot = 6;
                }
                else if (keyboardCur.IsKeyDown(Keys.D8))
                {
                    GameGlobals.Instance.SelectedItemBarSlot = 7;
                }
            }

            // Check if CurrentScreen is TestScreen
            if (ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.TestScreen)
            {
                // Debug Mode
                if (keyboardCur.IsKeyDown(Keys.B) && !GameGlobals.Instance.SwitchDebugMode)
                {
                    // Toggle the IsShowDetectBox flag
                    GameGlobals.Instance.IsDebugMode = !GameGlobals.Instance.IsDebugMode;

                    // Update the boolean variable to indicate that the "B" button has been pressed
                    GameGlobals.Instance.SwitchDebugMode = true;
                }
                else if (keyboardCur.IsKeyUp(Keys.B))
                {
                    // Update the boolean variable to indicate that the "B" button is not currently pressed
                    GameGlobals.Instance.SwitchDebugMode = false;
                }

                // Show Path Finding of Mobs
                if (keyboardCur.IsKeyDown(Keys.V) && !GameGlobals.Instance.SwitchShowPath)
                {
                    GameGlobals.Instance.IsShowPath = !GameGlobals.Instance.IsShowPath;

                    GameGlobals.Instance.SwitchShowPath = true;
                }
                else if (keyboardCur.IsKeyUp(Keys.V))
                {
                    GameGlobals.Instance.SwitchShowPath = false;
                }

                // Full Screen On/Off               
                if ((keyboardCur.IsKeyUp(Keys.PageUp) && keyboardPrev.IsKeyDown(Keys.PageUp))
                    && !GameGlobals.Instance.SwitchFullScreen)
                {
                    GameGlobals.Instance.SwitchFullScreen = !GameGlobals.Instance.SwitchFullScreen;
                    GameGlobals.Instance.IsFullScreen = true;

                    ScreenManager.Instance.GraphicsDeviceManager.IsFullScreen = true;
                    ScreenManager.Instance.GraphicsDeviceManager.ApplyChanges();
                }
                else if ((keyboardCur.IsKeyUp(Keys.PageUp) && keyboardPrev.IsKeyDown(Keys.PageUp))
                    && GameGlobals.Instance.SwitchFullScreen)
                {
                    GameGlobals.Instance.SwitchFullScreen = !GameGlobals.Instance.SwitchFullScreen;
                    GameGlobals.Instance.IsFullScreen = false;

                    ScreenManager.Instance.GraphicsDeviceManager.IsFullScreen = false;
                    ScreenManager.Instance.GraphicsDeviceManager.ApplyChanges();

                    // do it again
                    ScreenManager.Instance.GraphicsDeviceManager.IsFullScreen = false;
                    ScreenManager.Instance.GraphicsDeviceManager.ApplyChanges();
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            // Key Control
            var keyboardCur = GameGlobals.Instance.CurKeyboard;
            var keyboardPrev = GameGlobals.Instance.PrevKeyboard;

            // Mouse Control
            var mouseCur = GameGlobals.Instance.CurMouse;
            var mousePrev = GameGlobals.Instance.PrevMouse;

            Player.Update(gameTime, keyboardCur, keyboardPrev, mouseCur, mousePrev);

            // Check Player HP for Deadq
            if (Player.HP <= 0 && !IsPlayerDead)
            {
                IsPlayerDead = true;
            }

            if (IsPlayerDead)
            {
                if (GameGlobals.Instance.CurMouse.LeftButton == ButtonState.Pressed
                    && GameGlobals.Instance.PrevMouse.LeftButton == ButtonState.Released)
                {
                    RespawnPlayer();
                }
            }
        }

        public void UpdateMapPositionData()
        {
            
        }

        public void SetupPlayerPosition(ScreenManager.LoadMapAction loadAction)
        {
            if (loadAction == ScreenManager.LoadMapAction.LoadGameSave) return;

            var curMap = ScreenManager.Instance.CurrentMap;
            Player.PlayerData.CurrentMap = curMap;

            var mapPositionData = GameGlobals.Instance.MapLocationPointDatas.Where(m => m.Name.Equals(curMap));
            var positionData = mapPositionData.ElementAt(0).Positions.Where(p => p.Name.Equals("Respawn"));
            var position = new Vector2(
                (float)positionData.ElementAt(0).Value[0],
                (float)positionData.ElementAt(0).Value[1]);

            Player.Position = position;

            // Adjust HUD and camera positions
            GameGlobals.Instance.TopLeftCornerPosition = Player.Position - GameGlobals.Instance.GameScreenCenter;
            GameGlobals.Instance.InitialCameraPos = Player.Position;
            GameGlobals.Instance.AddingCameraPos = Vector2.Zero;
        }

        private void RespawnPlayer()
        {
            if (IsPlayerDead)
            {
                Player.HP = Player.MaxHP;

                var curMap = ScreenManager.Instance.CurrentMap;
                var mapPositionData = GameGlobals.Instance.MapLocationPointDatas.Where(m => m.Name.Equals(curMap));
                var positionData = mapPositionData.ElementAt(0).Positions.Where(p => p.Name.Equals("Respawn"));
                var position = new Vector2(
                    (float)positionData.ElementAt(0).Value[0],
                    (float)positionData.ElementAt(0).Value[1]);

                Player.Position = position;

                // Adjust HUD and camera positions
                GameGlobals.Instance.TopLeftCornerPosition = Player.Position - GameGlobals.Instance.GameScreenCenter;
                GameGlobals.Instance.InitialCameraPos = Player.Position;
                GameGlobals.Instance.AddingCameraPos = Vector2.Zero;

                var entities = EntityManager.Instance.Entities;
                foreach (var entity in entities.Where(e => !e.IsDestroyed))
                {
                    entity.AggroTimer = 0f;
                }

                IsPlayerDead = false;
                Player.IsDying = false;
            }
        }

        public void SetPlayerEXPCap(int level)
        {
            var ExpCapData = GameGlobals.Instance.ExperienceCapacityDatas
                .Where(expcap => expcap.Level.Equals(level)).ElementAt(0);

            Player.EXPMaxCap = ExpCapData.MaxCap;
        }

        public void AddPlayerEXP(int exp)
        {
            Player.EXP += exp;

            // Increase Level & Set ExpMaxCap
            if (Player.EXP >= Player.EXPMaxCap)
            {
                Player.Level++;
                Player.EXP -= Player.EXPMaxCap;

                SetPlayerEXPCap(Player.Level);
            }
        }

        public static PlayerManager Instance
        {
            get
            {
                instance ??= new PlayerManager();
                return instance;
            }
        }
    }
}
