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
        public float DisplayFeedTime { set; get; }
        public int GameSaveIdex { private set; get; }
        public List<GameSave> GameSave {private set; get;}
        public List<string> ItemsFeed { private set; get; }
        public List<Rectangle> CollistionObject { private set; get; }  // "Collision"
        public List<Rectangle> ObjectOnLayer1 { private set; get; }  // "ObjectOnLayer"
        public List<Rectangle> ObjectOnLayer2 { private set; get; }
        public List<Rectangle> TableCraft { private set; get; }
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
            GameSaveIdex = 0; // to be initial

            SwitchDetectBox = false;
            IsShowDetectBox = false;

            SwitchShowPath = false;
            IsShowPath = false;

            IsDetectedItem = false;
            ShowInsufficientSign = false;

            DisplayFeedTime = 6f;

            GameSave = new List<GameSave>();
            ItemsFeed = new List<string>();
            CollistionObject = new List<Rectangle>();
            ObjectOnLayer1 = new List<Rectangle>();
            ObjectOnLayer2 = new List<Rectangle>();
            TableCraft = new List<Rectangle>();

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
