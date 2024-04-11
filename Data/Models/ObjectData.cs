﻿namespace Medicraft.Data.Models
{
    public class ObjectData
    {
        public int Id { get; set; }
        public int ReferId { get; set; }
        public int Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double[] Position { get; set; }
        public bool IsRespawnable { get; set; } = true;
        public bool IsDestroyable { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public int CraftingType { get; set; }
    }
}
