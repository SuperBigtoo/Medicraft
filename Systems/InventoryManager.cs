using Medicraft.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Medicraft.Systems
{
    public class InventoryManager
    {
        public readonly int MaximunSlot;
        public int GoldCoin { set; get; }
        public Dictionary<string, int> Inventory { private set; get; }     

        private static InventoryManager instance;

        private InventoryManager()
        {
            MaximunSlot = GameGlobals.Instance.MaximunInventorySlot;
            Inventory = new Dictionary<string, int>();
            GoldCoin = 0;
        }

        public void InitializeInventory(InventoryData inventoryData)
        {
            // Setting Gold Coin
            GoldCoin = inventoryData.GoldCoin;

            // Setting Item in Inventory
            for (int i = 0; i < inventoryData.Inventory.Length; i++)
            {
                //if (inventoryData.Inventory.ElementAt(i) != 0)
                    Inventory.Add(i.ToString(), inventoryData.Inventory.ElementAt(i));
            }
        }

        public void AddItem(int referId, int amount)
        {
            // Gotta Check Item id if it already has in inventory and stackable or mot
            if (Inventory.ContainsKey(referId.ToString())
                && GameGlobals.Instance.ItemDatas[referId].Stackable)
            {
                Inventory[referId.ToString()] += amount;
            }
            else
            {
                Inventory.Add(referId.ToString(), amount);
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
