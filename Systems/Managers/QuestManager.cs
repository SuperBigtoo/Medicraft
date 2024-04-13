using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public bool AddQuest(int questId)
        {
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
                    System.Diagnostics.Debug.WriteLine($"Quest Add: true");
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

        public void Update(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update Quest Description Display
            UIManager.Instance.UpdateQuestDescription();

            // Update Quest in the list
            if (QuestList.Count == 0) return;

            foreach (var quest in QuestList)
            {
                quest.Update(deltaSeconds);
            }
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
            QuestData = GameGlobals.Instance.QuestDatas.FirstOrDefault
                (e => e.QuestId.Equals(QuestId));
            QuestStamp = PlayerManager.Instance.Player.PlayerData.ChapterProgression.FirstOrDefault
                (e => e.ChapterId.Equals(QuestData.ChapterId)).Quests.FirstOrDefault
                    (e => e.QuestId.Equals(QuestData.QuestId));
        }

        public virtual void Update(float deltaSeconds) { }
    }

    public class Quest101 : Quest
    {
        public Quest101(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {
            System.Diagnostics.Debug.WriteLine($"Quest101: {QuestData.ObjectiveName}");
        }
    }

    public class Quest102 : Quest
    {
        public Quest102(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest103 : Quest
    {
        public Quest103(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest104 : Quest
    {
        public Quest104(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest105 : Quest
    {
        public Quest105(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest106 : Quest
    {
        public Quest106(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest107 : Quest
    {
        public Quest107(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest108 : Quest
    {
        public Quest108(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest109 : Quest
    {
        public Quest109(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest110 : Quest
    {
        public Quest110(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest111 : Quest
    {
        public Quest111(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest112 : Quest
    {
        public Quest112(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest113 : Quest
    {
        public Quest113(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest114 : Quest
    {
        public Quest114(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }

    public class Quest115 : Quest
    {
        public Quest115(int questId) : base(questId) { }

        public override void Update(float deltaSeconds)
        {

        }
    }
}
