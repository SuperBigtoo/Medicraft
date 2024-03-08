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
            var craftableQuantity = 0;

            foreach (var ingredient in ingredients)
            {
                var isItemFound = InventoryManager.Instance.InventoryBag.TryGetValue(
                    ingredient.ItemId.ToString(), out InventoryItemData itemInBag);

                // if any of itemInBag if null or not found BREAK!  
                if (!isItemFound) return 0;

                // if any of itemInBag count is less than ingredient count then BREAK!!
                if (itemInBag.Count < ingredient.Quantity) return 0;

                // check craftable quantity with ingredient count and itemInBag count
                craftableQuantity = itemInBag.Count / ingredient.Quantity;
            }

            return craftableQuantity;
        }

        public void CraftingItem(int craftingItemId)
        {
            var recipeData = _craftingRecipeDatas.FirstOrDefault(i => i.RecipeId.Equals(craftingItemId));
            var ingredients = recipeData.Ingredients;

            foreach (var ingredient in ingredients)
            {
                var itemInBag = InventoryManager.Instance.InventoryBag.Values.FirstOrDefault(i => i.ItemId.Equals(ingredient.ItemId));

                itemInBag.Count -= ingredient.Quantity;

                if (itemInBag.Count == 0) 
                    InventoryManager.Instance.InventoryBag.Remove(itemInBag.ItemId.ToString());
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
