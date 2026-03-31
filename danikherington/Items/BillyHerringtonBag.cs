using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace danikherington.Items
{
    public class BillyHerringtonBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            // В новой версии SetStaticDefaults оставляем пустым!
            // Названия будут из Localization.hjson
        }

        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.consumable = true;
            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;

            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item2;
            Item.autoReuse = false;
            Item.noUseGraphic = true;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void RightClick(Player player)
        {
            var source = player.GetSource_OpenItem(Type);

            // ===== ВСЕГДА ВЫПАДАЮТ =====
            player.QuickSpawnItem(source, ItemID.GoldCoin, Main.rand.Next(5, 16));
            player.QuickSpawnItem(source, ItemID.SilverCoin, Main.rand.Next(50, 101));
            player.QuickSpawnItem(source, ModContent.ItemType<HaritonBar>(), Main.rand.Next(8, 16));

            // ===== СЛУЧАЙНЫЙ ДРОП =====
            int random = Main.rand.Next(5);
        }
        public override string Texture => "danikherington/Items/BillyHerringtonBag"; // стало
    }
}