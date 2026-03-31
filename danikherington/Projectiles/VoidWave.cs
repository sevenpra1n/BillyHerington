using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace danikherington.Projectiles
{
    public class VoidWave : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.scale = 1.7f;
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.alpha = 100;
            Projectile.light = 0.8f;

            // Изначально разрешаем проход сквозь стены
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // ЛОГИКА ПРОБИТИЯ 2-3 БЛОКОВ:
            // Снаряд становится осязаемым для стен только через некоторое время (примерно через 10 кадров)
            // Это позволит ему "прошивать" тонкие препятствия при выстреле в упор
            if (Projectile.timeLeft < 170)
            {
                Projectile.tileCollide = true;
            }

            if (Projectile.ai[0] == 1f)
            {
                Projectile.scale = 4.5f;
                Projectile.width = 100;
                Projectile.height = 100;
                Projectile.penetrate = -1;
                Lighting.AddLight(Projectile.Center, 1.2f, 0.2f, 0.9f);
            }
            else
            {
                Projectile.scale = 1.5f;
                Lighting.AddLight(Projectile.Center, 0.6f, 0.1f, 0.5f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // ЯРКИЙ РОЗОВЫЙ СЛЕД (используем частицу 61 - PurpleCrystal или 255)
            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale *= 1.8f; // Увеличили размер частиц

                if (Projectile.ai[0] == 1f) d.scale *= 1.5f; // У заряженного еще больше
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch);
                d.velocity *= 3f;
                d.noGravity = true;
                d.scale = 2.2f;
            }
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 240);
        }
    }
}