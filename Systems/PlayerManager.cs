using Medicraft.Data;
using Medicraft.Data.Models;
using Medicraft.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;
using System.IO;

namespace Medicraft.Systems
{
    public class PlayerManager
    {
        public Player Player { private set; get; }

        private static PlayerManager instance;
        private PlayerManager()
        {
        }

        public void Initialize(AnimatedSprite playerSprite, PlayerStats playerStats)
        {
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

                playerStats = gameSave.PlayerStats;
                Player = new Player(playerSprite, playerStats);
            }
            else
            {
                Player = new Player(playerSprite, playerStats);

                // Initialize camera position
                GameGlobals.Instance.InitialCameraPos = new Vector2((float)playerStats.Position[0]
                    , (float)playerStats.Position[1]);
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
