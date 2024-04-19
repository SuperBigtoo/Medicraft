using Medicraft.Data.Models;
using Medicraft.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;
using System.Collections.Generic;
using Medicraft.Systems.Spawners;
using Medicraft.Systems.TilemapRenderer;
using Medicraft.Systems.Managers;
using System.Linq;

namespace Medicraft.Screens.chapter_3
{
    public class BattleZone3 : PlayScreen
    {
        public string MapName { private set; get; } = "battlezone_3";

        public BattleZone3()
        {
            ScreenManager.Instance.CurrentMap = MapName;
            Camera.ResetCameraPosition(true);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            var tileSets = new Texture2D[]  // The maximum number of TileSet is 5
            {
                content.Load<Texture2D>("tiledmaps/textures/rpg_maker_vx_rtp_tileset_by_telles0808"),
                content.Load<Texture2D>("tiledmaps/textures/TS3")
            };
            tileMap = new TmxMap("Content/tiledmaps/chapter_3/battlezone_3.tmx");
            tileMapRender = new TilemapOrthogonalRender(tileMap, tileSets, GameGlobals.Instance.TILE_SIZE);

            // Load GameData from JSON file, such as Mobs and Items Data 
            entityDatas = content.Load<List<EntityData>>("data/chapter_3/battle/entites");
            objectDatas = content.Load<List<ObjectData>>("data/chapter_3/battle/objects");

            // If there is no Spawner for the Map yet then create a new one
            if (!GameGlobals.Instance.MobSpawners.Any(m => m.currMapName.Equals(MapName)))
            {
                var spawnerData = GameGlobals.Instance.SpawnerDatas.FirstOrDefault
                    (s => s.ChapterId.Equals(3));

                GameGlobals.Instance.MobSpawners.Add(new MobSpawner(
                    spawnerData, MapName, entityDatas, GameGlobals.Instance.EntitySpriteSheets));

                GameGlobals.Instance.ObjectSpawners.Add(new ObjectSpawner(
                    spawnerData, MapName, objectDatas));
            }

            // Adding Spawners to EntityManager & ObjectManager
            {
                EntityManager.Instance.Initialize(MapName);
                ObjectManager.Instance.Initialize(MapName);
            }

            // Adding DrawEffectSystem
            drawEffectSystem = new DrawEffectSystem();

            // Adding HUDSystem
            hudSystem = new HUDSystem();

            // Show Map Name Sign
            HUDSystem.ShowMapNameSign(1, "Tallinn: Battle Zone");

            // Music BG
            var bgMusic = GameGlobals.AddCurrentMapMusic(GameGlobals.Music.ch_3_Mon, content);
            GameGlobals.PlayBackgroundMusic(bgMusic, true, volumeScale);
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
