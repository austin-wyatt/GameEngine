using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Map.FeatureEquations
{
    internal class Path_1 : FeatureEquation
    {
        PathParams PathParams;

        internal Path_1(PathParams pathParams)
        {
            PathParams = pathParams;
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
                    case (int)Feature.StonePath:
                        tile.Properties.Type = TileType.Stone_1 + TileMap._randomNumberGen.Next() % 3;
                        break;
                }

                if (tile.Properties.Classification == TileClassification.Water) 
                {
                    tile.Properties.Type = TileType.WoodPlank;
                }

                tile.Properties.Classification = TileClassification.Ground;
                tile.Properties.MovementCost = 0.5f;

                tile.Outline = true;
                tile.NeverOutline = false;

                tile.Update();
            }
        }

        internal override void GenerateFeature()
        {
            ClearAffectedPoints();

            FeaturePoint startPoint = PathParams.Start;

            List<FeaturePoint> ringList = new List<FeaturePoint>();

            for (int i = 0; i < PathParams.Stops.Count; i++)
            {
                List<FeaturePoint> path = new List<FeaturePoint>();

                GetLine(startPoint, PathParams.Stops[i], path);

                for (int j = 0; j < path.Count; j++)
                {
                    int width = PathParams.Width / 2;

                    for (int k = 0; k <= width; k++)
                    {
                        ringList.Clear();
                        GetRingOfTiles(path[j], ringList, k);

                        ringList.ForEach(p =>
                        {
                            AddAffectedPoint(p, (int)Feature.StonePath);
                        });
                    }



                    AddAffectedPoint(path[j], (int)Feature.StonePath);
                }

                startPoint = PathParams.Stops[i];
            }
        }
    }


    internal struct PathParams
    {
        internal FeaturePoint Start;
        internal List<FeaturePoint> Stops;
        internal int Width;

        internal PathParams(FeaturePoint start, FeaturePoint end, int width = 1)
        {
            Start = start;
            Stops = new List<FeaturePoint>() { end };

            Width = width;
        }

        internal void AddStop(FeaturePoint stop)
        {
            Stops.Insert(Stops.Count - 1, stop);
        }

        /// <summary>
        /// Adds stops to the PathParams that "wiggle" or "meander" similar to how a river or road might.
        /// </summary>
        /// <param name="maxMeander">The maximum deviation (in tiles) from the straight path that is allowed.</param>
        /// <param name="density">A number [0:1] that determines how often a new stop will be created.</param>
        /// <param name="stepWidth">The maximum distance that a single meander can move.</param>
        /// <param name="seed">The number that will be used to seed the random number generator.</param>
        /// <param name="meanderProportion">A number [0:1] that represents the proportion of the meander that will apply to the X (0) or Y(1) value</param>
        internal void AddMeanderingPoints(int maxMeander, float density, int stepWidth, float meanderProportion, int seed)
        {
            float length = FeatureEquation.GetDistanceBetweenPoints(Start, Stops[^1]);

            Random rand = new Random(seed);

            int currentMeander = 0;
            bool meanderSign = false;

            FeaturePoint lastPoint = Start;
            FeaturePoint currentPoint = new FeaturePoint();

            //TODO, add support for multiple end points when calculating meandering points

            for (float i = 0; i < length; i++) 
            {
                if (rand.NextDouble() < density) 
                {
                    currentPoint.X = (int)Math.Round(CubeMethods.Lerp(Start.X, Stops[^1].X, i / length));
                    currentPoint.Y = (int)Math.Round(CubeMethods.Lerp(Start.Y, Stops[^1].Y, i / length));

                    if (currentMeander >= maxMeander)
                    {
                        meanderSign = false;
                    }
                    else if (currentMeander <= -maxMeander)
                    {
                        meanderSign = true;
                    }
                    else if (Math.Abs(currentMeander) > length - i) 
                    {
                        meanderSign = Math.Sign(currentMeander) == -1; //if we are negative then we want to move positive until we hit the end point and vice versa.
                    }
                    else
                    {
                        meanderSign = rand.Next() % 2 == 0;
                    }

                    currentMeander += meanderSign ? 1 : -1;

                    int movement = (meanderSign ? 1 : -1) * stepWidth;

                    currentPoint.X += (int)(movement * (1 - meanderProportion));
                    currentPoint.Y += (int)(movement * meanderProportion * -1);


                    AddStop(new FeaturePoint(currentPoint));

                    lastPoint = currentPoint;
                }
            }
        }
    }
}
