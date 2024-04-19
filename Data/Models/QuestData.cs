namespace Medicraft.Data.Models
{
    public class QuestData
    {
        public int QuestId { get; set; }
        public int ChapterId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string[] ObjectiveName { get; set; }
        public int ObjectiveValue { get; set; }
        public InventoryData QuestReward { get; set; }
    }

    public class QuestStamp
    {
        public int QuestId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int ObjectiveCount { get; set; } = 0;
        public bool IsQuestAccepted { get; set; } = false;
        public bool IsQuestDone { get; set; } = false;
        public bool IsQuestClear { get; set; } = false;
    }
}
