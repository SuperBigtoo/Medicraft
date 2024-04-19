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
using static Medicraft.Entities.Entity;
using Medicraft.Entities.Mobs.Friendly;
using MonoGame.Extended.Sprites;

namespace Medicraft.Screens.chapter_1
{
    public class Map1 : PlayScreen
    {
        public string MapName { private set; get; } = "map_1";

        public Map1()
        {
            ScreenManager.Instance.CurrentMap = MapName;
            Camera.ResetCameraPosition(true);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // New Tilemap
            tileMap = new TmxMap("Content/tiledmaps/chapter_1/map_1.tmx");

            // Load Tilesets Textures
            var tileSets = new Texture2D[]
            {
                content.Load<Texture2D>("tiledmaps/textures/rpg_maker_vx_rtp_tileset_by_telles0808"),
                content.Load<Texture2D>("tiledmaps/textures/TS1")
            };

            // Tile Renderer
            tileMapRender = new TilemapOrthogonalRender(tileMap, tileSets, GameGlobals.Instance.TILE_SIZE);

            // Load GameData from JSON file, such as Mobs and Items Data 
            entityDatas = content.Load<List<EntityData>>("data/chapter_1/town/entites");
            objectDatas = content.Load<List<ObjectData>>("data/chapter_1/town/objects");

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
            HUDSystem.ShowMapNameSign(0, "Nordlingen Town");

            // Music BG
            var bgMusic = GameGlobals.AddCurrentMapMusic(GameGlobals.Music.ch_1_Town, content);
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

            var questStamp_111 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                    (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                        (e => e.QuestId.Equals(111));

            var questStamp_201 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                    (e => e.ChapterId.Equals(2)).Quests.FirstOrDefault
                        (e => e.QuestId.Equals(201));

            // Intro Chapter 2
            {
                if (questStamp_111 != null && questStamp_111.IsQuestClear && !questStamp_201.IsQuestAccepted)
                {
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 2,
                        Type = "Daily",
                        Description = "Show dialog intro Chapter 2",
                        Dialogues = [
                            ("Noah" , "ไวโอเล็ตเป็นนักเวทประจำหมู่บ้าน คงจะช่วยต่อสู้ระหว่างเดินทางได้"),
                            ("Noah" , "ต้องลองไปคุยดูก่อนซะแล้ว"),
                            ("Noah" , "บ้านของ Violet อยู่ด้านบนแถวๆ ริมสระน้ำนี่เอง")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);

                    // Add NPC Violet
                    var EntityData = new EntityData()
                    {
                        Id = EntityManager.Instance.EntityCount() + 1,
                        CharId = 105,
                        Name = "Violet",
                        MobType = "QuestGiver",
                        QuestId = 201,
                        IsInteractable = true,
                        PathFindingType = 2,
                        NodeCycleTime = 10,
                        Position = [ 3577, 1553 ],
                        DialogData = [
                            new DialogData() {
                                Id = 0,
                                Type = "Quest",
                                Description = "Show 'onAccept' Quest: ช่วยชาวบ้าน",
                                QuestId = 201,
                                ChapterId = 2,
                                Stage = "onAccept",
                                Dialogues = [
                                    ("Noah", "ไง ไวโอเลต"),
                                    ("Noah", "ช่วงนี้เธอยุ่งอยู่รึเปล่า"),
                                    ("Violet", "ช่วงนี้ค่อนข้างยุ่งนิดหน่อยหนะ มีอะไรรึเปล่าโนอาห์"),
                                    ("Noah", "ตอนนี้แม่ฉันป่วยอยู่ที่เมือง Rothenburg น่ะ แล้วช่วงนี้มีมอนสเตอร์เยอะมาก เลยอยากให้ช่วยไปด้วยกันหน่อย"),
                                    ("Violet", "ได้สิ แต่ก่อนหน้านั้นนายต้องช่วยฉัยรักษาคนป่วยในหมู่บ้านก่อนนะ"),
                                    ("Noah", "ได้เลย ขอรบกวนด้วยนะ"),
                                ]
                            }
                        ]
                    };

                    var spriteSheet = GameGlobals.Instance.CompanionSpriteSheet[0];

                    EntityManager.Instance.AddEntity(
                        new Civilian(new AnimatedSprite(spriteSheet), EntityData, Vector2.One));
                }
            }
        }
    }
}
