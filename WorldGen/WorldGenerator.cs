using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
using danikherington.Tiles;

namespace danikherington
{
    public class WinterFortressGen : ModSystem
    {
        public static bool FortressSpawned = false;

        public override void SaveWorldData(TagCompound tag)
        {
            tag["WinterFortressSpawned"] = FortressSpawned;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            FortressSpawned = tag.ContainsKey("WinterFortressSpawned") && tag.GetBool("WinterFortressSpawned");
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int index = tasks.FindIndex(t => t.Name.Equals("Shinies"));
            if (index == -1)
                index = tasks.Count - 1;
            tasks.Insert(index + 1, new PassLegacy("Winter Fortress", GenerateFortress));
        }

        private static void GenerateFortress(GenerationProgress progress, GameConfiguration config)
        {
            if (FortressSpawned)
                return;

            progress.Message = "Building Winter Fortress...";

            const int FortW             = 80;
            const int FortH             = 100;
            const int MaxAttempts       = 2000;
            const int MinBorderDistance = 400;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                int cx = WorldGen.genRand.Next(MinBorderDistance, Main.maxTilesX - MinBorderDistance);

                // Find the surface tile at this X
                int cy = (int)Main.worldSurface - 30;
                while (cy < (int)Main.worldSurface + 60 && !Main.tile[cx, cy].HasTile)
                    cy++;

                if (cy >= (int)Main.worldSurface + 60 || cy < 40)
                    continue;

                // Must have enough horizontal space
                if (cx - FortW / 2 < MinBorderDistance || cx + FortW / 2 > Main.maxTilesX - MinBorderDistance)
                    continue;

                // Must be in a Snow biome
                if (!IsSnowBiome(cx, cy))
                    continue;

                BuildFortress(cx, cy, FortW, FortH);
                FortressSpawned = true;
                return;
            }
        }

        private static bool IsSnowBiome(int x, int y)
        {
            const int MinSnowTileCount = 30;
            int count = 0;
            for (int sx = x - 40; sx <= x + 40; sx++)
            {
                for (int sy = y; sy < y + 20; sy++)
                {
                    if (!WorldGen.InWorld(sx, sy))
                        continue;
                    int tt = Main.tile[sx, sy].TileType;
                    if (tt == TileID.SnowBlock || tt == TileID.IceBlock || tt == TileID.SnowBrick)
                        count++;
                }
            }
            return count >= MinSnowTileCount;
        }

        // -----------------------------------------------------------------------
        // Fortress layout (80 wide x 100 tall, measured downward from 'top'):
        //
        //  top+0  .. top+4   : Corner tower tops  (5 rows)
        //  top+5  .. top+14  : Crenellations zone (10 rows – alternating merlons)
        //  top+15 .. top+18  : Solid ceiling       (4 rows)
        //  top+19 .. top+53  : Second floor room   (35 rows interior)
        //  top+54 .. top+56  : Mid-floor divider   (3 rows stone)
        //  top+57 .. top+96  : First floor room    (40 rows interior)
        //  top+97 .. top+99  : Obsidian floor      (3 rows)
        //
        //  Wall thickness : 4 blocks on each side
        // -----------------------------------------------------------------------
        private static void BuildFortress(int cx, int groundY, int w, int h)
        {
            int left   = cx - w / 2;
            int top    = groundY - h + 1;   // top of fortress structure
            int bottom = groundY;           // ground level

            const ushort Stone       = TileID.GrayBrick;
            const ushort ObsFloor    = TileID.Obsidian;
            const int   StoneWall    = WallID.GrayBrick;
            const int   WallThick    = 4;

            int midFloorTop = top + 54;    // top row of mid-floor divider
            // The obsidian floor is 3 layers thick (bottom-2 .. bottom).
            // Place the portal origin one row above the top obsidian layer so
            // the solid tile at portalBaseY+1 (= bottom-2) satisfies AnchorBottom.
            int portalBaseY = bottom - 3;

            // --- 1. Clear the work area ---
            for (int x = left - 2; x <= left + w + 1; x++)
            {
                for (int y = top; y <= bottom + 2; y++)
                {
                    if (!WorldGen.InWorld(x, y)) continue;
                    WorldGen.KillTile(x, y, noItem: true);
                    WorldGen.EmptyLiquid(x, y);
                    WorldGen.KillWall(x, y);
                }
            }

            // --- 2. Obsidian floor (3 layers) ---
            for (int x = left; x < left + w; x++)
                for (int y = bottom - 2; y <= bottom; y++)
                    SafePlaceTile(x, y, ObsFloor);

            // --- 3. Outer side walls (from ceiling down to just above obsidian) ---
            for (int y = top + 19; y < bottom - 2; y++)
            {
                for (int x = left; x < left + WallThick; x++)
                    SafePlaceTile(x, y, Stone);
                for (int x = left + w - WallThick; x < left + w; x++)
                    SafePlaceTile(x, y, Stone);
            }

            // --- 4. Ceiling (4 rows solid) ---
            for (int x = left; x < left + w; x++)
                for (int y = top + 15; y < top + 19; y++)
                    SafePlaceTile(x, y, Stone);

            // --- 5. Mid-floor divider ---
            for (int x = left; x < left + w; x++)
                for (int y = midFloorTop; y < midFloorTop + 3; y++)
                    SafePlaceTile(x, y, Stone);

            // --- 6. Background walls (both floors) ---
            for (int x = left + WallThick; x < left + w - WallThick; x++)
            {
                // Second floor
                for (int y = top + 19; y < midFloorTop; y++)
                    WorldGen.PlaceWall(x, y, StoneWall);
                // First floor
                for (int y = midFloorTop + 3; y < bottom - 2; y++)
                    WorldGen.PlaceWall(x, y, StoneWall);
            }

            // --- 7. Battlements: alternating merlons (2 wide) across the top ---
            for (int x = left; x < left + w; x += 4)
            {
                for (int bx = x; bx < x + 2 && bx < left + w; bx++)
                    for (int y = top + 10; y < top + 15; y++)
                        SafePlaceTile(bx, y, Stone);
            }

            // --- 8. Corner towers (taller than standard battlements) ---
            int towerW = WallThick + 2;
            for (int x = left; x < left + towerW; x++)
                for (int y = top + 5; y < top + 15; y++)
                    SafePlaceTile(x, y, Stone);
            for (int x = left + w - towerW; x < left + w; x++)
                for (int y = top + 5; y < top + 15; y++)
                    SafePlaceTile(x, y, Stone);

            // Tower tops (extra height above battlements)
            for (int x = left; x < left + towerW; x++)
                for (int y = top; y < top + 5; y++)
                    SafePlaceTile(x, y, Stone);
            for (int x = left + w - towerW; x < left + w; x++)
                for (int y = top; y < top + 5; y++)
                    SafePlaceTile(x, y, Stone);

            // --- 9. Entrances: openings in the side walls (first floor level) ---
            for (int x = left; x < left + WallThick; x++)
                for (int y = bottom - 14; y < bottom - 2; y++)
                    WorldGen.KillTile(x, y, noItem: true);
            for (int x = left + w - WallThick; x < left + w; x++)
                for (int y = bottom - 14; y < bottom - 2; y++)
                    WorldGen.KillTile(x, y, noItem: true);

            // --- 10. Staircase opening in mid-floor divider ---
            for (int x = cx - 5; x <= cx + 5; x++)
                for (int y = midFloorTop; y < midFloorTop + 3; y++)
                    WorldGen.KillTile(x, y, noItem: true);

            // --- 11. Portal (first floor, center) ---
            // Portal is 6x6 with Origin(3,5).  PlaceObject(cx, portalBaseY) puts the
            // origin tile at (cx, portalBaseY), so the portal spans:
            //   x: cx-3 .. cx+2
            //   y: portalBaseY-5 .. portalBaseY
            // Solid obsidian is at portalBaseY+1 = bottom-2  ✓
            for (int x = cx - 4; x <= cx + 3; x++)
                for (int y = portalBaseY - 6; y <= portalBaseY; y++)
                {
                    WorldGen.KillTile(x, y, noItem: true);
                    WorldGen.KillWall(x, y);
                }
            WorldGen.PlaceObject(cx, portalBaseY, ModContent.TileType<MasterPortal>());

            // --- 12. Chests on second floor (sitting on the mid-floor divider) ---
            // PlaceChest: x = left tile of chest, y = bottom tile of chest;
            // the solid anchor is the divider row at midFloorTop.
            int chestY = midFloorTop - 1;
            PlaceFortressChest(left + 12,      chestY);
            PlaceFortressChest(cx - 1,         chestY);
            PlaceFortressChest(left + w - 14,  chestY);

            // --- 13. Torches ---
            WorldGen.PlaceTile(left + WallThick + 2, bottom - 8,       TileID.Torches, true);
            WorldGen.PlaceTile(left + w - WallThick - 3, bottom - 8,   TileID.Torches, true);
            WorldGen.PlaceTile(cx,                       bottom - 8,    TileID.Torches, true);
            WorldGen.PlaceTile(left + WallThick + 2, midFloorTop - 8,  TileID.Torches, true);
            WorldGen.PlaceTile(left + w - WallThick - 3, midFloorTop - 8, TileID.Torches, true);
        }

        private static void SafePlaceTile(int x, int y, ushort type)
        {
            if (!WorldGen.InWorld(x, y)) return;
            WorldGen.PlaceTile(x, y, type, true, true);
        }

        private static void PlaceFortressChest(int x, int y)
        {
            int idx = WorldGen.PlaceChest(x, y, TileID.Containers, false, 1); // style 1 = gold chest
            if (idx < 0) return;

            Chest chest = Main.chest[idx];
            if (chest == null) return;

            int slot = 0;

            chest.item[slot] = new Item();
            chest.item[slot].SetDefaults(ItemID.GoldCoin);
            chest.item[slot].stack = WorldGen.genRand.Next(5, 20);
            slot++;

            chest.item[slot] = new Item();
            chest.item[slot].SetDefaults(ItemID.SilverCoin);
            chest.item[slot].stack = WorldGen.genRand.Next(20, 75);
            slot++;

            int[] loot = { ItemID.Stone, ItemID.Wood, ItemID.Torch, ItemID.IronOre };
            chest.item[slot] = new Item();
            chest.item[slot].SetDefaults(loot[WorldGen.genRand.Next(loot.Length)]);
            chest.item[slot].stack = WorldGen.genRand.Next(10, 35);
        }
    }
}
