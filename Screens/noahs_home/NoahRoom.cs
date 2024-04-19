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
using System;
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.Screens.noahs_home
{
    public class NoahRoom : PlayScreen
    {
        public string MapName { private set; get; } = "noah_room";

        public NoahRoom()
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
                content.Load<Texture2D>("tiledmaps/textures/RoomMc"),
                content.Load<Texture2D>("tiledmaps/textures/extraobj_tile01"),
            };
            tileMap = new TmxMap("Content/tiledmaps/home/noah_room.tmx");
            tileMapRender = new TilemapOrthogonalRender(tileMap, tileSets, GameGlobals.Instance.TILE_SIZE);

            // Load GameData from JSON file, such as Mobs and Items Data 
            objectDatas = content.Load<List<ObjectData>>("data/noah_room/objects");

            // If there is no Spawner for the Map yet then create a new one
            if (!GameGlobals.Instance.MobSpawners.Any(m => m.currMapName.Equals(MapName)))
            {
                var spawnerData = GameGlobals.Instance.SpawnerDatas.FirstOrDefault
                    (s => s.ChapterId.Equals(1));

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
            HUDSystem.ShowMapNameSign(0, "Noah's Room");

            // Music BG
            var bgMusic = GameGlobals.AddCurrentMapMusic(GameGlobals.Music.dova_wagaya, content);
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

        protected override void TransitionScreenHandler(object sender, EventArgs e)
        {
            TransitionScreenEventArgs eventArgs = (TransitionScreenEventArgs)e;

            var questStamp_101 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                    (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                        (e => e.QuestId.Equals(101));

            // Quest 101: onGoing
            {            
                if (questStamp_101 != null && questStamp_101.IsQuestAccepted && !questStamp_101.IsQuestDone)
                {
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 2,
                        Type = "Quest",
                        Description = "Show 'onGoing' Quest: First Time?",
                        QuestId = 101,
                        ChapterId = 1,
                        Stage = "onGoing",
                        Dialogues = [
                            ("Noah" , "มาลองทำ ‘ยาจันทน์ลีลา’ สำหรับบรรเทาอาการไข้ตัวร้อน ไข้เปลี่ยนฤดู ดูกันเถอะ!!")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }

            // Quest 207
            {
                var questStamp_207 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(2)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(207));

                var startMiniGame = ObjectManager.Instance.GameObjects.FirstOrDefault
                        (e => e.Name.Equals("Start_MiniGame"));

                if (startMiniGame == null) return;

                if (questStamp_207.IsQuestClear)
                {                
                    startMiniGame.IsVisible = true;
                }
                else startMiniGame.IsVisible = false;
            }
        }

        protected override void GameObjectInteractingHandler(object sender, EventArgs e)
        {
            InteractingObjectEventArgs eventArgs = (InteractingObjectEventArgs)e;

            if (eventArgs.GameObject.Name.Equals("Start_MiniGame") && !Instance.IsMiniGameStart)
            {
                // Start MiniGame
                Instance.StartMiniGame();
            }
        }
    }
}
