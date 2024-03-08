using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class PlayerData
    {
        public int CharId { get; set; }
        public int Level { get; set; }
        public int EXP { get; set; }
        public string CurrentMap { get; set; }
        public double[] Position { get; set; }
        public AbilityData Abilities { get; set; }
        public InventoryData InventoryData { get; set; }
        public ItemCraftingProgressionData ItemCraftingProgression { get; set; }
        public ChapterProgressionData ChapterProgressionData { get; set; }
    }

    public class AbilityData
    {
        public int NormalSkillLevel { get; set; }
        public int BurstSkillLevel { get; set; }
        public int PassiveSkillLevel { get; set; }
    }

    public class InventoryData
    {
        public int GoldCoin { get; set; }
        public List<InventoryItemData> Inventory { get; set; }
    }

    public class ItemCraftingProgressionData
    {
        public List<CraftableItemData> ThaiTraditionalMedicine { get; set; }
    }

    public class CraftableItemData()
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public bool IsCraftable { get; set; }
    }

    public class ChapterProgressionData
    {
        public ChapterData Chapter_1 { get; set; }
        public ChapterData Chapter_2 { get; set; }
        public ChapterData Chapter_3 { get; set; }
        public ChapterData Chapter_4 { get; set; }
        public ChapterData Chapter_5 { get; set; }
        public ChapterData Chapter_6 { get; set; }
    }

    public class ExperienceCapacityData
    {
        public int Level { get; set; }
        public int MaxCap { get; set; }
    }
}
