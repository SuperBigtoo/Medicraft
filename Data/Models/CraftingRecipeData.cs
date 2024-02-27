using System.Collections.Generic;

namespace Medicraft.Data.Models
{
    public class CraftingRecipeData
    {
        public int RecipeId { get; set; }
        public string ResultItemName { get; set;}
        public string Category { get; set;}
        public int ResultQuantity { get; set; }
        public List<Ingredient> Ingredients { get; set; }
    }

    public class Ingredient
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }
}
