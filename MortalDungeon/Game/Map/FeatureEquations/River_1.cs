using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Map.FeatureEquations
{
    internal class River_1 : FeatureEquation
    {
        PathParams RiverParams;

        internal River_1(PathParams riverParams)
        {
            RiverParams = riverParams;
        }

        internal override void ApplyToTile(BaseTile tile, bool freshGeneration = true)
        {
            FeaturePoint affectedPoint = new FeaturePoint(PointToMapCoords(tile.TilePoint));

            if (!freshGeneration)
                return;

            if (AffectedPoints.TryGetValue(affectedPoint, out int value)) 
            {
                switch (value) 
                {
                    case (int)Feature.Water_1:
                        tile.Properties.Type = TileType.Water;
                        break;
                    case (int)Feature.Water_2:
                        tile.Properties.Type = TileType.AltWater;
                        break;
                }

                tile.Properties.Classification = TileClassification.Water;
                tile.Outline = false;
                tile.NeverOutline = true;

                tile.Update();
            }
        }

        internal override void GenerateFeature()
        {
            ClearAffectedPoints();

            FeaturePoint startPoint = RiverParams.Start;

            List<FeaturePoint> ringList = new List<FeaturePoint>();

            for (int i = 0; i < RiverParams.Stops.Count; i++)
            {
                List<FeaturePoint> path = new List<FeaturePoint>();

                GetLine(startPoint, RiverParams.Stops[i], path);

                for (int j = 0; j < path.Count; j++) 
                {
                    int width = RiverParams.Width / 2;

                    for (int k = 0; k <= width; k++) 
                    {
                        ringList.Clear();
                        GetRingOfTiles(path[j], ringList, k);

                        ringList.ForEach(p =>
                        {
                            AddAffectedPoint(p, TileMap._randomNumberGen.NextDouble() > 0.3 ? (int)Feature.Water_1 : (int)Feature.Water_2);
                        });
                    }



                    AddAffectedPoint(path[j], TileMap._randomNumberGen.NextDouble() > 0.3 ? (int)Feature.Water_1 : (int)Feature.Water_2);
                }

                startPoint = RiverParams.Stops[i];
            }
        }
    }
}
