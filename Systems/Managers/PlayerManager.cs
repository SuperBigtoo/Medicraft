using Medicraft.Data;
using Medicraft.Data.Models;
using Medicraft.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;
using System;
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
            var playerSprite = new AnimatedSprite(GameGlobals.Instance.PlayerSpriteSheet);

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
            GameGlobals.Instance.TopLeftCornerPosition = Player.Position - GameGlobals.Instance.GameScreenCenter;

            // Initialize equipment stats
            RefreshEquipmentStats(true);

            // Initialize display inventory item after init Player's inventory data
            GUIManager.Instance.InitInventoryItemDisplay();
            GUIManager.Instance.InitCraftableItemDisplay();
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

            // Only receive input if on PlayScreen or TestScreen
            if (!ScreenManager.Instance.IsTransitioning 
                && (ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.TestScreen
                || ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.Map1))
            {
                // Save Game for Test
                if (keyboardCur.IsKeyUp(Keys.M) && keyboardPrev.IsKeyDown(Keys.M))
                {
                    JsonFileManager.SaveGame();
                }

                // Open Inventory
                if (keyboardCur.IsKeyDown(Keys.I) && !GameGlobals.Instance.SwitchOpenInventoryPanel && !GameGlobals.Instance.IsOpenGUI
                    || (keyboardCur.IsKeyDown(Keys.I) || keyboardCur.IsKeyDown(Keys.Escape)) && !GameGlobals.Instance.SwitchOpenInventoryPanel
                        && GameGlobals.Instance.IsOpenGUI && GUIManager.Instance.CurrentGUI.Equals(GUIManager.InventoryPanel))
                {
                    GameGlobals.Instance.SwitchOpenInventoryPanel = true;

                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenInvenoryPanel & refresh inventory item display              
                    GameGlobals.Instance.IsOpenInventoryPanel = false;
                    if (GUIManager.Instance.CurrentGUI.Equals(GUIManager.InventoryPanel))
                    {
                        GUIManager.Instance.CurrentGUI = GUIManager.Hotbar;
                        GameGlobals.Instance.IsRefreshHotbar = false;
                    }
                    else GUIManager.Instance.CurrentGUI = GUIManager.InventoryPanel;
                }
                else if (keyboardCur.IsKeyUp(Keys.I))
                {
                    GameGlobals.Instance.SwitchOpenInventoryPanel = false;
                }

                // Open Crafting Panel 
                if (keyboardCur.IsKeyDown(Keys.O) && !GameGlobals.Instance.SwitchOpenCraftingPanel && !GameGlobals.Instance.IsOpenGUI
                    || (keyboardCur.IsKeyDown(Keys.O) || keyboardCur.IsKeyDown(Keys.Escape)) && !GameGlobals.Instance.SwitchOpenCraftingPanel
                        && GameGlobals.Instance.IsOpenGUI && GUIManager.Instance.CurrentGUI.Equals(GUIManager.CraftingPanel))
                {
                    GameGlobals.Instance.SwitchOpenCraftingPanel = true;

                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenCraftingPanel & refresh crafting item display       
                    GameGlobals.Instance.IsOpenCraftingPanel = false;
                    if (GUIManager.Instance.CurrentGUI.Equals(GUIManager.CraftingPanel))
                    {
                        GUIManager.Instance.CurrentGUI = GUIManager.Hotbar;
                        GameGlobals.Instance.IsRefreshHotbar = false;
                    }
                    else GUIManager.Instance.CurrentGUI = GUIManager.CraftingPanel;
                }
                else if (keyboardCur.IsKeyUp(Keys.O))
                {
                    GameGlobals.Instance.SwitchOpenCraftingPanel = false;
                }

                // Open Inspect Panel 
                if (keyboardCur.IsKeyDown(Keys.C) && !GameGlobals.Instance.SwitchOpenInspectPanel && !GameGlobals.Instance.IsOpenGUI
                    || (keyboardCur.IsKeyDown(Keys.C) || keyboardCur.IsKeyDown(Keys.Escape)) && !GameGlobals.Instance.SwitchOpenInspectPanel
                        && GameGlobals.Instance.IsOpenGUI && GUIManager.Instance.CurrentGUI.Equals(GUIManager.InspectPanel))
                {
                    GameGlobals.Instance.SwitchOpenInspectPanel = true;

                    // Pause PlayScreen
                    GameGlobals.Instance.IsGamePause = !GameGlobals.Instance.IsGamePause;
                    GameGlobals.Instance.IsOpenGUI = !GameGlobals.Instance.IsOpenGUI;

                    // Toggle IsOpenCraftingPanel & refresh crafting item display       
                    GameGlobals.Instance.IsOpenInspectPanel = false;
                    GUIManager.Instance.IsCharacterTabSelected = true;
                    if (GUIManager.Instance.CurrentGUI.Equals(GUIManager.InspectPanel))
                    {                     
                        GUIManager.Instance.CurrentGUI = GUIManager.Hotbar;
                        GameGlobals.Instance.IsRefreshHotbar = false;
                        GUIManager.Instance.ClearSkillDescription();
                    }
                    else GUIManager.Instance.CurrentGUI = GUIManager.InspectPanel;
                }
                else if (keyboardCur.IsKeyUp(Keys.C))
                {
                    GameGlobals.Instance.SwitchOpenInspectPanel = false;
                }

                // Select Item Bar Slot
                if (keyboardCur.IsKeyDown(Keys.D1) && !GameGlobals.Instance.SwitchSlot_1)
                {
                    GameGlobals.Instance.SwitchSlot_1 = true;

                    GameGlobals.Instance.CurrentSlotBarSelect = 0;
                    var slotBarItem = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(
                        i => i.Slot.Equals(InventoryManager.HotbarSlot_1));

                    if (slotBarItem != null) InventoryManager.Instance.UseItemInHotbar(slotBarItem.ItemId);
                }
                else if (keyboardCur.IsKeyUp(Keys.D1))
                {
                    GameGlobals.Instance.SwitchSlot_1 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D2) && !GameGlobals.Instance.SwitchSlot_2)
                {
                    GameGlobals.Instance.SwitchSlot_2 = true;

                    GameGlobals.Instance.CurrentSlotBarSelect = 1;
                    var slotBarItem = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(
                        i => i.Slot.Equals(InventoryManager.HotbarSlot_2));

                    if (slotBarItem != null) InventoryManager.Instance.UseItemInHotbar(slotBarItem.ItemId);
                }
                else if (keyboardCur.IsKeyUp(Keys.D2))
                {
                    GameGlobals.Instance.SwitchSlot_2 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D3) && !GameGlobals.Instance.SwitchSlot_3)
                {
                    GameGlobals.Instance.SwitchSlot_3 = true;

                    GameGlobals.Instance.CurrentSlotBarSelect = 2;
                    var slotBarItem = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(
                        i => i.Slot.Equals(InventoryManager.HotbarSlot_3));

                    if (slotBarItem != null) InventoryManager.Instance.UseItemInHotbar(slotBarItem.ItemId);
                }
                else if (keyboardCur.IsKeyUp(Keys.D3))
                {
                    GameGlobals.Instance.SwitchSlot_3 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D4) && !GameGlobals.Instance.SwitchSlot_4)
                {
                    GameGlobals.Instance.SwitchSlot_4 = true;

                    GameGlobals.Instance.CurrentSlotBarSelect = 3;
                    var slotBarItem = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(
                        i => i.Slot.Equals(InventoryManager.HotbarSlot_4));

                    if (slotBarItem != null) InventoryManager.Instance.UseItemInHotbar(slotBarItem.ItemId);
                }
                else if (keyboardCur.IsKeyUp(Keys.D4))
                {
                    GameGlobals.Instance.SwitchSlot_4 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D5) && !GameGlobals.Instance.SwitchSlot_5)
                {
                    GameGlobals.Instance.SwitchSlot_5 = true;

                    GameGlobals.Instance.CurrentSlotBarSelect = 4;
                    var slotBarItem = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(
                        i => i.Slot.Equals(InventoryManager.HotbarSlot_5));

                    if (slotBarItem != null) InventoryManager.Instance.UseItemInHotbar(slotBarItem.ItemId);
                }
                else if (keyboardCur.IsKeyUp(Keys.D5))
                {
                    GameGlobals.Instance.SwitchSlot_5 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D6) && !GameGlobals.Instance.SwitchSlot_6)
                {
                    GameGlobals.Instance.SwitchSlot_6 = true;

                    GameGlobals.Instance.CurrentSlotBarSelect = 5;
                    var slotBarItem = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(
                        i => i.Slot.Equals(InventoryManager.HotbarSlot_6));

                    if (slotBarItem != null) InventoryManager.Instance.UseItemInHotbar(slotBarItem.ItemId);
                }
                else if (keyboardCur.IsKeyUp(Keys.D6))
                {
                    GameGlobals.Instance.SwitchSlot_6 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D7) && !GameGlobals.Instance.SwitchSlot_7)
                {
                    GameGlobals.Instance.SwitchSlot_7 = true;

                    GameGlobals.Instance.CurrentSlotBarSelect = 6;
                    var slotBarItem = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(
                        i => i.Slot.Equals(InventoryManager.HotbarSlot_7));

                    if (slotBarItem != null) InventoryManager.Instance.UseItemInHotbar(slotBarItem.ItemId);
                }
                else if (keyboardCur.IsKeyUp(Keys.D7))
                {
                    GameGlobals.Instance.SwitchSlot_7 = false;
                }

                if (keyboardCur.IsKeyDown(Keys.D8) && !GameGlobals.Instance.SwitchSlot_8)
                {
                    GameGlobals.Instance.SwitchSlot_8 = true;

                    GameGlobals.Instance.CurrentSlotBarSelect = 7;
                    var slotBarItem = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(
                        i => i.Slot.Equals(InventoryManager.HotbarSlot_8));

                    if (slotBarItem != null) InventoryManager.Instance.UseItemInHotbar(slotBarItem.ItemId);
                }
                else if (keyboardCur.IsKeyUp(Keys.D8))
                {
                    GameGlobals.Instance.SwitchSlot_8 = false;
                }
            }

            // Check if CurrentScreen is TestScreen
            if (!ScreenManager.Instance.IsTransitioning
                && ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.TestScreen)
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

            // Update Crafting
            CraftingManager.Instance.Update(gameTime);

            // Check Player HP for Deadq
            if (Player.HP <= 0 && !IsPlayerDead) IsPlayerDead = true;

            if (IsPlayerDead)
                if (GameGlobals.Instance.CurMouse.LeftButton == ButtonState.Pressed
                    && GameGlobals.Instance.PrevMouse.LeftButton == ButtonState.Released)
                        RespawnPlayer();

            UpdateMapPositionData();
        }

        public void UpdateMapPositionData()
        {
            var EnteringZoneArea = GameGlobals.Instance.EnteringZoneArea;
            foreach (var zoneArea in EnteringZoneArea)
            {
                if (Player.BoundingDetectCollisions.Intersects(zoneArea.Bounds) && !ScreenManager.Instance.IsTransitioning)
                {
                    GameGlobals.Instance.InitialCameraPos = Player.Position;
                    var currLoadMapAction = ScreenManager.GetLoadMapAction(zoneArea.Name);

                    // if the zoneArea.Name not be found in LoadMapAction then return
                    if (currLoadMapAction == ScreenManager.LoadMapAction.None) return;

                    ScreenManager.Instance.CurrentLoadMapAction = currLoadMapAction;
                    ScreenManager.Instance.TranstisionToScreen(ScreenManager.Instance.GetPlayScreen());
                    break;
                }
            }
        }

        public void SetupPlayerPosition(ScreenManager.LoadMapAction loadAction)
        {
            if (loadAction == ScreenManager.LoadMapAction.LoadGameSave) return;

            var curMap = ScreenManager.Instance.CurrentMap;
            Player.PlayerData.CurrentMap = curMap;

            var mapPositionData = GameGlobals.Instance.MapLocationPointDatas
                .FirstOrDefault(m => m.Name.Equals(curMap));
            PositionData positionData;
            Vector2 position = Vector2.One;

            switch (loadAction)
            {
                default:

                case ScreenManager.LoadMapAction.NewGame:        
                    positionData = mapPositionData.Positions.FirstOrDefault(p => p.Name.Equals("Respawn"));
                    position = new Vector2(
                        (float)positionData.Value[0],
                        (float)positionData.Value[1]);
                    break;

                case ScreenManager.LoadMapAction.map_1_to_Test:     // Get position in Test where map_1 to Test 
                    positionData = mapPositionData.Positions.FirstOrDefault(p => p.Name.Equals("map_1_to_Test"));
                    position = new Vector2(
                        (float)positionData.Value[0],
                        (float)positionData.Value[1]);
                    break;

                case ScreenManager.LoadMapAction.Test_to_map_1:
                    positionData = mapPositionData.Positions.FirstOrDefault(p => p.Name.Equals("Test_to_map_1"));
                    position = new Vector2(
                        (float)positionData.Value[0],
                        (float)positionData.Value[1]);
                    break;
            }          

            Player.Position = position;

            //// Adjust HUD and camera positions
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

        public void SetPlayerExpMaxCap(int level)
        {
            var ExpCapData = GameGlobals.Instance.ExperienceCapacityDatas
                .FirstOrDefault(expcap => expcap.Level.Equals(level));

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

        public void RefreshEquipmentStats(bool isEquip)
        {
            for (int i = 0; i < 6; i++)
            {
                var itemEquipmentData = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(e => e.Slot.Equals(i));

                if (itemEquipmentData != null)
                {
                    var isEquipmentFound = InventoryManager.Instance.InventoryBag.TryGetValue
                    (itemEquipmentData.ItemId.ToString(), out InventoryItemData equipmentItem);

                    if (isEquipmentFound)
                    {
                        var equipmentStats = GameGlobals.Instance.EquipmentStatsDatas.FirstOrDefault
                            (e => e.ItemId.Equals(equipmentItem.ItemId));

                        foreach (var stats in equipmentStats.Stats)
                        {
                            switch (stats.Target)
                            {
                                case "TrueATK":
                                    var valueTrueATK = (int)(stats.Value);
                                    Player.ATK = isEquip ? (Player.ATK + valueTrueATK) : (Player.ATK - valueTrueATK);
                                    break;

                                case "ATK%":
                                    var valueATK = (int)(Player.BaseATK * stats.Value);
                                    Player.ATK = isEquip ? (Player.ATK + valueATK) : (Player.ATK - valueATK);
                                    break;

                                case "HP%":
                                    var valueMaxHP = (int)(Player.BaseMaxHP * stats.Value);
                                    var valueHP = (int)(Player.HP * stats.Value);
                                    Player.MaxHP = isEquip ? (Player.MaxHP + valueMaxHP) : (Player.MaxHP - valueMaxHP);
                                    Player.HP = isEquip ? (Player.HP + valueHP) : (Player.HP - valueHP);
                                    break;

                                case "Mana%":
                                    var valueMaxMana = (float)(Player.BaseMaxMana * stats.Value);
                                    var valueMana = (float)(Player.Mana * stats.Value);
                                    Player.MaxMana = isEquip ? (Player.MaxMana + valueMaxMana) : (Player.MaxMana - valueMaxMana);
                                    Player.Mana = isEquip ? (Player.Mana + valueMana) : (Player.Mana - valueMana);
                                    break;

                                case "DEF%":
                                    var valueDEF = (float)stats.Value;
                                    Player.DEF = isEquip ? (Player.DEF + valueDEF) : (Player.DEF - valueDEF);
                                    break;

                                case "Crit%":
                                    var valueCrit = (float)stats.Value;
                                    Player.Crit = isEquip ? (Player.Crit + valueCrit) : (Player.Crit - valueCrit);
                                    break;

                                case "CritDMG%":
                                    var valueCritDMG = (float)stats.Value;
                                    Player.CritDMG = isEquip ? (Player.CritDMG + valueCritDMG) : (Player.CritDMG - valueCritDMG);
                                    break;

                                case "Evasion%":
                                    var valueEvasion = (float)stats.Value;
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
