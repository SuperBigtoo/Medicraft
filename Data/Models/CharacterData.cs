﻿namespace Medicraft.Data.Models
{
    public class CharacterData
    {
        public int CharId { get; set; }
        public string Name { get; set; }
        public int Category { get; set; }
        public double ATK { get; set; }
        public double HP { get; set; }
        public double DEF_Percent { get; set; }
        public double Crit_Percent { get; set; }
        public double CritDMG_Percent { get; set; }
        public int Speed { get; set; }
        public double Evasion { get; set; }
    }

    public class SkillDescriptionData
    {
        public int SkillId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string Description { get; set; }
        public int SkillPointCost { get; set; }
        public int GoldCoinCost { get; set; }
    }
}
