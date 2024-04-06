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
    public class TestScreen : PlayScreen
    {
        public string MapName = "Test";

        public TestScreen()
        {
            ScreenManager.Instance.CurrentMap = MapName;
            Camera.ResetCameraPosition(true);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            var tileSetsDemo = new Texture2D[]  // The maximum number of TileSet is 5
            {
                content.Load<Texture2D>("tiledmaps/demo/rpg_maker_vx_rtp_tileset_by_telles0808"),
                content.Load<Texture2D>("tiledmaps/demo/homemc0"),
                content.Load<Texture2D>("tiledmaps/demo/TS1")
            };
            tileMap = new TmxMap("Content/tiledmaps/demo/Demo.tmx");
            tileMapRender = new TilemapOrthogonalRender(tileMap, tileSetsDemo, GameGlobals.Instance.TILE_SIZE);

            // Load GameData from JSON file, such as Mobs and Items Data 
            entityDatas = content.Load<List<EntityData>>("data/TestScreen/entites_demo");
            objectDatas = content.Load<List<ObjectData>>("data/TestScreen/objects_demo");

            // Adding Mobs to MobSpawner
            Dictionary<int, SpriteSheet> entitySpriteSheets = new()
            {
                { 100,  content.Load<SpriteSheet>("entity/mobs/friendly/cat/cat_animation.sf", new JsonContentLoader())},
                { 200,  content.Load<SpriteSheet>("entity/mobs/monster/slime/slimes_animation.sf", new JsonContentLoader())},
                { 201,  content.Load<SpriteSheet>("entity/mobs/monster/goblin/goblin_animation.sf", new JsonContentLoader())}
            };

            mobSpawner = new MobSpawner(GameGlobals.Instance.MobsTestSpawnTime
                , GameGlobals.Instance.MobsTestSpawnTimer);
            mobSpawner.SetupSpawner(entityDatas, entitySpriteSheets);
            EntityManager.Instance.Initialize(mobSpawner);

            // Adding GameObject to ObjectSpawner
            objectSpawner = new ObjectSpawner(GameGlobals.Instance.ObjectTestSpawnTime
                , GameGlobals.Instance.ObjectTestSpawnTimer);
            objectSpawner.SetupSpawner(objectDatas);
            ObjectManager.Instance.Initialize(objectSpawner);

            // Adding DrawEffectSystem
            drawEffectSystem = new DrawEffectSystem();

            // Adding HUDSystem
            hudSystem = new HUDSystem();
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
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
