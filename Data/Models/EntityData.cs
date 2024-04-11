using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class EntityData
    {
        public int Id { get; set; }
        public int CharId { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public int PathFindingType { get; set; }
        public int NodeCycleTime { get; set; }
        public string PartrolArea { get; set; }
        public double[][] RoutePoint { get; set; }
        public int AggroTime { get; set; }
        public double[] Position { get; set; }
        public List<InventoryItemData> TradingItemsData { get; set; }
    }
}
