using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Content;
using Medicraft.Entities;
using Medicraft.Data;
using Microsoft.Xna.Framework.Input;
using TiledSharp;
using System.Collections.Generic;

namespace Medicraft.Screens
{
    public class TestScreen : Screen
    {
        private HudSystem _hudSystem;
        //private TiledmapBGExtended _mapManager;
        //private TilemapIsometricRender _tileMapRender;
        private TilemapOrthogonalRender _tileMapRender;

        private bool wasBButtonPressed = false;

        private PlayerStats _playerStats;
        private List<EntityStats> _slimeStatsList;
        private BitmapFont _fontMinecraft, _fontSensation;

        public TestScreen()
        {
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Load Tile Map
            //_mapManager = new TiledMapBackgroundManager(Content, GraphicsDevice, Window, "tiledmaps/test1/level02");
            var _tileSet = Content.Load<Texture2D>("tiledmaps/test1/rpg_maker_vx_rtp_tileset_by_telles0808");
            var _tileMap = new TmxMap("Content/tiledmaps/test1/tilemap.tmx");
            _tileMapRender = new TilemapOrthogonalRender(_tileMap, _tileSet);

            // Test Load Data
            if (Singleton.Instance.gameSave.Count != 0)
            {
                var gameSave = Singleton.Instance.gameSave[Singleton.Instance.gameSaveIdex];
                _playerStats = gameSave.PlayerStats;

                Singleton.Instance.cameraPosition = new Vector2((float)gameSave.Camera_Position[0]
                    , (float)gameSave.Camera_Position[1]);

                Singleton.Instance.addingHudPos = new Vector2((float)gameSave.HUD_Position[0]
                    , (float)gameSave.HUD_Position[1]);
            }
            else
            {
                _playerStats = Content.Load<PlayerStats>("data/models/player_stats");
            }

            // Load Data Game from JSON file
            _slimeStatsList = Content.Load<List<EntityStats>>("data/models/slime");

            // Load bitmap font
            _fontMinecraft = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
            _fontSensation = Content.Load<BitmapFont>("fonts/Sensation/Sensation");

            // Adding Player to EntityList
            var _playerAnimation = Content.Load<SpriteSheet>("animation/MCSpriteSheet.sf", new JsonContentLoader());
            var _playerSprite = new AnimatedSprite(_playerAnimation);
            PlayerManager.Instance.player = new Player(_playerSprite, _playerStats);

            // Adding Slime to EntityList
            var _slimeAnimation = Content.Load<SpriteSheet>("animation/Slime_Green.sf", new JsonContentLoader());
            var _slimeSprite = new AnimatedSprite(_slimeAnimation);
            Vector2 _slimeScale = new Vector2(2.0f, 1.5f);
            EntityManager.Instance.AddEntity(new Slime(_slimeSprite, _slimeStatsList[0], _slimeScale));
            EntityManager.Instance.AddEntity(new Slime(_slimeSprite, _slimeStatsList[1], _slimeScale));

            // Adding HUD
            _hudSystem = new HudSystem(_fontSensation);
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            if (Singleton.Instance.IsGameActive)
            {
                Singleton.Instance.keyboardPreviose = Singleton.Instance.keyboardCurrent;
                Singleton.Instance.keyboardCurrent = Keyboard.GetState();

                EntityManager.Instance.Update(gameTime);

                Camera.SetPosition(Singleton.Instance.cameraPosition + Singleton.Instance.addingCameraPos);
                                
                if (Singleton.Instance.keyboardCurrent.IsKeyDown(Keys.M))
                {
                    JsonFileManager.SaveGame();
                }

                if (Singleton.Instance.keyboardCurrent.IsKeyDown(Keys.B) && !wasBButtonPressed)
                {
                    // Toggle the IsShowDetectBox flag
                    Singleton.Instance.IsShowDetectBox = !Singleton.Instance.IsShowDetectBox;

                    // Update the boolean variable to indicate that the "B" button has been pressed
                    wasBButtonPressed = true;
                }
                else if (Singleton.Instance.keyboardCurrent.IsKeyUp(Keys.B))
                {
                    // Update the boolean variable to indicate that the "B" button is not currently pressed
                    wasBButtonPressed = false;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_fontMinecraft, $"Name: {_slimeStatsList[0].Name}", new Vector2(50, 50), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"HP: {_slimeStatsList[0].HP}", new Vector2(50, 80), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"ATK: {_slimeStatsList[0].ATK}", new Vector2(50, 110), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"DEF: {_slimeStatsList[0].DEF_Percent}", new Vector2(50, 140), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"Speed: {_slimeStatsList[0].Speed}", new Vector2(50, 170), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"Evasion: {_slimeStatsList[0].Evasion}", new Vector2(50, 200), Color.White);

            //_mapManager.Draw(spriteBatch, _hudSystem);
            EntityManager.Instance.Draw(spriteBatch);

            _tileMapRender.Draw(spriteBatch);

            _hudSystem.DrawTest(spriteBatch);

            
        }
    }
}
