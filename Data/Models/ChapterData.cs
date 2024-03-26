﻿using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class ChapterData
    {
        public int ChapterId { get; set; }
        public string Name { get; set; }
        public bool IsChapterClear { get; set; }
        public List<QuestStamp> Quests { get; set; }
    }

    public class QuestStamp
    {
       public int QuestId { get; set; }
       public string Name { get; set; }
       public string Type { get; set; }
       public bool IsQuestClear { get; set; }
    }

    public class ChapterItemData
    {
        public string Name { get; set; }
        public int[] ItemDropId { get; set; }
    }
}
