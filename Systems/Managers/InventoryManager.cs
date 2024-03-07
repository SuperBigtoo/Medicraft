using Medicraft.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Medicraft.Systems.Managers
{
    public class InventoryManager
    {
        // Equipment & Item Bar Slot
        public const int Sword = 0;
        public const int Helmet = 1;
        public const int Chestplate = 2;
        public const int Glove = 3;
        public const int Boots = 4;
        public const int Ring = 5;

        public const int HotbarSlot_1 = 6;
        public const int HotbarSlot_2 = 7;
        public const int HotbarSlot_3 = 8;
        public const int HotbarSlot_4 = 9;
        public const int HotbarSlot_5 = 10;
        public const int HotbarSlot_6 = 11;
        public const int HotbarSlot_7 = 12;
        public const int HotbarSlot_8 = 13;

        public int MaximunSlot { private set; get; }
        public int MaximunCount { private set; get; }
        public int GoldCoin { private set; get; }
        public Dictionary<string, InventoryItemData> InventoryBag { private set; get; }
        public InventoryItemData SelectedInventoryItem { set; get; }

        private static InventoryManager instance;

        private InventoryManager()
        {
            MaximunSlot = GameGlobals.Instance.MaximunInventorySlot;
            MaximunCount = GameGlobals.Instance.MaximunItemCount;
            InventoryBag = [];
            GoldCoin = 0;
        }

        // Inventory Data
        public void InitializeInventory(InventoryData inventoryData)
        {
            // Clear Inventory
            GoldCoin = 0;
            InventoryBag.Clear();

            // Setup Gold Coin
            GoldCoin = inventoryData.GoldCoin;

            // Setup Item in Inventory
            foreach (var item in inventoryData.Inventory)
            {
                InventoryBag.Add(item.ItemId.ToString(), item);
            }
        }

        public bool IsInventoryFull(string itemId, int quantity)
        {
            var isItemFound = InventoryBag.TryGetValue(itemId, out InventoryItemData itemInBag);

            // Item not found && Bag is mot full yet
            if (!isItemFound && InventoryBag.Count < MaximunSlot) return false;

            // Item found and still reach maximum count
            if (isItemFound && itemInBag.Count + quantity < MaximunCount) return false;

            return true;
        }

        public void AddItem(int itemId, int quantity)
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(itemId));

            // Gotta Check Item id if it already has in inventory and stackable or mot
            if (InventoryBag.ContainsKey(itemId.ToString()) && itemData.Stackable)
            {
                InventoryBag[itemId.ToString()].Count += quantity;
            }
            else
            {           
                InventoryBag.Add(itemId.ToString(), new InventoryItemData()
                {
                    ItemId = itemId,
                    Count = quantity,
                    Slot = GameGlobals.Instance.DefaultInventorySlot
                });
            }

            HUDSystem.AddFeedItem(itemId, quantity);
        }

        public void AddGoldCoin(int goldCoin)
        {
            GoldCoin += goldCoin;
        }

        public bool UseItem(InventoryItemData selectedItem)
        {
            switch (selectedItem.GetCategory())
            {
                case "Consumable item":
                    // Get Item effect by effect id
                    var itemEffect = new ItemEffect(selectedItem.GetEffectId());
                    
                    // Activate the effect, if activated den delete 1 and if its 0 so remove item from inventory
                    if (itemEffect.Activate())
                    {
                        selectedItem.Count--;

                        if (selectedItem.Count == 0) InventoryBag.Remove(selectedItem.ItemId.ToString());

                        // refresh display item after selectedItem has been use
                        GUIManager.Instance.RefreshInvenrotyItemDisplay(true);
                        return true;
                    }
                    break;

                case "Equipment":
                    
                    SetEquipmentItem(selectedItem, selectedItem.EquipmentType());

                    // then set player stats from player

                    return true;
            }

            return false;
        }

        public bool UseItemInHotbar(int itemId)
        {
            InventoryBag.TryGetValue(itemId.ToString(), out InventoryItemData item);

            // Check if item isUsable
            if (GameGlobals.Instance.IsUsableItem(itemId))
            {
                UseItem(item);
                GUIManager.Instance.RefreshHotbarDisplay();
                return true;
            }

            return false;
        }

        public bool SetEquipmentItem(InventoryItemData newItem, int selectedSlot)
        {
            if (newItem.GetCategory().Equals("Equipment"))
            {
                if (selectedSlot >= 0 && selectedSlot <= 5)
                {
                    var itemInSlot = InventoryBag.Values.FirstOrDefault(i => i.Slot.Equals(selectedSlot));

                    if (itemInSlot != null) itemInSlot.Slot = GameGlobals.Instance.DefaultInventorySlot;

                    newItem.Slot = selectedSlot;
                    return true;
                }
            }

            return false;
        }

        public bool SetHotbarItem(InventoryItemData newItem, int selectedSlot)
        {
            if (selectedSlot >= 6 && selectedSlot <= 13)
            {
                var itemInSlot = InventoryBag.Values.FirstOrDefault(i => i.Slot.Equals(selectedSlot));

                if (itemInSlot != null) itemInSlot.Slot = GameGlobals.Instance.DefaultInventorySlot;

                newItem.Slot = selectedSlot;
                return true;
            }

            return false;
        }

        public int GetIHotbarSlot(string selectedSlot)
        {
            switch (selectedSlot)
            {
                case "Slot 1":
                    return HotbarSlot_1;

                case "Slot 2":
                    return HotbarSlot_2;

                case "Slot 3":
                    return HotbarSlot_3;

                case "Slot 4":
                    return HotbarSlot_4;

                case "Slot 5":
                    return HotbarSlot_5;

                case "Slot 6":
                    return HotbarSlot_6;

                case "Slot 7":
                    return HotbarSlot_7;

                case "Slot 8":
                    return HotbarSlot_8;

                default:
                    break;
            }

            return GameGlobals.Instance.DefaultInventorySlot;
        }

        public static InventoryManager Instance
        {
            get
            {
                instance ??= new InventoryManager();
                return instance;
            }
        }
    }
}
