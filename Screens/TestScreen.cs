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
using Medicraft.GameObjects;
using Medicraft.Systems.TilemapRenderer;

namespace Medicraft.Screens
{
    public class TestScreen : Screen
    {
        private HudSystem _hudSystem;
        private TilemapOrthogonalRender _tileMapRender;
        private List<EntityStats> _slimeStatsList;
        private List<ObjectStats> _itemStatsList;
        private BitmapFont _fontMinecraft, _fontSensation;
        private TmxMap _tileMap;

        public TestScreen() { }

        public override void LoadContent()
        {
            base.LoadContent();

            // Load bitmap font
            _fontSensation = Content.Load<BitmapFont>("fonts/Sensation/Sensation");
            _fontMinecraft = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
            var _fonts = new BitmapFont[]
            {
                _fontSensation,
                _fontMinecraft
            };

            // Initialize Player's Data !! Gonna be move to Load GameSave later !!
            var basePlayerStats = Content.Load<PlayerStats>("data/models/player_stats");
            var playerAnimation = Content.Load<SpriteSheet>("animation/mc/mc_spritesheet.sf", new JsonContentLoader());
            var playerSprite = new AnimatedSprite(playerAnimation);
            PlayerManager.Instance.Initialize(playerSprite, basePlayerStats);

            // Load Tile Map
            //_mapManager = new TiledMapBackgroundManager(Content, GraphicsDevice, Window, "tiledmaps/test1/level02");
            var _tileSets = new Texture2D[]     // The maximum number of TileSet is 5
            {
                Content.Load<Texture2D>("tiledmaps/demo/rpg_maker_vx_rtp_tileset_by_telles0808"),
                Content.Load<Texture2D>("tiledmaps/demo/homemc0"),
                Content.Load<Texture2D>("tiledmaps/demo/TS1")
            };
            _tileMap = new TmxMap("Content/tiledmaps/demo/Demo.tmx");
            _tileMapRender = new TilemapOrthogonalRender(_tileMap, _tileSets);

            // Load GameData from JSON file, such as Mobs and Items Data 
            _slimeStatsList = Content.Load<List<EntityStats>>("data/models/slime");
            _itemStatsList = Content.Load<List<ObjectStats>>("data/models/items_demo");

            // Adding Slime to MobSpawner
            var _slimeAnimation = Content.Load<SpriteSheet>("animation/mobs/slime/slimes_spritesheet.sf", new JsonContentLoader());
            var _slimeScale = new Vector2(3.0f, 2.5f);
            var _mobSpawner = new MobSpawner(10f);
            _mobSpawner.AddEntity(new Slime(new AnimatedSprite(_slimeAnimation), _slimeStatsList[0], _slimeScale));
            //_mobSpawner.AddEntity(new SlimeCopy(new AnimatedSprite(_slimeAnimation), _slimeStatsList[1], _slimeScale));
            EntityManager.Instance.Initialize(_mobSpawner);

            // Adding Items to ObjectSpawner
            var _itemAnimation = Content.Load<SpriteSheet>("items/items_demo.sf", new JsonContentLoader());
            var _objectSpawner = new ObjectSpawner(10f);
            _objectSpawner.AddGameObject(new Item(new AnimatedSprite(_itemAnimation), _itemStatsList[0], Vector2.One));
            _objectSpawner.AddGameObject(new Item(new AnimatedSprite(_itemAnimation), _itemStatsList[1], Vector2.One));
            _objectSpawner.AddGameObject(new Item(new AnimatedSprite(_itemAnimation), _itemStatsList[2], Vector2.One));
            ObjectManager.Instance.Initialize(_objectSpawner);

            // Adding HUD
            var _textures = new Texture2D[]
            {
                Content.Load<Texture2D>("items/heart"),
                Content.Load<Texture2D>("items/herb_1"),
                Content.Load<Texture2D>("items/herb_2"),
                Content.Load<Texture2D>("items/drug_1"),
                Content.Load<Texture2D>("items/gold_coin"),
                Content.Load<Texture2D>("ui/PressF"),
                Content.Load<Texture2D>("ui/insufficient"),
            };
            _hudSystem = new HudSystem(_fonts, _textures);
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

                ObjectManager.Instance.Update(gameTime);

                _tileMapRender.Update(gameTime);

                _hudSystem.Update(gameTime);
            }
       
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.DrawString(_fontMinecraft, $"Name: {_slimeStatsList[0].Name}", new Vector2(50, 50), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"HP: {_slimeStatsList[0].HP}", new Vector2(50, 80), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"ATK: {_slimeStatsList[0].ATK}", new Vector2(50, 110), Color.White);
            spriteBatch.DrawString(_fontMinecraft, $"Player: {PlayerManager.Instance.Player.Position.X} {PlayerManager.Instance.Player.Position.Y}", new Vector2(50, 140), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"CameraPosition: {GameGlobals.Instance.InitialCameraPos}", new Vector2(50, 170), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"ROWS: {GameGlobals.Instance.NUM_ROWS}", new Vector2(50, 200), Color.White);

            EntityManager.Instance.Draw(spriteBatch);

            ObjectManager.Instance.Draw(spriteBatch);

            if (!GameGlobals.Instance.IsShowPath)
            {
                _tileMapRender.Draw(spriteBatch);
            }

            _hudSystem.Draw(spriteBatch);
        }
    }
}
