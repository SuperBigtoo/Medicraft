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
        public bool IsShowPath { set; get; }
        public int GameSaveIdex { private set; get; }
        public List<GameSave> GameSave {private set; get;}
        public List<Rectangle> CollistionObject { private set; get; }  // "Collision"
        public List<Rectangle> OnGroundObject { private set; get; }  // "ObjectOnLayer"
        public int TILE_SIZE { set; get; }
        public int NUM_ROWS { set; get; }
        public int NUM_COLUMNS { set; get; }
        public int[,] Map { set; get; }

        private static GameGlobals instance;

        public const int BLOCK = 0;
        public const int ROAD = 1;
        public const int START = 2;
        public const int END = 3;
        public const int BLANK = 4;

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

            GameSave = new List<GameSave>();
            CollistionObject = new List<Rectangle>();
            OnGroundObject = new List<Rectangle>();
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
