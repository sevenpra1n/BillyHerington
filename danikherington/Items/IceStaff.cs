using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using danikherington.Projectiles;
using System;

namespace danikherington.Items
{
    public class IceStaff : ModItem
    {
        private int chargeTimer = 0;
        private bool fullyCharged = false;

        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.width = 46;
            Item.height = 46;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item28; // Ледяной звон
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<IceOrb>();
            Item.shootSpeed = 9f;
            Item.scale = 1.2f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            if (player.altFunctionUse == 2 && Main.mouseRight)
            {
                player.channel = true;
                chargeTimer++;

                // Таймер 1..2..3..4..5 со звуком и сменой цвета
                if (chargeTimer % 60 == 0 && chargeTimer <= 300)
                {
                    int seconds = chargeTimer / 60;
                    Color timerColor = seconds < 3 ? Color.Yellow : (seconds < 5 ? Color.Orange : Color.LimeGreen);

                    CombatText.NewText(player.getRect(), timerColor, seconds.ToString(), true);
                    SoundEngine.PlaySound(SoundID.MenuTick with { Pitch = seconds * 0.2f, Volume = 0.8f }, player.Center);
                }

                // Частицы зарядки
                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustDirect(player.Center + Main.rand.NextVector2Circular(100, 100), 0, 0, DustID.Ice);
                    d.velocity = (player.Center - d.position) * 0.2f;
                    d.noGravity = true;
                }

                if (chargeTimer == 300 && !fullyCharged)
                {
                    fullyCharged = true;
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.2f, Volume = 1.2f }, player.Center);
                    CombatText.NewText(player.getRect(), Color.Cyan, "Billy заряжен!", true);
                }

                player.itemTime = 10;
                player.itemAnimation = 10;
            }
            else
            {
                if (chargeTimer >= 300)
                {
                    // УЛЬТА: Осколочный дождь (5 комет с неба)
                    if (Main.myPlayer == player.whoAmI)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 spawnPos = Main.MouseWorld + new Vector2(Main.rand.Next(-150, 151), -700);
                            Vector2 velocity = new Vector2(Main.rand.Next(-2, 3), 16f);
                            // ai[0] = 1f означает, что это комета
                            Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawnPos, velocity, ModContent.ProjectileType<IceOrb>(), Item.damage * 3, 6f, player.whoAmI, 1f);
                        }
                    }
                    SoundEngine.PlaySound(SoundID.Item120 with { Volume = 1.5f, Pitch = -0.3f }, player.Center);
                }
                chargeTimer = 0;
                fullyCharged = false;
            }
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) return false;
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.Wood, 1).AddTile(TileID.WorkBenches).Register();
        }
    }
}