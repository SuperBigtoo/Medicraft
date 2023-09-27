﻿namespace Medicraft.Data.Models
{
    public class EntityStats
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ATK { get; set; }
        public int HP { get; set; }
        public double DEF_Percent { get; set; }
        public int Speed { get; set; }
        public double Evasion { get; set; }
        public double[] Position { get; set; }
    }
}
