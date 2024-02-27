using Medicraft.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Medicraft.Systems
{
    public class InventoryManager
    {
        public readonly int MaximunSlot;
        public int GoldCoin { set; get; }
        public Dictionary<string, InventoryItemData> InventoryBag { private set; get; }     

        private static InventoryManager instance;

        private InventoryManager()
        {
            MaximunSlot = GameGlobals.Instance.MaximunInventorySlot;
            InventoryBag = new Dictionary<string, InventoryItemData>();
            GoldCoin = 0;
        }

        public void InitializeInventory(InventoryData inventoryData)
        {
            // Clear Inventory
            GoldCoin = 0;
            InventoryBag.Clear();

            // Setting Gold Coin
            GoldCoin = inventoryData.GoldCoin;

            // Setting Item in Inventory
            for (int i = 0; i < inventoryData.Inventory.Count; i++)
            {
                //if (inventoryData.Inventory.ElementAt(i) != 0)
                InventoryBag.Add(inventoryData.Inventory.ElementAt(i).ItemId.ToString(), inventoryData.Inventory.ElementAt(i));
            }
        }

        public void AddItem(int itemId, int quantity)
        {          
            // Gotta Check Item id if it already has in inventory and stackable or mot
            if (InventoryBag.ContainsKey(itemId.ToString())
                && GameGlobals.Instance.ItemsDatas[itemId].Stackable)
            {
                HudSystem.AddFeed(itemId, quantity);

                InventoryBag[itemId.ToString()].Count += quantity;
            }
            else
            {
                HudSystem.AddFeed(itemId, quantity);

                InventoryBag.Add(itemId.ToString(), new InventoryItemData() {
                    ItemId = itemId,
                    Count = quantity,
                    Slot = GameGlobals.Instance.DefaultSlot
                });
            }
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
