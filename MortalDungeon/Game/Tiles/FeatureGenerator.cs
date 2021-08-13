using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Tiles
{
    public static class FeatureGenerator
    {

        public static void GenerateRiver(TilePoint origin, int width, int length)
        {
            if (!origin.IsValidTile())
                return;

            TileMap map = origin.ParentTileMap;

            int xPos = origin.X;
            int yPos = origin.Y;

            int wiggle = 0;

            BaseTile tile;
            for (int i = xPos; i < length; i++)
            {
                double num = TileMap._randomNumberGen.NextDouble();
                wiggle += num < 0.3 ? num < 0.10 ? 1 : -1 : 0;

                for (int j = 0; j < width; j++)
                {
                    if (map.IsValidTile(i, j + yPos + wiggle))
                    {
                        tile = map[i, j + yPos + wiggle];

                        tile.Properties.Type = TileType.Water;
                        tile.Properties.Classification = TileClassification.Water;
                        tile.Outline = false;
                        tile.NeverOutline = true;

                        tile.Update();
                    }

                }

            }
        }
    }
}
