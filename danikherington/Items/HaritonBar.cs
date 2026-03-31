using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace danikherington.Items
{
    public class HaritonBar : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Указываем количество кадров анимации (3 спрайта)
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(10, 3));

            // Позволяет предмету светиться в темноте (как Харитон!)
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Cyan;
            Item.material = true; // Это материал для крафта
        }

        // Добавим немного свечения самому предмету в мире
        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.4f, 0.9f, 1.0f); // Голубоватое свечение
        }

        public override void AddRecipes()
        {
            // Пример рецепта (можно поменять на руду, если она будет)
            CreateRecipe()
                .AddIngredient(ItemID.ChlorophyteBar, 1)
                .AddIngredient(ItemID.GlowingMushroom, 5)
                .AddTile(TileID.AdamantiteForge)
                .Register();
        }
    }
}