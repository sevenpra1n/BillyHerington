using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace danikherington
{
    public class DanikPlayer : ModPlayer
    {
        public bool hasFriendAccessory = false;
        public int comboCount = 0;

        // Таймер для заморозки комбо
        public int comboFreezeTimer = 0;

        // НОВОЕ: Таймер для дрожания экрана
        public int ScreenShakeTimer = 0;

        public float uiOpacity = 0f;
        public Vector2 uiShake = Vector2.Zero;
        public float uiFlash = 0f;

        public override void ResetEffects()
        {
            hasFriendAccessory = false;
        }

        // НОВОЕ: Метод, который физически трясет камеру, если таймер больше нуля
        public override void ModifyScreenPosition()
        {
            if (ScreenShakeTimer > 0)
            {
                // Создает случайное смещение в радиусе 10 пикселей
                Main.screenPosition += Main.rand.NextVector2Circular(10f, 10f);
            }
        }

        public override void PostUpdateEquips()
        {
            // Обработка таймера заморозки
            if (comboFreezeTimer > 0)
            {
                comboFreezeTimer--;
                if (comboFreezeTimer == 0)
                {
                    comboCount = 0;
                }
            }

            // НОВОЕ: Уменьшаем таймер тряски экрана каждый кадр
            if (ScreenShakeTimer > 0)
            {
                ScreenShakeTimer--;
            }

            // Плавное появление UI
            if (Player.HeldItem.type == ModContent.ItemType<Items.FireSword>())
                uiOpacity = MathHelper.Clamp(uiOpacity + 0.05f, 0f, 1f);
            else
                uiOpacity = MathHelper.Clamp(uiOpacity - 0.05f, 0f, 1f);

            uiShake *= 0.9f;
            uiFlash = MathHelper.Clamp(uiFlash - 0.05f, 0f, 1f);

            // Защита от вылета
            int max = hasFriendAccessory ? 15 : 5;
            if (comboCount > max) comboCount = 0;
        }
    }
}