﻿using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Medicraft.Systems
{
    public class Singleton
    {
        public MouseState mousePreviose, mouseCurrent;
        public KeyboardState keyboardPreviose, keyboardCurrent;
        public Vector2 addingHudPos { set; get; }
        public Vector2 cameraPosition { set; get; }
        public Vector2 addingCameraPos { set; get; }        
        public Vector2 gameScreen { private set; get; }
        public Vector2 gameScreen_Center { private set; get; }
        public bool IsGameActive { set; get; }
        public bool IsDebugMode { set; get; }
        public bool IsShowDetectBox { set; get; }
        public int gameSaveIdex { private set; get; }
        public List<GameSave> gameSave {private set; get;}
        public List<Rectangle> CollistionObject { private set; get; }  // "Collision"
        public List<Rectangle> OnGroundObject { private set; get; }  // "ObjectLayer3"

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

            IsDebugMode = false;
            IsShowDetectBox = false;

            CollistionObject = new List<Rectangle>();
            OnGroundObject = new List<Rectangle>();
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
