using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class ChapterData
    {
        public int ChapterId { get; set; }
        public string Name { get; set; }
        public bool IsIntroduce { get; set; } = false;
        public bool IsWarpPointUnlock { get; set; }
        public bool IsChapterClear { get; set; }
        public List<QuestStamp> Quests { get; set; }
    }

    public class ChapterItemData
    {
        public string Name { get; set; }
        public int[] ItemDropId { get; set; }
    }

    public class SpawnerData
    {
        public int ChapterId { get; set; }
        public bool IsBossDead { set; get; }
        public float BossSpawnTime { set; get; }
        public float BossSpawnTimer { set; get; }
        public List<MapSpawnTime> MapSpawnTimes { set; get; }

    }

    public class MapSpawnTime
    {
        public string SpawnerName { get; set; }
        public string MapName { get; set; }
        public float SpawnTime { set; get; }
        public float SpawnTimer { set; get; }
    }
}
