using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using danikherington.Tiles;

namespace danikherington
{
    public class MasterAltarGen : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int index = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
            if (index != -1)
            {
                tasks.Insert(index + 1, new PassLegacy("Master Fortress", (progress, config) => {
                    progress.Message = "Building Fortresses...";

                    for (int k = 0; k < 150; k++)
                    {
                        int x = WorldGen.genRand.Next(300, Main.maxTilesX - 300);
                        int y = WorldGen.genRand.Next((int)Main.worldSurface + 200, Main.maxTilesY - 500);

                        if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileID.Stone)
                        {

                            ushort brick = (ushort)TileID.BlueDungeonBrick;
                            ushort wall = (ushort)WallID.BlueDungeonTileUnsafe;

                            // 1. ВЫРЕЗАЕМ ЗАЛ (делаем чуть шире, 26x16)
                            for (int yy = y - 15; yy <= y; yy++)
                            {
                                for (int xx = x - 13; xx <= x + 13; xx++)
                                {
                                    WorldGen.KillTile(xx, yy, noItem: true);
                                    WorldGen.EmptyLiquid(xx, yy); // Убираем воду/лаву
                                    WorldGen.PlaceWall(xx, yy, (int)wall);
                                }
                            }

                            // 2. СТРОИМ РАМУ (пол, потолок, стены)
                            for (int yy = y - 15; yy <= y; yy++)
                            {
                                for (int xx = x - 13; xx <= x + 13; xx++)
                                {
                                    if (yy == y || yy == y - 15 || xx == x - 13 || xx == x + 13)
                                    {
                                        // Оставляем ворота
                                        if (!((xx == x - 13 || xx == x + 13) && yy > y - 7))
                                        {
                                            WorldGen.PlaceTile(xx, yy, brick, true, true);
                                        }
                                    }
                                }
                            }

                            // 3. ПОДГОТОВКА ПЯТАЧКА ПОД ПОРТАЛ (важно!)
                            // Очищаем фоновые стены в зоне 6x6, чтобы они не мешали установке
                            for (int yy = y - 6; yy < y; yy++)
                            {
                                for (int xx = x - 3; xx <= x + 2; xx++)
                                {
                                    WorldGen.KillWall(xx, yy);
                                }
                            }

                            // 4. УСТАНОВКА (success - bool)
                            // Для 6x6 с Origin(3,5) координаты x, y-1 должны сработать идеально
                            bool success = WorldGen.PlaceObject(x, y - 1, ModContent.TileType<MasterPortal>());

                            if (success)
                            {
                                WorldGen.PlaceTile(x, y - 12, (ushort)TileID.Torches, true);
                                WorldGen.PlacePot(x - 5, y - 1, 0);
                                WorldGen.PlacePot(x + 5, y - 1, 0);
                            }
                        }
                    }
                }));
            }
        }
    }
}