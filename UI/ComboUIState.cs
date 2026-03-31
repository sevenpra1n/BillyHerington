using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using ReLogic.Content;
using danikherington.Items;

namespace danikherington.UI
{
    public class ComboUIState : UIState
    {
        public override void Draw(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<DanikPlayer>();

            // Рисуем только если полоска видна
            if (modPlayer.uiOpacity <= 0f) return;

            string texturePath = modPlayer.hasFriendAccessory
                ? $"danikherington/Assets/UI/AccStage{modPlayer.comboCount}"
                : $"danikherington/Assets/UI/Stage{modPlayer.comboCount}";

            if (ModContent.HasAsset(texturePath))
            {
                var comboTexture = ModContent.Request<Texture2D>(texturePath).Value;

                // РАСЧЕТ ПОЗИЦИИ НАД ГОЛОВОЙ:
                // 1. Берем верхнюю точку игрока в мире (player.Top)
                // 2. Вычитаем Main.screenPosition, чтобы перевести мировые координаты в экранные
                Vector2 screenPos = player.Top - Main.screenPosition;

                // 3. Сдвигаем текстуру так, чтобы её центр совпадал с центром игрока
                // И поднимаем на 40 пикселей выше головы
                Vector2 basePos = new Vector2(
                    screenPos.X - (comboTexture.Width / 2),
                    screenPos.Y - comboTexture.Height - 40
                );

                // Добавляем эффект тряски
                Vector2 drawPos = basePos + modPlayer.uiShake;

                // Отрисовка основной текстуры
                spriteBatch.Draw(comboTexture, drawPos, Color.White * modPlayer.uiOpacity);

                // Отрисовка вспышки (блика) при ударе
                if (modPlayer.uiFlash > 0f)
                {
                    spriteBatch.Draw(comboTexture, drawPos, Color.White * modPlayer.uiFlash * modPlayer.uiOpacity);
                }
            }
        }
    }

    public class ComboUISystem : ModSystem
    {
        private UserInterface comboUserInterface;
        internal ComboUIState comboUIState;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                comboUIState = new ComboUIState();
                comboUserInterface = new UserInterface();
                comboUserInterface.SetState(comboUIState);
            }
        }

        public override void UpdateUI(GameTime gameTime) => comboUserInterface?.Update(gameTime);

        public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            // Важно: используем слой "Vanilla: Resource Bars" или "Vanilla: Entity Health Bars"
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Entity Health Bars"));
            if (index == -1) index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer("DanikMod: ComboOverHead", delegate {
                    comboUserInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                }, InterfaceScaleType.None)); // ТУТ ВАЖНО: InterfaceScaleType.None для привязки к миру!
            }
        }
    }
}