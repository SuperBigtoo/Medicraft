using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Serialization;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Content;
using TiledSharp;
using System.Collections.Generic;
using Medicraft.Systems.Spawners;
using Medicraft.Systems.TilemapRenderer;
using Medicraft.Systems.Managers;

namespace Medicraft.Screens
{
    public class TestScreen : Screen
    {
        public string MapName = "Test";

        private TilemapOrthogonalRender _tileMapRender;
        private TmxMap _tileMap;

        private HUDSystem _hudSystem;
        private DrawEffectSystem _drawEffectSystem;
        
        private List<EntityData> _entityDatas;
        private MobSpawner _mobSpawner;

        private List<ObjectData> _objectDatas;
        private ObjectSpawner _objectSpawner;

        public TestScreen()
        {
            ScreenManager.Instance.CurrentMap = MapName;

            GameGlobals.Instance.TopLeftCornerPosition = PlayerManager.Instance.Player.Position - GameGlobals.Instance.GameScreenCenter;
            GameGlobals.Instance.InitialCameraPos = PlayerManager.Instance.Player.Position;
            GameGlobals.Instance.AddingCameraPos = Vector2.Zero;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Set player position
            PlayerManager.Instance.SetupPlayerPosition(ScreenManager.Instance.CurrentLoadMapAction);

            var tileSetsDemo = new Texture2D[]  // The maximum number of TileSet is 5
            {
                _content.Load<Texture2D>("tiledmaps/demo/rpg_maker_vx_rtp_tileset_by_telles0808"),
                _content.Load<Texture2D>("tiledmaps/demo/homemc0"),
                _content.Load<Texture2D>("tiledmaps/demo/TS1")
            };
            _tileMap = new TmxMap("Content/tiledmaps/demo/Demo.tmx");
            _tileMapRender = new TilemapOrthogonalRender(_tileMap, tileSetsDemo, GameGlobals.Instance.TILE_SIZE);

            // Load GameData from JSON file, such as Mobs and Items Data 
            _entityDatas = _content.Load<List<EntityData>>("data/TestScreen/entites_demo");
            _objectDatas = _content.Load<List<ObjectData>>("data/TestScreen/objects_demo");

            // Adding Mobs to MobSpawner
            Dictionary<int, SpriteSheet> entitySpriteSheets = new()
            {
                { 100,  _content.Load<SpriteSheet>("entity/mobs/friendly/cat/cat_animation.sf", new JsonContentLoader())},
                { 200,  _content.Load<SpriteSheet>("entity/mobs/monster/slime/slimes_animation.sf", new JsonContentLoader())},
                { 201,  _content.Load<SpriteSheet>("entity/mobs/monster/goblin/goblin_animation.sf", new JsonContentLoader())}
            };

            _mobSpawner = new MobSpawner(GameGlobals.Instance.MobsTestSpawnTime
                , GameGlobals.Instance.MobsTestSpawnTimer);
            _mobSpawner.SetupSpawner(_entityDatas, entitySpriteSheets);
            EntityManager.Instance.Initialize(_mobSpawner);

            // Adding GameObject to ObjectSpawner
            _objectSpawner = new ObjectSpawner(GameGlobals.Instance.ObjectTestSpawnTime
                , GameGlobals.Instance.ObjectTestSpawnTimer);
            _objectSpawner.SetupSpawner(_objectDatas);
            ObjectManager.Instance.Initialize(_objectSpawner);

            // Adding DrawEffectSystem
            _drawEffectSystem = new DrawEffectSystem();

            // Adding HUDSystem
            _hudSystem = new HUDSystem();
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
            EntityManager.Instance.Update(gameTime);

            ObjectManager.Instance.Update(gameTime);

            _tileMapRender?.Update(gameTime);

            _drawEffectSystem?.Update(gameTime);

            _hudSystem?.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            EntityManager.Instance.Draw(spriteBatch);

            ObjectManager.Instance.Draw(spriteBatch);

            if (!GameGlobals.Instance.IsShowPath)
            {
                _tileMapRender?.Draw(spriteBatch);
            }

            _drawEffectSystem?.Draw(spriteBatch);

            _hudSystem?.Draw(spriteBatch);
        }
    }
}
