using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using danikherington.NPCs;
using Terraria.Audio; // ← Добавь это для звука

namespace danikherington.Items
{
    public class DanikHeadSummon : ModItem
    {
        public override string Texture => "danikherington/Items/DanikHeadSummon";

        public override void SetStaticDefaults()
        {
            // Оставляем пустым
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 20;
            Item.value = 100;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp; // Стиль поднятия вверх
            Item.consumable = true;
            Item.UseSound = SoundID.Item44; // Звук использования (не призыва!)
            Item.noMelee = true; // Не наносит урон

            Item.shoot = ModContent.NPCType<DanikHeadBoss>();
            Item.shootSpeed = 0f;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<DanikHeadBoss>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // 🔥 ЗВУК ПРИ ПРИЗЫВЕ БОССА
                SoundEngine.PlaySound(new SoundStyle("danikherington/Assets/Sounds/Custom/danikprime"), player.Center); // Рык

                // Спавним босса
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DanikHeadBoss>());
            }
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 3);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
}