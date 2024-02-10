namespace Medicraft.Data.Models
{
    public class PlayerData
    {
        public int Level { get; set; }
        public int EXP { get; set; }
        public double[] Position { get; set; }
        public AbilityData Abilities { get; set; }
        public InventoryData InventoryData { get; set; }
        public MedicineProgressionData MedicineProgressionData { get; set; }
        public ChapterProgressionData ChapterProgressionData { get; set; }
    }
}
