﻿using GeonBit.UI;
using Medicraft.Data;
using Medicraft.Data.Models;
using Medicraft.Systems.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Content;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using System.Collections.Generic;
using System.Linq;

namespace Medicraft.Systems
{
    public class GameGlobals
    {
        public ContentManager Content { private set; get; }

        // Global Variables
        public MouseState PrevMouse { set; get; }
        public MouseState CurMouse { set; get; }
        public KeyboardState PrevKeyboard { set; get; }
        public KeyboardState CurKeyboard { set; get; }
        public Point2 MousePosition { private set; get; }
        public Vector2 TopLeftCornerPosition { set; get; }
        public Vector2 InitialCameraPos { set; get; }
        public Vector2 AddingCameraPos { set; get; }
        public Vector2 GameScreen { private set; get; }
        public Vector2 GameScreenCenter { private set; get; }
        public bool IsGameActive { set; get; }
        public bool IsGamePause { set; get; }
        public bool SwitchOpenInventory { set; get; }
        public bool IsOpenInventory { set; get; }
        public bool IsInventoryRefreshed { set; get; }
        public bool SwitchDebugMode { set; get; }
        public bool IsDebugMode { set; get; }
        public bool SwitchShowPath { set; get; }
        public bool IsDetectedGameObject { set; get; }
        public bool ShowInsufficientSign { set; get; }
        public bool IsFullScreen { set; get; }
        public bool SwitchFullScreen { set; get; }
        public bool IsShowPath { set; get; }
        public bool IsTransitionFinished { set; get; }
        public bool IsEnteringBossFight { set; get; }
        public float TotalPlayTime { set; get; }
        public int MaxLevel { set; get; }
        
        // Boss
        public bool IsBoss_TestDead { set; get; }
        public float SpawnTime_Boss_Test { get; private set; }
        public float SpawnTimer_Boss_Test { get; set; }
        public bool IsBoss_1_Dead { set; get; }
        public float SpawnTime_Boss_1 { get; private set; }
        public float SpawnTimer_Boss_1 { get; set; }

        // GameSave
        public int GameSaveIdex { private set; get; }
        public string GameSavePath { private set; get; }
        public List<GameSaveData> GameSave { private set; get; }

        // Inventory
        public BuiltinThemes BuiltinTheme { set; get; }
        public int MaximunInventorySlot { private set; get; }
        public int MaximunItemCount { private set; get; }
        public int DefaultInventorySlot { private set; get; }
        public int MaxItemBarSlot { private set; get; }
        public int SelectedItemBarSlot { set; get; }
        public int SelectedItemInventory { set; get; }

        // Game Datas
        public PlayerData InitialPlayerData { private set; get; }
        public SpriteSheet PlayerAnimation { private set; get; }
        public BitmapFont FontSensation { private set; get; }
        public BitmapFont FontMinecraft { private set; get; }
        public BitmapFont FontTA8Bit { private set; get; }
        public BitmapFont FontTA8BitBold { private set; get; }
        public BitmapFont FontTA16Bit { private set; get; }
        public List<Texture2D> GuiTextures { private set; get; }
        public List<ExperienceCapacityData> ExperienceCapacityDatas { private set; get; }
        public List<MapLocationPointData> MapLocationPointDatas { private set; get; }         // All Point Loaction of Maps
        public List<ItemData> ItemsDatas { private set; get; }       // All items data
        // All equipments stats data
        // All item's effect data
        public List<CraftingRecipeData> CraftingRecipeDatas { private set; get; }       // All Crafting Recipe data
        public List<CharacterData> CharacterDatas { private set; get; }                 // All Character data
        public SpriteSheet ItemsPackSprites { private set; get; }                   // All Item sprite
        public SpriteSheet HitSpriteEffect { private set; get; }
        public SpriteSheet HitSkillSpriteEffect { private set; get; }
        public SpriteSheet BossSpriteEffect { private set; get; }
        public SpriteSheet StatesSpriteEffect { private set; get; }

        // Collecting Item Feed
        public List<InventoryItemData> CollectedItemFeed { private set; get; }      // Feed collected item
        public Point2 FeedPoint { private set; get; }
        public int MaximumItemFeed { private set; get; }
        public float DisplayFeedTime { set; get; }
        public float MaximumDisplayFeedTime { private set; get; }

        // Tilemap, Objective Area & Layer Depth
        public List<RectangleF> CollistionObject { private set; get; }       // "Collision"
        public List<RectangleF> TopLayerObject { private set; get; }
        public List<RectangleF> MiddleLayerObject { private set; get; }
        public List<RectangleF> BottomLayerObject { private set; get; }
        public List<RectangleF> CraftingTableArea { private set; get; }
        public List<RectangleF> SavingTableArea { private set; get; }
        public List<RectangleF> EnteringZoneArea { private set; get; }
        public List<PartrolAreaData> MobPartrolArea { private set; get; }
        public float TopEntityDepth { private set; get; }
        public float MiddleEntityDepth { private set; get; }
        public float BottomEntityDepth { private set; get; }
        public float TopObjectDepth { private set; get; }
        public float MiddleObjectDepth { private set; get; }
        public float BottomObjectDepth { private set; get; }
        public float BackgroundDepth { private set; get; }
        public int TILE_SIZE { set; get; }
        public int NUM_ROWS { set; get; }
        public int NUM_COLUMNS { set; get; }
        public int[,] Map { set; get; }

        public enum GuiTextureName
        {
            logo_wakeup,
            press_f,
            insufficient,
            health_bar,
            healthpoints_gauge,
            mana_gauge,
            health_bar_companion,
            healthpoints_gauge_companion,
            noah_profile,
            companion_profile,
            health_bar_boss,
            boss_gauge,
            item_bar,
            item_slot,
            gold_coin,
            heart,
            selected_slot,
            health_bar_alpha,
            health_bar_companion_alpha,
            health_bar_boss_alpha,
            quest_stamp,
            level_gui,
            exp_bar,
            exp_bar_alpha,
            exp_gauge,
            burst_skill_gui,
            burst_skill_gui_alpha,
            burst_skill_pic,
            normal_skill_gui,
            normal_skill_gui_alpha,
            normal_skill_pic,
            passive_skill_gui,
            passive_skill_gui_alpha,
            passive_skill_pic
        }

        private readonly Dictionary<GuiTextureName, int> guiTextureIndices = new()
        {
            { GuiTextureName.logo_wakeup, 0 },
            { GuiTextureName.press_f, 1 },
            { GuiTextureName.insufficient, 2 },
            { GuiTextureName.health_bar, 3 },
            { GuiTextureName.healthpoints_gauge, 4 },
            { GuiTextureName.mana_gauge, 5 },
            { GuiTextureName.health_bar_companion, 6 },
            { GuiTextureName.healthpoints_gauge_companion, 7 },
            { GuiTextureName.noah_profile, 8 },
            { GuiTextureName.companion_profile, 9 },
            { GuiTextureName.health_bar_boss, 10 },
            { GuiTextureName.boss_gauge, 11 },
            { GuiTextureName.item_bar, 12 },
            { GuiTextureName.item_slot, 13 },
            { GuiTextureName.gold_coin, 14 },
            { GuiTextureName.heart, 15 },
            { GuiTextureName.selected_slot, 16 },
            { GuiTextureName.health_bar_alpha, 17 },
            { GuiTextureName.health_bar_companion_alpha, 18 },
            { GuiTextureName.health_bar_boss_alpha, 19 },
            { GuiTextureName.quest_stamp, 20},
            { GuiTextureName.level_gui, 21},
            { GuiTextureName.exp_bar, 22},
            { GuiTextureName.exp_bar_alpha, 23},
            { GuiTextureName.exp_gauge, 24},
            { GuiTextureName.burst_skill_gui, 25},
            { GuiTextureName.burst_skill_gui_alpha, 26},
            { GuiTextureName.burst_skill_pic, 27},
            { GuiTextureName.normal_skill_gui, 28},
            { GuiTextureName.normal_skill_gui_alpha, 29},
            { GuiTextureName.normal_skill_pic, 30},
            { GuiTextureName.passive_skill_gui, 31},
            { GuiTextureName.passive_skill_gui_alpha, 32},
            { GuiTextureName.passive_skill_pic, 33},
        };

        // For Testing
        public int TestInt { set; get; }
        public Texture2D TestIcon { set; get; }

        private static GameGlobals instance;

        private GameGlobals()
        {
            GameScreen = new Vector2(1440, 900);
            GameScreenCenter = new Vector2(1440/2, 900/2);
            TopLeftCornerPosition = Vector2.Zero;
            InitialCameraPos = Vector2.Zero;
            AddingCameraPos = Vector2.Zero;

            IsGamePause = false;
            SwitchOpenInventory = false;
            IsOpenInventory = false;
            IsInventoryRefreshed = false;
            SwitchDebugMode = false;
            IsDebugMode = false;
            SwitchShowPath = false;
            IsShowPath = false;
            IsDetectedGameObject = false;
            ShowInsufficientSign = false;
            SwitchFullScreen = false;
            IsFullScreen = false;
            IsTransitionFinished = true;
            IsEnteringBossFight = false;

            IsBoss_TestDead = false;
            SpawnTime_Boss_Test = 60f;
            SpawnTimer_Boss_Test = SpawnTime_Boss_Test;

            IsBoss_1_Dead = false;
            SpawnTime_Boss_1 = 60f;
            SpawnTimer_Boss_1 = SpawnTime_Boss_1;

            MaxLevel = 30;
            TotalPlayTime = 0;
            GameSave = [];
            GameSaveIdex = 0; // to be initial
            GameSavePath = "save/gamesaves.json";

            FeedPoint = new(140f, 380f);

            BuiltinTheme = BuiltinThemes.hd;
            MaximunInventorySlot = 64;
            MaximunItemCount = 9999;
            DefaultInventorySlot = 999;
            MaxItemBarSlot = 8;
            SelectedItemBarSlot = 0;
            SelectedItemInventory = 0;

            GuiTextures = [];
            ExperienceCapacityDatas = [];
            MapLocationPointDatas = [];
            ItemsDatas = [];
            CharacterDatas = [];
            CraftingRecipeDatas = [];

            CollectedItemFeed = [];
            MaximumItemFeed = 6;
            DisplayFeedTime = 0;
            MaximumDisplayFeedTime = 6f;

            TILE_SIZE = 32;

            CollistionObject = [];
            TopLayerObject = [];
            MiddleLayerObject = [];
            BottomLayerObject = [];
            CraftingTableArea = [];
            SavingTableArea = [];
            EnteringZoneArea = [];
            MobPartrolArea = [];

            TopEntityDepth = 0.2f;
            MiddleEntityDepth = 0.4f;
            BottomEntityDepth = 0.6f;

            TopObjectDepth = 0.3f;
            MiddleObjectDepth = 0.5f;
            BottomObjectDepth = 0.7f;
            BackgroundDepth = 0.9f;

            TestInt = CraftingTableArea.Count;
        }

        public void Initialize(ContentManager Content)
        {
            this.Content = Content;
        }

        public void LoadContent()
        {
            //System.Diagnostics.Debug.WriteLine($"totalFrames : {(int)(_totalMilliseconds / totalDuration) % totalFrames}");

            // Load GameSave
            var gameSave = JsonFileManager.LoadFlie(GameGlobals.Instance.GameSavePath);
            if (gameSave.Count != 0)
            {
                foreach (var save in gameSave)
                {
                    GameSave.Add(save);
                }
            }

            // Load Map Position Data
            MapLocationPointDatas = Content.Load<List<MapLocationPointData>>("data/models/map_locations_point");

            // Load EXP Cap Data
            ExperienceCapacityDatas = Content.Load<List<ExperienceCapacityData>>("data/models/exp_capacity");

            // Load Initialize Player Data
            InitialPlayerData = Content.Load<PlayerData>("data/models/playerdata");
            PlayerAnimation = Content.Load<SpriteSheet>("entity/mc/mc_animation.sf", new JsonContentLoader());

            // Load Item Sprite Sheet
            ItemsPackSprites = Content.Load<SpriteSheet>("item/itemspack_spritesheet.sf", new JsonContentLoader());

            // Load Item Data
            ItemsDatas = Content.Load<List<ItemData>>("data/models/items");

            // Load Crafting Recipes Data
            CraftingRecipeDatas = Content.Load<List<CraftingRecipeData>>("data/models/crafting_recipes");

            // Load Character Datas
            CharacterDatas = Content.Load<List<CharacterData>>("data/models/characters");

            // Load Effect Sprite Sheet
            HitSpriteEffect = Content.Load<SpriteSheet>("effect/hit_effect.sf", new JsonContentLoader());
            HitSkillSpriteEffect = Content.Load<SpriteSheet>("effect/hit_skill_effect.sf", new JsonContentLoader());
            BossSpriteEffect = Content.Load<SpriteSheet>("effect/boss_effect.sf", new JsonContentLoader());
            StatesSpriteEffect = Content.Load<SpriteSheet>("effect/states.sf", new JsonContentLoader());

            // Load Bitmap Font          
            FontSensation = Content.Load<BitmapFont>("fonts/Sensation/Sensation");
            FontMinecraft = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
            FontTA8Bit = Content.Load<BitmapFont>("fonts/TA_8_Bit/TA_8_Bit");
            FontTA8BitBold = Content.Load<BitmapFont>("fonts/TA_8_Bit_Bold/TA_8_Bit_Bold");
            FontTA16Bit = Content.Load<BitmapFont>("fonts/TA_16_Bit/TA_16_Bit");

            // Load GUI Texture
            GuiTextures.Add(Content.Load<Texture2D>("gui/logo_wakeup"));                        // 0. logo_wakeup
            GuiTextures.Add(Content.Load<Texture2D>("gui/press_f"));                            // 1. press_f
            GuiTextures.Add(Content.Load<Texture2D>("gui/insufficient"));                       // 2. insufficient
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar"));                         // 3. health_bar
            GuiTextures.Add(Content.Load<Texture2D>("gui/healthpoints_gauge"));                 // 4. healthpoints_gauge
            GuiTextures.Add(Content.Load<Texture2D>("gui/mana_gauge"));                         // 5. mana_gauge
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_companion"));               // 6. health_bar_companion
            GuiTextures.Add(Content.Load<Texture2D>("gui/healthpoints_gauge_companion"));       // 7. healthpoints_gauge_companion
            GuiTextures.Add(Content.Load<Texture2D>("gui/noah_profile"));                       // 8. noah_profile
            GuiTextures.Add(Content.Load<Texture2D>("gui/companion_profile"));                  // 9. companion_profile
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_boss"));                    // 10. health_bar_boss
            GuiTextures.Add(Content.Load<Texture2D>("gui/boss_gauge"));                         // 11. boss_gauge
            GuiTextures.Add(Content.Load<Texture2D>("gui/item_bar"));                           // 12. item_bar
            GuiTextures.Add(Content.Load<Texture2D>("gui/item_slot"));                          // 13. item_slot
            GuiTextures.Add(Content.Load<Texture2D>("gui/gold_coin"));                          // 14. gold_coin
            GuiTextures.Add(Content.Load<Texture2D>("gui/heart"));                              // 15. heart
            GuiTextures.Add(Content.Load<Texture2D>("gui/selected_slot"));                      // 16. selected_slot
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_alpha"));                   // 17. health_bar_alpha
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_companion_alpha"));         // 18. health_bar_companion_alpha
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_boss_alpha"));              // 19. health_bar_boss_alpha
            GuiTextures.Add(Content.Load<Texture2D>("gui/quest_stamp"));                        // 20. quest_stamp
            GuiTextures.Add(Content.Load<Texture2D>("gui/level_gui"));                          // 21. level_gui
            GuiTextures.Add(Content.Load<Texture2D>("gui/exp_bar"));                            // 22. exp_bar
            GuiTextures.Add(Content.Load<Texture2D>("gui/exp_bar_alpha"));                      // 23. exp_bar_alpha
            GuiTextures.Add(Content.Load<Texture2D>("gui/exp_gauge"));                          // 24. exp_gauge
            GuiTextures.Add(Content.Load<Texture2D>("gui/burst_skill_gui"));                    // 25. burst_skill_gui
            GuiTextures.Add(Content.Load<Texture2D>("gui/burst_skill_gui_alpha"));              // 26. burst_skill_gui_alpha
            GuiTextures.Add(Content.Load<Texture2D>("gui/burst_skill_pic"));                    // 27. burst_skill_pic
            GuiTextures.Add(Content.Load<Texture2D>("gui/normal_skill_gui"));                   // 28. normal_skill_gui
            GuiTextures.Add(Content.Load<Texture2D>("gui/normal_skill_gui_alpha"));             // 29. normal_skill_gui_alpha
            GuiTextures.Add(Content.Load<Texture2D>("gui/normal_skill_pic"));                   // 30. normal_skill_pic
            GuiTextures.Add(Content.Load<Texture2D>("gui/passive_skill_gui"));                  // 31. passive_skill_gui
            GuiTextures.Add(Content.Load<Texture2D>("gui/passive_skill_gui_alpha"));            // 32. passive_skill_gui_alpha
            GuiTextures.Add(Content.Load<Texture2D>("gui/passive_skill_pic"));                  // 33. passive_skill_pic

            // Test
            // Initialize Player Data
            PlayerManager.Instance.Initialize();
        }

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            TotalPlayTime += deltaSeconds;

            var mouseState = Mouse.GetState();

            MousePosition = new Point2(mouseState.X, mouseState.Y);           
        }

        public Texture2D GetGuiTexture(GuiTextureName guiTextureName)
        {
            if (guiTextureIndices.TryGetValue(guiTextureName, out int index) && index < GuiTextures.Count)
            {
                return GuiTextures.ElementAt(index);
            }

            return null;
        }

        public static GameGlobals Instance
        {
            get
            {
                instance ??= new GameGlobals();
                return instance;
            }
        }
    }
}
