namespace Medicraft.Data.Models
{
    public class PlayerData
    {
        public int CharId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int EXP { get; set; }
        public int ATK { get; set; }
        public int HP { get; set; }
        public double DEF_Percent { get; set; }
        public double Crit_Percent { get; set; }
        public double CritDMG_Percent { get; set; }
        public int Speed { get; set; }
        public double Evasion { get; set; }
        public double[] Position { get; set; }
        public AbilityData Abilities { get; set; }
        public InventoryData InventoryData { get; set; }
        public MedicineProgressionData MedicineProgressionData { get; set; }
        public ChapterProgressionData ChapterProgressionData { get; set; }
    }
}
