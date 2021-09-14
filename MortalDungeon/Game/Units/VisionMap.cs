using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Lighting;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Units
{
    public class VisionGenerator
    {
        private Vector2i _position = new Vector2i();
        public Vector2i Position => _position; //global tile position

        public float Radius = 6; //in tiles

        public UnitTeam Team = UnitTeam.Unknown;

        public VisionGenerator() { }

        public VisionGenerator(VisionGenerator gen)
        {
            _position = gen._position;
            Radius = gen.Radius;
            Team = gen.Team;
        }

        public void SetPosition(TilePoint point) 
        {
            _position = Map.FeatureEquation.PointToMapCoords(point);
        }

        public void SetPosition(Vector2i point)
        {
            _position = point;
        }
    }

    public static class VisionMap
    {
        public const int DIMENSIONS = 150;
        public static int[,] Vision = new int[DIMENSIONS, DIMENSIONS];

        public static Vector2[,] ObstructionMap = new Vector2[DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE, DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE];

        public static List<LightObstruction> LightObstructions;


        public static void Initialize() 
        {

        }

        public static void Clear(UnitTeam teamToUpdate) 
        {
            int bitMask = 0;

            if (teamToUpdate != UnitTeam.Unknown) 
            {
                bitMask = int.MaxValue;
                bitMask -= BitFromUnitTeam(teamToUpdate);
            }

            for (int i = 0; i < DIMENSIONS; i++) 
            {
                for (int j = 0; j < DIMENSIONS; j++)
                {
                    Vision[i, j] = Vision[i, j] & bitMask;
                }
            }
        }

        public static void ClearObstructionMap()
        {
            for (int i = 0; i < DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE; i++)
            {
                for (int j = 0; j < DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE; j++)
                {
                    ObstructionMap[i, j].X = 0;
                    ObstructionMap[i, j].Y = 0;
                }
            }
        }

        private static int BitFromUnitTeam(UnitTeam team) 
        {
            return 1 << (int)team;
        }

        private static bool GetBit(int num, int bitNumber)
        {
            return (num & (1 << bitNumber)) != 0;
        }

        public static bool InVision(int x, int y, UnitTeam team) 
        {
            return GetBit(Vision[x, y], (int)team);
        }

        private static int _obstructionsChangeToken = -1;
        public static void SetObstructions(QueuedList<LightObstruction> obstructions, Scene scene) 
        {
            if (_obstructionsChangeToken == obstructions.CHANGE_TOKEN) return;

            _obstructionsChangeToken = obstructions.CHANGE_TOKEN;

            ClearObstructionMap();

            List<LightObstruction> obstructionsCopied = new List<LightObstruction>();
            List<Task> obstructionsTasks = new List<Task>();

            for (int i = 0; i < obstructions.Count; i++)
            {
                obstructionsCopied.Add(obstructions[i]);
            }

            Vector2i zeroPoint = scene._tileMapController.GetTopLeftTilePosition();


            const int BATCH_SIZE = 100;
            for (int i = 0; i < obstructionsCopied.Count; i += BATCH_SIZE)
            {
                int val = i;

                if (val + BATCH_SIZE >= obstructionsCopied.Count)
                {
                    obstructionsTasks.Add(new Task(() => calculateObstructions(val, obstructionsCopied.Count)));
                }
                else
                {
                    //calculateVision(i, i + 20);
                    obstructionsTasks.Add(new Task(() => calculateObstructions(val, val + BATCH_SIZE)));
                }
            }

            //calculateObstructions(0, obstructionsCopied.Count);

            void calculateObstructions(int start, int end)
            {
                for (int m = start; m < end; m++)
                {
                    LightObstruction obstruction = obstructionsCopied[m];

                    Vector2 obstructorCoordinates = obstruction.Position - zeroPoint;

                    if (Math.Abs((int)obstructorCoordinates.X % 2) == 1)
                    {
                        obstructorCoordinates.Y -= 0.5f;
                    }

                    const float rows = 10;

                    //int row = (int)Math.Floor((int)obstruction.ObstructionType / rows);
                    //int column = (int)((int)obstruction.ObstructionType - row * rows);

                    if (obstruction.ObstructionType == LightObstructionType.None)
                        continue;


                    int row = (int)Math.Floor((int)LightObstructionType.Full / rows);
                    int column = (int)((int)LightObstructionType.Full - row * rows);

                    Vector2i firstTexel = new Vector2i((int)(obstructorCoordinates.X * OBSTRUCTIONS_TEXELS_PER_TILE), (int)(obstructorCoordinates.Y * OBSTRUCTIONS_TEXELS_PER_TILE));

                    firstTexel.X = Math.Clamp(firstTexel.X, 0, DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE - 1);
                    firstTexel.Y = Math.Clamp(firstTexel.Y, 0, DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE - 1);

                    Vector2i currentTexel = new Vector2i(firstTexel.X, firstTexel.Y);

                    for (int i = 0; i < OBSTRUCTIONS_TEXELS_PER_TILE; i++)
                    {
                        for (int j = 0; j < OBSTRUCTIONS_TEXELS_PER_TILE; j++)
                        {
                            currentTexel.X = firstTexel.X + i;
                            currentTexel.Y = firstTexel.Y + j;

                            currentTexel.X = Math.Clamp(currentTexel.X, 0, DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE - 1);
                            currentTexel.Y = Math.Clamp(currentTexel.Y, 0, DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE - 1);

                            //Color obstructionColor = ObstructionSheet.GetPixel(column * TEXELS_PER_TILE_IDEAL + i * TEXELS_PER_TILE_IDEAL / OBSTRUCTIONS_TEXELS_PER_TILE
                            //    , row * TEXELS_PER_TILE_IDEAL + j * TEXELS_PER_TILE_IDEAL / OBSTRUCTIONS_TEXELS_PER_TILE);


                            if (ObstructionMap[currentTexel.X, currentTexel.Y].X > 0 || ObstructionMap[currentTexel.X, currentTexel.Y].Y > 0)
                            {
                                continue;
                            }
                            //else
                            //{
                            //    ObstructionMap[currentTexel.X, currentTexel.Y].X = obstructionColor.R;
                            //    ObstructionMap[currentTexel.X, currentTexel.Y].Y = obstructionColor.A;
                            //}

                            ObstructionMap[currentTexel.X, currentTexel.Y].X = 255;
                            ObstructionMap[currentTexel.X, currentTexel.Y].Y = 255;
                        }
                    }
                }
            }

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            for (int i = 0; i < obstructionsTasks.Count; i++)
            {
                obstructionsTasks[i].Start();
            }

            for (int i = 0; i < obstructionsTasks.Count; i++)
            {
                obstructionsTasks[i].Wait();
            }

            //Console.WriteLine($"Obstructions updated in {stopwatch.ElapsedMilliseconds}ms");
        }


        const int OBSTRUCTIONS_TEXELS_PER_TILE = 2;
        const int TEXELS_PER_TILE_APPROX = 2;
        const int REQUIRED_SUCCESSES = 2;
        const int TEXELS_PER_TILE_IDEAL = 32;

        private static int _visionChangeToken = -1;
        public static void CalculateVision(QueuedList<VisionGenerator> visionGenerators, Scene scene, UnitTeam teamToUpdate = UnitTeam.Unknown) 
        {
            if (_visionChangeToken == visionGenerators.CHANGE_TOKEN) return;

            _visionChangeToken = visionGenerators.CHANGE_TOKEN;


            Clear(teamToUpdate);

            List<VisionGenerator> generatorsCopied = new List<VisionGenerator>();

            for (int i = 0; i < visionGenerators.Count; i++) 
            {
                generatorsCopied.Add(visionGenerators[i]);
            }


            Vector2i zeroPoint = scene._tileMapController.GetTopLeftTilePosition();

            int counter = 0;

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            List<Task> generatorTasks = new List<Task>();

            const int BATCH_SIZE = 100;
            for (int i = 0; i < generatorsCopied.Count; i += BATCH_SIZE) 
            {
                int val = i;

                if (val + BATCH_SIZE >= generatorsCopied.Count)
                {
                    generatorTasks.Add(new Task(() => calculateVision(val, generatorsCopied.Count)));
                }
                else 
                {
                    //calculateVision(i, i + 20);
                    generatorTasks.Add(new Task(() => calculateVision(val, val + BATCH_SIZE)));
                }
            }

            void calculateVision(int start, int end) 
            {
                for (int m = start; m < end; m++)
                //visionGenerators.ForEach(generator =>
                {
                    VisionGenerator generator = generatorsCopied[m];

                    //if (generator.Team == UnitTeam.Unknown)
                    //    return;
                    if (generator.Team == UnitTeam.Unknown || generator.Radius == 0)
                        continue;

                    if (teamToUpdate != UnitTeam.Unknown && generator.Team != teamToUpdate)
                        continue;

                    Vector2 currTile = new Vector2();
                    Vector2 currTileTexel = new Vector2();
                    Vector2 workingTile = new Vector2();
                    Vector2 workingTileTexel = new Vector2();

                    Vector2 CenterTile = generator.Position - zeroPoint;
                    Vector2 CenterTileTexels = CenterTile * TEXELS_PER_TILE_APPROX;

                    CenterTileTexels.Y += TEXELS_PER_TILE_APPROX / 2;
                    CenterTileTexels.X += TEXELS_PER_TILE_APPROX / 2;

                    if (Math.Abs((int)CenterTile.X % 2) == 1)
                    {
                        CenterTileTexels.Y -= TEXELS_PER_TILE_APPROX / 2;
                    }

                    Vector2 obstructionColor = new Vector2();

                    float alpha_falloff = 1 / (generator.Radius * TEXELS_PER_TILE_APPROX);

                    for (int i = -(int)generator.Radius; i < generator.Radius; i++)
                    {
                        for (int j = -(int)generator.Radius; j < generator.Radius; j++)
                        {
                            int successes = 0;

                            workingTile.X = CenterTile.X;
                            workingTile.Y = CenterTile.Y;

                            currTile.X = i + CenterTile.X;
                            currTile.Y = j + CenterTile.Y;

                            if (currTile.X < 0 || currTile.Y < 0 || currTile.X > 149 || currTile.Y > 149) //throw out impossible situations
                                continue;

                            if (InVision((int)currTile.X, (int)currTile.Y, generator.Team)) //don't bother calculating if we already did the work
                                continue;

                            currTileTexel.X = currTile.X * TEXELS_PER_TILE_APPROX;
                            currTileTexel.Y = currTile.Y * TEXELS_PER_TILE_APPROX;

                            if (Math.Abs((int)currTile.X % 2) == 1)
                            {
                                currTileTexel.Y -= TEXELS_PER_TILE_APPROX / 2;
                            }

                            float baseX = currTileTexel.X;
                            float baseY = currTileTexel.Y;

                            //loop through each texel
                            for (int x = 0; x < TEXELS_PER_TILE_APPROX; x++)
                            {
                                for (int y = 0; y < TEXELS_PER_TILE_APPROX; y++)
                                {
                                    currTileTexel.X = baseX + x;
                                    currTileTexel.Y = baseY + y;

                                    float alphaValue = 1;
                                    float dist = (float)MathHelper.Sqrt((CenterTileTexels.X - currTileTexel.X) * (CenterTileTexels.X - currTileTexel.X) + (CenterTileTexels.Y - currTileTexel.Y) * (CenterTileTexels.Y - currTileTexel.Y));
                                    float step_length = 1 / dist;

                                    for (int q = 0; q < dist; q++) //loop from the center texel to the 
                                    {
                                        workingTileTexel = lerp(CenterTileTexels, currTileTexel, step_length * q); //the current texel we're examining

                                        if(workingTileTexel.X < 0 || workingTileTexel.Y < 0 
                                            || workingTileTexel.X >= DIMENSIONS * TEXELS_PER_TILE_APPROX 
                                            || workingTileTexel.Y >= DIMENSIONS * TEXELS_PER_TILE_APPROX) 
                                        {
                                            continue;
                                        }

                                    

                                        obstructionColor = ObstructionMap[(int)workingTileTexel.X, (int)workingTileTexel.Y];

                                        if (obstructionColor.X > 250 && obstructionColor.Y > 250)
                                        {
                                            //alphaValue -= alpha_falloff * 10;
                                            if (_saveToBitmap) 
                                            {
                                                OperationBitmap.SetPixel((int)workingTileTexel.X, (int)workingTileTexel.Y, System.Drawing.Color.FromArgb(255, 255, 0, 0));
                                            }

                                            if ((int)currTileTexel.X == (int)workingTileTexel.X && (int)currTileTexel.Y == (int)workingTileTexel.Y) 
                                            {
                                                successes += REQUIRED_SUCCESSES;
                                                x = TEXELS_PER_TILE_APPROX;
                                                //q = (int)dist + 2;
                                            }
                                            alphaValue = 0;
                                            break;
                                        }

                                        alphaValue -= alpha_falloff;

                                        if (alphaValue <= 0)
                                        {
                                            break;
                                        }
                                    }

                                    if (alphaValue > 0)
                                    {
                                        if (_saveToBitmap && !(currTileTexel.X < 0 || currTileTexel.Y < 0
                                            || currTileTexel.X >= DIMENSIONS * TEXELS_PER_TILE_APPROX
                                            || currTileTexel.Y >= DIMENSIONS * TEXELS_PER_TILE_APPROX))
                                        {
                                            OperationBitmap.SetPixel((int)currTileTexel.X, (int)currTileTexel.Y, System.Drawing.Color.FromArgb(255, 0, 0, 255));
                                        }
                                    
                                        successes++;
                                    }
                                }
                            }
                            //if (successes > 128) //out of 256 (256 being 100% vision coverage of a tile (16x16 samples))
                            //if (successes > 32) //out of 64 (64 being 100% vision coverage of a tile (8x8 samples))
                            //if (successes >= 8) //out of 16
                            if (successes >= REQUIRED_SUCCESSES) //out of 4
                            {
                                Vision[(int)currTile.X, (int)currTile.Y] |= BitFromUnitTeam(generator.Team);
                                counter++;
                            }
                        }
                    }
                }


                //if (_saveToBitmap)
                //for (int i = 0; i < 3; i++)
                //{
                //    for (int j = 0; j < 3; j++)
                //    {
                //        currTileTexel.X = CenterTileTexels.X + i;
                //        currTileTexel.Y = CenterTileTexels.Y + j;

                //        OperationBitmap.SetPixel((int)currTileTexel.X, (int)currTileTexel.Y, Color.FromArgb(255, 0, 255, 0));
                //    }
                //}
                //});
            }

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            for (int i = 0; i < generatorTasks.Count; i++)
            {
                generatorTasks[i].Start();
            }

            for (int i = 0; i < generatorTasks.Count; i++) 
            {
                generatorTasks[i].Wait();
            }

            //Console.WriteLine($"Vision updated in {stopwatch.ElapsedMilliseconds}ms");
        }

        //public static void CalculateVision2(QueuedList<VisionGenerator> visionGenerators, Scene scene, UnitTeam teamToUpdate = UnitTeam.Unknown) 
        //{

        //    if (_visionChangeToken == visionGenerators.CHANGE_TOKEN) return;

        //    _visionChangeToken = visionGenerators.CHANGE_TOKEN;

        //    LightObstructions = scene.LightObstructions;

        //    Vector2i zeroPoint = scene._tileMapController.GetTopLeftTilePosition();
            
        //    //I might wanna try and make this work but the fact that it's a hex grid complicates everything a lot.

        //    //lock (LightObstructions)
        //    lock (visionGenerators)
        //    {
        //        foreach (var generator in visionGenerators)
        //        {
        //            List<LightObstruction> obstructions = new List<LightObstruction>();

        //            float radiusTest = generator.Radius * generator.Radius;

        //            Vector2 generatorPos = generator.Position - zeroPoint;

        //            bool genOnOddTile = generatorPos.X % 2 == 1;
        //            if (genOnOddTile) 
        //            {
        //                generatorPos.Y -= 0.5f;
        //            }

        //            foreach (var obstruction in LightObstructions)
        //            {
        //                double dist = Math.Pow(generator.Position.X - obstruction.Position.X, 2) + Math.Pow(generator.Position.Y - obstruction.Position.Y, 2);

        //                if (dist <= radiusTest)
        //                {
        //                    obstructions.Add(obstruction);
        //                }
        //            }

        //            byte[,] vision = new byte[(int)generator.Radius, (int)generator.Radius];

        //            foreach (var obstruction in obstructions)
        //            {
        //                Vector2 obstructionPos = obstruction.Position - generator.Position + new Vector2(generator.Radius, generator.Radius);

        //                if (obstruction.Position.X % 2 == 1) 
        //                {
        //                    obstructionPos.Y -= 0.5f;
        //                }



        //            }

        //        }
        //    }
        //}

        private static Vector2 lerp(Vector2 a, Vector2 b, float t)
        {
            return a + (b - a) * t;
        }


        public static bool TargetInVision(TilePoint startPoint, TilePoint endPoint, int radius, Scene scene) 
        {
            return TargetInVision(Map.FeatureEquation.PointToMapCoords(startPoint), Map.FeatureEquation.PointToMapCoords(endPoint), radius, scene);
        }
        public static bool TargetInVision(Vector2i startPoint, Vector2i endPoint, int radius, Scene scene) 
        {
            VisionGenerator generator = new VisionGenerator() { Radius = radius };
            generator.SetPosition(startPoint);

            Vector2i zeroPoint = scene._tileMapController.GetTopLeftTilePosition();

            Vector2 currTile = new Vector2();
            Vector2 currTileTexel = new Vector2();
            Vector2 workingTile = new Vector2();
            Vector2 workingTileTexel;

            Vector2 CenterTile = generator.Position - zeroPoint;
            Vector2 CenterTileTexels = CenterTile * TEXELS_PER_TILE_APPROX;

            Vector2i EndTile = endPoint - zeroPoint;

            CenterTileTexels.Y += TEXELS_PER_TILE_APPROX / 2;
            CenterTileTexels.X += TEXELS_PER_TILE_APPROX / 2;

            if (Math.Abs((int)CenterTile.X % 2) == 1)
            {
                CenterTileTexels.Y -= TEXELS_PER_TILE_APPROX / 2;
            }

            float alpha_falloff = 1 / (generator.Radius * TEXELS_PER_TILE_APPROX);

            int successes = 0;

            workingTile.X = CenterTile.X;
            workingTile.Y = CenterTile.Y;

            currTile.X = EndTile.X;
            currTile.Y = EndTile.Y;


            currTileTexel.X = currTile.X * TEXELS_PER_TILE_APPROX;
            currTileTexel.Y = currTile.Y * TEXELS_PER_TILE_APPROX;

            if (Math.Abs((int)currTile.X % 2) == 1)
            {
                currTileTexel.Y -= TEXELS_PER_TILE_APPROX / 2;
            }

            float baseX = currTileTexel.X;
            float baseY = currTileTexel.Y;

            //loop through each texel
            for (int x = 0; x < TEXELS_PER_TILE_APPROX; x++)
            {
                for (int y = 0; y < TEXELS_PER_TILE_APPROX; y++)
                {
                    currTileTexel.X = baseX + x;
                    currTileTexel.Y = baseY + y;

                    float alphaValue = 1;
                    float dist = (float)MathHelper.Sqrt((CenterTileTexels.X - currTileTexel.X) * (CenterTileTexels.X - currTileTexel.X) + (CenterTileTexels.Y - currTileTexel.Y) * (CenterTileTexels.Y - currTileTexel.Y));
                    float step_length = 1 / dist;

                    for (int q = 0; q < dist; q++) //loop from the center texel to the 
                    {
                        workingTileTexel = lerp(CenterTileTexels, currTileTexel, step_length * q); //the current texel we're examining

                        if (workingTileTexel.X < 0 || workingTileTexel.Y < 0
                            || workingTileTexel.X >= DIMENSIONS * TEXELS_PER_TILE_APPROX
                            || workingTileTexel.Y >= DIMENSIONS * TEXELS_PER_TILE_APPROX)
                        {
                            continue;
                        }

                        Vector2 obstructionColor = ObstructionMap[(int)workingTileTexel.X, (int)workingTileTexel.Y];

                        if (obstructionColor.X > 250 && obstructionColor.Y > 250)
                        {
                            alphaValue = 0;
                            break;
                        }

                        alphaValue -= alpha_falloff;

                        if (alphaValue <= 0)
                        {
                            break;
                        }
                    }

                    if (alphaValue > 0)
                    {
                        successes++;
                    }
                }
            }
            //if (successes > 128) //out of 256 (256 being 100% vision coverage of a tile (16x16 samples))
            //if (successes > 32) //out of 64 (64 being 100% vision coverage of a tile (8x8 samples))
            //if (successes >= 8) //out of 16
            if (successes >= REQUIRED_SUCCESSES) //out of 4
            {
                return true;
            }
            
            return false;
        }

        public static List<Unit> GetUnitsInRadius(Unit castingUnit, List<Unit> units, int radius, Scene scene) 
        {
            List<Unit> returnList = new List<Unit>();

            TilePoint startPoint;

            if (castingUnit.Info.TemporaryPosition != null)
            {
                startPoint = castingUnit.Info.TemporaryPosition;
            }
            else 
            {
                startPoint = castingUnit.Info.TileMapPosition.TilePoint;
            }

            for (int i = 0; i < units.Count; i++) 
            {
                if (TargetInVision(startPoint, units[i].Info.TileMapPosition.TilePoint, radius, scene)) 
                {
                    returnList.Add(units[i]);
                }
            }

            return returnList;
        }

        /// <summary>
        /// Gets the team's vision (accounting for temporary position of units) and returns it in tile map cluster coordinates [0, 149]
        /// </summary>
        public static List<Vector2i> GetTeamVision(UnitTeam team, Scene scene) 
        {
            List<Vector2i> returnList = new List<Vector2i>();

            for (int m = 0; m < scene._units.Count; m++) 
            {
                VisionGenerator generator = new VisionGenerator(scene._units[m].VisionGenerator);

                if (generator.Team != team)
                    continue;

                if (scene._units[m].Info.TemporaryPosition != null) 
                {
                    generator.SetPosition(scene._units[m].Info.TemporaryPosition);
                }

                Vector2 currTile = new Vector2();
                Vector2 currTileTexel = new Vector2();
                Vector2 workingTile = new Vector2();
                Vector2 workingTileTexel;

                Vector2i zeroPoint = scene._tileMapController.GetTopLeftTilePosition();

                Vector2 CenterTile = generator.Position - zeroPoint;
                Vector2 CenterTileTexels = CenterTile * TEXELS_PER_TILE_APPROX;

                CenterTileTexels.Y += TEXELS_PER_TILE_APPROX / 2;
                CenterTileTexels.X += TEXELS_PER_TILE_APPROX / 2;

                if (Math.Abs((int)CenterTile.X % 2) == 1)
                {
                    CenterTileTexels.Y -= TEXELS_PER_TILE_APPROX / 2;
                }

                Vector2 obstructionColor;

                float alpha_falloff = 1 / (generator.Radius * TEXELS_PER_TILE_APPROX);

                for (int i = -(int)generator.Radius; i < generator.Radius; i++)
                {
                    for (int j = -(int)generator.Radius; j < generator.Radius; j++)
                    {
                        int successes = 0;

                        workingTile.X = CenterTile.X;
                        workingTile.Y = CenterTile.Y;

                        currTile.X = i + CenterTile.X;
                        currTile.Y = j + CenterTile.Y;

                        if (currTile.X < 0 || currTile.Y < 0 || currTile.X > 149 || currTile.Y > 149) //throw out impossible situations
                            continue;

                        currTileTexel.X = currTile.X * TEXELS_PER_TILE_APPROX;
                        currTileTexel.Y = currTile.Y * TEXELS_PER_TILE_APPROX;

                        if (Math.Abs((int)currTile.X % 2) == 1)
                        {
                            currTileTexel.Y -= TEXELS_PER_TILE_APPROX / 2;
                        }

                        float baseX = currTileTexel.X;
                        float baseY = currTileTexel.Y;

                        //loop through each texel
                        for (int x = 0; x < TEXELS_PER_TILE_APPROX; x++)
                        {
                            for (int y = 0; y < TEXELS_PER_TILE_APPROX; y++)
                            {
                                currTileTexel.X = baseX + x;
                                currTileTexel.Y = baseY + y;

                                float alphaValue = 1;
                                float dist = (float)MathHelper.Sqrt((CenterTileTexels.X - currTileTexel.X) * (CenterTileTexels.X - currTileTexel.X) + (CenterTileTexels.Y - currTileTexel.Y) * (CenterTileTexels.Y - currTileTexel.Y));
                                float step_length = 1 / dist;

                                for (int q = 0; q < dist; q++) //loop from the center texel to the 
                                {
                                    workingTileTexel = lerp(CenterTileTexels, currTileTexel, step_length * q); //the current texel we're examining

                                    if (workingTileTexel.X < 0 || workingTileTexel.Y < 0
                                        || workingTileTexel.X >= DIMENSIONS * TEXELS_PER_TILE_APPROX
                                        || workingTileTexel.Y >= DIMENSIONS * TEXELS_PER_TILE_APPROX)
                                    {
                                        continue;
                                    }



                                    obstructionColor = ObstructionMap[(int)workingTileTexel.X, (int)workingTileTexel.Y];

                                    if (obstructionColor.X > 250 && obstructionColor.Y > 250)
                                    {
                                        alphaValue = 0;
                                        break;
                                    }

                                    alphaValue -= alpha_falloff;

                                    if (alphaValue <= 0)
                                    {
                                        break;
                                    }
                                }

                                if (alphaValue > 0)
                                {
                                    successes++;
                                }
                            }
                        }

                        if (successes >= REQUIRED_SUCCESSES) //out of 4
                        {
                            returnList.Add(new Vector2i((int)currTile.X, (int)currTile.Y));
                        }
                    }
                }
            }

            return returnList;
        }

        public static void SaveObstructionMap() 
        {
            Bitmap bitmap = new Bitmap(DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE, DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE);

            for (int i = 0; i < DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE; i++) 
            {
                for (int j = 0; j < DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE; j++) 
                {
                    var col = System.Drawing.Color.FromArgb(Math.Clamp((int)ObstructionMap[i, j].Y, 0, 255), Math.Clamp((int)ObstructionMap[i, j].X, 0, 255), 0, 0);

                    bitmap.SetPixel(i, j, col);
                }
            }

            bitmap.Save("C:\\Users\\tgiyb\\Pictures\\GameEngine\\ObstructionMap.bmp");
        }

        public static Bitmap OperationBitmap = new Bitmap(DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE, DIMENSIONS * OBSTRUCTIONS_TEXELS_PER_TILE);
        public static bool _saveToBitmap = false;
        public static void SaveOperationMap()
        {
            OperationBitmap.Save("C:\\Users\\tgiyb\\Pictures\\GameEngine\\OperationMap.bmp");
            _saveToBitmap = false;
        }
    }
}
