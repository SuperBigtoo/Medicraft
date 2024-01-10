﻿
namespace Medicraft.Data.Models
{
    public class GameSave
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedTime { get; set; }
        public string LastUpdated { get; set; }
        public double[] CameraPosition { get; set; }
        public double[] HUDPosition { get; set; }
        public PlayerStats PlayerStats { get; set; }
    }
}
