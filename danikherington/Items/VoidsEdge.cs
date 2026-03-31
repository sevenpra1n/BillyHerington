using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using danikherington.Projectiles;
using System;
using System.Collections.Generic;

namespace danikherington.Items
{
    public class VoidsEdge : ModItem
    {
        private int chargeTimer = 0;
        private bool fullyCharged = false;

        public override void SetDefaults()
        {
            Item.damage = 45;
            Item.DamageType = DamageClass.Melee;
            Item.width = 54;
            Item.height = 54;
            Item.scale = 1.7f;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5;
            Item.value = Item.buyPrice(0, 0, 26, 0);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<VoidWave>();
            Item.shootSpeed = 15f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            if (player.altFunctionUse == 2 && Main.mouseRight)
            {
                player.channel = true;
                chargeTimer++;

                // ИСПРАВЛЕНИЕ: Теперь игрок поворачивается за мышкой при зарядке
                player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;

                // ТВОЙ ЦИФРОВОЙ ТАЙМЕР
                if (chargeTimer % 60 == 0 && chargeTimer <= 300)
                {
                    int seconds = chargeTimer / 60;
                    Color timerColor = seconds < 3 ? Color.Yellow : (seconds < 5 ? Color.Orange : Color.LimeGreen);
                    CombatText.NewText(player.getRect(), timerColor, seconds.ToString(), true);
                    SoundEngine.PlaySound(SoundID.MenuTick with { Pitch = seconds * 0.2f, Volume = 0.8f }, player.Center);
                }

                // ТВОИ ЭФФЕКТЫ ЗАСАСЫВАНИЯ
                float intensity = chargeTimer / 300f;
                if (Main.rand.NextFloat() < intensity)
                {
                    Dust d = Dust.NewDustDirect(player.Center + Main.rand.NextVector2Circular(80, 80), 0, 0, DustID.Shadowflame);
                    d.velocity = (player.Center - d.position) * 0.25f;
                    d.noGravity = true;
                    d.scale = 1.2f + intensity;
                }

                if (chargeTimer == 300 && !fullyCharged)
                {
                    fullyCharged = true;
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.2f, Volume = 1.5f }, player.Center);
                    CombatText.NewText(player.getRect(), Color.Cyan, "Я готов ебашить!", true);
                }

                player.itemTime = 10;
                player.itemAnimation = 10;
            }
            else
            {
                if (chargeTimer >= 300)
                {
                    if (Main.myPlayer == player.whoAmI)
                    {
                        // ИСПРАВЛЕНИЕ: Выстрел летит точно в курсор
                        Vector2 velocity = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero) * 20f;
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, velocity, ModContent.ProjectileType<VoidWave>(), Item.damage * 4, 12f, player.whoAmI, 1f);
                    }
                    SoundEngine.PlaySound(SoundID.Item62 with { Volume = 1.8f, Pitch = -0.5f }, player.Center);
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

        // ТВОИ ЭФФЕКТЫ ВЗМАХА
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            for (int i = 0; i < 6; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.Blood : DustID.Shadowflame;
                Dust d = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, dustType, 0f, 0f, 100, default, 1.2f);
                d.noGravity = true;
            }
        }

        // ТВОЯ МЕХАНИКА КРИТА
        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Main.rand.NextFloat() < 0.10f)
            {
                modifiers.SourceDamage *= 2f;
                SoundStyle customCrit = new SoundStyle("danikherington/Sounds/CritSound") { Volume = 2.5f, Pitch = 0.0f, MaxInstances = 3 };
                SoundStyle vanillaCrit = SoundID.Item71 with { Volume = 1.9f, Pitch = -0.3f };

                SoundEngine.PlaySound(customCrit, target.Center);
                SoundEngine.PlaySound(vanillaCrit, target.Center);
                CombatText.NewText(target.getRect(), Color.Red, "Dungeon..", true);

                for (int i = 0; i < 20; i++)
                {
                    Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, 0f, 0f, 100, default, 2f);
                    d.noGravity = true;
                    d.velocity *= 3f;
                }
            }
        }

        // ОБЫЧНЫЙ ВЫСТРЕЛ ПРИ ВЗМАХЕ
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(5));
            Projectile.NewProjectile(source, position, perturbedSpeed, type, damage / 2, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.Wood, 10).AddTile(TileID.WorkBenches).Register();
        }
    }
}