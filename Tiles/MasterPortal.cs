using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using danikherington.Items;
using danikherington.NPCs.Bosses;

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
            // MasterToken now drops from the boss itself; nothing drops when the portal is broken.
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (player.ConsumeItem(ModContent.ItemType<MasterKey>()))
            {
                // Spawn boss the same way as using the summon item (NPC.SpawnOnPlayer).
                // Only the server (or single-player) authorises NPC spawns.
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DungeonMaster>());
                }
                else
                {
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent,
                        number: player.whoAmI,
                        number2: ModContent.NPCType<DungeonMaster>());
                }
                // Portal stays in place; no sound or text.
                return true;
            }
            // No key — complete silence, nothing happens.
            return false;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.8f; g = 0.2f; b = 1.0f;
        }
    }
}