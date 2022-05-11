using Empyrean.Engine_Classes;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using Empyrean.Game.Tiles.Meshes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Empyrean.Definitions.BlendControls
{
    [Serializable]
    public class BlendPath : BlendControl
    {
        public BezierCurve Curve;
        public List<Vector2> Points = new List<Vector2>();
        public int ThicknessMin;
        public int ThicknessMax;

        /// <summary>
        /// how many pixels should be calculated at a time before filling that section with color 
        /// </summary>
        public int FillSectionLength = 100;

        /// <summary>
        /// How often a line should be sampled from the bezier curve
        /// </summary>
        public float StepResolution = 0.01f;


        public override void ApplyControl()
        {
            if (Points.Count == 0) return;

            Curve = new BezierCurve(Points);

            Stopwatch stopwatch = Stopwatch.StartNew();

            BlendPoint originPoint = BlendHelper.GetBlendPointFromFeaturePoint(Origin);

            BlendPoint globalPoint = new BlendPoint();

            Vector2 pointA;
            Vector2 pointB;

            float distance;
            float distReciprocal = 1;

            //A multiple for the distance to ensure we aren't skipping any pixels
            const float DISTANCE_FLEX = 2f;

            HashSet<TileChunk> chunksList = new HashSet<TileChunk>();
            HashSet<TileChunk> usedChunks = new HashSet<TileChunk>();

            int r = 0;
            int g = 0;
            int b = 0;

            float offset;
            PaletteLocation loc;

            HashSet<BlendPoint> currentPoints = new HashSet<BlendPoint>();
            HashSet<BlendPoint> wallSet = new HashSet<BlendPoint>();

            BlendPoint startingMin = new BlendPoint();
            BlendPoint endingMin = new BlendPoint();
            BlendPoint startingMax = new BlendPoint();
            BlendPoint endingMax = new BlendPoint();

            FeaturePoint featurePoint = TileMapHelpers.GetTopLeftFeaturePoint();
            featurePoint.X--;
            featurePoint.Y--;
            BlendPoint minLoadedPoint = BlendHelper.GetBlendPointFromFeaturePoint(featurePoint);

            featurePoint = TileMapHelpers.GetBottomRightFeaturePoint();
            featurePoint.X++;
            featurePoint.Y++;
            BlendPoint maxLoadedPoint = BlendHelper.GetBlendPointFromFeaturePoint(featurePoint);


            void fillMaxThicknessWalls(float startVal, float endVal)
            {
                Curve.Parallel = ThicknessMax;
                bool first = true;

                for (float i = startVal; i < endVal; i += StepResolution)
                {
                    pointA = Curve.CalculatePoint(i - StepResolution);
                    pointB = Curve.CalculatePoint(i);

                    if (first)
                    {
                        startingMax.X = (int)Math.Round(pointA.X);
                        startingMax.Y = (int)Math.Round(pointA.Y);
                        first = false;
                    }

                    distance = (float)Math.Sqrt((pointA.X - pointB.X) * (pointA.X - pointB.X) + (pointA.Y - pointB.Y) * (pointA.Y - pointB.Y)) * DISTANCE_FLEX;
                    distReciprocal = 1 / distance;

                    for (int j = 0; j <= distance; j++)
                    {
                        globalPoint.X = (int)Math.Round(MathHelper.Lerp(pointA.X, pointB.X, distReciprocal * j) + originPoint.X);
                        globalPoint.Y = (int)Math.Round(MathHelper.Lerp(pointA.Y, pointB.Y, distReciprocal * j) + originPoint.Y);

                        wallSet.Add(globalPoint);
                    }
                }

                endingMax.X = globalPoint.X - originPoint.X;
                endingMax.Y = globalPoint.Y - originPoint.Y;
            }



            float beginningResolution = 0;

            float i = StepResolution;

            while (i <= 1)
            {
                beginningResolution = i;

                Curve.Parallel = ThicknessMin;

                bool first = true;

                for (i = beginningResolution; i < 1; i += StepResolution)
                {
                    if (wallSet.Count > FillSectionLength)
                    {
                        goto CALCULATE_FILL_SECTION;
                    }

                    pointA = Curve.CalculatePoint(i - StepResolution);
                    pointB = Curve.CalculatePoint(i);

                    if (first)
                    {
                        startingMin.X = (int)Math.Round(pointA.X);
                        startingMin.Y = (int)Math.Round(pointA.Y);
                        first = false;
                    }

                    distance = (float)Math.Sqrt((pointA.X - pointB.X) * (pointA.X - pointB.X) + (pointA.Y - pointB.Y) * (pointA.Y - pointB.Y)) * DISTANCE_FLEX;
                    distReciprocal = 1 / distance;

                    for (int j = 0; j <= distance; j++)
                    {
                        globalPoint.X = (int)Math.Round(MathHelper.Lerp(pointA.X, pointB.X, distReciprocal * j) + originPoint.X);
                        globalPoint.Y = (int)Math.Round(MathHelper.Lerp(pointA.Y, pointB.Y, distReciprocal * j) + originPoint.Y);

                        wallSet.Add(globalPoint);
                    }
                }

                CALCULATE_FILL_SECTION:
                endingMin.X = globalPoint.X - originPoint.X;
                endingMin.Y = globalPoint.Y - originPoint.Y;

                fillMaxThicknessWalls(beginningResolution, i);

                if (!((GMath.InsideBounds(endingMax.X + originPoint.X, minLoadedPoint.X, maxLoadedPoint.X) ||
                    GMath.InsideBounds(endingMin.X + originPoint.X, minLoadedPoint.X, maxLoadedPoint.X) ||
                    GMath.InsideBounds(startingMin.X + originPoint.X, minLoadedPoint.X, maxLoadedPoint.X) ||
                    GMath.InsideBounds(startingMax.X + originPoint.X, minLoadedPoint.X, maxLoadedPoint.X)) &&
                    (GMath.InsideBounds(endingMax.Y + originPoint.Y, minLoadedPoint.Y, maxLoadedPoint.Y) ||
                    GMath.InsideBounds(endingMin.Y + originPoint.Y, minLoadedPoint.Y, maxLoadedPoint.Y) ||
                    GMath.InsideBounds(startingMin.Y + originPoint.Y, minLoadedPoint.Y, maxLoadedPoint.Y) ||
                    GMath.InsideBounds(startingMax.Y + originPoint.Y, minLoadedPoint.Y, maxLoadedPoint.Y)
                    )))
                {
                    wallSet.Clear();
                    continue;
                }

                BlendPoint seedPoint = new BlendPoint((int)((startingMin.X + startingMax.X) / 2 + (endingMax.X - startingMin.X) * distReciprocal * 10),
                    (startingMin.Y + startingMax.Y) / 2);

                seedPoint.X += originPoint.X;
                seedPoint.Y += originPoint.Y;

                wallSet.Add(seedPoint);

                #region start of section cap wall
                distance = (float)Math.Sqrt((startingMin.X - startingMax.X) * (startingMin.X - startingMax.X) +
                    (startingMin.Y - startingMax.Y) * (startingMin.Y - startingMax.Y)) * DISTANCE_FLEX;
                distReciprocal = 1 / distance;
                for (int j = 0; j <= distance; j++)
                {
                    globalPoint.X = (int)Math.Round(MathHelper.Lerp(startingMin.X, startingMax.X, distReciprocal * j) + originPoint.X);
                    globalPoint.Y = (int)Math.Round(MathHelper.Lerp(startingMin.Y, startingMax.Y, distReciprocal * j) + originPoint.Y);

                    wallSet.Add(globalPoint);
                }
                #endregion

                #region end of section cap wall
                distance = (float)Math.Sqrt((endingMin.X - endingMax.X) * (endingMin.X - endingMax.X) +
                    (endingMin.Y - endingMax.Y) * (endingMin.Y - endingMax.Y)) * DISTANCE_FLEX;
                distReciprocal = 1 / distance;
                for (int j = 0; j <= distance; j++)
                {

                    globalPoint.X = (int)Math.Round(MathHelper.Lerp(endingMin.X, endingMax.X, distReciprocal * j) + originPoint.X);
                    globalPoint.Y = (int)Math.Round(MathHelper.Lerp(endingMin.Y, endingMax.Y, distReciprocal * j) + originPoint.Y);

                    wallSet.Add(globalPoint);
                }
                #endregion


                foreach (var point in wallSet)
                {
                    BlendHelper.GetChunksFromBlendPoint(point, in chunksList);

                    foreach (var foundChunk in chunksList)
                    {
                        usedChunks.Add(foundChunk);

                        BlendPoint newPoint = BlendHelper.ConvertGlobalToLocalBlendPoint(point, foundChunk);

                        if (newPoint.IsValidBoundsOnly())
                        {
                            #region Color mapping from chunk
                            r = 0;
                            g = 0;
                            b = 0;

                            if (Red != TileType.None)
                            {
                                loc = foundChunk.BlendMap.AddOrGetTypePaletteLocation(Red);

                                SetColorByLocation(ref r, ref g, ref b, 255, loc);
                            }

                            #endregion

                            Color col = Color.FromArgb(0, r, g, b);


                            foundChunk.BlendMap.DirectBitmap.SetPixel(newPoint.X, newPoint.Y, col);
                        }
                    }

                    chunksList.Clear();
                }


                currentPoints.Clear();
                if(BlendHelper.FloodFill(wallSet, ref currentPoints, seedPoint))
                {
                    //Color dirt = Color.FromArgb(255, 0, 0, 255);

                    foreach (var point in currentPoints)
                    {
                        BlendHelper.GetChunksFromBlendPoint(point, in chunksList);

                        foreach (var foundChunk in chunksList)
                        {
                            usedChunks.Add(foundChunk);

                            BlendPoint newPoint = BlendHelper.ConvertGlobalToLocalBlendPoint(point, foundChunk);

                            if (newPoint.IsValidBoundsOnly())
                            {
                                #region Color mapping from chunk
                                r = 0;
                                g = 0;
                                b = 0;

                                if (Red != TileType.None)
                                {
                                    loc = foundChunk.BlendMap.AddOrGetTypePaletteLocation(Red);

                                    SetColorByLocation(ref r, ref g, ref b, 255, loc);
                                }

                                #endregion

                                Color col = Color.FromArgb(0, r, g, b);

                                foundChunk.BlendMap.DirectBitmap.SetPixel(newPoint.X, newPoint.Y, col);
                            }
                        }

                        chunksList.Clear();
                    }
                }

                wallSet.Clear();
            }


            foreach (var chunk in usedChunks)
            {
                chunk.BlendMap.UpdateTexture();
            }

            Console.WriteLine($"Control applied in {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
