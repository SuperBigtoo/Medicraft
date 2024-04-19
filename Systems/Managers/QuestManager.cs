using Medicraft.Data.Models;
using Medicraft.Entities.Mobs;
using Medicraft.Entities.Mobs.Friendly;
using Medicraft.Screens;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.Systems.Managers
{
    public class QuestManager
    {
        public List<Quest> QuestList { private set; get; }

        private static QuestManager instance;

        private QuestManager()
        {
            QuestList = [];
        }

        public void InitQuest(List<ChapterData> chapterDatas)
        {
            bool isQuestAdded = false;

            foreach (var chapter in chapterDatas)
            {
                foreach (var questStamp in chapter.Quests)
                {
                    if (questStamp.IsQuestAccepted && !questStamp.IsQuestDone && !questStamp.IsQuestClear)
                    {
                        // Add quest
                        var questData = GameGlobals.Instance.QuestDatas.FirstOrDefault
                                (e => e.QuestId.Equals(questStamp.QuestId));

                        if (questData != null)
                        {
                            AddQuest(questData.ChapterId, questData.QuestId);
                            isQuestAdded = true;
                            break;
                        }
                    }
                }

                if (isQuestAdded) break;
            }
        }

        public bool AddQuest(int chapterId, int questId)
        {
            var questStamp = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                (e => e.ChapterId.Equals(chapterId)).Quests.FirstOrDefault
                    (e => e.QuestId.Equals(questId));

            if (questStamp == null || questStamp.IsQuestClear)
            {
                return false;
            }

            // Construct the type name based on the questId
            string typeName = "Medicraft.Systems.Managers.Quest" + questId;

            // Get the type object based on the type name
            Type questType = Type.GetType(typeName);

            // Check if the questType is not null
            if (questType != null)
            {
                // Get the constructor that accepts a quest ID parameter
                ConstructorInfo constructor = questType.GetConstructor([typeof(int)]);

                // Check if the constructor is not null
                if (constructor != null)
                {
                    // Create an instance of the quest class using the constructor
                    object questInstance = constructor.Invoke([questId]);

                    // Add the quest instance to the QuestList
                    QuestList.Add((Quest)questInstance);
                    System.Diagnostics.Debug.WriteLine($"Quest {questId} Add: true");
                    PlaySoundEffect(Sound.quest);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Check if the constructor is not null: false");
                    return false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Check if the questType is not null: false");
                return false;
            }
        }

        public void QuestComplete(Quest quest)
        {
            // Reward to player
            // items
            foreach (var item in quest.QuestData.QuestReward.Items)
                InventoryManager.Instance.AddItem(item.ItemId, item.Count);

            // Exp
            InventoryManager.Instance.AddGoldCoin(
                $"Complete Quest: {quest.QuestData.Name}",
                quest.QuestData.QuestReward.GoldCoin);

            // Gold
            PlayerManager.Instance.AddPlayerEXP(quest.QuestData.QuestReward.EXPReward);

            // Remove completed quest from list
            QuestList.Remove(quest);
            PlaySoundEffect(Sound.Bingo);

            // Set QuestStamp IsQuestClear to true
            quest.QuestStamp.IsQuestClear = true;

            // Show Quest Complete
            HUDSystem.ShowQuestComplete();
        }

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Reset ShowQuestObjectiveClear
            GameGlobals.Instance.ShowQuestObjectiveClear = false;

            // Update Quest Description Display
            UIManager.Instance.UpdateQuestDescription();

            // Update Quest in the list
            if (QuestList.Count == 0) return;

            foreach (var quest in QuestList)
                quest.Update(deltaSeconds);
        }

        public static QuestManager Instance
        {
            get
            {
                instance ??= new QuestManager();
                return instance;
            }
        }
    }

    public class Quest
    {
        public int QuestId { get; set; }
        public QuestData QuestData { get; set; }
        public QuestStamp QuestStamp { get; set; }

        protected Quest(int questId)
        {
            QuestId = questId;
            QuestData = Instance.QuestDatas.FirstOrDefault
                (e => e.QuestId.Equals(QuestId));
            QuestStamp = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                (e => e.ChapterId.Equals(QuestData.ChapterId)).Quests.FirstOrDefault
                    (e => e.QuestId.Equals(QuestData.QuestId));

            UIManager.Instance.CloseDialogEventHandler += DialogClosedHandler;
            UIManager.Instance.TradeEventHandler += TradingHandler;
            InventoryManager.Instance.EventHandler += ItemAddedHandler;
            ScreenManager.Instance.EventHandler += TransitionScreenHandler;
            PlayerManager.Instance.ObjectEventHandler += GameObjectInteractingHandler;
            PlayerManager.Instance.KillingEventHandler += KillingHostileMobHandler;
            PlayerManager.Instance.InteractEventHandler += FriendlyMobInteractingHandler;
            CraftingManager.Instance.EventHandler += CraftingHandler;
        }

        public virtual void Update(float deltaSeconds)
        {
            if (QuestStamp.IsQuestDone)
            {
                Instance.ShowQuestObjectiveClear = true;
            }
        }

        // Event handler
        // When Dialog Close
        protected virtual void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);
            }
        }

        // When Trading
        protected virtual void TradingHandler(object sender, EventArgs e)
        {
            TradingItemEventArgs eventArgs = (TradingItemEventArgs)e;
        }

        // When Item Added
        protected virtual void ItemAddedHandler(object sender, EventArgs e)
        {
            ItemAddedEventArgs eventArgs = (ItemAddedEventArgs)e;
        }

        // When Transition Screen
        protected virtual void TransitionScreenHandler(object sender, EventArgs e)
        {
            TransitionScreenEventArgs eventArgs = (TransitionScreenEventArgs)e;
        }

        // When Interacting with GameObject
        protected virtual void GameObjectInteractingHandler(object sender, EventArgs e)
        {
            InteractingObjectEventArgs eventArgs = (InteractingObjectEventArgs)e;
        }

        // When Interacting with Friendly Mob
        protected virtual void FriendlyMobInteractingHandler(object sender, EventArgs e)
        {
            InteractingFriendlyMobEventArgs eventArgs = (InteractingFriendlyMobEventArgs)e;
        }

        // When killing a Hostile Mob
        protected virtual void KillingHostileMobHandler(object sender, EventArgs e)
        {
            KillingMobEventArgs eventArgs = (KillingMobEventArgs)e;
        }

        // When crafting an Item
        protected virtual void CraftingHandler(object sender, EventArgs e)
        {
            CraftingEventArgs eventArgs = (CraftingEventArgs)e;
        }
    }

    public class Quest101(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void CraftingHandler(object sender, EventArgs e)
        {
            CraftingEventArgs eventArgs = (CraftingEventArgs)e;

            var craftingItemId = eventArgs.RecipeData.RecipeId.ToString();

            foreach (var objectiveName in QuestData.ObjectiveName)
            {
                if (craftingItemId.Equals(objectiveName))
                {
                    QuestStamp.ObjectiveCount++;

                    if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                    {
                        QuestStamp.IsQuestDone = true;
                        PlaySoundEffect(Sound.quest);
                        // 101: onDone
                        UIManager.Instance.CreateDialog(new DialogData()
                        {
                            Id = 3,
                            Type = "Quest",
                            Description = "Show 'onDone' Quest: First Time?",
                            QuestId = QuestId,
                            ChapterId = 1,
                            Stage = "onDone",
                            Dialogues = [
                                ("Noah", "ตอนนี้ได้รับยาชนิดแรกมาแล้ว แต่ถ้าอยากผลิตเพิ่มคงต้องไปหาวัตถุดิบมาอีก"),
                                ("Noah", "สามารถตรวจสอบยาชนิดต่างๆที่ผลิตได้ในสมุดสะสมยาได้เลย"),
                                ("Noah", "ในสมุดสะสมยาจะมีทั้ง ชื่อยา สรรพคุณ วัตถุดิบ ปริมาณที่ควรใช้ และข้อควรระวังอยู่"),
                                ("Noah", "สะดวกใช่มั้ยล่ะครับ")
                            ]
                        }, PlayerManager.Instance.Player.Name, false);
                    }
                }
            }
        }
    }

    public class Quest102(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);

            var quesObject = ObjectManager.Instance.GameObjects.FirstOrDefault
                (o => o.Name.Equals("QuestObject_102"));

            if (!QuestStamp.IsQuestDone)
            {
                if (quesObject != null)
                    quesObject.IsVisible = true;
            }
            else
            {
                if (quesObject != null)
                    quesObject.IsVisible = false;
            }
        }

        protected override void GameObjectInteractingHandler(object sender, EventArgs e)
        {
            InteractingObjectEventArgs eventArgs = (InteractingObjectEventArgs)e;

            if (eventArgs.GameObject.Name.Equals("QuestObject_102"))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest);
                    // 102: onDone
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 3,
                        Type = "Quest",
                        Description = "Show 'onDone' Quest: สำรวจแปลงผัก",
                        QuestId = QuestId,
                        ChapterId = 1,
                        Stage = "onDone",
                        Dialogues = [
                            ("Noah", "มีแปลงผักอยู่ด้วยล่ะ"),
                            ("Noah", "แต่ไม่มีเมล็ดพันธุ์สำหรับการปลูกอยู่เลย"),
                            ("Noah", "คงต้องเข้าหมู่บ้านไปซื้อเมล็ดพันธุ์ซักชนิดแล้วล่ะ"),
                            ("Noah", "ลองไปร้านขายของในหมู่บ้านดูดีกว่า")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 103: onAccept
                UIManager.Instance.CreateDialog(new DialogData()
                {
                    Id = 1,
                    Type = "Quest",
                    Description = "Show 'onAccept' Quest: ไปซื้อเมล็ดพันธ์และปลูกผัก",
                    QuestId = 103,
                    ChapterId = 1,
                    Stage = "onAccept",
                    Dialogues = [
                            ( "Noah", "ที่ร้านขายของต้องมีเมล็ดพันธุ์พืชที่ใช้ปลูกเพื่อทำอาหารอยู่เพียบเลย" ),
                            ( "Noah", "จำได้ว่าร้านจะต้องอยู่ข้างหน้านี่แน่เลย" )
                    ]
                }, PlayerManager.Instance.Player.Name, true);
            }
        }
    }

    public class Quest103(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void TradingHandler(object sender, EventArgs e)
        {
            TradingItemEventArgs eventArgs = (TradingItemEventArgs)e;

            var tradItemType = eventArgs.Action + "_" + eventArgs.ItemData.Category; 

            if (tradItemType.Equals(QuestData.ObjectiveName[0]))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest);

                    // 103: onDone
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 3,
                        Type = "Quest",
                        Description = "Show 'onDone' Quest: สำรวจแปลงผัก",
                        QuestId = QuestId,
                        ChapterId = 1,
                        Stage = "onDone",
                        Dialogues = [
                            ("แม่ค้าขายของ", "5555 ขอบคุณที่มาอุดหนุนนะจ๊ะ Noah"),
                            ("Noah", "ฮิฮิ ;)"),
                            ("Noah", "ตอนนี้ก็มีเมล็ดพันธุ์แล้วล่ะ ลองกลับไปปลูกที่แปลงดูดีกว่า")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 104: onAccept
                var questStamp_104 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(104));

                questStamp_104.IsQuestAccepted = QuestManager.Instance.AddQuest(1, 104);
            }
        }
    }

    public class Quest104(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void TransitionScreenHandler(object sender, EventArgs e)
        {
            TransitionScreenEventArgs eventArgs = (TransitionScreenEventArgs)e;

            if (eventArgs.GameScreen == ScreenManager.GameScreen.NoahHome)
            {
                // Quest 104: onGoing
                {
                    if (QuestStamp != null && QuestStamp.IsQuestAccepted && !QuestStamp.IsQuestDone)
                    {
                        UIManager.Instance.CreateDialog(new DialogData()
                        {
                            Id = 2,
                            Type = "Quest",
                            Description = "Show 'onGoing' Quest: ปลูกผักที่แปลง",
                            QuestId = QuestId,
                            ChapterId = 1,
                            Stage = "onGoing",
                            Dialogues = [
                                ("Noah" , "ลองไปดูที่แปลงปลูกผักเลยดีกว่า")
                            ]
                        }, PlayerManager.Instance.Player.Name, false);
                    }
                }
            }
        }

        protected override void GameObjectInteractingHandler(object sender, EventArgs e)
        {
            InteractingObjectEventArgs eventArgs = (InteractingObjectEventArgs)e;

            if (eventArgs.GameObject.GetType().Name.Equals("Crop"))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest);

                    // 104: onDone
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 3,
                        Type = "Quest",
                        Description = "Show 'onDone' Quest: ปลูกผักที่แปลง",
                        QuestId = QuestId,
                        ChapterId = 1,
                        Stage = "onDone",
                        Dialogues = [
                            ("Noah", "เรียบร้อยยย"),
                            ("Noah", "ถ้าเก็บเกี่ยวได้แล้วจะเอาไปทำอะไรกินดีนะ")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 105: onAccept
                var questStamp_105 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(105));

                if (questStamp_105 != null && !questStamp_105.IsQuestAccepted)
                {
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 1,
                        Type = "Quest",
                        Description = "Show 'onAccept' Quest: เอาผักที่ปลูกเสร็จมาทำอาหารกิน",
                        QuestId = 105,
                        ChapterId = 1,
                        Stage = "onAccept",
                        Dialogues = [
                            ( "Noah", "หิวแล้ว.." ),
                            ( "Noah", "เอาผลผลิตพวกนี้เข้าบ้านไปทำอาหารเลยดีกว่า" )
                        ]
                    }, PlayerManager.Instance.Player.Name, true);
                }
            }
        }
    }

    public class Quest105(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void CraftingHandler(object sender, EventArgs e)
        {
            CraftingEventArgs eventArgs = (CraftingEventArgs)e;

            var craftingType = "Crafting" + "_" + eventArgs.RecipeData.Category;

            if (craftingType.Equals(QuestData.ObjectiveName[0]))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest);

                    // 105: onDone
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 3,
                        Type = "Quest",
                        Description = "Show 'onDone' Quest: เอาผักที่ปลูกเสร็จมาทำอาหารกิน",
                        QuestId = QuestId,
                        ChapterId = 1,
                        Stage = "onDone",
                        Dialogues = [
                            ("Noah", "ยังเหลือของสดอยู่เลย"),
                            ("Noah", "เข้าเมืองแล้วลองเอาไปขายให้กับแม่ค้าดูหน่อยละกัน")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }
        }
    }

    public class Quest106(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void TradingHandler(object sender, EventArgs e)
        {
            TradingItemEventArgs eventArgs = (TradingItemEventArgs)e;

            var tradItemType = eventArgs.Action + "_" + eventArgs.ItemData.Category;

            if (tradItemType.Equals(QuestData.ObjectiveName[0]))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest);

                    // 106: onDone
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 3,
                        Type = "Quest",
                        Description = "Show 'onDone' Quest: เอาผักที่เหลือไปขายที่ TOWN",
                        QuestId = QuestId,
                        ChapterId = 1,
                        Stage = "onDone",
                        Dialogues = [
                            ("แม่ค้าขายของ", "เอาของของเจ๊มาขายคืนรึป่าวเนี่ย Noah"),
                            ("Noah", "ฮิฮิ ;)"),
                            ("Noah", "ได้มาหลายโกลเลย เอาไปซื้ออาหารสำหรับมื้อเย็นกันเถอะ"),
                            ("Noah", "จำได้ว่า แถวๆด้านล่างน่าจะมีคนขายอาหารอยู่ ลองไปดูดีกว่า")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 107: onAccept
                var questStamp_107 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(107));

                if (questStamp_107 != null && !questStamp_107.IsQuestAccepted)
                {
                    questStamp_107.IsQuestAccepted = QuestManager.Instance.AddQuest(1, 107);
                }
            }
        }
    }

    public class Quest107(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void TradingHandler(object sender, EventArgs e)
        {
            TradingItemEventArgs eventArgs = (TradingItemEventArgs)e;

            var tradItemType = eventArgs.Action + "_" + eventArgs.ItemData.Category;

            if (tradItemType.Equals(QuestData.ObjectiveName[0]))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest);

                    // 107: onDone
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 3,
                        Type = "Quest",
                        Description = "Show 'onDone' Quest: เอาเงินไปซื้ออาหาร",
                        QuestId = QuestId,
                        ChapterId = 1,
                        Stage = "onDone",
                        Dialogues = [
                            ("ยายขายกับข้าว", "อยากกินอะไรล่ะ พึ่งทำเสร็จใหม่ๆเลยนะ"),
                            ("Noah", "ทำไมถึงมีแต่ ซุปเห็ดกับผัดกระหล่ำปลี ล่ะเนี่ย"),
                            ("Noah", "แต่ไม่เป็นไร เผื่อวันไหนไม่อยากทำอาหารเอง แบบนี้ก็สะดวกดีเหมือนกันนะ"),
                            ("Noah", "เงินที่เหลือเอาไปซื้ออาวุธดีกว่า ช่วงนี้มอนสเตอร์มีมากขึ้นแปลกๆด้วย"),
                            ("Noah", "บ้านของชั่งตีเหล็กน่าจะอยู่ทางด้านขวานี่"),
                            ("Noah", "ลองแวะไปหาดูหน่อยดีกว่า")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 108: onAccept
                var questStamp_108 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(108));

                if (questStamp_108 != null && !questStamp_108.IsQuestAccepted)
                {
                    questStamp_108.IsQuestAccepted = QuestManager.Instance.AddQuest(1, 108);
                }
            }
        }
    }

    public class Quest108(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void TradingHandler(object sender, EventArgs e)
        {
            TradingItemEventArgs eventArgs = (TradingItemEventArgs)e;

            var tradItemType = eventArgs.Action + "_" + eventArgs.ItemData.Category;

            if (tradItemType.Equals(QuestData.ObjectiveName[0]))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest);

                    // 108: onDone
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 3,
                        Type = "Quest",
                        Description = "Show 'onDone' Quest: เอาผักที่เหลือไปขายที่ TOWN",
                        QuestId = QuestId,
                        ChapterId = 1,
                        Stage = "onDone",
                        Dialogues = [
                            ("ชั่งตีเหล็ก", "จะมาซื้ออาวุธชิ้นไหนล่ะ เลือกดูได้เลย"),
                            ("ชั่งตีเหล็ก", "หรือจะมาทำเองก็ได้นะข้างๆนี่ ขอแค่เอาวัตถุดิบมาเองก็พอ"),
                            ("Noah", "อ่อ ขอบคุณครับเยี่ยมไปเลย"),
                            ("Noah", "งั้นลองไปโจมตีมอนสเตอร์รอบๆหมู่บ้านดูดีกว่า")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 109: onAccept
                var questStamp_109 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(109));

                if (questStamp_109 != null && !questStamp_109.IsQuestAccepted)
                {
                    questStamp_109.IsQuestAccepted = QuestManager.Instance.AddQuest(1, 109);
                }
            }
        }
    }

    public class Quest109(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void KillingHostileMobHandler(object sender, EventArgs e)
        {
            KillingMobEventArgs eventArgs = (KillingMobEventArgs)e;

            var mobType = "Killing_" + eventArgs.HostileMob.EntityType.ToString();

            if (mobType.Equals(QuestData.ObjectiveName[0]))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest);

                    // 109: onDone
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 3,
                        Type = "Quest",
                        Description = "Show 'onDone' Quest: ทดลองไปโจมตีมอนสเตอร์",
                        QuestId = QuestId,
                        ChapterId = 1,
                        Stage = "onDone",
                        Dialogues = [
                            ("Noah", "วันนี้ออกมาทำหลายอย่างเลย เหนื่อยแล้วล่ะ กลับบ้านไปพักซักหน่อยละกัน")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 110: onAccept
                var questStamp_110 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(110));

                if (questStamp_110 != null && !questStamp_110.IsQuestAccepted)
                {
                    questStamp_110.IsQuestAccepted = QuestManager.Instance.AddQuest(1, 110);
                }
            }
        }
    }

    public class Quest110(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void GameObjectInteractingHandler(object sender, EventArgs e)
        {
            InteractingObjectEventArgs eventArgs = (InteractingObjectEventArgs)e;

            if (eventArgs.GameObject.GetType().Name.Equals("RestPoint"))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest); 
                }
            }
        }

        protected override void TransitionScreenHandler(object sender, EventArgs e)
        {
            TransitionScreenEventArgs eventArgs = (TransitionScreenEventArgs)e;

            if (eventArgs.GameScreen == ScreenManager.GameScreen.NoahRoom 
                && QuestStamp.IsQuestDone && !QuestStamp.IsQuestClear)
            {
                QuestStamp.IsQuestDone = true;

                // 110: onDone
                UIManager.Instance.CreateDialog(new DialogData()
                {
                    Id = 3,
                    Type = "Quest",
                    Description = "Show 'onDone' Quest: ปลูกผักที่แปลง",
                    QuestId = QuestId,
                    ChapterId = 1,
                    Stage = "onDone",
                    Dialogues = [
                        ("Noah", "วันนี้จดหมายของพ่อน่าจะมาถึงแล้ว"),
                        ("Noah", "ทั้งสองคนจะสบายดีมั้ยนะ")
                    ]
                }, PlayerManager.Instance.Player.Name, false);
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 111: onAccept
                var questStamp_111 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(1)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(111));

                if (questStamp_111 != null && !questStamp_111.IsQuestAccepted)
                {
                    questStamp_111.IsQuestAccepted = QuestManager.Instance.AddQuest(1, 111);
                }
            }
        }
    }

    public class Quest111(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);

            var quesObject = ObjectManager.Instance.GameObjects.FirstOrDefault
                (o => o.Name.Equals("QuestObject_111"));

            if (!QuestStamp.IsQuestDone)
            {
                if (quesObject != null)
                    quesObject.IsVisible = true;
            }
            else
            {
                if (quesObject != null)
                    quesObject.IsVisible = false;
            }
        }

        protected override void GameObjectInteractingHandler(object sender, EventArgs e)
        {
            InteractingObjectEventArgs eventArgs = (InteractingObjectEventArgs)e;

            if (eventArgs.GameObject.Name.Equals("QuestObject_111"))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest);

                    // 111: onDone
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 3,
                        Type = "Quest",
                        Description = "Show 'onDone' Quest: สำรวจแปลงผัก",
                        QuestId = QuestId,
                        ChapterId = 1,
                        Stage = "onDone",
                        Dialogues = [
                            ("*ข้อความในจดหมาย*", "......"),
                            ("*ข้อความในจดหมาย*", "ถึง โนอาห์ ณ หมู่บ้าน Nordlingen.."),
                            ("*ข้อความในจดหมาย*", "ระหว่างเดินทางแม่ของลูกอยู่ๆก็ป่วยกะทันหัน พ่อตรวจดูแล้วยังไม่รู้สาเหตุเลย ตอนนี้พ่อเลยพาแม่มาพักอยู่ที่คลินิคเมือง Rothenburg"),
                            ("*ข้อความในจดหมาย*", "ลูกช่วยแวะมาดูแลแม่หน่อยนะ ส่วนพ่อจะออกจากเมืองไปหายารักษาดู ช่วงนี้มอนสเตอร์เยอะมาก ระหว่างมาก็ระวังตัวด้วยล่ะ!!"),
                            ("Noah", "แม่จะเป็นอะไรมากรึเปล่านะ"),
                            ("Noah", "มอนสเตอร์เยอะแบบนี้อาจจะสู้ไม่ไหว คงต้องชวนไวโอเล็ตไปด้วยแล้วล่ะ")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                var playerData = PlayerManager.Instance.Player.PlayerData;

                var chapterPorgress = playerData.ChapterProgression.FirstOrDefault
                    (e => e.ChapterId.Equals(1));
                chapterPorgress.IsChapterClear = true;

                var craftingProgression = playerData.CraftingProgression.ThaiTraditionalMedicine;

                foreach (var item in craftingProgression)
                {
                    if (item.ItemId.Equals(200) || item.ItemId.Equals(202) 
                        || item.ItemId.Equals(203) || item.ItemId.Equals(204))
                            item.IsCraftable = true;
                }
            }
        }
    }

    public class Quest201(int questId) : Quest(questId)
    {
        private bool isNPC_201_200Clear = false;
        private bool isNPC_201_202Clear = false;
        private bool isNPC_201_203Clear = false;
        private bool isNPC_201_204Clear = false;

        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void TransitionScreenHandler(object sender, EventArgs e)
        {
            TransitionScreenEventArgs eventArgs = (TransitionScreenEventArgs)e;

            var entities = EntityManager.Instance.Entities.Where
                    (e => e.Name.Equals("NPC-201_200") || e.Name.Equals("NPC-201_202")
                    || e.Name.Equals("NPC-201_203") || e.Name.Equals("NPC-201_204")).Cast<FriendlyMob>();

            foreach (var entity in entities)
            {
                entity.MobType = FriendlyMob.FriendlyMobType.QuestGiver;
                entity.IsInteractable = true;

                if (entity.Name.Equals("NPC-201_200") && isNPC_201_200Clear)
                {
                    entity.MobType = FriendlyMob.FriendlyMobType.Civilian;
                    entity.IsInteractable = false;
                }

                if (entity.Name.Equals("NPC-201_202") && isNPC_201_202Clear)
                {
                    entity.MobType = FriendlyMob.FriendlyMobType.Civilian;
                    entity.IsInteractable = false;
                }

                if (entity.Name.Equals("NPC-201_203") && isNPC_201_203Clear)
                {
                    entity.MobType = FriendlyMob.FriendlyMobType.Civilian;
                    entity.IsInteractable = false;
                }

                if (entity.Name.Equals("NPC-201_204") && isNPC_201_204Clear)
                {
                    entity.MobType = FriendlyMob.FriendlyMobType.Civilian;
                    entity.IsInteractable = false;
                }
            }          
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);

                // Add Companions : Violet
                PlayerManager.Instance.Player.PlayerData.Companions.Add(new CompanionData()
                {
                    CharId = 1,
                    Level = PlayerManager.Instance.Player.Level,
                    CurrentHPPercentage = 1,
                    Abilities = new AbilityData()
                    {
                        NormalSkillLevel = PlayerManager.Instance.Player.PlayerData.Abilities.NormalSkillLevel,
                        BurstSkillLevel = PlayerManager.Instance.Player.PlayerData.Abilities.BurstSkillLevel,
                        PassiveSkillLevel = PlayerManager.Instance.Player.PlayerData.Abilities.PassiveSkillLevel,
                    },
                    IsSummoned = false
                });

                PlayerManager.Instance.InitializeCompanion();

                EntityManager.Instance.entities.RemoveAll(e => e.Name.Equals("Violet"));

                var entities = EntityManager.Instance.Entities.Where
                    (e => e.Name.Equals("NPC-201_200") || e.Name.Equals("NPC-201_202")
                    || e.Name.Equals("NPC-201_203") || e.Name.Equals("NPC-201_204")).Cast<FriendlyMob>();

                foreach (var entity in entities)
                {
                    entity.MobType = FriendlyMob.FriendlyMobType.QuestGiver;
                    entity.IsInteractable = true;
                }
            }

            // OnGoing
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onGoing")
                && QuestStamp.IsQuestDone)
            {
                UIManager.Instance.CreateDialog(new DialogData()
                {
                    Id = 3,
                    Type = "Quest",
                    Description = "Show 'onDone' Quest: สำรวจแปลงผัก",
                    QuestId = QuestId,
                    ChapterId = 2,
                    Stage = "onDone",
                    Dialogues = [
                            ("Violet", "เยี่ยมเลย Noah เท่านี่เราก็พร้อมออกเดินทางกันได้เลย"),
                            ("Violet", "แต่คงต้องเคลียร์ดันเจี้ยนก่อน เราถึงจะไปยัง Rothenburg ได้"),
                            ("Violet", "เธอสู้ได้ใช่มั้ย"),
                            ("Noah", "ได้อยู่แล้ว มาลุยกันเลย"),
                            ("Noah", "พร้อมจะลุยกันมานานแล้ว")
                        ]
                }, PlayerManager.Instance.Player.Name, false);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 202: onAccept
                var questStamp_202 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(2)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(202));

                if (questStamp_202 != null && !questStamp_202.IsQuestAccepted)
                {
                    questStamp_202.IsQuestAccepted = QuestManager.Instance.AddQuest(2, 202);
                }
            }
        }

        protected override void FriendlyMobInteractingHandler(object sender, EventArgs e)
        {
            InteractingFriendlyMobEventArgs eventArgs = (InteractingFriendlyMobEventArgs)e;

            var mobName = eventArgs.FriendlyMob.Name;

            if (mobName.Equals("NPC-201_200") && !isNPC_201_200Clear &&
                InventoryManager.Instance.InventoryBag.Values.Any(e => e.ItemId == 200))
            {
                QuestStamp.ObjectiveCount++;
                isNPC_201_200Clear = true;
                InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(e => e.ItemId == 200).Count--;

                eventArgs.FriendlyMob.IsInteractable = false;
                eventArgs.FriendlyMob.MobType = FriendlyMob.FriendlyMobType.Civilian;              

                // Dialog onGoin
                {
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 2,
                        Type = "Quest",
                        Description = "Show 'onGoing' Quest: ช่วยชาวบ้าน",
                        QuestId = QuestId,
                        ChapterId = 2,
                        Stage = "onGoing",
                        Dialogues = [
                            ("Velma" , "ขอบคุณมากๆค่ะคุณหมอโนอาห์")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }

            if (mobName.Equals("NPC-201_202") && !isNPC_201_202Clear &&
                InventoryManager.Instance.InventoryBag.Values.Any(e => e.ItemId == 202))
            {
                QuestStamp.ObjectiveCount++;
                isNPC_201_202Clear = true;
                InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(e => e.ItemId == 202).Count--;

                eventArgs.FriendlyMob.IsInteractable = false;
                eventArgs.FriendlyMob.MobType = FriendlyMob.FriendlyMobType.Civilian;

                // Dialog onGoin
                {
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 2,
                        Type = "Quest",
                        Description = "Show 'onGoing' Quest: ช่วยชาวบ้าน",
                        QuestId = QuestId,
                        ChapterId = 2,
                        Stage = "onGoing",
                        Dialogues = [
                            ("Evenn" , "เกือบต้องเป็นลมที่นี่ซะแล้ว ขอบคุณมากนะครับหมอ")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }

            if (mobName.Equals("NPC-201_203") && !isNPC_201_203Clear &&
                InventoryManager.Instance.InventoryBag.Values.Any(e => e.ItemId == 203))
            {
                QuestStamp.ObjectiveCount++;
                isNPC_201_203Clear = true;
                InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(e => e.ItemId == 203).Count--;

                eventArgs.FriendlyMob.IsInteractable = false;
                eventArgs.FriendlyMob.MobType = FriendlyMob.FriendlyMobType.Civilian;              

                // Dialog onGoin
                {
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 2,
                        Type = "Quest",
                        Description = "Show 'onGoing' Quest: ช่วยชาวบ้าน",
                        QuestId = QuestId,
                        ChapterId = 2,
                        Stage = "onGoing",
                        Dialogues = [
                            ("Emma" , "ขอบคุณค่ะหมอ ฉันจะระวังตอนป้อนนม แล้วก็ให้น้องทานอาหารเป็นเวลานะคะ")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }

            if (mobName.Equals("NPC-201_204") && !isNPC_201_204Clear &&
                InventoryManager.Instance.InventoryBag.Values.Any(e => e.ItemId == 204))
            {
                QuestStamp.ObjectiveCount++;
                isNPC_201_204Clear = true;
                InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(e => e.ItemId == 204).Count--;

                eventArgs.FriendlyMob.IsInteractable = false;
                eventArgs.FriendlyMob.MobType = FriendlyMob.FriendlyMobType.Civilian;          

                // Dialog onGoin
                {
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 2,
                        Type = "Quest",
                        Description = "Show 'onGoing' Quest: ช่วยชาวบ้าน",
                        QuestId = QuestId,
                        ChapterId = 2,
                        Stage = "onGoing",
                        Dialogues = [
                            ("Jack" , "ขอบใจนะโนอาห์ เก่งเหมือนพ่อจริงๆ")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }

            if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
            {
                QuestStamp.IsQuestDone = true;
                PlaySoundEffect(Sound.quest);
            }
        }
    }

    public class Quest202(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void TransitionScreenHandler(object sender, EventArgs e)
        {
            TransitionScreenEventArgs eventArgs = (TransitionScreenEventArgs)e;

            if (eventArgs.GameScreen == ScreenManager.GameScreen.Dungeon1
                && QuestStamp.IsQuestAccepted && !QuestStamp.IsQuestDone)
            {
                QuestStamp.IsQuestDone = true;

                // 202: onDone
                UIManager.Instance.CreateDialog(new DialogData()
                {
                    Id = 3,
                    Type = "Quest",
                    Description = "Show 'onDone' Quest: 202",
                    QuestId = QuestId,
                    ChapterId = 2,
                    Stage = "onDone",
                    Dialogues = [
                        ("Noah", "คงต้องเคลียร์ดันเจี้ยนนี้ถึงจะเดินทางไปได้ราบรื่นสินะ เธอสู้ได้ใช่มั้ย"),
                        ("Violet", "ได้อยู่แล้ว มาลุยกันเลย")
                    ]
                }, PlayerManager.Instance.Player.Name, false);
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 203: onAccept
                var questStamp_203 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(2)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(203));

                if (questStamp_203 != null && !questStamp_203.IsQuestAccepted)
                {
                    questStamp_203.IsQuestAccepted = QuestManager.Instance.AddQuest(2, 203);
                }
            }
        }
    }

    public class Quest203(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void KillingHostileMobHandler(object sender, EventArgs e)
        {
            KillingMobEventArgs eventArgs = (KillingMobEventArgs)e;

            var mobType = "Killing_" + eventArgs.HostileMob.Name;

            if (mobType.Equals(QuestData.ObjectiveName[0]))
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                    PlaySoundEffect(Sound.quest);

                    // 203: onDone
                    UIManager.Instance.CreateDialog(new DialogData()
                    {
                        Id = 3,
                        Type = "Quest",
                        Description = "Show 'onDone' Quest: 203",
                        QuestId = QuestId,
                        ChapterId = 2,
                        Stage = "onDone",
                        Dialogues = [
                            ("Noah", "โอเค ไปต่อกันเลย")
                        ]
                    }, PlayerManager.Instance.Player.Name, false);
                }
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 204: onAccept
                var questStamp_204 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(2)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(204));

                if (questStamp_204 != null && !questStamp_204.IsQuestAccepted)
                {
                    questStamp_204.IsQuestAccepted = QuestManager.Instance.AddQuest(2, 204);
                }
            }
        }
    }

    public class Quest204(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void TransitionScreenHandler(object sender, EventArgs e)
        {
            TransitionScreenEventArgs eventArgs = (TransitionScreenEventArgs)e;

            if (eventArgs.GameScreen == ScreenManager.GameScreen.Map2
                && QuestStamp.IsQuestAccepted && !QuestStamp.IsQuestDone)
            {
                QuestStamp.IsQuestDone = true;

                // 204: onDone
                UIManager.Instance.CreateDialog(new DialogData()
                {
                    Id = 3,
                    Type = "Quest",
                    Description = "Show 'onDone' Quest: 204",
                    QuestId = QuestId,
                    ChapterId = 2,
                    Stage = "onDone",
                    Dialogues = [
                        ("Noah", "ไม่ได้มาที่นี่นานแล้ว ดูดีขึ้นเยอะเลยนะเนี่ย"),
                        ("Violet", "ยังดูเหมือนเดิมเลย")
                    ]
                }, PlayerManager.Instance.Player.Name, false);
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 205: onAccept
                var questStamp_205 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(2)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(205));

                if (questStamp_205 != null && !questStamp_205.IsQuestAccepted)
                {
                    questStamp_205.IsQuestAccepted = QuestManager.Instance.AddQuest(2, 205);
                }
            }
        }
    }

    public class Quest205(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void GameObjectInteractingHandler(object sender, EventArgs e)
        {
            InteractingObjectEventArgs eventArgs = (InteractingObjectEventArgs)e;

            if (eventArgs.GameObject.GetType().Name.Equals("WarpPoint")
                && QuestStamp.IsQuestAccepted && !QuestStamp.IsQuestDone
                && ScreenManager.Instance.CurrentScreen == ScreenManager.GameScreen.Map2)
            {
                QuestStamp.IsQuestDone = true;

                // Unlock WarpPoint ch.2
                PlayerManager.Instance.Player.PlayerData.ChapterProgression[1].IsWarpPointUnlock = true;

                // Closing WarpPoint and reset current gui panel
                // Pause PlayScreen
                Instance.IsGamePause = !Instance.IsGamePause;
                Instance.IsOpenGUI = !Instance.IsOpenGUI;

                // Toggle the IsOpenWarpPointPanel flag
                Instance.IsOpenWarpPointPanel = false;
                Instance.IsRefreshPlayScreenUI = false;
                UIManager.Instance.CurrentUI = UIManager.PlayScreen;

                // 205: onDone
                UIManager.Instance.CreateDialog(new DialogData()
                {
                    Id = 3,
                    Type = "Quest",
                    Description = "Show 'onDone' Quest: 205",
                    QuestId = QuestId,
                    ChapterId = 2,
                    Stage = "onDone",
                    Dialogues = [
                        ("Noah", "การปลดล็อคจุดวาร์ปช่วยให้การเดินทางสะดวกสบายยิ่งขึ้น "),
                        ("Noah", "สามารถเดินทางข้ามเมืองได้อย่างง่ายดาย สะดวกดีใช่มั้ยล่ะ"),
                        ("Noah", "เสร็จแล้วก็ไปหาแม่กันเถอะ")
                    ]
                }, PlayerManager.Instance.Player.Name, false);
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 206: onAccept
                var questStamp_206 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(2)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(206));

                if (questStamp_206 != null && !questStamp_206.IsQuestAccepted)
                {
                    questStamp_206.IsQuestAccepted = QuestManager.Instance.AddQuest(2, 206);
                }

                // Add NPC Violet
                var EntityData = new EntityData()
                {
                    Id = EntityManager.Instance.EntityCount() + 1,
                    CharId = 121,
                    Name = "แม่โนอาห์",
                    MobType = "QuestGiver",
                    QuestId = 206,
                    IsInteractable = true,
                    PathFindingType = 2,
                    NodeCycleTime = 10,
                    Position = [2289, 2134],
                    DialogData = [
                        new DialogData() {
                                Id = 3,
                                Type = "Quest",
                                Description = "Show 'onDone' Quest: 206",
                                QuestId = 206,
                                ChapterId = 2,
                                Stage = "onDone",
                                Dialogues = [
                                    ("แม่โนอาห์", "แค่กๆ โนอาห์มาแล้วหรอลูก เดินทางเป็นยังไงบ้าง"),
                                    ("Noah", "ปกติดีครับแม่ แล้วมันเกิดอะไรขึ้นหรอครับ แม่ถึงป่วยแบบนี้"),
                                    ("แม่โนอาห์", "แม่กับพ่อโดนมอนสเตอร์รุมระหว่างทางน่ะ แต่ตอนสู้เสร็จอยู่ๆแม่ก็หน้ามืดล้มลงไปเลย "),
                                    ("แม่โนอาห์", "ตอนนี้ก็ปวดหัวอยู่ด้วย ตอนนี้คงกลับบ้านไม่ไหว ฝากลูกเอาวัตถุดิบยากลับไปเปิดร้านแทนพ่อแม่ด้วยนะ"),
                                    ("แม่โนอาห์", "แม่คงต้องอยู่ที่นี่พักใหญ่เลยล่ะ เดี๋ยวรอกลับพร้อมพ่อของลูกก็ได้"),
                                ]
                        },
                        new DialogData() {
                                Id = 4,
                                Type = "Quest",
                                Description = "Show 'onClear' Quest: 206",
                                QuestId = 206,
                                ChapterId = 2,
                                Stage = "onClear",
                                Dialogues = [
                                    ("แม่โนอาห์", "แม่คงต้องอยู่ที่นี่พักใหญ่เลยล่ะ เดี๋ยวรอกลับพร้อมพ่อของลูกก็ได้")
                                ]
                        }
                    ]
                };

                Instance.EntitySpriteSheets.TryGetValue(121, out SpriteSheet spriteSheet);

                EntityManager.Instance.AddEntity(
                    new Civilian(new AnimatedSprite(spriteSheet), EntityData, Vector2.One));
            }
        }
    }

    public class Quest206(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void FriendlyMobInteractingHandler(object sender, EventArgs e)
        {
            InteractingFriendlyMobEventArgs eventArgs = (InteractingFriendlyMobEventArgs)e;

            if (eventArgs.FriendlyMob.MobType == FriendlyMob.FriendlyMobType.QuestGiver
                && eventArgs.FriendlyMob.EntityData.QuestId == 206)
            {
                QuestStamp.ObjectiveCount++;

                if (QuestStamp.ObjectiveCount >= QuestData.ObjectiveValue)
                {
                    QuestStamp.IsQuestDone = true;
                }
            }
        }

        protected override void TransitionScreenHandler(object sender, EventArgs e)
        {
            TransitionScreenEventArgs eventArgs = (TransitionScreenEventArgs)e;

            if (eventArgs.GameScreen == ScreenManager.GameScreen.Map2
                && QuestStamp.IsQuestAccepted && !QuestStamp.IsQuestClear)
            {
                // Add NPC Violet
                var EntityData = new EntityData()
                {
                    Id = EntityManager.Instance.EntityCount() + 1,
                    CharId = 121,
                    Name = "แม่โนอาห์",
                    MobType = "QuestGiver",
                    QuestId = 206,
                    IsInteractable = true,
                    PathFindingType = 2,
                    NodeCycleTime = 10,
                    Position = [2289, 2134],
                    DialogData = [
                        new DialogData() {
                                Id = 3,
                                Type = "Quest",
                                Description = "Show 'onDone' Quest: 206",
                                QuestId = 206,
                                ChapterId = 2,
                                Stage = "onDone",
                                Dialogues = [
                                    ("แม่โนอาห์", "แค่กๆ โนอาห์มาแล้วหรอลูก เดินทางเป็นยังไงบ้าง"),
                                    ("Noah", "ปกติดีครับแม่ แล้วมันเกิดอะไรขึ้นหรอครับ แม่ถึงป่วยแบบนี้"),
                                    ("แม่โนอาห์", "แม่กับพ่อโดนมอนสเตอร์รุมระหว่างทางน่ะ แต่ตอนสู้เสร็จอยู่ๆแม่ก็หน้ามืดล้มลงไปเลย "),
                                    ("แม่โนอาห์", "ตอนนี้ก็ปวดหัวอยู่ด้วย ตอนนี้คงกลับบ้านไม่ไหว ฝากลูกเอาวัตถุดิบยากลับไปเปิดร้านแทนพ่อแม่ด้วยนะ"),
                                    ("แม่โนอาห์", "แม่คงต้องอยู่ที่นี่พักใหญ่เลยล่ะ เดี๋ยวรอกลับพร้อมพ่อของลูกก็ได้"),
                                ]
                        },
                        new DialogData() {
                                Id = 4,
                                Type = "Quest",
                                Description = "Show 'onClear' Quest: 206",
                                QuestId = 206,
                                ChapterId = 2,
                                Stage = "onClear",
                                Dialogues = [
                                    ("แม่โนอาห์", "แม่คงต้องอยู่ที่นี่พักใหญ่เลยล่ะ เดี๋ยวรอกลับพร้อมพ่อของลูกก็ได้")
                                ]
                        }
                    ]
                };

                Instance.EntitySpriteSheets.TryGetValue(121, out SpriteSheet spriteSheet);

                EntityManager.Instance.AddEntity(
                    new Civilian(new AnimatedSprite(spriteSheet), EntityData, Vector2.One));
            }
        }

        protected override void DialogClosedHandler(object sender, EventArgs e)
        {
            DialogClosedEventArgs eventArgs = (DialogClosedEventArgs)e;

            var dialogData = eventArgs.DialogData;

            // OnAccept
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onAccept"))
            {
                HUDSystem.ShowQuestObjective(QuestData.Name);
            }

            // OnDone
            if (dialogData.Type.Equals("Quest")
                && dialogData.QuestId.Equals(QuestId)
                && dialogData.Stage.Equals("onDone"))
            {
                QuestManager.Instance.QuestComplete(this);

                // Quest 207: onAccept
                var questStamp_207 = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                        (e => e.ChapterId.Equals(2)).Quests.FirstOrDefault
                            (e => e.QuestId.Equals(207));

                if (questStamp_207 != null && !questStamp_207.IsQuestAccepted)
                {
                    questStamp_207.IsQuestAccepted = QuestManager.Instance.AddQuest(2, 207);
                }
            }
        }
    }

    public class Quest207(int questId) : Quest(questId)
    {
        public override void Update(float deltaSeconds)
        {
            base.Update(deltaSeconds);
        }

        protected override void TransitionScreenHandler(object sender, EventArgs e)
        {
            TransitionScreenEventArgs eventArgs = (TransitionScreenEventArgs)e;

            if (eventArgs.GameScreen == ScreenManager.GameScreen.NoahRoom
                && QuestStamp.IsQuestAccepted && !QuestStamp.IsQuestDone)
            {
                QuestStamp.IsQuestDone = true;

                // 207: onDone
                UIManager.Instance.CreateDialog(new DialogData()
                {
                    Id = 3,
                    Type = "Quest",
                    Description = "Show 'onDone' Quest: 207",
                    QuestId = QuestId,
                    ChapterId = 2,
                    Stage = "onDone",
                    Dialogues = [
                        ("Noah", "การเปิดร้านขายยาก็ง่ายมากเลย"),
                        ("Noah", "แค่เปิดโหมดร้านขายยาในบ้าน และทำยาตามคำสั่งซื้อที่ได้รับ ตามเวลาเปิดปิดร้านก็พอแล้ว"),
                        ("Noah", "มาเปิดร้านเลยเถอะ!")
                    ]
                }, PlayerManager.Instance.Player.Name, false);
            }
        }
    }
}
