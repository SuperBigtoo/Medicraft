using Medicraft.Data;
using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Content;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public Vector2 HUDPosition { set; get; }
        public Vector2 InitialCameraPos { set; get; }
        public Vector2 AddingCameraPos { set; get; }        
        public Vector2 GameScreen { private set; get; }
        public Vector2 GameScreenCenter { private set; get; }
        public bool IsGameActive { set; get; }
        public bool SwitchDetectBox { set; get; }
        public bool IsShowDetectBox { set; get; }
        public bool SwitchShowPath { set; get; }
        public bool IsDetectedGameObject { set; get; }
        public bool ShowInsufficientSign { set; get; } 
        public bool IsShowPath { set; get; }

        // GameSave
        public int GameSaveIdex { private set; get; }
        public string GameSavePath { private set; get; }
        public List<GameSaveData> GameSave { private set; get;}

        // Inventory
        public int MaximunInventorySlot { private set; get; }
        public int MaximunItemCount { private set; get; }
        public int DefaultSlot { private set; get; }

        // Game Datas
        public BitmapFont FontTA16Bit { private set; get; }
        public List<ItemData> ItemsDatas { private set; get; }       // All items data
        // All equipments stats data
        // All item's effect data
        public List<CraftingRecipeData> CraftingRecipeDatas { private set; get; }       // All Crafting Recipe data
        public List<CharacterData> CharacterDatas { private set; get; }                 // All Character data
        public SpriteSheet ItemsPackSprites { private set; get; }                   // All Item sprite

        // Collecting Item Feed
        public List<InventoryItemData> CollectedItemFeed { private set; get; }      // Feed collected item
        public int MaximumItemFeed { private set; get; }
        public float DisplayFeedTime { set; get; }
        public float MaximumDisplayFeedTime { private set; get; }

        // Tilemap & Layer Depth
        public List<Rectangle> CollistionObject { private set; get; }       // "Collision"
        public List<Rectangle> TopLayerObject { private set; get; }         // "ObjectOnLayer"
        public List<Rectangle> MiddleLayerObject { private set; get; }
        public List<Rectangle> BottomLayerObject { private set; get; }
        public List<Rectangle> TableCraftArea { private set; get; }
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

            SwitchDetectBox = false;
            IsShowDetectBox = false;

            SwitchShowPath = false;
            IsShowPath = false;

            IsDetectedGameObject = false;
            ShowInsufficientSign = false;

            GameSave = [];
            GameSaveIdex = 0; // to be initial
            GameSavePath = "save/gamesaves.json";

            MaximunInventorySlot = 64;
            MaximunItemCount = 9999;
            DefaultSlot = 999;

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
            TableCraftArea = [];

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

            // Load Item Datas
            ItemsDatas = Content.Load<List<ItemData>>("data/models/items");

            // Load Item Sprite Sheet
            ItemsPackSprites = Content.Load<SpriteSheet>("item/itemspack_spritesheet.sf", new JsonContentLoader());

            // Load Crafting Recipes Data
            CraftingRecipeDatas = Content.Load<List<CraftingRecipeData>>("data/models/crafting_recipes");

            // Load Character Datas
            CharacterDatas = Content.Load<List<CharacterData>>("data/models/characters");

            // Load Font Bitmap
            FontTA16Bit = Content.Load<BitmapFont>("fonts/TA_16_Bit/TA_16_Bit");
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
