using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class PlayerData
    {
        public int CharId { get; set; }
        public int Level { get; set; }
        public int EXP { get; set; }
        public double[] Position { get; set; }
        public AbilityData Abilities { get; set; }
        public InventoryData InventoryData { get; set; }
        public MedicineProgressionData MedicineProgressionData { get; set; }
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

    public class MedicineProgressionData
    {
        public bool Medicine_200 { get; set; }
        public bool Medicine_201 { get; set; }
        public bool Medicine_202 { get; set; }
        public bool Medicine_203 { get; set; }
        public bool Medicine_204 { get; set; }
        public bool Medicine_205 { get; set; }
        public bool Medicine_206 { get; set; }
        public bool Medicine_207 { get; set; }
        public bool Medicine_208 { get; set; }
        public bool Medicine_209 { get; set; }
        public bool Medicine_210 { get; set; }
        public bool Medicine_211 { get; set; }
        public bool Medicine_212 { get; set; }
        public bool Medicine_213 { get; set; }
        public bool Medicine_214 { get; set; }
        public bool Medicine_215 { get; set; }
        public bool Medicine_216 { get; set; }
        public bool Medicine_217 { get; set; }
        public bool Medicine_218 { get; set; }
        public bool Medicine_219 { get; set; }
        public bool Medicine_220 { get; set; }
        public bool Medicine_221 { get; set; }
        public bool Medicine_222 { get; set; }
        public bool Medicine_223 { get; set; }
        public bool Medicine_224 { get; set; }
        public bool Medicine_225 { get; set; }
        public bool Medicine_226 { get; set; }
        public bool Medicine_227 { get; set; }
        public bool Medicine_228 { get; set; }
        public bool Medicine_229 { get; set; }
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
}
