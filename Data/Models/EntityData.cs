using System;
using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class EntityData
    {
        public int Id { get; set; }
        public int CharId { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public int PathFindingType { get; set; }
        public int NodeCycleTime { get; set; }
        public string PartrolArea { get; set; }
        public double[][] RoutePoint { get; set; }
        public int AggroTime { get; set; }
        public double[] Position { get; set; }

        // For FriendlyMob
        public string MobType { get; set; } = string.Empty;
        public bool IsInteractable { get; set; } = false;
        public List<InventoryItemData> TradingItemsData { get; set; } = null;
        public List<DialogData> DialogData { get; set; } = null;

        // For Quest Giver
        public int QuestId { get; set; }

        // For Boss
        public int BossChapterId { get; set; } = 0;
    }

    public class DialogData
    {
        public enum DialogType
        {
            Daily,
            Quest   // if it quest den has 4 stage of dialog in 'Stage': onAccept, onGoing, onDone and onClear
        }

        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        // For Quest Dialog
        public int QuestId { get; set; }
        public int ChapterId { get; set; }
        public string Stage { get; set; }

        // Dialogues
        public List<(string, string)> Dialogues { get; set; }
    }
}
