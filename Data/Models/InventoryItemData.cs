using Medicraft.Systems;
using System.Linq;

namespace Medicraft.Data.Models
{
    public class InventoryItemData
    {
        public int ItemId { get; set; }
        public int Count { get; set; }
        public int Slot { get; set; } = GameGlobals.Instance.DefaultInventorySlot;

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

            return itemData.IsUsable;
        }

        public int GetEffectId()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.Effect;
        }

        public bool IsStackable()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.IsStackable;
        }

        public int EquipmentType()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.EquipmentType;
        }

        public int GetBuyingPrice()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.BuyingPrice;
        }

        public int GetSellingPrice()
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(ItemId));

            return itemData.SellingPrice;
        }
    }
}
