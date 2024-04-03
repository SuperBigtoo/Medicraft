﻿using Medicraft.Data.Models;
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
using Medicraft.Entities;

namespace Medicraft.Screens.chapter_1
{
    public class Map1 : PlayScreen
    {
        public string MapName = "map_1";

        public Map1()
        {
            ScreenManager.Instance.CurrentMap = MapName;
            Camera.ResetCameraPosition(true);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Load map_1
            var tileSets = new Texture2D[]  // The maximum number of TileSet is 5
            {
                _content.Load<Texture2D>("tiledmaps/textures/rpg_maker_vx_rtp_tileset_by_telles0808"),
                _content.Load<Texture2D>("tiledmaps/textures/TS1")
            };
            _tileMap = new TmxMap("Content/tiledmaps/chapter_1/map_1.tmx");
            _tileMapRender = new TilemapOrthogonalRender(_tileMap, tileSets, GameGlobals.Instance.TILE_SIZE);

            // Load GameData from JSON file, such as Mobs and Items Data 
            _entityDatas = _content.Load<List<EntityData>>("data/chapter_1/entites");
            //_objectDatas = _content.Load<List<ObjectData>>("data/TestScreen/objects_demo");

            // Adding Mobs to MobSpawner
            Dictionary<int, SpriteSheet> entitySpriteSheets = new()
            {
                { 200,  _content.Load<SpriteSheet>("entity/mobs/monster/slime/slimes_animation.sf", new JsonContentLoader())}
            };

            _mobSpawner = new MobSpawner(GameGlobals.Instance.MobsTestSpawnTime
                , GameGlobals.Instance.MobsTestSpawnTimer);
            _mobSpawner.SetupSpawner(_entityDatas, entitySpriteSheets);
            EntityManager.Instance.Initialize(_mobSpawner);

            // Adding GameObject to ObjectSpawner
            //_objectSpawner = new ObjectSpawner(10f);
            //_objectSpawner.SetupSpawner(_objectDatas);
            //ObjectManager.Instance.Initialize(_objectSpawner);

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
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
