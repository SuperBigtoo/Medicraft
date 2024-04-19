using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static Medicraft.Systems.GameGlobals;

namespace Medicraft.Systems.Managers
{
    public class CraftingManager
    {
        private static CraftingManager instance;

        private readonly List<CraftingRecipeData> _craftingRecipeDatas;

        public List<CraftableItem> CraftableMedicineItem { get; private set; }
        public List<CraftableItem> CraftableFoodItem { get; private set; }
        public List<CraftableItem> CraftableEquipmentItem { get; private set; }

        public CraftableItem CraftingItemSelected { set; get; }

        private float _deltaSeconds;

        private CraftingManager()
        {
            _craftingRecipeDatas = GameGlobals.Instance.CraftingRecipeDatas; 

            InitializeCraftableItem();
        }

        private void InitializeCraftableItem()
        {
            // Check Craftable Thai medicine
            CraftableMedicineItem = PlayerManager.Instance.Player.PlayerData.CraftingProgression.ThaiTraditionalMedicine;
            CraftableFoodItem = PlayerManager.Instance.Player.PlayerData.CraftingProgression.ConsumableItem;
            CraftableEquipmentItem = PlayerManager.Instance.Player.PlayerData.CraftingProgression.EquipmentItem;
        }

        public int GetCraftableNumber(int craftingItemId)
        {
            var recipeData = _craftingRecipeDatas.FirstOrDefault(i => i.RecipeId.Equals(craftingItemId));
            var ingredients = recipeData.Ingredients;
            var craftableQuantity = new List<int>();

            foreach (var ingredient in ingredients)
            {
                var stackItem = InventoryManager.Instance.InventoryBag.Where
                    (i => i.Value.ItemId.Equals(ingredient.ItemId));

                // if any of itemInBag if null or not found BREAK!  
                if (stackItem == null || !stackItem.Any()) return 0;

                // Sum the count of item in stackItem
                int totalCount = 0;
                foreach (var item in stackItem)
                {
                    totalCount += item.Value.Count;
                }

                // if any of itemInBag count is less than ingredient count then BREAK!!
                if (totalCount < ingredient.Quantity) return 0;

                // check craftable quantity with ingredient count and itemInBag count
                craftableQuantity.Add(totalCount / ingredient.Quantity);
            }

            return craftableQuantity.Min();
        }

        public void CraftingItem(int craftingItemId, int itemQuantity)
        {
            var recipeData = _craftingRecipeDatas.FirstOrDefault
                (i => i.RecipeId.Equals(craftingItemId));
            var ingredients = recipeData.Ingredients;
            int totalCount = 0;

            for (int i = 0; i < itemQuantity; i++)
            {
                foreach (var ingredient in ingredients)
                {
                    var stackItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                        (i => i.Value.ItemId.Equals(ingredient.ItemId));

                    stackItem.Value.Count -= ingredient.Quantity;

                    if (stackItem.Value.Count == 0)
                    {
                        InventoryManager.Instance.InventoryBag.Remove(stackItem.Key);
                    }
                    else if (stackItem.Value.Count < 0)
                    {
                        var tempCount = stackItem.Value.Count;
                        InventoryManager.Instance.InventoryBag.Remove(stackItem.Key);

                        var newStackItem = InventoryManager.Instance.InventoryBag.FirstOrDefault
                            (i => i.Value.ItemId.Equals(ingredient.ItemId));
                        newStackItem.Value.Count += tempCount;
                    }
                }

                totalCount += recipeData.ResultQuantity;
            }

            OnCraftingItem(new CraftingEventArgs(recipeData));
            InventoryManager.Instance.AddItem(recipeData.RecipeId, totalCount);

            if (recipeData.Category.Equals("Thai traditional medicine"))
            {
                PlaySoundEffect([Sound.CraftingPotion1, Sound.CraftingPotion2, Sound.CraftingPotion3, Sound.CraftingPotion4]);
            }
            else PlaySoundEffect(Sound.Crafting1);
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;    
        }

        // Define a delegate for the event handler
        public delegate void CraftingEventHandler(object sender, EventArgs e);

        // Define an event based on the delegate
        public event CraftingEventHandler EventHandler;

        // Method to raise the event
        public virtual void OnCraftingItem(CraftingEventArgs e)
        {
            EventHandler?.Invoke(this, e);
        }

        public static CraftingManager Instance
        {
            get
            {
                instance ??= new CraftingManager();
                return instance;
            }
        }
    }

    public class CraftingEventArgs(CraftingRecipeData recipeData) : EventArgs
    {
        public CraftingRecipeData RecipeData { get; } = recipeData;
    }
}
