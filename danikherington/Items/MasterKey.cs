using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace danikherington.Items
{
    public class MasterKey : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 20;
            Item.maxStack = 99;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 1, 0, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 1) // Твой тестовый рецепт
                .Register();
        }
    }
}