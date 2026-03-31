using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;

namespace danikherington.Items
{
    public class BossBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.BossBag[Type] = true;
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.consumable = true;
            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.Purple;
            Item.expert = true;
        }

        public override bool CanRightClick() => true;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Золото
            itemLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 5, 10));
            // Зелья
            itemLoot.Add(ItemDropRule.Common(ItemID.GreaterHealingPotion, 1, 5, 8));

            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<VoidsEdge>(), 1));

            // Сюда можно добавить твой меч, когда узнаем его имя в коде!
        }
    }
}