using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace danikherington.Projectiles
{
    public class FriendHeadProjectile : ModProjectile
    {
        // Указываем путь к текстуре головы
        public override string Texture => "danikherington/Assets/Textures/FriendFace";

        public override void SetStaticDefaults()
        {
            // Считается снарядом, а не врагом
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // Длина хвоста
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            // Основные характеристики
            Projectile.width = 16;          // Размер хитбокса
            Projectile.height = 16;
            Projectile.scale = 1f;

            // Тип и поведение
            Projectile.friendly = true;      // Дружественный (наносит урон врагам)
            Projectile.hostile = false;      // Не враждебный игроку
            Projectile.DamageType = DamageClass.Melee; // Тип урона (как у меча)
            Projectile.penetrate = 1;        // Сколько врагов может пробить
            Projectile.timeLeft = 300;       // Живет 5 секунд (60 тиков = 1 сек)

            // Физика
            Projectile.ignoreWater = true;    // Игнорирует воду
            Projectile.tileCollide = true;     // Сталкивается с блоками
            Projectile.extraUpdates = 0;       // Дополнительные обновления (для скорости)
        }

        public override void AI()
        {
            // Вращение снаряда (для эффекта)
            Projectile.rotation += 0.1f;

            // Создаем светящийся след (для красоты)
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f);
            }
        }

        // Что происходит при попадании во врага
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Поджигаем врага (как и меч)
            target.AddBuff(BuffID.OnFire, 300);

            // Эффект при попадании (частицы)
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(target.position, target.width, target.height, DustID.WhiteTorch);
            }
        }

        // Что происходит при столкновении с блоками
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Взрывные частицы при ударе о блок
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch);
            }

            // Снаряд исчезает
            Projectile.Kill();
            return false;
        }

        // Эффект когда снаряд исчезает
        public override void Kill(int timeLeft)
        {
            // Последний взрыв частиц
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
            }
        }

        // Рисуем хвост (для эффекта)
        public override bool PreDraw(ref Color lightColor)
        {
            // Рисуем след из предыдущих позиций
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                Vector2 drawPos = Projectile.oldPos[i] + new Vector2(Projectile.width / 2, Projectile.height / 2) - Main.screenPosition;
                Color color = lightColor * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length) * 0.3f;
                Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture).Value, drawPos, null, color, Projectile.rotation, new Vector2(Projectile.width / 2, Projectile.height / 2), Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}