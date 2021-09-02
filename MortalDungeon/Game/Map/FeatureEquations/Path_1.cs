﻿using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Map.FeatureEquations
{
    public class Path_1 : FeatureEquation
    {
        PathParams PathParams;

        public Path_1(PathParams pathParams)
        {
            PathParams = pathParams;
        }

        public override void ApplyToTile(BaseTile tile)
        {
            FeaturePoint affectedPoint = new FeaturePoint(PointToMapCoords(tile.TilePoint));

            if (AffectedPoints.TryGetValue(affectedPoint, out Feature value))
            {
                switch (value)
                {
                    case Feature.StonePath:
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

        public override void GenerateFeature()
        {
            AffectedPoints.Clear();

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
                            UpdatePoint(p);
                        });
                    }



                    UpdatePoint(path[j]);
                }

                startPoint = PathParams.Stops[i];
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
            AffectedPoints.TryAdd(point, Feature.StonePath);
        }
    }


    public struct PathParams
    {
        public FeaturePoint Start;
        public List<FeaturePoint> Stops;
        public int Width;

        public PathParams(FeaturePoint start, FeaturePoint end, int width = 1)
        {
            Start = start;
            Stops = new List<FeaturePoint>() { end };

            Width = width;
        }

        public void AddStop(FeaturePoint stop)
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
        public void AddMeanderingPoints(int maxMeander, float density, int stepWidth, float meanderProportion, int seed)
        {
            float length = FeatureEquation.GetDistanceBetweenPoints(Start, Stops[^1]);

            Random rand = new Random(seed);

            int currentMeander = 0;
            bool meanderSign = false;

            FeaturePoint lastPoint = Start;
            FeaturePoint currentPoint = new FeaturePoint();

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
