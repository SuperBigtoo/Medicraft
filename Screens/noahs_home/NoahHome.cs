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

namespace Medicraft.Screens.noahs_home
{
    public class NoahHome : PlayScreen
    {
        public string MapName { private set; get; } = "noah_home";

        public NoahHome()
        {
            ScreenManager.Instance.CurrentMap = MapName;
            Camera.ResetCameraPosition(true);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            var tileSets = new Texture2D[]  // The maximum number of TileSet is 5
            {
                content.Load<Texture2D>("tiledmaps/textures/TS1"),
                content.Load<Texture2D>("tiledmaps/textures/rpg_maker_vx_rtp_tileset_by_telles0808"),               
            };
            tileMap = new TmxMap("Content/tiledmaps/home/noah_home.tmx");
            tileMapRender = new TilemapOrthogonalRender(tileMap, tileSets, GameGlobals.Instance.TILE_SIZE, 12378);

            // Load GameData from JSON file, such as Mobs and Items Data 
            objectDatas = content.Load<List<ObjectData>>("data/noah_home/objects");

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
            HUDSystem.ShowMapNameSign(0, "Noah's Home");

            // Music BG
            var bgMusic = GameGlobals.AddCurrentMapMusic(GameGlobals.Music.dova_pastel_green, content);
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

            // Quest 101: onAccept
            var questStamp_101 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                    (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                        (e => e.QuestId.Equals(101));

            if (!questStamp_101.IsQuestClear)
            {             
                if (questStamp_101 != null && !questStamp_101.IsQuestAccepted)
                {
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 1,
                        Type = "Quest",
                        Description = "Show 'onAccept' Quest: First Time?",
                        QuestId = 101,
                        ChapterId = 1,
                        Stage = "onAccept",
                        Dialogues = [
                            ( "Noah", "สวัสดีครับ ผมชื่อโนอาห์ เป็นนักผจญภัยและหมอยาประจำหมู่บ้าน Nordlingen" ),
                            ( "Noah", "ตอนนี้พ่อกับแม่ของผมออกเดินทางหายาสมุนไพรใหม่ๆมารักษาผู้ป่วยอยู่" ),
                            ( "Noah", "ผมเลยต้องดูแลบ้านให้ทั้งสองคน ดังนั้น มาเริ่มสำรวจบ้านไปพร้อมๆกับผมกันนะครับ" )
                        ]
                    }, PlayerManager.Instance.Player.Name, true);
                }
            }

            // Quest 102: onAccept
            var questStamp_102 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                    (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                        (e => e.QuestId.Equals(102));

            // check if quest 101 is clear?
            if (questStamp_101.IsQuestClear)
            {              
                if (questStamp_102 != null && !questStamp_102.IsQuestAccepted)
                {
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 1,
                        Type = "Quest",
                        Description = "Show 'onAccept' Quest: สำรวจแปลงผัก",
                        QuestId = 102,
                        ChapterId = 1,
                        Stage = "onAccept",
                        Dialogues = [
                            ("Noah", "เอาล่ะ ตอนนี้เรามาลองสำรวจรอบๆบ้านกันดูดีกว่า"),
                            ("Noah", "เมื่อกี้เหมือนเห็นแปลงผักอยู่ข้างๆ เลย"),
                            ("Noah", "ลองไปดูหน่อยดีกว่า")
                        ]
                    }, PlayerManager.Instance.Player.Name, true);
                }
            }

            // Quest 106: onAccept
            var questStamp_106 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                    (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                        (e => e.QuestId.Equals(106));

            var questStamp_105 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                    (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                        (e => e.QuestId.Equals(105));

            // check if quest 105 is clear?
            if (questStamp_105.IsQuestClear)
            {
                if (questStamp_106 != null && !questStamp_106.IsQuestAccepted)
                {
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 1,
                        Type = "Quest",
                        Description = "Show 'onAccept' Quest: เอาผักที่เหลือไปขายที่ TOWN",
                        QuestId = 106,
                        ChapterId = 1,
                        Stage = "onAccept",
                        Dialogues = [
                            ("Noah", "เอาล่ะ ลองไปหาแม่ค้าดูอีกรอบดีกว่า"),
                            ("Noah", "จำได้ว่าพวกพ่อค้าแม่ค้าที่หมู่บ้านรับซื้อทุกอย่างที่อยากขายเลยล่ะ ทั้งผัก สมุนไพร หรือแม้กระทั่งวัตถุดิบจากมอนสเตอร์!")
                        ]
                    }, PlayerManager.Instance.Player.Name, true);
                }
            }
        }
    }
}
