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

        private List<EntityStats> _slimeStatsList;

        private BitmapFont _fontMinecraft, _fontSensation;

        public TestScreen()
        {   
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Initialize Player's Data !! Gonna be move to Load GameSave later !!
            var playerStats = ScreenManager.Instance.Content.Load<PlayerStats>("data/models/player_stats");
            var playerAnimation = ScreenManager.Instance.Content.Load<SpriteSheet>("animation/mc/mc_animation.sf", new JsonContentLoader());
            var playerSprite = new AnimatedSprite(playerAnimation);
            PlayerManager.Instance.Initialize(playerSprite, playerStats);

            // Load Tile Map
            //_mapManager = new TiledMapBackgroundManager(Content, GraphicsDevice, Window, "tiledmaps/test1/level02");
            var _tileSet = Content.Load<Texture2D>("tiledmaps/test1/rpg_maker_vx_rtp_tileset_by_telles0808");
            var _tileMap = new TmxMap("Content/tiledmaps/test1/tilemap.tmx");
            _tileMapRender = new TilemapOrthogonalRender(_tileMap, _tileSet);

            // Load Data Game from JSON file
            _slimeStatsList = Content.Load<List<EntityStats>>("data/models/slime");

            // Load bitmap font
            _fontMinecraft = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
            _fontSensation = Content.Load<BitmapFont>("fonts/Sensation/Sensation");

            // Adding Slime to EntityList
            var _slimeAnimation = Content.Load<SpriteSheet>("animation/mobs/slime/slime_green.sf", new JsonContentLoader());
            Vector2 _slimeScale = new Vector2(2.0f, 1.5f);
            EntityManager.Instance.AddEntity(new Slime(new AnimatedSprite(_slimeAnimation), _slimeStatsList[0], _slimeScale));
            EntityManager.Instance.AddEntity(new Slime(new AnimatedSprite(_slimeAnimation), _slimeStatsList[1], _slimeScale));           

            // Adding HUD
            _hudSystem = new HudSystem(_fontSensation);
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Dispose()
        {
            //_mapManager.Dispose();
            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            if (GameGlobals.Instance.IsGameActive)
            {
                GameGlobals.Instance.keyboardPreviose = GameGlobals.Instance.keyboardCurrent;
                GameGlobals.Instance.keyboardCurrent = Keyboard.GetState();

                EntityManager.Instance.Update(gameTime);
                                
                if (GameGlobals.Instance.keyboardCurrent.IsKeyDown(Keys.M))
                {
                    JsonFileManager.SaveGame();
                }

                if (GameGlobals.Instance.keyboardCurrent.IsKeyDown(Keys.B) && !GameGlobals.Instance.IsDebugMode)
                {
                    // Toggle the IsShowDetectBox flag
                    GameGlobals.Instance.IsShowDetectBox = !GameGlobals.Instance.IsShowDetectBox;

                    // Update the boolean variable to indicate that the "B" button has been pressed
                    GameGlobals.Instance.IsDebugMode = true;
                }
                else if (GameGlobals.Instance.keyboardCurrent.IsKeyUp(Keys.B))
                {
                    // Update the boolean variable to indicate that the "B" button is not currently pressed
                    GameGlobals.Instance.IsDebugMode = false;
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
