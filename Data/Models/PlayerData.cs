using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class PlayerData
    {
        public int CharId { get; set; }
        public int Level { get; set; }
        public int EXP { get; set; }
        public double CurrentHPPercentage { get; set; }
        public double CurrentManaPercentage { get; set; }
        public string CurrentMap { get; set; }
        public double[] Position { get; set; }
        public int SkillPoint { get; set; }
        public AbilityData Abilities { get; set; }
        public List<CompanionData> Companions { get; set; }
        public InventoryData InventoryData { get; set; }
        public List<ObjectData> Crops { get; set; }
        public CraftingProgressionData CraftingProgression { get; set; }
        public List<ChapterData> ChapterProgression { get; set; }
    }

    public class AbilityData
    {
        public int NormalSkillLevel { get; set; }
        public int BurstSkillLevel { get; set; }
        public int PassiveSkillLevel { get; set; }
    }

    public class CompanionData
    {
        public int CharId { get; set; }
        public int Level { get; set; }
        public double CurrentHPPercentage { get; set; }
        public AbilityData Abilities { get; set; }
        public bool IsSummoned { get; set; }
    }

    public class InventoryData
    {
        public int GoldCoin { get; set; }
        public int EXPReward { get; set; }
        public List<InventoryItemData> Items { get; set; }
    }

    public class CraftingProgressionData
    {
        public List<CraftableItem> ThaiTraditionalMedicine { get; set; }
        public List<CraftableItem> ConsumableItem { get; set; }
        public List<CraftableItem> EquipmentItem { get; set; }
    }

    public class CraftableItem()
    {
        public int ItemId { get; set; }
        public bool IsCraftable { get; set; }
    }

    public class ExperienceCapacityData
    {
        public int Level { get; set; }
        public int MaxCap { get; set; }
    }
}
