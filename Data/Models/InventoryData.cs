using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class InventoryData
    {
        public int GoldCoin { get; set; }
        public List<InventoryItemData> Inventory { get; set; }
    }
}