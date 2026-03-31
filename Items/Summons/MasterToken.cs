using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using danikherington.NPCs.Bosses; // ИСПРАВЛЕНО

namespace danikherington.Items.Summons
{
    public class MasterToken : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 20;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Purple;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<DungeonMaster>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // 1. Стандартный рык босса
                SoundEngine.PlaySound(SoundID.Roar, player.position);

                // 2. Твой кастомный звук призыва
                // Файл должен лежать в Sounds/SummonSound.wav
                SoundEngine.PlaySound(new SoundStyle("danikherington/Sounds/SummonSound")
                {
                    Volume = 2.5f,
                    Pitch = 0f
                }, player.position);

                int type = ModContent.NPCType<DungeonMaster>();
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, type);
                }
                else
                {
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                }
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.DirtBlock, 1).Register();
        }
    }
}