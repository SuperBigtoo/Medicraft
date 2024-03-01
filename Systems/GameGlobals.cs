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
        public Vector2 HUDPosition { set; get; }
        public Vector2 InitialCameraPos { set; get; }
        public Vector2 AddingCameraPos { set; get; }
        public Vector2 GameScreen { private set; get; }
        public Vector2 GameScreenCenter { private set; get; }
        public bool IsGameActive { set; get; }
        public bool SwitchDebugMode { set; get; }
        public bool IsDebugMode { set; get; }
        public bool SwitchShowPath { set; get; }
        public bool IsDetectedGameObject { set; get; }
        public bool ShowInsufficientSign { set; get; }
        public bool IsShowPath { set; get; }
        public bool IsTransitionFinished { set; get; }
        public float TotalPlayTime { set; get; }

        // GameSave
        public int GameSaveIdex { private set; get; }
        public string GameSavePath { private set; get; }
        public List<GameSaveData> GameSave { private set; get; }

        // Inventory
        public int MaximunInventorySlot { private set; get; }
        public int MaximunItemCount { private set; get; }
        public int DefaultSlot { private set; get; }

        // Game Datas
        public PlayerData InitialPlayerData { private set; get; }
        public SpriteSheet PlayerAnimation { private set; get; }
        public BitmapFont FontSensation { private set; get; }
        public BitmapFont FontMinecraft { private set; get; }
        public BitmapFont FontTA8Bit { private set; get; }
        public BitmapFont FontTA8BitBold { private set; get; }
        public BitmapFont FontTA16Bit { private set; get; }
        public List<Texture2D> GuiTextures { private set; get; }
        public List<MapPositionData> MapPositionDatas { private set; get; }         // All Point Loaction of Maps
        public List<ItemData> ItemsDatas { private set; get; }       // All items data
        // All equipments stats data
        // All item's effect data
        public List<CraftingRecipeData> CraftingRecipeDatas { private set; get; }       // All Crafting Recipe data
        public List<CharacterData> CharacterDatas { private set; get; }                 // All Character data
        public SpriteSheet ItemsPackSprites { private set; get; }                   // All Item sprite

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
        public List<RectangleF> WarpPointArea { private set; get; }
        public List<RectangleF> MobPartrolArea { private set; get; }
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
            select,
            health_bar_alpha,
            health_bar_companion_alpha,
            health_bar_boss_alpha
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
            { GuiTextureName.select, 16 },
            { GuiTextureName.health_bar_alpha, 17 },
            { GuiTextureName.health_bar_companion_alpha, 18 },
            { GuiTextureName.health_bar_boss_alpha, 19 },
        };

        // For Testing
        public int TestInt { set; get; }
        public Texture2D TestIcon { set; get; }

        private static GameGlobals instance;

        private GameGlobals()
        {
            GameScreen = new Vector2(1440, 900);
            GameScreenCenter = new Vector2(1440/2, 900/2);
            HUDPosition = Vector2.Zero;
            InitialCameraPos = Vector2.Zero;
            AddingCameraPos = Vector2.Zero; 

            SwitchDebugMode = false;
            IsDebugMode = false;

            SwitchShowPath = false;
            IsShowPath = false;

            IsDetectedGameObject = false;
            ShowInsufficientSign = false;

            IsTransitionFinished = true;

            TotalPlayTime = 0;
            GameSave = [];
            GameSaveIdex = 0; // to be initial
            GameSavePath = "save/gamesaves.json";

            FeedPoint = new(140f, 380f);
            MaximunInventorySlot = 64;
            MaximunItemCount = 9999;
            DefaultSlot = 999;

            GuiTextures = [];
            MapPositionDatas = [];
            ItemsDatas = [];
            CharacterDatas = [];
            CraftingRecipeDatas = [];

            CollectedItemFeed = [];
            MaximumItemFeed = 6;
            DisplayFeedTime = 0;
            MaximumDisplayFeedTime = 6f;

            CollistionObject = [];
            TopLayerObject = [];
            MiddleLayerObject = [];
            BottomLayerObject = [];
            CraftingTableArea = [];
            SavingTableArea = [];
            WarpPointArea = [];
            MobPartrolArea = [];

            TopEntityDepth = 0.2f;
            MiddleEntityDepth = 0.4f;
            BottomEntityDepth = 0.6f;

            TopObjectDepth = 0.3f;
            MiddleObjectDepth = 0.5f;
            BottomObjectDepth = 0.7f;
            BackgroundDepth = 0.9f;

            TestInt = 0;
        }

        public void Initialize(ContentManager Content)
        {
            this.Content = Content;
        }

        public void LoadContent()
        {         
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
            MapPositionDatas = Content.Load<List<MapPositionData>>("data/models/map_positions");

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
            GuiTextures.Add(Content.Load<Texture2D>("gui/select"));                             // 16. select
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_alpha"));                   // 17. health_bar_alpha
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_companion_alpha"));         // 18. health_bar_companion_alpha
            GuiTextures.Add(Content.Load<Texture2D>("gui/health_bar_boss_alpha"));              // 19. health_bar_boss_alpha


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
