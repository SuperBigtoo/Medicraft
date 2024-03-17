﻿using Medicraft.Data.Models;
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
        public int KeyIdex { set; get; }
        public Dictionary<int, InventoryItemData> InventoryBag { private set; get; }
        public KeyValuePair<int, InventoryItemData> ItemSelected { set; get; }

        private static InventoryManager instance;

        private InventoryManager()
        {
            MaximunSlot = GameGlobals.Instance.MaximunInventorySlot;
            MaximunCount = GameGlobals.Instance.MaximunItemCount;
            InventoryBag = [];
            KeyIdex = 0;
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
                InventoryBag.Add(KeyIdex++, item);
            }
        }

        public bool IsInventoryFull(int itemId, int quantity)
        {
            var itemInBag = InventoryBag.FirstOrDefault(i => i.Value.ItemId.Equals(itemId)).Value;

            // Item not found && Bag is mot full yet
            if (itemInBag == null && InventoryBag.Count < MaximunSlot)
                return false;

            // Item found and not reach maximum count
            if (itemInBag != null && (itemInBag.Count + quantity < MaximunCount || InventoryBag.Count < MaximunSlot))
                return false;

            return true;
        }

        public void AddItem(int itemId, int quantity)
        {
            var itemData = GameGlobals.Instance.ItemsDatas.FirstOrDefault(i => i.ItemId.Equals(itemId));

            var itemInBag = InventoryBag.Values.FirstOrDefault(i => i.ItemId.Equals(itemId));

            if (itemInBag != null && itemInBag.IsStackable())
            {
                itemInBag.Count += quantity;

                if (itemInBag.Count > MaximunCount)
                {
                    var tempCount = itemInBag.Count - MaximunCount;
                    itemInBag.Count -= MaximunCount;

                    // Add new stack to Inventory
                    InventoryBag.Add(KeyIdex++, new InventoryItemData()
                    {
                        ItemId = itemId,
                        Count = tempCount,
                        Slot = GameGlobals.Instance.DefaultInventorySlot
                    });
                }
            }
            else
            {
                // Add new item to Inventory
                InventoryBag.Add(KeyIdex++, new InventoryItemData()
                {
                    ItemId = itemId,
                    Count = quantity,
                    Slot = GameGlobals.Instance.DefaultInventorySlot
                });
            }

            HUDSystem.AddFeedItem(itemId, quantity);
            GUIManager.Instance.RefreshHotbarDisplay();
        }

        public void AddGoldCoin(int goldCoin)
        {
            GoldCoin += goldCoin;
        }

        public bool UseItem(int keyIndex, InventoryItemData item)
        {
            switch (item.GetCategory())
            {
                case "Consumable item":
                    // Get Item effect by effect id
                    var itemEffect = new ItemEffect(item.GetEffectId());
                    
                    // Activate the effect, if activated den delete 1 and if its 0 so remove item from inventory
                    if (itemEffect.Activate())
                    {
                        item.Count--;

                        if (item.Count == 0) InventoryBag.Remove(keyIndex);

                        // refresh display item after selectedItem has been use
                        GUIManager.Instance.RefreshInvenrotyItemDisplay(true);
                        return true;
                    }
                    break;

                case "Equipment":
                    
                    SetEquipmentItem(item, item.EquipmentType());

                    GUIManager.Instance.RefreshInvenrotyItemDisplay(true);
                    return true;
            }

            return false;
        }

        public bool UnEquip(InventoryItemData equipmentItem)
        {
            if (equipmentItem != null)
            {
                // For dis one, gonna refresh da stats before changing the slot of equipment item
                PlayerManager.Instance.RefreshEquipmentStats(equipmentItem, false);
                equipmentItem.Slot = GameGlobals.Instance.DefaultInventorySlot;              
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

                    if (itemInSlot != null)
                    {                     
                        PlayerManager.Instance.RefreshEquipmentStats(itemInSlot, false);
                        itemInSlot.Slot = GameGlobals.Instance.DefaultInventorySlot;
                    }

                    // then set player stats from player
                    PlayerManager.Instance.RefreshEquipmentStats(newItem, true);
                    newItem.Slot = selectedSlot;
                    return true;
                }
            }
            return false;
        }

        public bool UseItemInHotbar(int keyIndex)
        {
            InventoryBag.TryGetValue(keyIndex, out InventoryItemData item);

            // Check if item isUsable
            if (item.IsUsable())
            {
                UseItem(keyIndex, item);
                GUIManager.Instance.RefreshHotbarDisplay();
                return true;
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

                GUIManager.Instance.RefreshInvenrotyItemDisplay(true);
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
