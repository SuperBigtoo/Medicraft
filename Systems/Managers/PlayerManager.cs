using Medicraft.Data.Models;
using Medicraft.Entities;
using Medicraft.Entities.Companion;
using Medicraft.Entities.Mobs;
using Medicraft.Entities.Mobs.Friendly;
using Medicraft.GameObjects;
using Medicraft.Systems.PathFinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.Systems.Managers
{
    public class PlayerManager
    {
        // Player: Noah
        public Player Player { private set; get; }
        public bool IsPlayerDead { private set; get; }
        public bool IsRespawning { set; get; }
        public KeyValuePair<int, InventoryItemData> SelectedHotbarItem { private set; get; }

        // Object
        public bool IsDetectedObject { private set; get; }
        public GameObject DetectedObject { private set; get; }

        // Mobs : Friendly
        public bool IsDetectedInteractableMob { private set; get; }
        public FriendlyMob DetectedInteractableMob { private set; get; }

        // Companions
        public List<Companion> Companions { private set; get; }
        public int CurrCompaIndex { private set; get; }
        public bool IsCompanionSummoned { set; get; }
        public bool IsCompanionDead { set; get; }
        public bool IsRecallCompanion { set; get; }
        private const float recallTime = 3f;
        private float _recallTimer = 0;

        private static PlayerManager instance;
        private PlayerManager()
        {
            IsDetectedObject = false;
            IsRespawning = false;
            IsPlayerDead = false;

            Companions = [];
            CurrCompaIndex = 0;
            IsCompanionSummoned = false;
            IsCompanionDead = false;
            IsRecallCompanion = false;
        }

        public void Initialize(bool isNewGame)
        {
            var initialPlayerData = GameGlobals.Instance.InitialPlayerData;
            var playerSprite = new AnimatedSprite(GameGlobals.Instance.PlayerSpriteSheet);


            if (!isNewGame)
            {
                ScreenManager.Instance.CurrentLoadMapAction = ScreenManager.LoadMapAction.LoadSave;

                // Load save game according to selected index. 
                var gameSaveData = GameGlobals.Instance.GameSave[GameGlobals.Instance.SelectedGameSaveIndex];

                // Set Total Playtime
                GameGlobals.Instance.TotalPlayTime = gameSaveData.TotalPlayTime[0] * 3600 
                                                    + gameSaveData.TotalPlayTime[1] * 60
                                                    + gameSaveData.TotalPlayTime[2];

                // Initialize Spawner Data
                GameGlobals.Instance.SetSpawnerDatas(gameSaveData.SpawnerDatas);

                // Initialize Player's Inventory
                var inventoryData = gameSaveData.PlayerData.InventoryData;
                InventoryManager.Instance.InitializeInventory(inventoryData);

                // Initial Player
                var basePlayerData = gameSaveData.PlayerData;
                ScreenManager.Instance.CurrentMap = basePlayerData.CurrentMap;
                Player = new Player(playerSprite, basePlayerData);            
            }
            else // In case New Game
            {
                ScreenManager.Instance.CurrentLoadMapAction = ScreenManager.LoadMapAction.NewGame;

                // Initialize Spawner Data
                GameGlobals.Instance.SetSpawnerDatas(
                    GameGlobals.Instance.Content.Load<List<SpawnerData>>("data/models/spawnersdata"));

                // Initialize Player's Inventory
                var inventoryData = initialPlayerData.InventoryData;
                InventoryManager.Instance.InitializeInventory(inventoryData);

                // Initial Player
                Player = new Player(playerSprite, initialPlayerData);

                // Initialize camera position
                GameGlobals.Instance.InitialCameraPos = new Vector2((float)initialPlayerData.Position[0]
                    , (float)initialPlayerData.Position[1]);
            }

            // Other Stuff
            SetPlayerExpMaxCap(Player.Level);
            GameGlobals.Instance.TopLeftCornerPos = Player.Position - GameGlobals.Instance.GameScreenCenter;

            SelectedHotbarItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                (i => i.Value.Slot.Equals(InventoryManager.HotbarSlot_1));

            // Initialize equipment stats
            var itemEquipmentData = InventoryManager.Instance.InventoryBag.Values.Where
                (e => e.Slot >= 0 && e.Slot < 6);

            foreach (var item in itemEquipmentData)
                RefreshEquipmentStats(item, true);

            // Initialize Companion
            InitializeCompanion();

            // Initialize Crop
            ObjectManager.Instance.InitCropObject(Player.PlayerData.Crops);

            // Initialize display inventory item after init Player's inventory data
            UIManager.Instance.InitInventoryItemDisplay();
            UIManager.Instance.InitCraftableItemDisplay();
        }

        private void InitializeCompanion()
        {
            Companions.Clear();

            var spriteSheet = GameGlobals.Instance.CompanionSpriteSheet;
            var indexCompa = 0;

            if (Player.PlayerData.Companions != null || Player.PlayerData.Companions.Count != 0)
                foreach (var compaData in Player.PlayerData.Companions)
                {
                    switch (compaData.CharId)
                    {
                        case 1:
                            // Violet
                            Companions.Add(new Violet(new AnimatedSprite(spriteSheet[indexCompa]), compaData, Vector2.One, indexCompa++));
                            break;

                        case 2:
                            break;

                        case 3:
                            break;
                    }
                }

        }

        public void SummonCurrentCompanion()
        {
            if (Companions.Count != 0)
            {
                foreach (var compa in Companions)
                    compa.CompanionData.IsSummoned = false;

                Companions[CurrCompaIndex].Position = Player.Position;
                Companions[CurrCompaIndex].pathFinding = new AStar(
                    (int)Companions[CurrCompaIndex].BoundingDetectCollisions.Center.X,
                    (int)Companions[CurrCompaIndex].BoundingDetectCollisions.Center.Y,
                    (int)Player.BoundingDetectCollisions.Center.X,
                    (int)Player.BoundingDetectCollisions.Center.Y);

                Companions[CurrCompaIndex].CompanionData.IsSummoned = true;
                IsCompanionDead = false;

                PlaySoundEffect(Sound.Onmtp_Inspiration08_1);
            }
        }

        public static void UpdateGameController(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;         

            // Key Control
            GameGlobals.Instance.PrevKeyboard = GameGlobals.Instance.CurKeyboard;
            GameGlobals.Instance.CurKeyboard = Keyboard.GetState();

            // Mouse Control
            GameGlobals.Instance.PrevMouse = GameGlobals.Instance.CurMouse;
            GameGlobals.Instance.CurMouse = Mouse.GetState();
            var keyboardCur = GameGlobals.Instance.CurKeyboard;
            var keyboardPrev = GameGlobals.Instance.PrevKeyboard;
            var mouseCur = GameGlobals.Instance.CurMouse;
            var mousePrev = GameGlobals.Instance.PrevMouse;

            var saveGameKey = GameGlobals.Instance.SaveGameKeyForTest;
            var recallCompaKey = GameGlobals.Instance.RecallCompanionKey;
            var pauseMenuKey = GameGlobals.Instance.PauseMenuKey;
            var invenKey = GameGlobals.Instance.OpenInventoryKey;
            var craftingKey = GameGlobals.Instance.OpenCraftingKey;
            var inspectKey = GameGlobals.Instance.OpenInspectKey;
            var debugModeKey = GameGlobals.Instance.DebugModeKey;
            var showPathFindingKey = GameGlobals.Instance.ShowPathFindingKey;

            // Only receive input if on PlayScreen and not showing Dialog
            if (!ScreenManager.Instance.IsTransitioning && !UIManager.Instance.IsShowDialogUI
                && ScreenManager.Instance.CurrentScreen != ScreenManager.GameScreen.SplashScreen
                && ScreenManager.Instance.CurrentScreen != ScreenManager.GameScreen.MainMenuScreen)
            {
                // Open SaveMenu
                if (keyboardCur.IsKeyDown(saveGameKey) && !GameGlobals.Instance.SwitchOpenSaveMenuPanel && !GameGlobals.Instance.IsOpenGUI && ScreenManager.Instance.CurrentMap.Equals("Test")
                    || (keyboardCur.IsKeyDown(saveGameKey) || keyboardCur.IsKeyDown(pauseMenuKey)) && !GameGlobals.Instance.SwitchOpenSaveMenuPanel
                    && GameGlobals.Instance.IsOpenGUI && UIManager.Instance.CurrentUI.Equals(UIManager.SaveMenu))
                {
                    GameGlobals.Instance.SwitchOpenSaveMenuPanel = true;

                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenSaveMenuPanel              
                    GameGlobals.Instance.IsOpenSaveMenuPanel = false;
                    if (UIManager.Instance.CurrentUI.Equals(UIManager.SaveMenu))
                    {
                        UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                        GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                        PlaySoundEffect(Sound.Cancel1);
                    }
                    else
                    {
                        UIManager.Instance.CurrentUI = UIManager.SaveMenu;
                        PlaySoundEffect(Sound.Click1);
                    }
                }
                else if (keyboardCur.IsKeyUp(saveGameKey))
                {
                    GameGlobals.Instance.SwitchOpenSaveMenuPanel = false;
                }

                // Call Companion to follow Player
                if (!GameGlobals.Instance.IsOpenGUI && keyboardCur.IsKeyDown(recallCompaKey))
                {
                    Instance.IsRecallCompanion = true;
                }
                else if (!GameGlobals.Instance.IsOpenGUI && keyboardCur.IsKeyUp(recallCompaKey))
                {
                    if (Instance._recallTimer < recallTime)
                    {
                        Instance._recallTimer += deltaSeconds;
                    }
                    else
                    {
                        Instance._recallTimer = 0;
                        Instance.IsRecallCompanion = false;
                    }
                }

                // Open Pause Menu
                if (keyboardCur.IsKeyDown(pauseMenuKey) && !GameGlobals.Instance.SwitchOpenPauseMenuPanel && !GameGlobals.Instance.IsOpenGUI && GameGlobals.Instance.IsPauseMenuAllowed
                    || keyboardCur.IsKeyDown(pauseMenuKey) && !GameGlobals.Instance.SwitchOpenPauseMenuPanel && GameGlobals.Instance.IsOpenGUI
                    && UIManager.Instance.CurrentUI.Equals(UIManager.PauseMenu) && GameGlobals.Instance.IsPauseMenuAllowed)
                {
                    GameGlobals.Instance.SwitchOpenPauseMenuPanel = true;

                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenPauseMenu              
                    GameGlobals.Instance.IsOpenPauseMenu = false;
                    if (UIManager.Instance.CurrentUI.Equals(UIManager.PauseMenu))
                    {
                        UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                        GameGlobals.Instance.IsRefreshPlayScreenUI = false;

                        PlaySoundEffect(Sound.Unpause);
                    }
                    else
                    {
                        UIManager.Instance.CurrentUI = UIManager.PauseMenu;

                        PlaySoundEffect(Sound.Pause);
                    }
                }
                else if (keyboardCur.IsKeyUp(pauseMenuKey))
                {
                    GameGlobals.Instance.SwitchOpenPauseMenuPanel = false;
                }

                // Open Inventory
                if (keyboardCur.IsKeyDown(invenKey) && !GameGlobals.Instance.SwitchOpenInventoryPanel && !GameGlobals.Instance.IsOpenGUI
                    || (keyboardCur.IsKeyDown(invenKey) || keyboardCur.IsKeyDown(pauseMenuKey)) && !GameGlobals.Instance.SwitchOpenInventoryPanel
                    && GameGlobals.Instance.IsOpenGUI && UIManager.Instance.CurrentUI.Equals(UIManager.InventoryPanel))
                {
                    GameGlobals.Instance.SwitchOpenInventoryPanel = true;

                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenInvenoryPanel & refresh inventory item display              
                    GameGlobals.Instance.IsOpenInventoryPanel = false;
                    if (UIManager.Instance.CurrentUI.Equals(UIManager.InventoryPanel))
                    {
                        UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                        GameGlobals.Instance.IsRefreshPlayScreenUI = false;

                        PlaySoundEffect(Sound.PickUpBag);
                    }
                    else
                    {
                        UIManager.Instance.CurrentUI = UIManager.InventoryPanel;

                        PlaySoundEffect(Sound.PickUpBag);
                    }
                }
                else if (keyboardCur.IsKeyUp(invenKey))
                {
                    GameGlobals.Instance.SwitchOpenInventoryPanel = false;
                }

                // Open Crafting Panel 
                if (keyboardCur.IsKeyDown(craftingKey) && !GameGlobals.Instance.SwitchOpenCraftingPanel && !GameGlobals.Instance.IsOpenGUI && ScreenManager.Instance.CurrentMap.Equals("Test")
                    || (keyboardCur.IsKeyDown(craftingKey) || keyboardCur.IsKeyDown(pauseMenuKey)) && !GameGlobals.Instance.SwitchOpenCraftingPanel
                    && GameGlobals.Instance.IsOpenGUI && UIManager.Instance.CurrentUI.Equals(UIManager.CraftingPanel))
                {
                    GameGlobals.Instance.SwitchOpenCraftingPanel = true;

                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenCraftingPanel & refresh crafting item display       
                    GameGlobals.Instance.IsOpenCraftingPanel = false;
                    if (UIManager.Instance.CurrentUI.Equals(UIManager.CraftingPanel))
                    {
                        UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                        GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                        PlaySoundEffect(Sound.Cancel1);
                    }
                    else
                    {
                        UIManager.Instance.CurrentUI = UIManager.CraftingPanel;
                        PlaySoundEffect(Sound.Click1);
                    }
                }
                else if (keyboardCur.IsKeyUp(craftingKey))
                {
                    GameGlobals.Instance.SwitchOpenCraftingPanel = false;
                }

                // Open Inspect Panel 
                if (keyboardCur.IsKeyDown(inspectKey) && !GameGlobals.Instance.SwitchOpenInspectPanel && !GameGlobals.Instance.IsOpenGUI
                    || (keyboardCur.IsKeyDown(inspectKey) || keyboardCur.IsKeyDown(pauseMenuKey)) && !GameGlobals.Instance.SwitchOpenInspectPanel
                    && GameGlobals.Instance.IsOpenGUI && UIManager.Instance.CurrentUI.Equals(UIManager.InspectPanel))
                {
                    GameGlobals.Instance.SwitchOpenInspectPanel = true;

                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenInspectPanel      
                    GameGlobals.Instance.IsOpenInspectPanel = false;
                    UIManager.Instance.IsCharacterTabSelected = true;
                    if (UIManager.Instance.CurrentUI.Equals(UIManager.InspectPanel))
                    {
                        UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                        GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                        UIManager.Instance.ClearSkillDescription("Character");
                        PlaySoundEffect(Sound.Cancel1);
                    }
                    else
                    {
                        UIManager.Instance.CurrentUI = UIManager.InspectPanel;
                        PlaySoundEffect(Sound.Click1);
                    }
                }
                else if (keyboardCur.IsKeyUp(inspectKey))
                {
                    GameGlobals.Instance.SwitchOpenInspectPanel = false;
                }

                // Close Trading Panel
                if ((keyboardCur.IsKeyDown(Keys.T) || keyboardCur.IsKeyDown(pauseMenuKey)) && !GameGlobals.Instance.SwitchOpenTradingPanel
                    && GameGlobals.Instance.IsOpenGUI && UIManager.Instance.CurrentUI.Equals(UIManager.TradingPanel))
                {
                    GameGlobals.Instance.SwitchOpenTradingPanel = true;

                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenInspectPanel      
                    GameGlobals.Instance.IsOpenTradingPanel = false;
                    UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                    GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                    PlaySoundEffect(Sound.Cancel1);
                }
                else if (keyboardCur.IsKeyUp(Keys.T))
                {
                    GameGlobals.Instance.SwitchOpenTradingPanel = false;
                }

                // Close WarpPoint Panel
                if (keyboardCur.IsKeyDown(pauseMenuKey) && !GameGlobals.Instance.SwitchOpenWarpPointPanel
                    && GameGlobals.Instance.IsOpenGUI && UIManager.Instance.CurrentUI.Equals(UIManager.WarpPointPanel))
                {
                    GameGlobals.Instance.SwitchOpenWarpPointPanel = true;

                    // Toggle Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenWarpPointPanel      
                    GameGlobals.Instance.IsOpenWarpPointPanel = false;
                    UIManager.Instance.CurrentUI = UIManager.PlayScreen;
                    GameGlobals.Instance.IsRefreshPlayScreenUI = false;
                    PlaySoundEffect(Sound.Cancel1);
                }
                else if (keyboardCur.IsKeyUp(Keys.T))
                {
                    GameGlobals.Instance.SwitchOpenWarpPointPanel = false;
                }

                // Select Item Bar Slot
                if (keyboardCur.IsKeyDown(Keys.D1) && !GameGlobals.Instance.SwitchSlot_1)
                {
                    GameGlobals.Instance.SwitchSlot_1 = true;
                    GameGlobals.Instance.CurrentHotbarSelect = 0;

                    Instance.SelectedHotbarItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Value.Slot.Equals(InventoryManager.HotbarSlot_1));

                    if (Instance.SelectedHotbarItem.Value != null)
                        InventoryManager.Instance.UseItemInHotbar(Instance.SelectedHotbarItem.Key);

                }
                else if (keyboardCur.IsKeyUp(Keys.D1))
                {
                    GameGlobals.Instance.SwitchSlot_1 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D2) && !GameGlobals.Instance.SwitchSlot_2)
                {
                    GameGlobals.Instance.SwitchSlot_2 = true;
                    GameGlobals.Instance.CurrentHotbarSelect = 1;

                    Instance.SelectedHotbarItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Value.Slot.Equals(InventoryManager.HotbarSlot_2));

                    if (Instance.SelectedHotbarItem.Value != null)
                        InventoryManager.Instance.UseItemInHotbar(Instance.SelectedHotbarItem.Key);
                }
                else if (keyboardCur.IsKeyUp(Keys.D2))
                {
                    GameGlobals.Instance.SwitchSlot_2 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D3) && !GameGlobals.Instance.SwitchSlot_3)
                {
                    GameGlobals.Instance.SwitchSlot_3 = true;
                    GameGlobals.Instance.CurrentHotbarSelect = 2;

                    Instance.SelectedHotbarItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Value.Slot.Equals(InventoryManager.HotbarSlot_3));

                    if (Instance.SelectedHotbarItem.Value != null)
                        InventoryManager.Instance.UseItemInHotbar(Instance.SelectedHotbarItem.Key);
                }
                else if (keyboardCur.IsKeyUp(Keys.D3))
                {
                    GameGlobals.Instance.SwitchSlot_3 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D4) && !GameGlobals.Instance.SwitchSlot_4)
                {
                    GameGlobals.Instance.SwitchSlot_4 = true;
                    GameGlobals.Instance.CurrentHotbarSelect = 3;

                    Instance.SelectedHotbarItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Value.Slot.Equals(InventoryManager.HotbarSlot_4));

                    if (Instance.SelectedHotbarItem.Value != null)
                        InventoryManager.Instance.UseItemInHotbar(Instance.SelectedHotbarItem.Key);
                }
                else if (keyboardCur.IsKeyUp(Keys.D4))
                {
                    GameGlobals.Instance.SwitchSlot_4 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D5) && !GameGlobals.Instance.SwitchSlot_5)
                {
                    GameGlobals.Instance.SwitchSlot_5 = true;
                    GameGlobals.Instance.CurrentHotbarSelect = 4;

                    Instance.SelectedHotbarItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Value.Slot.Equals(InventoryManager.HotbarSlot_5));

                    if (Instance.SelectedHotbarItem.Value != null)
                        InventoryManager.Instance.UseItemInHotbar(Instance.SelectedHotbarItem.Key);
                }
                else if (keyboardCur.IsKeyUp(Keys.D5))
                {
                    GameGlobals.Instance.SwitchSlot_5 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D6) && !GameGlobals.Instance.SwitchSlot_6)
                {
                    GameGlobals.Instance.SwitchSlot_6 = true;
                    GameGlobals.Instance.CurrentHotbarSelect = 5;

                    Instance.SelectedHotbarItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Value.Slot.Equals(InventoryManager.HotbarSlot_6));

                    if (Instance.SelectedHotbarItem.Value != null)
                        InventoryManager.Instance.UseItemInHotbar(Instance.SelectedHotbarItem.Key);
                }
                else if (keyboardCur.IsKeyUp(Keys.D6))
                {
                    GameGlobals.Instance.SwitchSlot_6 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D7) && !GameGlobals.Instance.SwitchSlot_7)
                {
                    GameGlobals.Instance.SwitchSlot_7 = true;
                    GameGlobals.Instance.CurrentHotbarSelect = 6;
                    Instance.SelectedHotbarItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Value.Slot.Equals(InventoryManager.HotbarSlot_7));

                    if (Instance.SelectedHotbarItem.Value != null)
                        InventoryManager.Instance.UseItemInHotbar(Instance.SelectedHotbarItem.Key);
                }
                else if (keyboardCur.IsKeyUp(Keys.D7))
                {
                    GameGlobals.Instance.SwitchSlot_7 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D8) && !GameGlobals.Instance.SwitchSlot_8)
                {
                    GameGlobals.Instance.SwitchSlot_8 = true;
                    GameGlobals.Instance.CurrentHotbarSelect = 7;

                    Instance.SelectedHotbarItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Value.Slot.Equals(InventoryManager.HotbarSlot_8));

                    if (Instance.SelectedHotbarItem.Value != null)
                        InventoryManager.Instance.UseItemInHotbar(Instance.SelectedHotbarItem.Key);
                }
                else if (keyboardCur.IsKeyUp(Keys.D8))
                {
                    GameGlobals.Instance.SwitchSlot_8 = false;
                }

                // Debug Mode
                if (keyboardCur.IsKeyDown(debugModeKey) && !GameGlobals.Instance.SwitchDebugMode)
                {
                    // Toggle the IsShowDetectBox flag
                    GameGlobals.Instance.IsDebugMode = !GameGlobals.Instance.IsDebugMode;

                    // Update the boolean variable to indicate that the "B" button has been pressed
                    GameGlobals.Instance.SwitchDebugMode = true;
                }
                else if (keyboardCur.IsKeyUp(debugModeKey))
                {
                    // Update the boolean variable to indicate that the "B" button is not currently pressed
                    GameGlobals.Instance.SwitchDebugMode = false;
                }

                // Show Path Finding of Mobs
                if (keyboardCur.IsKeyDown(showPathFindingKey) && !GameGlobals.Instance.SwitchShowPath)
                {
                    GameGlobals.Instance.IsShowPath = !GameGlobals.Instance.IsShowPath;

                    GameGlobals.Instance.SwitchShowPath = true;
                }
                else if (keyboardCur.IsKeyUp(showPathFindingKey))
                {
                    GameGlobals.Instance.SwitchShowPath = false;
                }
            }

            //// Check if CurrentScreen is TestScreen
            //if (!ScreenManager.Instance.IsTransitioning && !UIManager.Instance.IsShowDialogUI
            //    && (ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.TestScreen
            //    || ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.Map1))
            //{
                
            //}
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

            // Update Crafting
            CraftingManager.Instance.Update(gameTime);

            // Update Quest
            QuestManager.Instance.Update(gameTime);

            // Check Player and Companion HP for Deadq
            if (Player.HP <= 0 && !IsPlayerDead)
            {
                IsPlayerDead = true;

                // Show DeadPanel
                UIManager.Instance.ShowDeadPanel();

                PlaySoundEffect(Sound.Dead);        
                PlayBackgroundMusic(GameGlobals.Instance.DyingSong, false, 1f);
            }

            if (Companions.Count != 0)
                if (Companions[CurrCompaIndex].IsDead && !IsCompanionDead)
                    IsCompanionDead = true;

            if (IsPlayerDead)
            {
                GameGlobals.Instance.IsEnteringBossFight = false;

                if (IsRespawning) RespawnPlayer();
            }

            if (IsCompanionDead)
            {
                IsCompanionSummoned = false;
                Companions[CurrCompaIndex].CompanionData.IsSummoned = false;
            }

            // This will check if the player enters the area that is the point of crossing over to another map
            UpdateEnteringZone();
        }

        public void UpdateEnteringZone()
        {
            var EnteringZoneArea = GameGlobals.Instance.EnteringZoneArea;
            foreach (var zoneArea in EnteringZoneArea)
            {
                if (Player.BoundingDetectCollisions.Intersects(zoneArea.Bounds) && !ScreenManager.Instance.IsTransitioning)
                {
                    // Check if clear chapter 1?
                    if (zoneArea.Bounds.Equals("dungeon_1_to_map_2") 
                        && !Player.PlayerData.ChapterProgression[0].IsChapterClear) break;

                    GameGlobals.Instance.InitialCameraPos = Player.Position;
                    var currLoadMapAction = ScreenManager.GetLoadMapAction(zoneArea.Name);

                    // if the zoneArea.Name not be found in LoadMapAction then return
                    if (currLoadMapAction == ScreenManager.LoadMapAction.LoadSave) break;

                    ScreenManager.Instance.CurrentLoadMapAction = currLoadMapAction;
                    ScreenManager.Instance.TranstisionToScreen(ScreenManager.Instance.GetPlayScreenByLoadMapAction());
                    break;
                }
            }
        }

        public void SetupPlayerPosition()
        { 
            var loadAction = ScreenManager.Instance.CurrentLoadMapAction;

            if (loadAction == ScreenManager.LoadMapAction.LoadSave) return;

            var curMap = ScreenManager.Instance.CurrentMap;
            //Player.PlayerData.CurrentMap = curMap;

            var mapPositionData = GameGlobals.Instance.MapLocationPointDatas.FirstOrDefault
                (m => m.Name.Equals(curMap));
            PositionData positionData;
            Vector2 position = Vector2.One;

            if (loadAction == ScreenManager.LoadMapAction.NewGame)
            {
                positionData = mapPositionData.Positions.FirstOrDefault(p => p.Name.Equals("Respawn"));
                position = new Vector2(
                    (float)positionData.Value[0],
                    (float)positionData.Value[1]);
            }
            else
            {
                var loadActionString = Enum.GetName(typeof(ScreenManager.LoadMapAction), loadAction);

                positionData = mapPositionData.Positions.FirstOrDefault(p => p.Name.Equals(loadActionString));
                position = new Vector2(
                    (float)positionData.Value[0],
                    (float)positionData.Value[1]);
            }          

            Player.Position = position;

            //// Adjust HUD and camera positions
            GameGlobals.Instance.TopLeftCornerPos = Player.Position - GameGlobals.Instance.GameScreenCenter;
            GameGlobals.Instance.InitialCameraPos = Player.Position;
            GameGlobals.Instance.AddingCameraPos = Vector2.Zero;
        }

        private void RespawnPlayer()
        {
            string curMap = ScreenManager.Instance.CurrentMap;
            string diffMap = string.Empty;
            string loadMapAction = string.Empty;

            switch (ScreenManager.Instance.CurrentMap)
            {
                case "Test":
                    diffMap = "Test";
                    break;

                case "map_1":
                case "battlezone_1":
                case "dungeon_1":
                    diffMap = "map_1";
                    loadMapAction = "warp_to_map_1";
                    break;

                case "map_2":
                case "battlezone_2":
                case "dungeon_2":
                    diffMap = "map_2";
                    loadMapAction = "warp_to_map_2";
                    break;

                case "map_3":
                case "battlezone_3":
                case "dungeon_3":
                    diffMap = "map_3";
                    loadMapAction = "warp_to_map_3";
                    break;
            }

            if (IsPlayerDead)
            {
                // if died in the same map
                if (curMap.Equals(diffMap))
                {
                    if (!ScreenManager.Instance.IsTransitioning)
                        ScreenManager.Instance.TranstisionToScreen(ScreenManager.GameScreen.None);

                    if (ScreenManager.Instance.DoActionAtMidTransition)
                    {
                        ScreenManager.Instance.DoActionAtMidTransition = false;

                        Player.HP = Player.MaxHP;
                        Player.Mana = Player.MaxMana;

                        var entities = EntityManager.Instance.Entities;
                        foreach (var entity in entities.Where(e => !e.IsDestroyed))
                        {
                            entity.IsAggro = false;
                            entity.AggroTimer = 0f;
                        }

                        IsRespawning = false;
                        IsPlayerDead = false;
                        Player.IsDying = false;

                        var mapPositionData = GameGlobals.Instance.MapLocationPointDatas.FirstOrDefault
                            (m => m.Name.Equals(curMap));

                        var positionData = mapPositionData.Positions.FirstOrDefault
                            (p => p.Name.Equals("Respawn"));

                        var position = new Vector2(
                            (float)positionData.Value[0],
                            (float)positionData.Value[1]);

                        Player.Position = position;

                        if (Companions.Count != 0)
                            if (!IsCompanionDead)
                            {
                                IsCompanionSummoned = false;
                                Companions[CurrCompaIndex].IsAggro = false;
                                Companions[CurrCompaIndex].ActionTimer = 0f;
                            }

                        // Adjust HUD and camera positions
                        GameGlobals.Instance.TopLeftCornerPos = Player.Position - GameGlobals.Instance.GameScreenCenter;
                        GameGlobals.Instance.InitialCameraPos = Player.Position;
                        GameGlobals.Instance.AddingCameraPos = Vector2.Zero;
                    }
                }
                else
                {
                    if (!ScreenManager.Instance.IsTransitioning)
                    {
                        Player.HP = Player.MaxHP;
                        Player.Mana = Player.MaxMana;

                        var entities = EntityManager.Instance.Entities;
                        foreach (var entity in entities.Where(e => !e.IsDestroyed))
                        {
                            entity.IsAggro = false;
                            entity.AggroTimer = 0f;
                        }

                        IsRespawning = false;
                        IsPlayerDead = false;
                        Player.IsDying = false;

                        if (Companions.Count != 0)
                            if (!IsCompanionDead)
                            {
                                IsCompanionSummoned = false;
                                Companions[CurrCompaIndex].IsAggro = false;
                                Companions[CurrCompaIndex].ActionTimer = 0f;
                            }

                        GameGlobals.Instance.InitialCameraPos = Player.Position;
                        var currLoadMapAction = ScreenManager.GetLoadMapAction(loadMapAction);

                        // if the zoneArea.Name not be found in LoadMapAction then return
                        if (currLoadMapAction == ScreenManager.LoadMapAction.LoadSave) return;

                        ScreenManager.Instance.CurrentLoadMapAction = currLoadMapAction;
                        ScreenManager.Instance.TranstisionToScreen(ScreenManager.Instance.GetPlayScreenByLoadMapAction());
                    }                      
                }
            }
        }

        public void SetPlayerExpMaxCap(int level)
        {
            var ExpCapData = GameGlobals.Instance.ExperienceCapacityDatas.FirstOrDefault
                (expcap => expcap.Level.Equals(level));

            Player.EXPMaxCap = ExpCapData.MaxCap;
        }

        public void AddPlayerEXP(int exp)
        {
            if (Player.Level < GameGlobals.Instance.MaxLevel)
            {
                Player.EXP += exp;

                // Increase Level & Set ExpMaxCap
                if (Player.EXP >= Player.EXPMaxCap)
                {
                    Player.Level++;
                    Player.PlayerData.SkillPoint++;

                    PlaySoundEffect(Sound.LevelUp);

                    // Increasing Companions Level too
                    foreach (var compa in Companions)
                    {
                        compa.Level++;
                        compa.CompanionData.Level++;

                        if (compa.Level % 3 == 0)
                        {
                            compa.CompanionData.Abilities.NormalSkillLevel++;
                            compa.CompanionData.Abilities.BurstSkillLevel++;
                            compa.CompanionData.Abilities.PassiveSkillLevel++;
                        }

                        ReStatsCompanion(compa);
                    }

                    // Re-Stats
                    ReStatsPlayer();

                    if (Player.Level == GameGlobals.Instance.MaxLevel)
                    {
                        SetPlayerExpMaxCap(Player.Level);
                        Player.EXP = Player.EXPMaxCap;
                    }
                    else
                    {
                        Player.EXP -= Player.EXPMaxCap;
                        SetPlayerExpMaxCap(Player.Level);
                    }
                }
            } 
        }

        private void ReStatsPlayer()
        {
            var charData = GameGlobals.Instance.CharacterDatas.FirstOrDefault
                (c => c.CharId.Equals(Player.CharId));

            Player.SetCharacterStats(charData, Player.Level);

            // Set current HP & Mana
            Player.HP = (float)(Player.BaseMaxHP * Player.GetCurrentHealthPercentage());
            Player.Mana = (float)(Player.BaseMaxMana * Player.GetCurrentManaPercentage());

            // Now re-stats Equipments
            var itemEquipmentData = InventoryManager.Instance.InventoryBag.Values.Where
                (e => e.Slot >= 0 && e.Slot < 6);

            foreach (var item in itemEquipmentData)
                RefreshEquipmentStats(item, true);
        }

        private void ReStatsCompanion(Companion compa)
        {
            var charData = GameGlobals.Instance.CharacterDatas.FirstOrDefault
                (c => c.CharId.Equals(compa.CharId));

            compa.SetCharacterStats(charData, compa.Level);

            // Set current HP and Mana
            compa.HP = (float)(compa.BaseMaxHP * compa.GetCurrentHealthPercentage());
            compa.Mana = (float)(compa.BaseMaxMana * compa.GetCurrentManaPercentage());
        }

        public void RefreshEquipmentStats(InventoryItemData itemEquipmentData, bool isEquip)
        {
            if (itemEquipmentData != null)
            {
                var equipmentStats = GameGlobals.Instance.EquipmentStatsDatas.FirstOrDefault
                        (e => e.ItemId.Equals(itemEquipmentData.ItemId));

                foreach (var stats in equipmentStats.Stats)
                {
                    switch (stats.Target)
                    {
                        case "TrueATK":
                            var valueTrueATK = (float)Math.Round(stats.Value, 2);
                            Player.ATK = isEquip ? (Player.ATK + valueTrueATK) : (Player.ATK - valueTrueATK);
                            break;

                        case "ATK%":
                            var valueATK = (float)Math.Round(Player.BaseATK * stats.Value, 2);
                            Player.ATK = isEquip ? (Player.ATK + valueATK) : (Player.ATK - valueATK);
                            break;

                        case "HP%":
                            var valueMaxHP = (float)Math.Round(Player.BaseMaxHP * stats.Value, 2);
                            var valueHP = (float)Math.Round(Player.HP * stats.Value, 2);
                            Player.MaxHP = isEquip ? (Player.MaxHP + valueMaxHP) : (Player.MaxHP - valueMaxHP);
                            Player.HP = isEquip ? (Player.HP + valueHP) : (Player.HP - (valueHP - (int)(stats.Value * 100)));
                            break;

                        case "Mana%":
                            var valueMaxMana = (float)Math.Round(Player.BaseMaxMana * stats.Value, 2);
                            var valueMana = (float)Math.Round(Player.Mana * stats.Value, 2);
                            Player.MaxMana = isEquip ? (Player.MaxMana + valueMaxMana) : (Player.MaxMana - valueMaxMana);
                            Player.Mana = isEquip ? (Player.Mana + valueMana) : (Player.Mana - (valueMana - (int)(stats.Value * 100)));
                            break;

                        case "DEF%":
                            var valueDEF = (float)Math.Round(stats.Value, 2);
                            Player.DEF = isEquip ? (Player.DEF + valueDEF) : (Player.DEF - valueDEF);
                            break;

                        case "Crit%":
                            var valueCrit = (float)Math.Round(stats.Value, 2);
                            Player.Crit = isEquip ? (Player.Crit + valueCrit) : (Player.Crit - valueCrit);
                            break;

                        case "CritDMG%":
                            var valueCritDMG = (float)Math.Round(stats.Value, 2);
                            Player.CritDMG = isEquip ? (Player.CritDMG + valueCritDMG) : (Player.CritDMG - valueCritDMG);
                            break;

                        case "Evasion%":
                            var valueEvasion = (float)Math.Round(stats.Value, 2);
                            Player.Evasion = isEquip ? (Player.Evasion + valueEvasion) : (Player.Evasion - valueEvasion);
                            break;

                        case "Speed%":
                            var valueSpeed = (int)(Player.BaseSpeed * stats.Value);
                            Player.Speed = isEquip ? (Player.Speed + valueSpeed) : (Player.Speed - valueSpeed);
                            break;
                    }
                }
            }
        }

        public bool UpSkillLevel(string skillName)
        {
            var normalSkillLevel = Player.PlayerData.Abilities.NormalSkillLevel;
            var burstSkillLevel = Player.PlayerData.Abilities.BurstSkillLevel;
            var passiveSkillLevel = Player.PlayerData.Abilities.PassiveSkillLevel;

            SkillDescriptionData skillData;

            switch (skillName)
            {
                case "I've got the Scent!":
                    skillData = GameGlobals.Instance.SkillDescriptionDatas.FirstOrDefault
                        (s => s.Name.Equals(skillName) && s.Level.Equals(normalSkillLevel));

                    if (normalSkillLevel != 10)
                    {
                        if (normalSkillLevel == 3 && Player.Level < 11)
                            return false;

                        if (normalSkillLevel == 6 && Player.Level < 21)
                            return false;

                        Player.PlayerData.Abilities.NormalSkillLevel++;
                        Player.PlayerData.SkillPoint -= skillData.SkillPointCost;
                        InventoryManager.Instance.ReduceGoldCoin(skillData.GoldCoinCost);
                    }
                    return true;

                case "Noah Strike":
                    skillData = GameGlobals.Instance.SkillDescriptionDatas.FirstOrDefault
                        (s => s.Name.Equals(skillName) && s.Level.Equals(burstSkillLevel));

                    if (burstSkillLevel != 10)
                    {
                        if (burstSkillLevel == 3 && Player.Level < 11)
                            return false;

                        if (burstSkillLevel == 6 && Player.Level < 21)
                            return false;

                        Player.PlayerData.Abilities.BurstSkillLevel++;
                        Player.PlayerData.SkillPoint -= skillData.SkillPointCost;
                        InventoryManager.Instance.ReduceGoldCoin(skillData.GoldCoinCost);
                    }
                    return true;

                case "Survivalist":
                    skillData = GameGlobals.Instance.SkillDescriptionDatas.FirstOrDefault
                        (s => s.Name.Equals(skillName) && s.Level.Equals(passiveSkillLevel));

                    if (passiveSkillLevel != 10)
                    {
                        if (passiveSkillLevel == 3 && Player.Level < 11)
                            return false;

                        if (passiveSkillLevel == 6 && Player.Level < 21)
                            return false;

                        Player.PlayerData.Abilities.PassiveSkillLevel++;
                        Player.PlayerData.SkillPoint -= skillData.SkillPointCost;
                        InventoryManager.Instance.ReduceGoldCoin(skillData.GoldCoinCost);
                    }
                    return true;
            }

            return false;
        }

        public void CheckInteraction(KeyboardState keyboardCur, KeyboardState keyboardPrev)
        {           
            IsDetectedObject = false;
            IsDetectedInteractableMob = false;

            // Clear IsDectected for gameObjects first
            var gameObjects = ObjectManager.Instance.GameObjects.Where(o => o.IsDetectable);
            foreach (var gameObject in gameObjects)
            {
                gameObject.IsDetected = false;
            }   

            // Check Dectection Object
            foreach (var gameObject in gameObjects.Where(o => o.IsVisible))
            {
                if (Player.BoundingInteraction.Intersects(gameObject.BoundingInteraction))
                {
                    IsDetectedObject = true;
                    DetectedObject = gameObject;
                    gameObject.IsDetected = true;
                    break;
                }
                else DetectedObject = null;
            }

            // For Crop in NoahHome
            if (ScreenManager.Instance.CurrentMap.Equals("noah_home"))
            {
                // Clear Detected
                var crops = ObjectManager.Instance.Crops;
                foreach (var crop in crops)
                {
                    crop.IsDetected = false;
                }

                // Check Dectection
                foreach (var crop in crops.Where(o => o.IsVisible))
                {
                    if (Player.BoundingInteraction.Intersects(crop.BoundingInteraction))
                    {
                        IsDetectedObject = true;
                        DetectedObject = crop;
                        crop.IsDetected = true;
                        System.Diagnostics.Debug.WriteLine($"crop : {crop.CropStage}");
                        break;
                    }
                    else DetectedObject ??= null;
                }
            }

            // Clear IsDectected for interactableMobs
            var interactableMobs = EntityManager.Instance.Entities.Where
                (e => e.EntityType == Entity.EntityTypes.Friendly).Cast<FriendlyMob>();

            foreach (var mob in interactableMobs.Where(e => e.IsInteractable))
            {
                mob.IsDetected = false;
            }

            // Check Detection Interactable Mob
            foreach (var mob in interactableMobs.Where(e => e.IsInteractable))
            {
                if (Player.BoundingInteraction.Intersects(mob.BoundingInteraction))
                {
                    IsDetectedInteractableMob = true;
                    DetectedInteractableMob = mob;
                    mob.IsDetected = true;
                    break;
                }
                else DetectedInteractableMob = null;
            }

            // Check Interaction
            if (keyboardCur.IsKeyUp(Keys.F) && keyboardPrev.IsKeyDown(Keys.F))
            {
                if (IsDetectedObject && DetectedObject != null)
                {
                    CheckGameObject(DetectedObject);
                }
                else if (IsDetectedInteractableMob && DetectedInteractableMob != null)
                {
                    CheckFriendlyMob(DetectedInteractableMob);
                }
            }
        }

        private void CheckGameObject(GameObject gameObject)
        {
            if (Player.BoundingInteraction.Intersects(gameObject.BoundingInteraction))
            {
                switch (gameObject.ObjectType)
                {
                    case GameObject.GameObjectType.QuestObject:
                        break;

                    case GameObject.GameObjectType.Item:
                        // Collecting Item into Player's Inventory
                        Item item = gameObject as Item;
                        var itemId = item.ReferId;
                        var quantityDrop = item.QuantityDrop;

                        // Check Inventory                   
                        if (!item.IsCollected
                            && !InventoryManager.Instance.IsInventoryFull(itemId, quantityDrop))
                        {
                            item.IsCollected = true;
                        }
                        else HUDSystem.ShowInsufficientSign();
                        break;

                    case GameObject.GameObjectType.Crop:
                        Crop crop = gameObject as Crop;
                        // Cropping or Collecting
                        if (crop.CropStage == Crop.CropStages.Empty)
                        {
                            crop.Cropping(SelectedHotbarItem);
                        }
                        else if (crop.CropStage == Crop.CropStages.Harvestable)
                        {
                            if (!crop.IsCollected
                                && !InventoryManager.Instance.IsInventoryFull(crop.ReferId, crop.QuantityDrop))
                            {
                                crop.IsCollected = true;
                            }
                            else HUDSystem.ShowInsufficientSign();
                        }

                        break;

                    case GameObject.GameObjectType.CraftingTable:
                        // Open Crafting Item Panel
                        CraftingTable craftingTable = gameObject as CraftingTable;
                        craftingTable.OpenCraftingPanel();
                        break;

                    case GameObject.GameObjectType.SavingTable:
                        // Open Saving Game Panel
                        SavingTable.OpenSavingPanel();
                        break;

                    case GameObject.GameObjectType.WarpPoint:
                        // Open WarpPoint Panel
                        WarpPoint warpPoint = gameObject as WarpPoint;
                        warpPoint.OpenWarpPointPanel();
                        break;

                    case GameObject.GameObjectType.RestPoint:
                        // Resting
                        RestPoint restPoint = gameObject as RestPoint;
                        restPoint.Rest();
                        break;
                }
            }
        }

        private void CheckFriendlyMob(FriendlyMob friendlyMob)
        {
            if (Player.BoundingInteraction.Intersects(friendlyMob.BoundingInteraction))
            {
                switch (friendlyMob.MobType)
                {
                    case FriendlyMob.FriendlyMobType.Animal:

                    case FriendlyMob.FriendlyMobType.Civilian:

                    case FriendlyMob.FriendlyMobType.QuestGiver:
                        friendlyMob.Interact();
                        break;

                    case FriendlyMob.FriendlyMobType.Vendor:
                        Vendor vendorMob = friendlyMob as Vendor;
                        vendorMob.OpenTradingPanel();
                        break;
                }
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
