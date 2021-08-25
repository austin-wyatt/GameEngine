using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Map.FeatureEquations
{
    public class Forest_1 : FeatureEquation
    {
        ForestParams ForestParams;
        Random NumberGen;


        public Forest_1(ForestParams forestParams)
        {
            ForestParams = forestParams;
            NumberGen = new Random(HashCoordinates(forestParams.Origin.X, forestParams.Origin.Y));
        }

        public override void ApplyToTile(BaseTile tile)
        {
            FeaturePoint affectedPoint = new FeaturePoint(PointToMapCoords(tile.TilePoint));

            if (AffectedPoints.TryGetValue(affectedPoint, out Feature value))
            {
                switch (value)
                {
                    case Feature.Tree_1:
                        if ((tile.Properties.Type == TileType.Grass || tile.Properties.Type == TileType.Grass_2) && tile.Structure == null) 
                        {
                            tile.AddStructure(new Tree(tile.TileMap, tile, 0));
                        }
                        break;
                    case Feature.Tree_2:
                        if ((tile.Properties.Type == TileType.Grass || tile.Properties.Type == TileType.Grass_2) && tile.Structure == null)
                        {
                            tile.AddStructure(new Tree(tile.TileMap, tile, 1));
                        }
                        break;
                }

                tile.Update();
            }
        }

        public override void GenerateFeature()
        {
            AffectedPoints.Clear();

            FeaturePoint startPoint = ForestParams.Origin;

            List<FeaturePoint> path = new List<FeaturePoint>();

            UpdatePoint(startPoint);

            for (int i = 0; i < ForestParams.Radius; i++)
            {
                path.Clear();
                GetRingOfTiles(startPoint, path, i);

                for (int j = 0; j < path.Count; j++) 
                {
                    UpdatePoint(path[j]);
                }
            }
        }

        public override bool AffectsMap(TileMap map)
        {
            return true;
        }


        public override void ApplyToMap(TileMap map)
        {
            map.Tiles.ForEach(t =>
            {
                ApplyToTile(t);
            });
        }

        internal override void UpdatePoint(FeaturePoint point)
        {
            if (NumberGen.NextDouble() > 1 - ForestParams.Density) 
            {
                AffectedPoints.TryAdd(point, NumberGen.NextDouble() > 0.5 ? Feature.Tree_1 : Feature.Tree_2);
            }
        }
    }


    public struct ForestParams
    {
        public FeaturePoint Origin;
        public int Radius;
        public double Density;

        public ForestParams(FeaturePoint origin, int radius = 1, double density = 0.7)
        {
            Origin = origin;

            Radius = radius;
            Density = density;
        }
    }
}
