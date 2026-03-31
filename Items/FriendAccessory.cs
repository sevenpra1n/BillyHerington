using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace danikherington.Items
{
    public class FriendAccessory : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true; // Это делает предмет аксессуаром
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 15);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Говорим нашему игроку, что аксессуар надет
            player.GetModPlayer<DanikPlayer>().hasFriendAccessory = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}