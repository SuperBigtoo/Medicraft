using Medicraft.Data;
using Medicraft.Data.Models;
using Medicraft.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;
using System.Collections.Generic;
using System.Linq;

namespace Medicraft.Systems
{
    public class PlayerManager
    {
        public Player Player { private set; get; }
        public PlayerData InitialPlayerData { private set; get; }
        public bool IsPlayerDead { private set; get; }

        private static PlayerManager instance;
        private PlayerManager()
        {           
            IsPlayerDead = false;
        }

        public void Initialize(AnimatedSprite playerSprite, PlayerData initialPlayerData)
        {
            InitialPlayerData = initialPlayerData;

            if (GameGlobals.Instance.GameSave.Count != 0)
            {
                // Load save game according to selected index. 
                var gameSave = GameGlobals.Instance.GameSave[GameGlobals.Instance.GameSaveIdex];               

                // Initialize camera position
                GameGlobals.Instance.InitialCameraPos = new Vector2((float)gameSave.CameraPosition[0]
                    , (float)gameSave.CameraPosition[1]);

                // Initialize HUD position
                GameGlobals.Instance.HUDPosition = new Vector2((float)gameSave.HUDPosition[0]
                    , (float)gameSave.HUDPosition[1]);

                // Initialize Player's Inventory
                var inventoryData = gameSave.PlayerData.InventoryData;
                InventoryManager.Instance.InitializeInventory(inventoryData);

                // Initial Player
                var basePlayerData = gameSave.PlayerData;
                Player = new Player(playerSprite, basePlayerData);
            }
            else // In case New Game
            {
                // Initial Player
                Player = new Player(playerSprite, initialPlayerData);

                // Initialize camera position
                GameGlobals.Instance.InitialCameraPos = new Vector2((float)initialPlayerData.Position[0]
                    , (float)initialPlayerData.Position[1]);

                // Initialize Player's Inventory
                var inventoryData = initialPlayerData.InventoryData;
                InventoryManager.Instance.InitializeInventory(inventoryData);
            }
        }

        public void Update(GameTime gameTime)
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

            // Check if CurrentScreen is TestScreen
            if (ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.TestScreen)
            {
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
            }
           

            Player.Update(gameTime, keyboardCur, keyboardPrev, mouseCur, mousePrev);

            // Check Player HP for Deadq
            if (Player.HP <= 0 && !IsPlayerDead)
            {
                IsPlayerDead = true;
            }

            if (IsPlayerDead)
            {
                if (GameGlobals.Instance.mouseCurrent.LeftButton == ButtonState.Pressed
                    && GameGlobals.Instance.mousePreviose.LeftButton == ButtonState.Released)
                {
                    RespawnPlayer();
                }
            }
        }

        private void RespawnPlayer()
        {           
            if (IsPlayerDead)
            {
                Player.HP = Player.GetStats().HP; // for testing
                Player.Position = new Vector2((float)InitialPlayerData.Position[0]
                    , (float)InitialPlayerData.Position[1]);

                // Adjust HUD and camera positions
                GameGlobals.Instance.HUDPosition = Player.Position - new Vector2(720, 450);
                GameGlobals.Instance.InitialCameraPos = Player.Position;
                GameGlobals.Instance.AddingCameraPos = Vector2.Zero;

                var entities = EntityManager.Instance.Entities;
                foreach (var entity in entities.Where(e => !e.IsDestroyed))
                {
                    entity.AggroTime = 0f;
                }

                IsPlayerDead = false;
                Player.IsDying = false;
            }
        }

        public static PlayerManager Instance
        {
            get
            {
                instance ??= new PlayerManager();
                return instance;
            }
        }
    }
}
