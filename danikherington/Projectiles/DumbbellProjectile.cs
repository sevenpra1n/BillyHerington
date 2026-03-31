using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace danikherington.Projectiles
{
    public class DumbbellProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1f;
            Projectile.friendly = false; // Враждебный к игроку
            Projectile.hostile = true;    // Наносит урон игроку
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.aiStyle = 1; // Простая баллистика
            AIType = ProjectileID.DemonScythe;
        }

        public override void AI()
        {
            // Вращение гантели
            Projectile.rotation += 0.3f;

            // След
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Iron, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // Звук удара
            Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit4, Projectile.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Частицы при ударе
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Iron);
            }
            return true;
        }
    }
}