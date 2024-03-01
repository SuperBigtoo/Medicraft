using Medicraft.Data;
using Medicraft.Data.Models;
using Medicraft.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Medicraft.Systems.Managers
{
    public class PlayerManager
    {
        public Player Player { private set; get; }
        public bool IsPlayerDead { private set; get; }

        private static PlayerManager instance;
        private PlayerManager()
        {
            IsPlayerDead = false;
        }

        public void Initialize()
        {
            var initialPlayerData = GameGlobals.Instance.InitialPlayerData;
            var playerSprite = new AnimatedSprite(GameGlobals.Instance.PlayerAnimation);

            if (GameGlobals.Instance.GameSave.Count != 0)
            {
                // Load save game according to selected index. 
                var gameSave = GameGlobals.Instance.GameSave[GameGlobals.Instance.GameSaveIdex];

                // Set Total Playtime
                GameGlobals.Instance.TotalPlayTime = gameSave.TotalPlayTime[0] * 3600 
                                                    + gameSave.TotalPlayTime[1] * 60
                                                    + gameSave.TotalPlayTime[2];

                //// Initialize camera position
                //GameGlobals.Instance.InitialCameraPos = new Vector2((float)gameSave.CameraPosition[0]
                //    , (float)gameSave.CameraPosition[1]);

                //// Initialize HUD position
                //GameGlobals.Instance.HUDPosition = new Vector2((float)gameSave.HUDPosition[0]
                //    , (float)gameSave.HUDPosition[1]);

                // Initialize Player's Inventory
                var inventoryData = gameSave.PlayerData.InventoryData;
                InventoryManager.Instance.InitializeInventory(inventoryData);

                // Initial Player
                var basePlayerData = gameSave.PlayerData;
                Player = new Player(playerSprite, basePlayerData);

                // Adjust HUD and camera positions
                GameGlobals.Instance.HUDPosition = Player.Position - GameGlobals.Instance.GameScreenCenter;
                GameGlobals.Instance.InitialCameraPos = Player.Position;
                GameGlobals.Instance.AddingCameraPos = Vector2.Zero;
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

            GameGlobals.Instance.HUDPosition = Player.Position - GameGlobals.Instance.GameScreenCenter;
        }

        public void Update(GameTime gameTime)
        {
            // Key Controller
            GameGlobals.Instance.PrevKeyboard = GameGlobals.Instance.CurKeyboard;
            GameGlobals.Instance.CurKeyboard = Keyboard.GetState();
            var keyboardCur = GameGlobals.Instance.CurKeyboard;
            var keyboardPrev = GameGlobals.Instance.PrevKeyboard;

            // Mouse Controller
            GameGlobals.Instance.PrevMouse = GameGlobals.Instance.CurMouse;
            GameGlobals.Instance.CurMouse = Mouse.GetState();
            var mouseCur = GameGlobals.Instance.CurMouse;
            var mousePrev = GameGlobals.Instance.PrevMouse;

            if (keyboardCur.IsKeyUp(Keys.M) && keyboardPrev.IsKeyDown(Keys.M))
            {
                JsonFileManager.SaveGame();
            }

            // Check if CurrentScreen is TestScreen
            if (ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.TestScreen)
            {
                // Debug Mode
                if (keyboardCur.IsKeyDown(Keys.B) && !GameGlobals.Instance.SwitchDebugMode)
                {
                    // Toggle the IsShowDetectBox flag
                    GameGlobals.Instance.IsDebugMode = !GameGlobals.Instance.IsDebugMode;

                    // Update the boolean variable to indicate that the "B" button has been pressed
                    GameGlobals.Instance.SwitchDebugMode = true;
                }
                else if (keyboardCur.IsKeyUp(Keys.B))
                {
                    // Update the boolean variable to indicate that the "B" button is not currently pressed
                    GameGlobals.Instance.SwitchDebugMode = false;
                }

                // Show Path Finding of Mobs
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
                if (GameGlobals.Instance.CurMouse.LeftButton == ButtonState.Pressed
                    && GameGlobals.Instance.PrevMouse.LeftButton == ButtonState.Released)
                {
                    RespawnPlayer();
                }
            }
        }

        public void UpdateMapPositionData()
        {
            
        }

        public void SetupPlayer(ScreenManager.GameScreen gameScreen, ScreenManager.LoadMapAction loadAction)
        {
            if (loadAction == ScreenManager.LoadMapAction.LoadGameSave) return;

            switch (gameScreen)
            {
                case ScreenManager.GameScreen.TestScreen:
                    var mapPositionData = GameGlobals.Instance.MapPositionDatas.Where(m => m.Name.Equals("Test"));
                    var positionData = mapPositionData.ElementAt(0).Positions.Where(p => p.Name.Equals("LastPosition"));
                    var position = new Vector2(
                        (float)positionData.ElementAt(0).Value[0],
                        (float)positionData.ElementAt(0).Value[1]);

                    Player.Position = position;
                    break;
            }

            // Adjust HUD and camera positions
            GameGlobals.Instance.HUDPosition = Player.Position - GameGlobals.Instance.GameScreenCenter;
            GameGlobals.Instance.InitialCameraPos = Player.Position;
            GameGlobals.Instance.AddingCameraPos = Vector2.Zero;
        }

        private void RespawnPlayer()
        {
            if (IsPlayerDead)
            {
                Player.HP = Player.MaximumHP;

                var respawnPos = new Vector2(
                    (float)GameGlobals.Instance.InitialPlayerData.Position[0],
                    (float)GameGlobals.Instance.InitialPlayerData.Position[1]);

                Player.Position = respawnPos;

                // Adjust HUD and camera positions
                GameGlobals.Instance.HUDPosition = Player.Position - GameGlobals.Instance.GameScreenCenter;
                GameGlobals.Instance.InitialCameraPos = Player.Position;
                GameGlobals.Instance.AddingCameraPos = Vector2.Zero;

                var entities = EntityManager.Instance.Entities;
                foreach (var entity in entities.Where(e => !e.IsDestroyed))
                {
                    entity.AggroTimer = 0f;
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
