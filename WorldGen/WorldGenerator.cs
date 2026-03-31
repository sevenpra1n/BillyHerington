using System;
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

            // --- 10. Staircase opening in mid-floor divider (widened for staircase access) ---
            for (int x = cx - 5; x <= cx + 8; x++)
                for (int y = midFloorTop; y < midFloorTop + 3; y++)
                    WorldGen.KillTile(x, y, noItem: true);

            // --- 11. Portal (first floor, center) ---
            for (int x = cx - 4; x <= cx + 3; x++)
                for (int y = portalBaseY - 6; y <= portalBaseY; y++)
                {
                    WorldGen.KillTile(x, y, noItem: true);
                    WorldGen.KillWall(x, y);
                }
            WorldGen.PlaceObject(cx, portalBaseY, ModContent.TileType<MasterPortal>());

            // --- 12. Chests on second floor ---
            int chestY = midFloorTop - 1;
            PlaceFortressChest(left + 12,      chestY);
            PlaceFortressChest(cx - 1,         chestY);
            PlaceFortressChest(left + w - 14,  chestY);

            // --- 13. Demon torches in strategic locations ---
            // First floor (wall-mounted positions)
            PlaceDemonTorch(left + WallThick + 2, bottom - 8);
            PlaceDemonTorch(left + WallThick + 2, bottom - 20);
            PlaceDemonTorch(left + w - WallThick - 3, bottom - 8);
            PlaceDemonTorch(left + w - WallThick - 3, bottom - 20);
            PlaceDemonTorch(cx - 10, bottom - 8);
            PlaceDemonTorch(cx + 10, bottom - 8);
            // Second floor
            PlaceDemonTorch(left + WallThick + 2, midFloorTop - 8);
            PlaceDemonTorch(left + WallThick + 2, midFloorTop - 18);
            PlaceDemonTorch(left + w - WallThick - 3, midFloorTop - 8);
            PlaceDemonTorch(left + w - WallThick - 3, midFloorTop - 18);
            PlaceDemonTorch(cx, midFloorTop - 8);

            // --- 14. Staircase: platforms from 1st floor up to mid-floor opening ---
            // Goes from near the right wall diagonally up-left to the opening edge.
            {
                int sX0 = cx + 6;                        // start at opening right edge
                int sY0 = midFloorTop + 3;               // just below the opening (top of 1st floor)
                int sX1 = left + w - WallThick - 2;      // near right wall
                int sY1 = bottom - 4;                    // just above obsidian floor
                int dxS = sX1 - sX0;                     // horizontal span
                int dyS = sY1 - sY0;                     // vertical span
                if (dxS > 0)
                {
                    for (int step = 0; step <= dxS; step++)
                    {
                        int sx = sX0 + step;
                        int sy = sY0 + (int)Math.Round((double)step * dyS / dxS);
                        // Clear headroom above each step
                        for (int hy = sy - 3; hy < sy; hy++)
                            WorldGen.KillTile(sx, hy, noItem: true);
                        WorldGen.PlaceTile(sx, sy, TileID.Platforms, true, false, -1, 0);
                    }
                }
            }

            // --- 15. Stone arch over the portal ---
            // Left pillar
            for (int y = portalBaseY - 5; y <= portalBaseY; y++)
                SafePlaceTile(cx - 4, y, Stone);
            // Right pillar
            for (int y = portalBaseY - 5; y <= portalBaseY; y++)
                SafePlaceTile(cx + 3, y, Stone);
            // Top beam
            for (int x = cx - 4; x <= cx + 3; x++)
                SafePlaceTile(x, portalBaseY - 6, Stone);
            // Arch curve (trim corners to simulate arch)
            WorldGen.KillTile(cx - 4, portalBaseY - 6, noItem: true);
            WorldGen.KillTile(cx + 3, portalBaseY - 6, noItem: true);
            SafePlaceTile(cx - 3, portalBaseY - 7, Stone);
            SafePlaceTile(cx + 2, portalBaseY - 7, Stone);
            SafePlaceTile(cx - 1, portalBaseY - 8, Stone);
            SafePlaceTile(cx,     portalBaseY - 8, Stone);
            // Re-clear portal interior in case arch blocks overlapped
            for (int x = cx - 3; x <= cx + 2; x++)
                for (int y = portalBaseY - 5; y <= portalBaseY; y++)
                    WorldGen.KillTile(x, y, noItem: true);

            // --- 16. Mosaic floor around portal ---
            // Alternating GrayBrick / MarbleBlock pattern on the obsidian floor rows
            for (int mx = cx - 8; mx <= cx + 7; mx++)
            {
                int mosaicY = bottom - 2;
                ushort mosaicTile = ((mx - (cx - 8)) % 2 == 0) ? TileID.GrayBrick : TileID.MarbleBlock;
                if (mx >= left + WallThick && mx < left + w - WallThick)
                {
                    WorldGen.KillTile(mx, mosaicY, noItem: true);
                    SafePlaceTile(mx, mosaicY, mosaicTile);
                }
            }

            // --- 17. Entrance arches (stone arch frame above each doorway) ---
            // Left entrance arch: top lintel + jambs
            int lEntX = left + WallThick; // first interior column after left wall
            for (int x = lEntX - 1; x <= lEntX + 1; x++)
                SafePlaceTile(x, bottom - 15, Stone);               // lintel
            SafePlaceTile(lEntX - 1, bottom - 14, Stone);           // left jamb top
            SafePlaceTile(lEntX + 1, bottom - 14, Stone);           // right jamb top

            // Right entrance arch
            int rEntX = left + w - WallThick - 1;
            for (int x = rEntX - 1; x <= rEntX + 1; x++)
                SafePlaceTile(x, bottom - 15, Stone);
            SafePlaceTile(rEntX - 1, bottom - 14, Stone);
            SafePlaceTile(rEntX + 1, bottom - 14, Stone);

            // --- 18. Decorative banners on 1st-floor walls ---
            // Left wall banners
            WorldGen.PlaceTile(left + WallThick - 1, bottom - 20, TileID.Banners, true, false, -1, 0);
            WorldGen.PlaceTile(left + WallThick - 1, bottom - 30, TileID.Banners, true, false, -1, 0);
            // Right wall banners
            WorldGen.PlaceTile(left + w - WallThick, bottom - 20, TileID.Banners, true, false, -1, 0);
            WorldGen.PlaceTile(left + w - WallThick, bottom - 30, TileID.Banners, true, false, -1, 0);
            // 2nd floor banners
            WorldGen.PlaceTile(left + WallThick - 1, midFloorTop - 12, TileID.Banners, true, false, -1, 0);
            WorldGen.PlaceTile(left + w - WallThick,  midFloorTop - 12, TileID.Banners, true, false, -1, 0);

            // --- 19. Platform shelves on 1st-floor walls ---
            // Short platform shelves mounted on the walls (for decoration / item storage feel)
            for (int x = left + WallThick; x < left + WallThick + 4; x++)
                WorldGen.PlaceTile(x, bottom - 18, TileID.Platforms, true, false, -1, 0);
            for (int x = left + w - WallThick - 4; x < left + w - WallThick; x++)
                WorldGen.PlaceTile(x, bottom - 18, TileID.Platforms, true, false, -1, 0);
            // 2nd floor shelves
            for (int x = left + WallThick; x < left + WallThick + 4; x++)
                WorldGen.PlaceTile(x, midFloorTop - 15, TileID.Platforms, true, false, -1, 0);
            for (int x = left + w - WallThick - 4; x < left + w - WallThick; x++)
                WorldGen.PlaceTile(x, midFloorTop - 15, TileID.Platforms, true, false, -1, 0);

            // --- 20. Traps at entrances (dart traps + stone pressure plates + red wire) ---
            // Left entrance
            PlaceEntryTrap(left + 1, bottom - 10, bottom - 3, firesLeft: false);
            // Right entrance
            PlaceEntryTrap(left + w - 2, bottom - 10, bottom - 3, firesLeft: true);

            // --- 21. Traps on 2nd floor ---
            PlaceFloorTrap(left + WallThick + 6,       midFloorTop - 20, midFloorTop - 1, firesRight: true);
            PlaceFloorTrap(left + w - WallThick - 7,   midFloorTop - 20, midFloorTop - 1, firesRight: false);
            // Ceiling dart traps hanging from the 2nd-floor ceiling, plates on the floor
            PlaceCeilingDart(cx - 12, top + 19, midFloorTop - 1);
            PlaceCeilingDart(cx + 12, top + 19, midFloorTop - 1);

            // --- 22. Spike traps along the staircase (hazard for intruders) ---
            {
                int sX0 = cx + 8;
                int sX1 = left + w - WallThick - 4;
                int sY0 = midFloorTop + 5;
                int sY1 = bottom - 6;
                int dxS = sX1 - sX0;
                int dyS = sY1 - sY0;
                if (dxS > 0)
                {
                    for (int step = 2; step <= dxS - 2; step += 4) // every 4th stair step
                    {
                        int sx = sX0 + step;
                        int sy = sY0 + (int)Math.Round((double)step * dyS / dxS);
                        // Place spike just below the platform
                        WorldGen.PlaceTile(sx, sy + 1, TileID.Spikes, true, true);
                        // Pressure plate ON the platform step
                        WorldGen.PlaceTile(sx, sy, TileID.PressurePlates, true, false, -1, 0);
                        // Wire: plate to spike (just one tile apart)
                        Main.tile[sx, sy].RedWire     = true;
                        Main.tile[sx, sy + 1].RedWire = true;
                    }
                }
            }
        }

        // Place a Demon Torch (style = TorchID.Demon) at a wall position.
        private static void PlaceDemonTorch(int x, int y)
        {
            if (!WorldGen.InWorld(x, y)) return;
            WorldGen.PlaceTile(x, y, TileID.Torches, true, false, -1, TorchID.Demon);
        }

        // Dart trap + pressure plate wired together at a side-entrance.
        // The dart trap is placed at (wallX, trapY) and fires horizontally.
        // The pressure plate sits on the floor at floorY.
        private static void PlaceEntryTrap(int wallX, int trapY, int floorY, bool firesLeft)
        {
            if (!WorldGen.InWorld(wallX, trapY) || !WorldGen.InWorld(wallX, floorY)) return;

            int dartStyle = firesLeft ? 1 : 0; // 0 = fires right, 1 = fires left
            WorldGen.PlaceTile(wallX, trapY, TileID.DartTrap, true, false, -1, dartStyle);
            WorldGen.PlaceTile(wallX, floorY, TileID.PressurePlates, true, false, -1, 0);

            // Vertical red wire connecting pressure plate to dart trap
            for (int wy = Math.Min(trapY, floorY); wy <= Math.Max(trapY, floorY); wy++)
                Main.tile[wallX, wy].RedWire = true;
        }

        // Dart trap on a wall inside a floor, firing horizontally.
        private static void PlaceFloorTrap(int wallX, int trapY, int plateY, bool firesRight)
        {
            if (!WorldGen.InWorld(wallX, trapY) || !WorldGen.InWorld(wallX, plateY)) return;

            int dartStyle = firesRight ? 0 : 1;
            WorldGen.PlaceTile(wallX, trapY, TileID.DartTrap, true, false, -1, dartStyle);
            WorldGen.PlaceTile(wallX, plateY, TileID.PressurePlates, true, false, -1, 0);

            for (int wy = Math.Min(trapY, plateY); wy <= Math.Max(trapY, plateY); wy++)
                Main.tile[wallX, wy].RedWire = true;
        }

        // Dart trap mounted on ceiling, aimed downward (vertical dart using style).
        private static void PlaceCeilingDart(int x, int ceilingY, int plateY)
        {
            if (!WorldGen.InWorld(x, ceilingY) || !WorldGen.InWorld(x, plateY)) return;

            WorldGen.PlaceTile(x, ceilingY, TileID.DartTrap, true, false, -1, 2); // style 2 = fires down
            WorldGen.PlaceTile(x, plateY,   TileID.PressurePlates, true, false, -1, 0);

            for (int wy = ceilingY; wy <= plateY; wy++)
                Main.tile[x, wy].RedWire = true;
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

            int[] loot = { ItemID.StoneBlock, ItemID.Wood, ItemID.Torch, ItemID.IronOre };
            chest.item[slot] = new Item();
            chest.item[slot].SetDefaults(loot[WorldGen.genRand.Next(loot.Length)]);
            chest.item[slot].stack = WorldGen.genRand.Next(10, 35);
        }
    }
}
