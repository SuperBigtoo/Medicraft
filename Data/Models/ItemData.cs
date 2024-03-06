using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class ItemData
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool Usable { get; set; }
        public int Effect { get; set; }
        public bool Stackable { get; set; }
        public int EquipmentType { get; set; }
        public int[] QuantityDropRange { get; set; }
    }

    public class ItemEffectData
    {
        public int ItemEffectId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<Effect> Effects { get; set; }
    }

    public class Effect
    {
        public string Target { get; set; }
        public string ActivationType { get; set; }
        public string EffectType { get; set; }
        public double Value { get; set; }
        public float Time { get; set; }
        public float Timer { get; set; }       
        public bool IsActive { get; set; }
    }
}
