using Medicraft.Data;
using Medicraft.Data.Models;
using Medicraft.Entities;
using Medicraft.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;
using System.Collections.Generic;

namespace Medicraft.Systems
{
    public class PlayerManager
    {
        public Player Player { private set; get; }

        public PlayerStats BasePlayerStats { private set; get; }

        public Dictionary<string, int> Inventory { private set; get; }

        public int Coin { set; get; }

        private static PlayerManager instance;
        private PlayerManager()
        {
            Inventory = new Dictionary<string, int>()
            {
                {"herb_1", 0},
                {"herb_2", 0},
                {"drug", 0}
            };

            Coin = 0;
        }

        public void Initialize(AnimatedSprite playerSprite, PlayerStats basePlayerStats)
        {
            BasePlayerStats = basePlayerStats;

            if (GameGlobals.Instance.GameSave.Count != 0)
            {
                // Load save game according to selected index. 
                var gameSave = GameGlobals.Instance.GameSave[GameGlobals.Instance.GameSaveIdex];               

                // Initialize camera position
                GameGlobals.Instance.InitialCameraPos = new Vector2((float)gameSave.Camera_Position[0]
                    , (float)gameSave.Camera_Position[1]);

                // Initialize HUD position
                GameGlobals.Instance.HUDPosition = new Vector2((float)gameSave.HUD_Position[0]
                    , (float)gameSave.HUD_Position[1]);

                var playerStats = gameSave.PlayerStats;
                Player = new Player(playerSprite, playerStats);
            }
            else // In case New Game
            {
                Player = new Player(playerSprite, basePlayerStats);

                // Initialize camera position
                GameGlobals.Instance.InitialCameraPos = new Vector2((float)basePlayerStats.Position[0]
                    , (float)basePlayerStats.Position[1]);
            }
        }

        public void Update(GameTime gameTime, float playerFrontDepth, float playerBehideDepth)
        {
            // Key Controller
            GameGlobals.Instance.keyboardPreviose = GameGlobals.Instance.keyboardCurrent;
            GameGlobals.Instance.keyboardCurrent = Keyboard.GetState();
            var keyboardCur = GameGlobals.Instance.keyboardCurrent;
            var keyboardPrev = GameGlobals.Instance.keyboardPreviose;

            // Mouse Controller
            GameGlobals.Instance.mousePreviose = GameGlobals.Instance.mouseCurrent;
            GameGlobals.Instance.mouseCurrent = Mouse.GetState();
            var mouseCur = GameGlobals.Instance.mouseCurrent;
            var mousePrev = GameGlobals.Instance.mousePreviose;

            if (keyboardCur.IsKeyDown(Keys.M))
            {
                JsonFileManager.SaveGame();
            }

            if (keyboardCur.IsKeyDown(Keys.B) && !GameGlobals.Instance.SwitchDetectBox)
            {
                // Toggle the IsShowDetectBox flag
                GameGlobals.Instance.IsShowDetectBox = !GameGlobals.Instance.IsShowDetectBox;

                // Update the boolean variable to indicate that the "B" button has been pressed
                GameGlobals.Instance.SwitchDetectBox = true;
            }
            else if (keyboardCur.IsKeyUp(Keys.B))
            {
                // Update the boolean variable to indicate that the "B" button is not currently pressed
                GameGlobals.Instance.SwitchDetectBox = false;
            }

            if (keyboardCur.IsKeyDown(Keys.V) && !GameGlobals.Instance.SwitchShowPath)
            {
                GameGlobals.Instance.IsShowPath = !GameGlobals.Instance.IsShowPath;

                GameGlobals.Instance.SwitchShowPath = true;
            }
            else if (keyboardCur.IsKeyUp(Keys.V))
            {
                GameGlobals.Instance.SwitchShowPath = false;
            }

            Player.Update(gameTime, keyboardCur, keyboardPrev
                , mouseCur, mousePrev
                , playerFrontDepth, playerBehideDepth);
        }

        public static PlayerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PlayerManager();
                }
                return instance;
            }
        }
    }
}
