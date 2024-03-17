using Medicraft.Data.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Medicraft.Systems.Managers
{
    public class CraftingManager
    {
        private static CraftingManager instance;

        private readonly List<CraftingRecipeData> _craftingRecipeDatas;

        public List<CraftableItemData> CraftableMedicineItem { get; private set; }
        public List<CraftableItemData> CraftableFoodItem { get; private set; }
        public List<CraftableItemData> CraftableEquipmentItem { get; private set; }

        public CraftableItemData CraftingItemSelected { set; get; }

        private float _deltaSeconds;

        private CraftingManager()
        {
            _craftingRecipeDatas = GameGlobals.Instance.CraftingRecipeDatas; 

            InitializeCraftableItem();
        }

        private void InitializeCraftableItem()
        {
            // Check Craftable Thai medicine
            CraftableMedicineItem = PlayerManager.Instance.Player.PlayerData.ItemCraftingProgression.ThaiTraditionalMedicine;
            CraftableFoodItem = [];
            CraftableEquipmentItem = [];
        }

        public int GetCraftableNumber(int craftingItemId)
        {
            var recipeData = _craftingRecipeDatas.FirstOrDefault(i => i.RecipeId.Equals(craftingItemId));
            var ingredients = recipeData.Ingredients;
            var craftableQuantity = new List<int>();

            foreach (var ingredient in ingredients)
            {
                //var isItemFound = InventoryManager.Instance.InventoryBag.TryGetValue(
                //    ingredient.ItemId.ToString(), out InventoryItemData itemInBag);

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

        public void CraftingItem(int craftingItemId)
        {
            var recipeData = _craftingRecipeDatas.FirstOrDefault(i => i.RecipeId.Equals(craftingItemId));
            var ingredients = recipeData.Ingredients;

            foreach (var ingredient in ingredients)
            {
                //var itemInBag = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(i => i.ItemId.Equals(ingredient.ItemId));

                //itemInBag.Count -= ingredient.Quantity;

                //if (itemInBag.Count == 0) 
                //    InventoryManager.Instance.InventoryBag.Remove(itemInBag.ItemId.ToString());

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

            InventoryManager.Instance.AddItem(recipeData.RecipeId, recipeData.ResultQuantity);
        }

        public void Update(GameTime gameTime)
        {
            _deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;    
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
}
