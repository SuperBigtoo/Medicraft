using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Medicraft.Systems
{
    public class Singleton
    {
        public MouseState mousePreviose, mouseCurrent;
        public KeyboardState keyboardPreviose, keyboardCurrent;
        public Vector2 addingHudPos;
        public Vector2 cameraPosition;
        public Vector2 addingCameraPos;
        public bool IsGameActive;
        public Vector2 gameScreen
        {
            private set; get;
        }
        public Vector2 gameScreen_Center
        {
            private set; get;
        }

        public int gameSaveIdex;
        public readonly List<GameSave> gameSave;

        private static Singleton instance;

        private Singleton()
        {
            gameScreen = new Vector2(1440, 900);
            gameScreen_Center = new Vector2(1440/2, 900/2);
            addingHudPos = Vector2.Zero;
            cameraPosition = gameScreen_Center;
            addingCameraPos = Vector2.Zero;
            gameSave = new List<GameSave>();
            gameSaveIdex = 0; // to be initial
        }

        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Singleton();
                }
                return instance;
            }
        }
    }
}
