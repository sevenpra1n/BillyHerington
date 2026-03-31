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
        // Prevents KillMultiTile from dropping the token when the portal is
        // destroyed via right-click activation (so the token goes to inventory instead).
        private static bool _activatedByKey = false;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileSpelunker[Type] = true;
            Main.tileLighted[Type] = true;

            // 6x6 multi-tile
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 6;
            TileObjectData.newTile.Height = 6;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;

            // Origin at (3, 5) — bottom-centre of the 6x6 grid
            TileObjectData.newTile.Origin = new Point16(3, 5);
            TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.SolidTile, 6, 0);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(150, 0, 250));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // Only drop the token when the tile is broken by other means (e.g. pickaxe).
            // When activated via RightClick the token is placed in inventory directly.
            if (!_activatedByKey)
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 96, 96, ModContent.ItemType<MasterToken>());
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (player.ConsumeItem(ModContent.ItemType<MasterKey>()))
            {
                SoundEngine.PlaySound(SoundID.Roar, new Vector2(i * 16, j * 16));
                player.GetModPlayer<DanikPlayer>().ScreenShakeTimer = 100;

                // Give MasterToken directly to the player's inventory
                player.QuickSpawnItem(new EntitySource_TileInteraction(player, i, j), ModContent.ItemType<MasterToken>());

                // Find the top-left origin of this 6x6 tile and destroy it.
                // CoordinateWidth (16) + CoordinatePadding (2) = 18 pixels per column/row.
                Tile tile = Framing.GetTileSafely(i, j);
                int originX = i - tile.TileFrameX / 18;
                int originY = j - tile.TileFrameY / 18;

                _activatedByKey = true;
                try
                {
                    WorldGen.KillTile(originX, originY, noItem: true);
                }
                finally
                {
                    _activatedByKey = false;
                }

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