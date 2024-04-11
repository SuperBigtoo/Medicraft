using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class ItemData
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool IsUsable { get; set; }
        public int Effect { get; set; }
        public bool IsStackable { get; set; }
        public int EquipmentType { get; set; }
        public int[] QuantityDropRange { get; set; }
        public int BuyingPrice { get; set; } = 0;
        public int SellingPrice { get; set; } = 0;
    }

    public class EquipmentStatsData
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int EquipmentType { get; set; }
        public List<Stats> Stats { get; set; }
    }

    public class Stats
    {
        public string Target { get; set; }
        public double Value { get; set; }
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

    public class MedicineDescriptionData
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Characteristics { get; set; }
        public string Recipe { get; set; }
        public string Instructions { get; set; }
        public string HowtoUse { get; set; }
        public string Warning { get; set; }
    }
}
