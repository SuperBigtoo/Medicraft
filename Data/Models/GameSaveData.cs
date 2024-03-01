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
    }
}
