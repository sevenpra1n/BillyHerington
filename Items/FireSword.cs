using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Audio;
using danikherington.Projectiles;

namespace danikherington.Items
{
    public class FireSword : ModItem
    {
        public override string Texture => "danikherington/Items/FireSword";

        public override void SetDefaults()
        {
            Item.damage = 4;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.scale = 1.7f;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(gold: 26);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            var modPlayer = player.GetModPlayer<DanikPlayer>();

            // Если сейчас идет задержка показа финальной стадии — новые удары не считаем
            if (modPlayer.comboFreezeTimer > 0) return;

            // Визуальные эффекты при обычном ударе
            modPlayer.uiFlash = 1f;
            modPlayer.uiShake = new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f));

            int threshold = modPlayer.hasFriendAccessory ? 15 : 5;
            modPlayer.comboCount++;

            // Проверяем достижение максимальной стадии
            if (modPlayer.comboCount >= threshold)
            {
                // Устанавливаем задержку в 30 кадров (полсекунды), чтобы текстура не исчезала сразу
                modPlayer.comboFreezeTimer = 30;
                modPlayer.uiFlash = 2.5f; // Усиленная вспышка
                modPlayer.uiShake = new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f));

                // Звук завершения комбо
                SoundEngine.PlaySound(SoundID.MaxMana, player.Center);

                // Фразы и мощный урон
                string[] powerPhrases = { "Ульта Даника!", "Кончаю!!", "Соси хуй!", "Глотай мой пенис!", "Это тебе за билли!" };
                string randomPhrase = powerPhrases[Main.rand.Next(powerPhrases.Length)];
                CombatText.NewText(target.getRect(), Color.Cyan, randomPhrase, true);

                int powerDamage = damageDone * 3;
                target.StrikeNPC(new NPC.HitInfo
                {
                    Damage = powerDamage,
                    Knockback = hit.Knockback,
                    HitDirection = hit.HitDirection,
                    SourceDamage = powerDamage
                });

                // Кастомный звук мощного удара
                SoundEngine.PlaySound(new SoundStyle("danikherington/Assets/Sounds/Custom/powerHit"), target.Center);

                // Призыв голов
                Vector2 direction = Vector2.Normalize(target.Center - player.Center);
                if (modPlayer.hasFriendAccessory)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 speed = direction.RotatedBy(MathHelper.ToRadians(-15 + (15 * i))) * 12f;
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, speed, ModContent.ProjectileType<FriendHeadProjectile>(), 30, hit.Knockback, player.whoAmI);
                    }
                }
                else
                {
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, direction * 12f, ModContent.ProjectileType<FriendHeadProjectile>(), 30, hit.Knockback, player.whoAmI);
                }
            }

            if (Main.rand.NextBool(2)) target.AddBuff(BuffID.OnFire, 300);

            // Частицы при ударе
            for (int i = 0; i < 40; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(target.width, target.height), DustID.Cloud);
                dust.noGravity = true;
                dust.velocity *= 1.2f;
                dust.scale = Main.rand.NextFloat(1f, 1.3f);
            }
        }

        public override void AddRecipes()
        {
            // Способ №1: С использованием Демонитовых слитков (Порча)
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HaritonBar>(), 16)
                .AddIngredient(ItemID.DemoniteBar, 30) // Слитки порчи
                .AddIngredient(ItemID.Obsidian, 90)
                .AddTile(TileID.Anvils)
                .Register();

            // Способ №2: С использованием Кримтановых слитков (Багрянец)
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<HaritonBar>(), 16)
                .AddIngredient(ItemID.CrimtaneBar, 30) // Слитки багрянца
                .AddIngredient(ItemID.Obsidian, 90)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (var line in tooltips)
            {
                if (line.Name == "ItemName")
                {
                    line.Text = "Ярость Даника";
                    line.OverrideColor = new Color(220, 20, 60);
                }
                if (line.Name == "Tooltip0")
                {
                    line.Text = "Накапливайте комбо для мощного удара!";
                    line.OverrideColor = new Color(0, 191, 255);
                }
            }
        }
    }
}