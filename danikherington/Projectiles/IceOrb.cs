using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

namespace danikherington.Projectiles
{
    public class IceOrb : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 30;  // Увеличенный хитбокс
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.scale = 1.4f; // Увеличенный визуальный размер
        }

        public override void AI()
        {
            // Голубое свечение
            Lighting.AddLight(Projectile.Center, 0.1f, 0.6f, 1.0f);

            // Частицы льда
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ice);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }

            // Логика кометы (если ai[0] == 1)
            if (Projectile.ai[0] == 1f)
            {
                Projectile.scale = 2.8f; // Комета огромная
                Projectile.rotation += 0.2f;
                // Комета не наводится, пока не упадет низко
                if (Projectile.velocity.Y < 10) Projectile.ai[0] = 0f;
                return;
            }

            // САМОНАВЕДЕНИЕ С ПРОВЕРКОЙ СТЕН
            float range = 500f;
            float speed = 11f;
            NPC target = null;
            float minDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    // Проверка дистанции + Collision.CanHit (не видит сквозь стены)
                    if (dist < minDist && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height))
                    {
                        minDist = dist;
                        target = npc;
                    }
                }
            }

            if (target != null)
            {
                Vector2 moveDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, moveDir * speed, 0.07f);
            }

            Projectile.rotation += 0.05f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 300); // Сильное обморожение
        }

        public override void OnKill(int timeLeft)
        {
            // Звук разбитого льда
            SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);

            // Если это была комета (ai[0] == 1), создаем 3 обычных сферы
            if (Projectile.ai[0] == 1f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(6, 6), Projectile.type, Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                }
            }

            // Эффект осколков
            for (int i = 0; i < 20; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ice);
                d.velocity *= 2.5f;
                d.noGravity = false;
            }
        }
    }
}