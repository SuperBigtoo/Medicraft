using Medicraft.Systems;
using System.Linq;

namespace Medicraft.Data.Models
{
    public class InventoryItemData
    {
        public int ItemId { get; set; }
        public int Count { get; set; }
        public int Slot { get; set; }

        public string GetName()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.Name;
        }

        public string GetCategory()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.Category;
        }

        public string GetDescription()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.Description;
        }

        public bool IsUsable()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.Usable;
        }

        public int GetEffectId()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.Effect;
        }

        public bool IsStackable()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.Stackable;
        }
    }
}
