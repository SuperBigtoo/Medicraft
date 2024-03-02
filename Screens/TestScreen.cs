using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Content;
using Medicraft.Entities;
using TiledSharp;
using System.Collections.Generic;
using Medicraft.Systems.Spawners;
using Medicraft.GameObjects;
using Medicraft.Systems.TilemapRenderer;
using Medicraft.Systems.Managers;

namespace Medicraft.Screens
{
    public class TestScreen : Screen
    {
        private List<EntityData> _entityDataList;
        private MobSpawner _mobSpawner;

        private List<ObjectData> _itemDataList;

        private TmxMap _tileMap;

        public TestScreen()
        {
            ScreenName = ScreenManager.GameScreen.TestScreen;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            PlayerManager.Instance.SetupPlayer(ScreenName, ScreenManager.LoadMapAction.NewGame);

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
            _entityDataList = Content.Load<List<EntityData>>("data/TestScreen/entites_demo");
            _itemDataList = Content.Load<List<ObjectData>>("data/TestScreen/objects_demo");

            // Adding Slime to MobSpawner
            Dictionary<int, SpriteSheet> SpriteSheets = new()
            {
                { 200,  Content.Load<SpriteSheet>("entity/mobs/monster/slime/slimes_animation.sf", new JsonContentLoader())}
            };

            _mobSpawner = new MobSpawner(10f);
            _mobSpawner.SetupSpawner(_entityDataList, SpriteSheets);
            EntityManager.Instance.Initialize(_mobSpawner);

            // Adding Items to ObjectSpawner
            //var _itemAnimation = Content.Load<SpriteSheet>("item/items_demo.sf", new JsonContentLoader());
            var _itemSprite = GameGlobals.Instance.ItemsPackSprites;
            var _objectSpawner = new ObjectSpawner(10f);
            _objectSpawner.AddGameObject(new Item(new AnimatedSprite(_itemSprite), _itemDataList[0], Vector2.One));
            _objectSpawner.AddGameObject(new Item(new AnimatedSprite(_itemSprite), _itemDataList[1], Vector2.One));
            _objectSpawner.AddGameObject(new Item(new AnimatedSprite(_itemSprite), _itemDataList[2], Vector2.One));
            ObjectManager.Instance.Initialize(_objectSpawner);

            // Adding HUD
            HudSystem = new HUDSystem();
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
            EntityManager.Instance.Draw(spriteBatch);

            ObjectManager.Instance.Draw(spriteBatch);

            if (!GameGlobals.Instance.IsShowPath)
            {
                TileMapRender?.Draw(spriteBatch);
            }

            HudSystem?.Draw(spriteBatch);

            //spriteBatch.Draw(GameGlobals.Instance.TestIcon, new Vector2(500, 550), Color.White);
        }
    }
}
