using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Content;
using Medicraft.Entities;
using TiledSharp;
using System.Collections.Generic;
using Medicraft.Systems.Spawners;
using Medicraft.Items;

namespace Medicraft.Screens
{
    public class TestScreen : Screen
    {
        private HudSystem _hudSystem;

        private TilemapOrthogonalRender _tileMapRender;

        private List<EntityStats> _slimeStatsList;
        private List<ItemStats> _itemStatsList;

        private BitmapFont _fontMinecraft, _fontSensation;

        private TmxMap _tileMap;

        public TestScreen()
        {   
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Initialize Player's Data !! Gonna be move to Load GameSave later !!
            var basePlayerStats = Content.Load<PlayerStats>("data/models/player_stats");
            var playerAnimation = Content.Load<SpriteSheet>("animation/mc/mc_spritesheet.sf", new JsonContentLoader());
            var playerSprite = new AnimatedSprite(playerAnimation);
            PlayerManager.Instance.Initialize(playerSprite, basePlayerStats);

            // Load Tile Map
            //_mapManager = new TiledMapBackgroundManager(Content, GraphicsDevice, Window, "tiledmaps/test1/level02");
            var _tileSet = Content.Load<Texture2D>("tiledmaps/demo/rpg_maker_vx_rtp_tileset_by_telles0808");
            _tileMap = new TmxMap("Content/tiledmaps/demo/Demo.tmx");
            _tileMapRender = new TilemapOrthogonalRender(_tileMap, _tileSet);

            // Load Data Game from JSON file
            _slimeStatsList = Content.Load<List<EntityStats>>("data/models/slime");
            _itemStatsList = Content.Load<List<ItemStats>>("data/models/items_demo");

            // Load bitmap font
            _fontMinecraft = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
            _fontSensation = Content.Load<BitmapFont>("fonts/Sensation/Sensation");

            // Adding Slime to MobSpawner
            var _slimeAnimation = Content.Load<SpriteSheet>("animation/mobs/slime/slimes_spritesheet.sf", new JsonContentLoader());
            var _slimeScale = new Vector2(3.0f, 2.5f);
            var _mobSpawner = new MobSpawner(10f);
            _mobSpawner.AddEntity(new Slime(new AnimatedSprite(_slimeAnimation), _slimeStatsList[0], _slimeScale));
            EntityManager.Instance.Initialize(_mobSpawner);

            // Adding Items to ItemSpawner
            var _itemAnimation = Content.Load<SpriteSheet>("items/items_demo.sf", new JsonContentLoader());
            var _itemSpawner = new ItemSpawner(10f);
            _itemSpawner.AddItem(new Herb1(new AnimatedSprite(_itemAnimation), _itemStatsList[0], Vector2.One));
            _itemSpawner.AddItem(new Herb1(new AnimatedSprite(_itemAnimation), _itemStatsList[1], Vector2.One));
            _itemSpawner.AddItem(new Herb1(new AnimatedSprite(_itemAnimation), _itemStatsList[2], Vector2.One));
            ItemManager.Instance.Initialize(_itemSpawner);

            // Adding HUD
            var textureList = new Texture2D[]
            {
                Content.Load<Texture2D>("items/heart"),
                Content.Load<Texture2D>("items/herb_1"),
                Content.Load<Texture2D>("items/herb_2"),
                Content.Load<Texture2D>("items/drug_1"),
                Content.Load<Texture2D>("items/gold_coin"),
                Content.Load<Texture2D>("ui/PressF"),
                Content.Load<Texture2D>("ui/insufficient"),
            };
            _hudSystem = new HudSystem(_fontSensation, _fontMinecraft, textureList);
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
            if (GameGlobals.Instance.IsGameActive)
            {
                EntityManager.Instance.Update(gameTime);

                ItemManager.Instance.Update(gameTime);

                _hudSystem.Update(gameTime);
            }
       
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.DrawString(_fontMinecraft, $"Name: {_slimeStatsList[0].Name}", new Vector2(50, 50), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"HP: {_slimeStatsList[0].HP}", new Vector2(50, 80), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"ATK: {_slimeStatsList[0].ATK}", new Vector2(50, 110), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"Player: {PlayerManager.Instance.Player.Position.X} {PlayerManager.Instance.Player.Position.Y}", new Vector2(50, 140), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"CameraPosition: {GameGlobals.Instance.InitialCameraPos}", new Vector2(50, 170), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"ROWS: {GameGlobals.Instance.NUM_ROWS}", new Vector2(50, 200), Color.White);

            EntityManager.Instance.Draw(spriteBatch);

            ItemManager.Instance.Draw(spriteBatch);

            if (!GameGlobals.Instance.IsShowPath)
            {
                _tileMapRender.Draw(spriteBatch);
            }

            _hudSystem.Draw(spriteBatch);
        }
    }
}
