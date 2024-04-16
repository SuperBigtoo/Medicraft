using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class GameSaveData
    {
        public int SaveId { get; set; }
        public string Name { get; set; }
        public string CreatedTime { get; set; }
        public string LastUpdated { get; set; }
        public int[] TotalPlayTime { get; set; }
        public PlayerData PlayerData { get; set; } 
        public List<SpawnerData> SpawnerDatas { get; set; }
    }

    public class GameConfigData
    {
        public bool IsFullScreen { get; set; }
        public double SFXVolume { get; set; }
        public double BGMusicVolume { get; set;}
    }
}
