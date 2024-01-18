using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Medicraft.Systems
{
    public class GameGlobals
    {
        public MouseState mousePreviose, mouseCurrent;
        public KeyboardState keyboardPreviose, keyboardCurrent;
        public Vector2 HUDPosition { set; get; }
        public Vector2 InitialCameraPos { set; get; }
        public Vector2 AddingCameraPos { set; get; }        
        public Vector2 GameScreen { private set; get; }
        public Vector2 GameScreenCenter { private set; get; }
        public bool IsGameActive { set; get; }
        public bool SwitchDetectBox { set; get; }
        public bool IsShowDetectBox { set; get; }
        public bool SwitchShowPath { set; get; }
        public bool IsDetectedItem { set; get; }
        public bool ShowInsufficientSign { set; get; } 
        public bool IsShowPath { set; get; }
        public int GameSaveIdex { private set; get; }
        public string GameSavePath { private set; get; }
        public List<GameSaveData> GameSave { private set; get;}
        public int MaximunInventorySlot { private set; get; }
        public List<ItemData> ItemDatas { set; get; }
        public List<int> CollectedItemFeed { private set; get; }
        public int MaximumItemFeed { private set; get; }
        public float DisplayFeedTime { set; get; }
        public float MaximumDisplayFeedTime { private set; get; }
        public List<Rectangle> CollistionObject { private set; get; }  // "Collision"
        public List<Rectangle> TopLayerObject { private set; get; }  // "ObjectOnLayer"
        public List<Rectangle> MiddleLayerObject { private set; get; }
        public List<Rectangle> BottomLayerObject { private set; get; }
        public List<Rectangle> TableCraft { private set; get; }
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

        public int test_int { set; get; }

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

            IsDetectedItem = false;
            ShowInsufficientSign = false;

            GameSave = new List<GameSaveData>();
            GameSaveIdex = 0; // to be initial
            GameSavePath = "save/gamesaves.json";

            MaximunInventorySlot = 64;
            ItemDatas = new List<ItemData>();

            CollectedItemFeed = new List<int>();
            MaximumItemFeed = 6;
            DisplayFeedTime = 0;
            MaximumDisplayFeedTime = 6f;

            CollistionObject = new List<Rectangle>();
            TopLayerObject = new List<Rectangle>();
            MiddleLayerObject = new List<Rectangle>();
            BottomLayerObject = new List<Rectangle>();
            TableCraft = new List<Rectangle>();

            TopEntityDepth = 0.2f;
            MiddleEntityDepth = 0.4f;
            BottomEntityDepth = 0.6f;

            TopObjectDepth = 0.3f;
            MiddleObjectDepth = 0.5f;
            BottomObjectDepth = 0.7f;
            BackgroundDepth = 0.9f;

            test_int = 0;
        }

        public static GameGlobals Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameGlobals();
                }
                return instance;
            }
        }
    }
}
