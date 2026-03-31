using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using danikherington.Projectiles;
using Terraria.GameContent.ItemDropRules;
using danikherington;

namespace danikherington.NPCs
{
    public class DanikHeadBoss : ModNPC
    {
        private int phase = 1;
        private int attackTimer = 0;
        private int chargeTimer = 0;
        private int dashCooldown = 0;
        private bool isCharging = false;
        private Vector2 chargeDirection;
        private bool hasPlayedSpawnSound = false; // флаг, чтобы звук не повторялся
        private ReLogic.Utilities.SlotId musicId;
        private bool isMusicStarted = false;

        // Для 3 фазы
        private int berserkTimer = 0;
        private int roarTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 80;
            NPC.damage = 35;
            NPC.defense = 11;
            NPC.lifeMax = 7860;
            NPC.boss = true;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.npcSlots = 5f;
            NPC.HitSound = SoundID.NPCHit1;
            Music = 0;
        }

        public override void AI()
        {

            if (!hasPlayedSpawnSound)
            {
                SoundEngine.PlaySound(new SoundStyle("danikherington/Assets/Sounds/Custom/BillyHerringtonTheme")
                {
                    Volume = 1f
                }); // БЕЗ NPC.Center!
                hasPlayedSpawnSound = true;

                SoundStyle musicStyle = new SoundStyle("danikherington/Assets/Sounds/Custom/BillyHerringtonTheme")
                {
                    Volume = 0.5f,
                    IsLooped = true
                };
                musicId = SoundEngine.PlaySound(musicStyle);
            }



            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];

                if (player.dead || !player.active)
                {
                    NPC.velocity.Y -= 1f;
                    NPC.timeLeft = 10;
                    return;
                }
            }

            // ===== ПРОВЕРКА ФАЗ =====
            float lifeRatio = (float)NPC.life / NPC.lifeMax;

            // Переход во вторую фазу (50%)
            if (lifeRatio < 0.5f && phase == 1)
            {
                EnterPhase2();
            }

            // Переход в третью фазу - БЕРСЕРК (10%)
            if (lifeRatio < 0.1f && phase == 2)
            {
                EnterPhase3();
            }

            float distance = Vector2.Distance(NPC.Center, player.Center);

            // ===== БЕРСЕРК ФАЗА - ОСОБОЕ ПОВЕДЕНИЕ =====
            if (phase == 3)
            {
                BerserkBehavior(player, distance);
            }
            else
            {
                // ===== ОБЫЧНОЕ ПОВЕДЕНИЕ (ФАЗЫ 1-2) =====
                NormalBehavior(player, distance);
            }

            // ===== ТАРАН (ДЛЯ ВСЕХ ФАЗ) =====
            HandleCharging(player, distance);

            // ===== СТРЕЛЬБА ГАНТЕЛЯМИ =====
            HandleShooting(player);

            // ===== АНТИЗАСТРЕВАНИЕ =====
            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
            {
                NPC.velocity.Y = -6f;
                if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.position.Y -= 30;
                }
            }

            // Вращение для эффекта
            NPC.rotation = NPC.velocity.X * 0.05f;
        }

        private void EnterPhase2()
        {
            phase = 2;

            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemRuby, 0f, 0f, 0, Color.Red, 2f);
            }
            SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

            NPC.damage = 52;
            NPC.defense = 14;

            NPC.netUpdate = true;
        }

        private void EnterPhase3()
        {
            phase = 3;

            // ЭПИЧНЫЙ ПЕРЕХОД!
            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedTorch, 0f, 0f, 0, Color.Red, 3f);
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldFlame, 0f, 0f, 0, Color.Gold, 2f);
            }

            // Громкий рык
            SoundEngine.PlaySound(new SoundStyle("danikherington/Assets/Sounds/Custom/fuckyou"), NPC.Center);

            // Сообщение в чат
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("Билли Херингтон в ярости! 💢", Color.Red);
            }

            // Увеличение характеристик
            NPC.damage = 64;
            NPC.defense = 17;

            berserkTimer = 0;
            roarTimer = 0;

            NPC.netUpdate = true;
        }

        private void NormalBehavior(Player player, float distance)
        {
            if (!isCharging)
            {
                if (phase == 1)
                {
                    // Первая фаза: кружит вокруг
                    if (distance > 200f)
                    {
                        Vector2 direction = Vector2.Normalize(player.Center - NPC.Center);
                        NPC.velocity = Vector2.Lerp(NPC.velocity, direction * 6f, 0.05f);
                    }
                    else if (distance < 120f)
                    {
                        Vector2 direction = Vector2.Normalize(NPC.Center - player.Center);
                        NPC.velocity = Vector2.Lerp(NPC.velocity, direction * 3f, 0.03f);
                    }
                    else
                    {
                        Vector2 orbitDirection = Vector2.Normalize(player.Center - NPC.Center);
                        Vector2 perpendicular = new Vector2(-orbitDirection.Y, orbitDirection.X);
                        NPC.velocity = Vector2.Lerp(NPC.velocity, perpendicular * 4f, 0.02f);
                    }
                }
                else if (phase == 2)
                {
                    // Вторая фаза: агрессивнее
                    if (distance > 300f)
                    {
                        Vector2 direction = Vector2.Normalize(player.Center - NPC.Center);
                        NPC.velocity = Vector2.Lerp(NPC.velocity, direction * 8f, 0.07f);
                    }
                    else if (distance < 150f)
                    {
                        Vector2 direction = Vector2.Normalize(NPC.Center - player.Center);
                        NPC.velocity = Vector2.Lerp(NPC.velocity, direction * 2f, 0.02f);
                    }
                    else
                    {
                        float angle = Main.GameUpdateCount * 0.03f;
                        Vector2 circleOffset = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle)) * 100f;
                        Vector2 targetPos = player.Center + circleOffset;

                        Vector2 direction = Vector2.Normalize(targetPos - NPC.Center);
                        NPC.velocity = Vector2.Lerp(NPC.velocity, direction * 7f, 0.04f);
                    }
                }
            }
        }

        private void BerserkBehavior(Player player, float distance)
        {
            berserkTimer++;

            // 1. БЕШЕНОЕ УСКОРЕНИЕ
            float speed = 15f; // Очень быстро!

            if (distance > 150f)
            {
                // Рвется к игроку
                Vector2 direction = Vector2.Normalize(player.Center - NPC.Center);
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed, 0.1f);
            }
            else if (distance < 80f)
            {
                // Отскакивает, но быстро
                Vector2 direction = Vector2.Normalize(NPC.Center - player.Center);
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * speed * 0.5f, 0.1f);
            }

            // 2. ПЕРИОДИЧЕСКИЙ РЫК
            roarTimer++;
            if (roarTimer > 180) // Каждые 3 секунды
            {
                SoundEngine.PlaySound(new SoundStyle("danikherington/Assets/Sounds/Custom/fuckyou"), NPC.Center);

                // Эффект ударной волны
                for (int i = 0; i < 20; i++)
                {
                    Vector2 dustPos = NPC.Center + new Vector2(
                        Main.rand.Next(-100, 100),
                        Main.rand.Next(-100, 100)
                    );
                    Dust.NewDustPerfect(dustPos, DustID.RedTorch, Vector2.Zero, 0, Color.Red, 2f);
                }

                roarTimer = 0;
            }

            // 3. СЛЕД ИСКР (для визуала)
            if (Main.rand.NextBool(2))
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedTorch,
                             NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f, 0, Color.Red, 1.5f);
            }
        }

        private void HandleCharging(Player player, float distance)
        {
            // ТАРАН - чаще в 3 фазе
            if (!isCharging)
            {
                int chargeCooldown = phase == 3 ? 120 : 240; // В 3 фазе вдвое чаще
                int chargeChance = phase == 3 ? 60 : 120; // Чаще срабатывает

                dashCooldown--;
                if (dashCooldown <= 0 && distance < 350f && distance > 100f && Main.rand.NextBool(chargeChance))
                {
                    isCharging = true;
                    chargeTimer = phase == 3 ? 20 : 40; // Быстрее подготовка в 3 фазе
                    chargeDirection = Vector2.Normalize(player.Center - NPC.Center);

                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);

                    dashCooldown = chargeCooldown;
                }
            }

            if (isCharging)
            {
                chargeTimer--;

                if (chargeTimer > 0)
                {
                    NPC.velocity *= 0.95f;

                    for (int i = 0; i < (phase == 3 ? 5 : 3); i++)
                    {
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedTorch, 0f, 0f, 0, Color.Red, phase == 3 ? 2f : 1.5f);
                    }
                }
                else
                {
                    // Скорость тарана зависит от фазы
                    float chargeSpeed = phase == 3 ? 25f : 18f;
                    NPC.velocity = chargeDirection * chargeSpeed;

                    if (chargeTimer < -20 || Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    {
                        isCharging = false;
                        NPC.velocity *= 0.3f;
                    }
                }
            }
        }

        private void HandleShooting(Player player)
        {
            // Стрельба гантелями
            if (phase >= 2) // 2 и 3 фазы
            {
                int shootDelay = phase == 3 ? 30 : 60; // В 3 фазе стреляет в 2 раза чаще

                attackTimer++;
                if (attackTimer > shootDelay)
                {
                    int projectileCount = phase == 3 ? 4 : 2; // В 3 фазе 4 гантели за раз
                    ShootDumbbells(player.Center, projectileCount);
                    attackTimer = 0;
                }
            }
        }

        private void ShootDumbbells(Vector2 targetPos, int count = 2)
        {
            Vector2 direction = Vector2.Normalize(targetPos - NPC.Center);

            for (int i = 0; i < count; i++)
            {
                // Разброс зависит от фазы (в 3 фазе меньше разброс - точнее)
                float spreadAmount = phase == 3 ? 5f : 8f;
                float spread = MathHelper.ToRadians(spreadAmount * (i - count / 2));
                Vector2 spreadDirection = direction.RotatedBy(spread);

                // Скорость снарядов выше в 3 фазе
                float projectileSpeed = phase == 3 ? 16f : 12f;

                int projType = ModContent.ProjectileType<DumbbellProjectile>();

                Projectile.NewProjectile(
                    NPC.GetSource_FromThis(),
                    NPC.Center,
                    spreadDirection * projectileSpeed,
                    projType,
                    phase == 10 ? 15 : 20, // Больше урона
                    2f,
                    Main.myPlayer
                );
            }

            // Звук выстрела (громче в 3 фазе)
            SoundEngine.PlaySound(phase == 3 ? SoundID.DD2_BetsyFireballShot : SoundID.Item17, NPC.Center);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (isCharging)
            {
                isCharging = false;

                // Эффект мощного удара
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(target.position, target.width, target.height, DustID.RedTorch, 0f, 0f, 0, Color.Red, 2f);
                }

                SoundEngine.PlaySound(new SoundStyle("danikherington/Assets/Sounds/Custom/powerHit"), NPC.Center);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {

            // Обычный дроп (если не Expert режим)
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

            // Добавляем обычный дроп сюда 


            // Монеты
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.GoldCoin, 1, 5, 15));

            // Добавляем правило в общий лут
            npcLoot.Add(notExpertRule);
        }

        public override void OnKill()
        {

            if (musicId.IsValid)
            {
                SoundEngine.TryGetActiveSound(musicId, out var activeSound);
                if (activeSound != null && activeSound.IsPlaying)
                {
                    activeSound.Stop();
                }
            }

            SoundEngine.PlaySound(new SoundStyle("danikherington/Assets/Sounds/Custom/yeah")
            {
                Volume = 1f
            }); // БЕЗ NPC.Center!

            if (Main.netMode != NetmodeID.Server)
            {
                if (phase == 3)
                {
                    Main.NewText("Билли Херингтон успокоился... 💀", Color.Gold);
                }
                else
                {
                    Main.NewText("Билли Херингтон ушёл наверх... снова. 💪", Color.Gold);
                }
            }

            // В Expert/Master режиме кидаем мешочек
            if (Main.expertMode || Main.masterMode)
            {
                int bagItem = ModContent.ItemType<Items.BillyHerringtonBag>();
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), bagItem, 1);
            }
            else
            {
                // В обычном режиме - монеты (без гантели)
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.GoldCoin, Main.rand.Next(8, 20));
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.SilverCoin, Main.rand.Next(50, 100));
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Выбираем текстуру в зависимости от фазы
            string texturePath;
            if (phase == 1)
            {
                texturePath = "danikherington/NPCs/DanikHeadBoss";
            }
            else
            {
                texturePath = "danikherington/NPCs/DanikHeadBoss_Angry";
            }

            // Загружаем текстуру
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;

            Vector2 drawPos = NPC.Center - screenPos;
            Rectangle frame = texture.Frame(1, 1, 0, 0);
            Vector2 origin = frame.Size() / 2;

            // Цвет: красноватый во второй и третьей фазах
            Color color;
            if (phase == 1)
                color = drawColor;
            else if (phase == 2)
                color = Color.Lerp(drawColor, Color.Red, 0.4f);
            else // phase 3 - еще краснее
                color = Color.Lerp(drawColor, Color.Red, 0.8f);

            spriteBatch.Draw(texture, drawPos, frame, color, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}