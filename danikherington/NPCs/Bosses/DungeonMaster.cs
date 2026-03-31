using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using System;
using Terraria.GameContent.ItemDropRules;
using danikherington.Items;
using ReLogic.Content;
using ReLogic.Utilities;

namespace danikherington.NPCs.Bosses
{
    
    public class DungeonMaster : ModNPC
    {
        private bool IsOneShotDash = false;
        private int dashTimer = 0;
        private bool playedRageSound = false;
        private float handRotation = 0f;

        // Переменные для музыки и фаз
        private SlotId musicSlotMain;
        private SlotId musicSlotRage;
        private int rageSubPhaseTimer = 0;
        private int rageStage = 0;

        private float handOffsetTimer = 0f;
        private Vector2 leftHandOffset;
        private Vector2 rightHandOffset;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 120;
            NPC.height = 120;
            NPC.damage = 80;
            NPC.defense = 60;
            NPC.lifeMax = 53500;

            NPC.HitSound = SoundID.NPCHit4; // Звук металла
            NPC.DeathSound = SoundID.NPCDeath14;

            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.value = Item.buyPrice(0, 15, 0, 0);
            NPC.SpawnWithHigherTime(30);
            Music = 0;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (player.dead || !player.active)
            {
                StopAllMusic();
                NPC.TargetClosest(false);
                NPC.velocity.Y -= 0.3f;
                if (NPC.timeLeft > 10) NPC.timeLeft = 10;
                return;
            }

            float healthPercent = (float)NPC.life / NPC.lifeMax;

            // --- МУЗЫКАЛЬНЫЙ КОНТРОЛЛЕР ---
            if (healthPercent > 0.30f)
            {
                if (!SoundEngine.TryGetActiveSound(musicSlotMain, out _))
                {
                    musicSlotMain = SoundEngine.PlaySound(new SoundStyle("danikherington/Sounds/BossTheme") { IsLooped = true, Volume = 0.5f, Type = SoundType.Music });
                }
            }
            else
            {
                if (SoundEngine.TryGetActiveSound(musicSlotMain, out var sMain)) sMain.Stop();

                if (!playedRageSound)
                {
                    SoundEngine.PlaySound(new SoundStyle("danikherington/Sounds/RageTransition") { Volume = 3.5f, Pitch = 0f }, NPC.Center);
                    playedRageSound = true;
                    Main.NewText("НЕБО ПОГЛОТИЛА ТЬМА...", 255, 0, 0);
                    Main.dayTime = false;
                    Main.time = 0;
                    if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.WorldData);
                }

                if (playedRageSound && !SoundEngine.TryGetActiveSound(musicSlotRage, out _))
                {
                    musicSlotRage = SoundEngine.PlaySound(new SoundStyle("danikherington/Sounds/RageTheme") { IsLooped = true, Volume = 0.7f, Type = SoundType.Music });
                }
            }

            NPC.ai[0]++;

            // --- ФАЗА 3 (НИЖЕ 30% HP) ---
            if (healthPercent <= 0.30f)
            {
                Lighting.AddLight(NPC.Center, 2f, 0.5f, 0.8f);
                rageSubPhaseTimer++;

                if (rageSubPhaseTimer >= 360)
                {
                    rageSubPhaseTimer = 0;
                    rageStage++;
                    if (rageStage > 2) rageStage = 0;

                    SoundEngine.PlaySound(SoundID.Item15, NPC.Center);
                    for (int i = 0; i < 30; i++) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, 0, 0, 100, default, 2f);
                }

                // СТАДИЯ 0: ШИПЫ (Медленно)
                if (rageStage == 0)
                {
                    NPC.velocity = Vector2.Lerp(NPC.velocity, (player.Center + new Vector2(0, -300) - NPC.Center) * 0.05f, 0.02f);
                    if (NPC.ai[0] % 60 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                Vector2 shootVel = new Vector2(0, 7f).RotatedBy(MathHelper.ToRadians(i * 30));
                                int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVel, ProjectileID.SpikyBall, 30, 1f, 255);
                                Main.projectile[p].hostile = true;
                                Main.projectile[p].friendly = false;
                            }
                        }
                    }
                }
                // СТАДИЯ 1: ЛАЗЕРЫ (Держит дистанцию)
                else if (rageStage == 1)
                {
                    Vector2 dist = player.Center - NPC.Center;
                    if (dist.Length() < 350f) NPC.velocity = Vector2.Lerp(NPC.velocity, -dist.SafeNormalize(Vector2.Zero) * 8f, 0.05f);
                    else NPC.velocity = Vector2.Lerp(NPC.velocity, dist.SafeNormalize(Vector2.Zero) * 6f, 0.05f);

                    if (NPC.ai[0] % 30 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item33, NPC.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 laserVel = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 14f;
                            int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, laserVel, ProjectileID.DeathLaser, 80, 1f, 255);
                            Main.projectile[p].hostile = true;
                            Main.projectile[p].friendly = false;
                        }
                    }
                }
                // СТАДИЯ 2: МОЛНИИ
                else if (rageStage == 2)
                {
                    NPC.velocity *= 0.9f;
                    if (NPC.ai[0] % 45 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Thunder, player.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 spawnPos = player.Center + new Vector2(Main.rand.Next(-50, 51), -700);
                            int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, new Vector2(0, 16f), ProjectileID.ThunderStaffShot, 135, 2f, 255);
                            Main.projectile[p].hostile = true;
                            Main.projectile[p].friendly = false;
                            Main.projectile[p].tileCollide = false;
                        }
                    }
                }
            }
            else
            {
                // --- ФАЗЫ 1-2 ---
                bool secondPhase = healthPercent <= 0.6f;
                float moveSpeed = secondPhase ? 9f : 6f;
                Vector2 targetPos = player.Center + new Vector2(0, -250);
                Vector2 moveTo = (targetPos - NPC.Center);
                NPC.velocity = (NPC.velocity * 20f + (moveTo.Length() > moveSpeed ? moveTo.SafeNormalize(Vector2.Zero) * moveSpeed : moveTo)) / 21f;

                if (secondPhase && NPC.ai[0] % 100 == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 11f, ProjectileID.DeathLaser, 50, 1f, 255);
                        Main.projectile[p].hostile = true;
                        Main.projectile[p].friendly = false;
                    }
                }

                dashTimer++;
                if (dashTimer >= 180)
                {
                    dashTimer = 0;
                    IsOneShotDash = Main.rand.NextFloat() < 0.05f;
                    NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * (secondPhase ? 18f : 14f);
                    if (IsOneShotDash) SoundEngine.PlaySound(SoundID.Zombie105, NPC.Center);
                }
            }
        }

        public override void OnKill()
        {
            StopAllMusic();

            // Включаем день
            Main.dayTime = true;
            Main.time = 0; // Устанавливает время на начало дня (4:30 AM)

            // Если это сервер, нужно отправить пакет данных всем игрокам, чтобы время обновилось у всех
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
            }

            SoundEngine.PlaySound(new SoundStyle("danikherington/Sounds/DeathSound") { Volume = 3.5f, Pitch = 0f }, NPC.Center);

            // Визуальный эффект: вспышка света при возвращении дня
            for (int i = 0; i < 50; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.YellowStarDust, 0f, 0f, 100, default, 2f);
            }

            Main.NewText("DungeonMaster отступил, И СОЛНЦЕ ВЗОШЛО СНОВА...", 255, 255, 0);
        }
        
        private void StopAllMusic()
        {
            if (SoundEngine.TryGetActiveSound(musicSlotMain, out var s1)) s1.Stop();
            if (SoundEngine.TryGetActiveSound(musicSlotRage, out var s2)) s2.Stop();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            handOffsetTimer += 0.05f;

            Vector2 inertia = NPC.velocity * -2f;
            leftHandOffset = Vector2.Lerp(leftHandOffset, new Vector2(-110, (float)Math.Sin(handOffsetTimer) * 15f) + inertia, 0.1f);
            rightHandOffset = Vector2.Lerp(rightHandOffset, new Vector2(110, (float)Math.Sin(handOffsetTimer) * 15f) + inertia, 0.1f);

            string handPath = "danikherington/NPCs/Bosses/DungeonMaster_Hand";
            if (ModContent.HasAsset(handPath))
            {
                Texture2D handTex = ModContent.Request<Texture2D>(handPath).Value;
                Vector2 origin = handTex.Size() / 3f;
                // ИСПРАВЛЕНИЕ: null вторым аргументом для корректной работы с Vector2 (позицией)
                spriteBatch.Draw(handTex, NPC.Center + leftHandOffset - screenPos, null, drawColor, NPC.rotation * 0.5f, origin, NPC.scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(handTex, NPC.Center + rightHandOffset - screenPos, null, drawColor, NPC.rotation * 0.5f, origin, NPC.scale, SpriteEffects.FlipHorizontally, 0f);
            }

            if (healthPercent <= 0.30f)
            {
                string ragePath = Texture + "_Rage";
                if (ModContent.HasAsset(ragePath))
                {
                    Texture2D rageTex = ModContent.Request<Texture2D>(ragePath).Value;
                    spriteBatch.Draw(rageTex, NPC.Center - screenPos, null, drawColor, NPC.rotation, rageTex.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);
                    return false;
                }
            }
            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float healthPercent = (float)NPC.life / NPC.lifeMax;
            if (healthPercent <= 0.30f)
            {
                string ragePath = Texture + "_Rage";
                if (ModContent.HasAsset(ragePath))
                {
                    Texture2D glowTexture = ModContent.Request<Texture2D>(ragePath, AssetRequestMode.ImmediateLoad).Value;
                    Vector2 drawOrigin = new Vector2(glowTexture.Width / 2, glowTexture.Height / 2);
                    spriteBatch.Draw(glowTexture, NPC.Center - screenPos, null, Color.White * 0.8f, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

                    // Добавим свечение рукам в фазе ярости
                    string handPath = "danikherington/NPCs/Bosses/DungeonMaster_Hand";
                    if (ModContent.HasAsset(handPath))
                    {
                        Texture2D handTexture = ModContent.Request<Texture2D>(handPath, AssetRequestMode.ImmediateLoad).Value;
                        float idleAnim = (float)Math.Sin(handOffsetTimer) * 15f;
                        spriteBatch.Draw(handTexture, NPC.Center + new Vector2(-100, idleAnim) - screenPos, null, Color.White * 0.5f, NPC.rotation * 0.5f, new Vector2(handTexture.Width / 2, handTexture.Height / 2), NPC.scale, SpriteEffects.None, 0f);
                        spriteBatch.Draw(handTexture, NPC.Center + new Vector2(100, idleAnim) - screenPos, null, Color.White * 0.5f, NPC.rotation * 0.5f, new Vector2(handTexture.Width / 2, handTexture.Height / 2), NPC.scale, SpriteEffects.FlipHorizontally, 0f);
                    }
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<BossBag>()));
        }

    }
}