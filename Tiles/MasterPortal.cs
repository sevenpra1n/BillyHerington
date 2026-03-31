using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using danikherington.Items;
using danikherington.Items.Summons;

namespace danikherington.Tiles
{
    public class MasterPortal : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileSpelunker[Type] = true;
            Main.tileLighted[Type] = true;

            // Настройка 6x6
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 6;
            TileObjectData.newTile.Height = 6;
            // Указываем высоты строк (16 пикселей на блок)
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2; // Стандартный отступ в спрайте

            // Точка опоры: (3, 5) — это центр нижней линии блоков объекта 6x6
            TileObjectData.newTile.Origin = new Point16(3, 5);
            TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.SolidTile, 6, 0);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(150, 0, 250));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 96, 96, ModContent.ItemType<MasterToken>());
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (player.ConsumeItem(ModContent.ItemType<MasterKey>()))
            {
                SoundEngine.PlaySound(SoundID.Roar, new Vector2(i * 16, j * 16));
                player.GetModPlayer<DanikPlayer>().ScreenShakeTimer = 100;
                WorldGen.KillTile(i, j);
                return true;
            }
            return false;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.8f; g = 0.2f; b = 1.0f;
        }
    }
}