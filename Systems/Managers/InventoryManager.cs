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

        public const int ItemBarSlot_1 = 6;
        public const int ItemBarSlot_2 = 7;
        public const int ItemBarSlot_3 = 8;
        public const int ItemBarSlot_4 = 9;
        public const int ItemBarSlot_5 = 10;
        public const int ItemBarSlot_6 = 11;
        public const int ItemBarSlot_7 = 12;
        public const int ItemBarSlot_8 = 13;

        public int MaximunSlot { private set; get; }
        public int MaximunCount { private set; get; }
        public int GoldCoin { private set; get; }
        public Dictionary<string, InventoryItemData> InventoryBag { private set; get; }
        public InventoryItemData SelectedInventoryItem { set; get; }
        public InventoryItemData[] EquipmentAndBarItemSlot { private set; get; }

        private static InventoryManager instance;

        private InventoryManager()
        {
            MaximunSlot = GameGlobals.Instance.MaximunInventorySlot;
            MaximunCount = GameGlobals.Instance.MaximunItemCount;
            InventoryBag = [];
            EquipmentAndBarItemSlot = new InventoryItemData[14];
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

                if (item.Slot != GameGlobals.Instance.DefaultInventorySlot)
                {
                    InitInSlotItem(item);
                }
            }
        }

        public void InitInSlotItem(InventoryItemData item)
        {
            if (item.Slot >= 0 && item.Slot < EquipmentAndBarItemSlot.Length)
            {
                EquipmentAndBarItemSlot[item.Slot] = item;
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
                    
                    SetEquipmentOrItemBarSlot(selectedItem, selectedItem.EquipmentType());

                    // then set player stats from player

                    return true;
            }

            return false;
        }

        public bool UseItemInSlotBar(int itemId)
        {
            InventoryBag.TryGetValue(itemId.ToString(), out InventoryItemData item);
            SelectedInventoryItem = item;

            // Check if item isUsable
            if (GameGlobals.Instance.IsUsableItem(itemId))
            {
                UseItem(item);
                GUIManager.Instance.RefreshItemBarDisplay();
                return true;
            }

            return false;
        }

        public void SetEquipmentOrItemBarSlot(InventoryItemData item, int selectedSlot)
        {
            if (item.GetCategory().Equals("Equipment"))
            {
                if (selectedSlot >= 0 && selectedSlot <= 5)
                {
                    if (EquipmentAndBarItemSlot[selectedSlot] != null)
                    {
                        EquipmentAndBarItemSlot[selectedSlot].Slot = GameGlobals.Instance.DefaultInventorySlot;
                    }

                    EquipmentAndBarItemSlot[selectedSlot] = item;
                }
            }
            else
            {
                if (selectedSlot >= 6 && selectedSlot <= 13)
                {
                    if (EquipmentAndBarItemSlot[selectedSlot] != null)
                    {
                        EquipmentAndBarItemSlot[selectedSlot].Slot = GameGlobals.Instance.DefaultInventorySlot;
                    }

                    EquipmentAndBarItemSlot[selectedSlot] = item;
                }
            }
        }

        public int SetItemBarSlot(int selectedSlot)
        {
            switch (selectedSlot)
            {
                case 1:
                    return ItemBarSlot_1;

                case 2:
                    return ItemBarSlot_2;

                case 3:
                    return ItemBarSlot_3;

                case 4:
                    return ItemBarSlot_4;

                case 5:
                    return ItemBarSlot_5;

                case 6:
                    return ItemBarSlot_6;

                case 7:
                    return ItemBarSlot_7;

                case 8:
                    return ItemBarSlot_8;

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
