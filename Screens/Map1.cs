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
using Medicraft.Entities;

namespace Medicraft.Screens
{
    public class Map1 : Screen
    {
        public string MapName = "map_1";

        private TilemapOrthogonalRender _tileMapRender;
        private TmxMap _tileMap;

        private HUDSystem _hudSystem;
        private DrawEffectSystem _drawEffectSystem;

        private List<EntityData> _entityDatas;
        private MobSpawner _mobSpawner;

        private List<ObjectData> _objectDatas;
        private ObjectSpawner _objectSpawner;

        public Map1()
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

            // Load map_1
            var tileSets = new Texture2D[]  // The maximum number of TileSet is 5
            {
                _content.Load<Texture2D>("tiledmaps/textures/rpg_maker_vx_rtp_tileset_by_telles0808"),
                _content.Load<Texture2D>("tiledmaps/textures/TS1")
            };
            _tileMap = new TmxMap("Content/tiledmaps/chapter_1/map_1.tmx");
            _tileMapRender = new TilemapOrthogonalRender(_tileMap, tileSets, GameGlobals.Instance.TILE_SIZE);

            // Load GameData from JSON file, such as Mobs and Items Data 
            //_entityDatas = _content.Load<List<EntityData>>("data/TestScreen/entites_demo");
            //_objectDatas = _content.Load<List<ObjectData>>("data/TestScreen/objects_demo");

            // Adding Mobs to MobSpawner
            //Dictionary<int, SpriteSheet> entitySpriteSheets = new()
            //{
            //    { 200,  _content.Load<SpriteSheet>("entity/mobs/monster/slime/slimes_animation.sf", new JsonContentLoader())}
            //};
            //_mobSpawner = new MobSpawner(10f);
            //_mobSpawner.SetupSpawner(_entityDatas, entitySpriteSheets);
            //EntityManager.Instance.Initialize(_mobSpawner);

            // Adding GameObject to ObjectSpawner
            //_objectSpawner = new ObjectSpawner(10f);
            //_objectSpawner.SetupSpawner(_objectDatas);
            //ObjectManager.Instance.Initialize(_objectSpawner);

            // Adding DrawEffectSystem
            //_drawEffectSystem = new DrawEffectSystem();

            // Adding HUDSystem
            _hudSystem = new HUDSystem();
        }

        public override void UnloadContent()
        {
            EntityManager.Instance.ClearEntity();

            ObjectManager.Instance.ClearGameObject();

            base.UnloadContent();
        }

        public override void Dispose()
        {
            _drawEffectSystem?.Dispose();

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
