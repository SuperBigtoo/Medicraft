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
        private List<EntityData> _slimeStatsList;
        private List<ObjectData> _itemDataList;
        private BitmapFont _fontMinecraft, _fontSensation, _fontTA8Bit, _fontTA8BitBold, _fontTA16Bit;
        private TmxMap _tileMap;

        public TestScreen() { }

        public override void LoadContent()
        {
            base.LoadContent();         

            // Load bitmap font
            _fontSensation = Content.Load<BitmapFont>("fonts/Sensation/Sensation");
            _fontMinecraft = Content.Load<BitmapFont>("fonts/Mincraft_Ten/Mincraft_Ten");
            _fontTA8Bit = Content.Load<BitmapFont>("fonts/TA_8_Bit/TA_8_Bit");
            _fontTA8BitBold = Content.Load<BitmapFont>("fonts/TA_8_Bit_Bold/TA_8_Bit_Bold");
            _fontTA16Bit = Content.Load<BitmapFont>("fonts/TA_16_Bit/TA_16_Bit");
            var _fonts = new BitmapFont[]
            {
                _fontSensation,
                _fontMinecraft,
                _fontTA8Bit,
                _fontTA8BitBold,
                _fontTA16Bit
            };

            // Initialize Player's Data !! Gonna be move to Load GameSave later !!
            var initialPlayerStats = Content.Load<PlayerData>("data/models/playerdata");
            var playerAnimation = Content.Load<SpriteSheet>("animation/mc/mc_animation.sf", new JsonContentLoader());
            var playerSprite = new AnimatedSprite(playerAnimation);
            PlayerManager.Instance.Initialize(playerSprite, initialPlayerStats);

            // Load Tile Map
            //_mapManager = new TiledMapBackgroundManager(Content, GraphicsDevice, Window, "tiledmaps/test1/level02");

            var _tileSetsDemo = new Texture2D[]     // The maximum number of TileSet is 5
            {
                Content.Load<Texture2D>("tiledmaps/demo/rpg_maker_vx_rtp_tileset_by_telles0808"),
                Content.Load<Texture2D>("tiledmaps/demo/homemc0"),
                Content.Load<Texture2D>("tiledmaps/demo/TS1")
            };
            _tileMap = new TmxMap("Content/tiledmaps/demo/Demo.tmx");
            TileMapRender = new TilemapOrthogonalRender(_tileMap, _tileSetsDemo);

            //var _tileSetsTestMap1 = new Texture2D[]     // The maximum number of TileSet is 5
            //{
            //    Content.Load<Texture2D>("tiledmaps/chapter_1/rpg_maker_vx_rtp_tileset_by_telles0808"),
            //    Content.Load<Texture2D>("tiledmaps/chapter_1/TS1"),
            //    Content.Load<Texture2D>("tiledmaps/chapter_1/TS1-2")
            //};
            //_tileMap = new TmxMap("Content/tiledmaps/chapter_1/dun1.tmx");
            //_tileMapRender = new TilemapOrthogonalRender(_tileMap, _tileSetsTestMap1);

            // Load GameData from JSON file, such as Mobs and Items Data 
            _slimeStatsList = Content.Load<List<EntityData>>("data/TestScreen/entites_demo");
            _itemDataList = Content.Load<List<ObjectData>>("data/TestScreen/objects_demo");

            // Adding Slime to MobSpawner
            var _slimeAnimation = Content.Load<SpriteSheet>("animation/mobs/slime/slimes_animation.sf", new JsonContentLoader());
            var _mobSpawner = new MobSpawner(10f);
            _mobSpawner.AddEntity(new Slime(new AnimatedSprite(_slimeAnimation), _slimeStatsList[0], Vector2.One));
            //_mobSpawner.AddEntity(new SlimeCopy(new AnimatedSprite(_slimeAnimation), _slimeStatsList[1], _slimeScale));
            EntityManager.Instance.Initialize(_mobSpawner);

            // Adding Items to ObjectSpawner
            var _itemAnimation = Content.Load<SpriteSheet>("items/items_demo.sf", new JsonContentLoader());
            var _objectSpawner = new ObjectSpawner(10f);
            _objectSpawner.AddGameObject(new Item(new AnimatedSprite(_itemAnimation), _itemDataList[0], Vector2.One));
            _objectSpawner.AddGameObject(new Item(new AnimatedSprite(_itemAnimation), _itemDataList[1], Vector2.One));
            _objectSpawner.AddGameObject(new Item(new AnimatedSprite(_itemAnimation), _itemDataList[2], Vector2.One));
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
            HudSystem = new HudSystem(_fonts, _textures, new AnimatedSprite(_itemAnimation));
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

                TileMapRender?.Update(gameTime);

                HudSystem?.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.DrawString(_fontMinecraft, $"Name: {_slimeStatsList[0].Name}", new Vector2(50, 50), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"HP: {_slimeStatsList[0].HP}", new Vector2(50, 80), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"ATK: {_slimeStatsList[0].ATK}", new Vector2(50, 110), Color.White);
            //spriteBatch.DrawString(_fontTA8Bit, $"ตำแหน่ง Player: {(int)PlayerManager.Instance.Player.Position.X} {(int)PlayerManager.Instance.Player.Position.Y}", new Vector2(50, -100), Color.White);
            //spriteBatch.DrawString(_fontTA8BitBold, $"จำนวนไอเทม: {GameGlobals.Instance.ItemDatas.Count} ", new Vector2(50, -70), Color.White);
            //spriteBatch.DrawString(_fontTA16Bit, $"ItemId: {GameGlobals.Instance.ItemDatas[0].ItemId} | Name: {GameGlobals.Instance.ItemDatas[0].Name} | Stackable: {GameGlobals.Instance.ItemDatas[0].Stackable}", new Vector2(50, -40), Color.White);
            //spriteBatch.DrawString(_fontTA8BitBold, $"Inventory Test: {InventoryManager.Instance.Inventory.Count} {InventoryManager.Instance.GoldCoin}", new Vector2(50, 170), Color.White);
            //spriteBatch.DrawString(_fontMinecraft, $"ROWS: {GameGlobals.Instance.NUM_ROWS}", new Vector2(50, 200), Color.White);            

            EntityManager.Instance.Draw(spriteBatch);

            ObjectManager.Instance.Draw(spriteBatch);

            if (!GameGlobals.Instance.IsShowPath)
            {
                TileMapRender?.Draw(spriteBatch);
            }

            HudSystem?.Draw(spriteBatch);
        }
    }
}
