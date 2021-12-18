using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Map.FeatureEquations
{
    internal class Forest_1 : FeatureEquation
    {
        ForestParams ForestParams;
        Random NumberGen;


        internal Forest_1(ForestParams forestParams)
        {
            ForestParams = forestParams;
            NumberGen = new Random(HashCoordinates(forestParams.Origin.X, forestParams.Origin.Y));
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
                    case (int)Feature.Tree_1:
                        if ((tile.Properties.Type == TileType.Grass || tile.Properties.Type == TileType.Grass_2) && tile.Structure == null) 
                        {
                            new Tree(tile.TileMap, tile, 0);
                        }
                        break;
                    case (int)Feature.Tree_2:
                        if ((tile.Properties.Type == TileType.Grass || tile.Properties.Type == TileType.Grass_2) && tile.Structure == null)
                        {
                            new Tree(tile.TileMap, tile, 1);
                        }
                        break;
                }

                tile.Update();
            }
        }

        internal override void GenerateFeature()
        {
            ClearAffectedPoints();

            FeaturePoint startPoint = ForestParams.Origin;

            List<FeaturePoint> path = new List<FeaturePoint>();

            AddAffectedPoint(startPoint, 0);

            for (int i = 0; i < ForestParams.Radius; i++)
            {
                path.Clear();
                GetRingOfTiles(startPoint, path, i);

                for (int j = 0; j < path.Count; j++) 
                {
                    if (NumberGen.NextDouble() > 1 - ForestParams.Density)
                    {
                        AddAffectedPoint(path[j], NumberGen.NextDouble() > 0.5 ? (int)Feature.Tree_1 : (int)Feature.Tree_2);
                    }
                }
            }
        }

    }


    internal struct ForestParams
    {
        internal FeaturePoint Origin;
        internal int Radius;
        internal double Density;

        internal ForestParams(FeaturePoint origin, int radius = 1, double density = 0.7)
        {
            Origin = origin;

            Radius = radius;
            Density = density;
        }
    }
}
