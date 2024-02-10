﻿namespace Medicraft.Data.Models
{
    public class CharacterData
    {
        public int CharId { get; set; }
        public string Name { get; set; }
        public int Category { get; set; }
        public int ATK { get; set; }
        public int HP { get; set; }
        public double DEF_Percent { get; set; }
        public double Crit_Percent { get; set; }
        public double CritDMG_Percent { get; set; }
        public int Speed { get; set; }
        public double Evasion { get; set; }
    }
}
