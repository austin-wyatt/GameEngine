using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using Empyrean.Game.Tiles.Meshes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Empyrean.Engine_Classes.MiscOperations
{
    public static class CubeMethods
    {
        public static Dictionary<Direction, Vector3i> CubeDirections = new Dictionary<Direction, Vector3i>
        {
            { Direction.SouthWest, new Vector3i(-1, 0, 1) },
            { Direction.South, new Vector3i(0, -1, 1) },
            { Direction.SouthEast, new Vector3i(1, -1, 0) },
            { Direction.NorthEast, new Vector3i(1, 0, -1) },
            { Direction.North, new Vector3i(0, 1, -1) },
            { Direction.NorthWest, new Vector3i(-1, 1, 0) },
            { Direction.None, new Vector3i(0, 0, 0) },
        };

        public static Dictionary<Vector3i, Direction> CubeDirectionsInverted = new Dictionary<Vector3i, Direction>
        {
            { new Vector3i(-1, 0, 1), Direction.SouthWest },
            { new Vector3i(0, -1, 1), Direction.South },
            { new Vector3i(1, -1, 0), Direction.SouthEast },
            { new Vector3i(1, 0, -1), Direction.NorthEast },
            { new Vector3i(0, 1, -1), Direction.North },
            { new Vector3i(-1, 1, 0), Direction.NorthWest },
            { new Vector3i(0, 0, 0), Direction.None },
        };

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        public static Vector3 CubeLerp(ref Vector3 start, ref Vector3 end, float t)
        {
            return new Vector3(Lerp(start.X, end.X, t), Lerp(start.Y, end.Y, t), Lerp(start.Z, end.Z, t));
        }


        public static Vector3i CubeRound(ref Vector3 cube)
        {
            //float rx = (float)Math.Round(cube.X, MidpointRounding.ToZero);
            //float ry = (float)Math.Round(cube.Y, MidpointRounding.ToZero);
            //float rz = (float)Math.Round(cube.Z, MidpointRounding.ToZero);

            float rx = (float)Math.Round(cube.X, MidpointRounding.AwayFromZero);
            float ry = (float)Math.Round(cube.Y, MidpointRounding.AwayFromZero);
            float rz = (float)Math.Round(cube.Z, MidpointRounding.AwayFromZero);

            float x_diff = Math.Abs(rx - cube.X);
            float y_diff = Math.Abs(ry - cube.Y);
            float z_diff = Math.Abs(rz - cube.Z);

            if (x_diff > y_diff && x_diff > z_diff)
            {
                rx = -ry - rz;
            }
            else if (y_diff >= z_diff)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            //return new Vector3i((int)rx, (int)ry, (int)rz);
            return new Vector3i((int)Math.Floor(rx), (int)Math.Floor(ry), (int)Math.Floor(rz));
        }
        public static Vector3i CubeNeighbor(ref Vector3i cube, Direction direction)
        {
            return cube + CubeDirections[direction];
        }

        public static void CubeNeighborInPlace(ref Vector3i cube, Direction direction)
        {
            var dir = CubeDirections[direction];

            cube.X += dir.X;
            cube.Y += dir.Y;
            cube.Z += dir.Z;
        }

        public static Vector3i OffsetToCube(FeaturePoint offset)
        {
            Vector3i cubeCoord = new Vector3i
            {
                X = offset.X,
                Z = offset.Y - (offset.X + (offset.X & 1)) / 2
            };
            cubeCoord.Y = -cubeCoord.X - cubeCoord.Z;

            return cubeCoord;
        }

        public static void OffsetToCube(FeaturePoint offset, ref Vector3i cubeCoord)
        {
            cubeCoord.X = offset.X;
            cubeCoord.Z = offset.Y - (offset.X + (offset.X & 1)) / 2;
            cubeCoord.Y = -cubeCoord.X - cubeCoord.Z;
        }

        public static int GetDistanceBetweenPoints(Vector3i a, Vector3i b)
        {
            return (int)((Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) * 0.5);
        }

        public static int GetDistanceBetweenPoints(Tile tileA, Tile tileB)
        {
            Vector3i a = OffsetToCube(tileA.ToFeaturePoint());
            Vector3i b = OffsetToCube(tileB.ToFeaturePoint());

            return (int)((Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) * 0.5);
        }

        public static Vector2i CubeToOffset(Vector3i cube)
        {
            Vector2i offsetCoord = new Vector2i
            {
                X = cube.X,
                Y = (int)(cube.Z + (cube.X + (cube.X & 1)) * 0.5)
            };

            return offsetCoord;
        }

        public static void CubeToOffset(Vector3i cube, ref Vector2i offsetCoord)
        {
            offsetCoord.X = cube.X;
            offsetCoord.Y = cube.Z + (cube.X + (cube.X & 1)) / 2;
        }

        public static FeaturePoint CubeToFeaturePoint(Vector3i cube)
        {
            FeaturePoint offsetCoord = new FeaturePoint(cube.X, cube.Z + (cube.X + (cube.X & 1)) / 2);

            return offsetCoord;
        }

        public static void CubeToFeaturePoint(Vector3i cube, ref FeaturePoint offsetCoord)
        {
            offsetCoord.X = cube.X;
            offsetCoord.Y = cube.Z + (cube.X + (cube.X & 1)) / 2;
        }

        public static Vector3i OffsetToCube(Vector2i offset)
        {
            Vector3i cubeCoord = new Vector3i
            {
                X = offset.X,
                Z = offset.Y - (offset.X + (offset.X & 1)) / 2
            };
            cubeCoord.Y = -cubeCoord.X - cubeCoord.Z;

            return cubeCoord;
        }

        public static void OffsetToCube(Vector2i offset, ref Vector3i cubeCoord)
        {
            cubeCoord.X = offset.X;
            cubeCoord.Z = offset.Y - (offset.X + (offset.X & 1)) / 2;
            cubeCoord.Y = -cubeCoord.X - cubeCoord.Z;
        }

        public static Vector3i OffsetToCube(TilePoint offset)
        {
            return OffsetToCube(offset.ToFeaturePoint());
        }

        public static Vector3i RotateCube(Vector3i cube, int rotations) 
        {
            Vector3i rotatedCube = cube;

            Vector3i temp = new Vector3i();

            for (int i = 0; i < rotations; i++) 
            {
                temp.X = rotatedCube.X;
                temp.Y = rotatedCube.Y;
                temp.Z = rotatedCube.Z;

                rotatedCube.X = -temp.Y;
                rotatedCube.Y = -temp.Z;
                rotatedCube.Z = -temp.X;
            }

            return rotatedCube;
        }


        private const float SQRT_3_OVER_3 = 0.57736f;
        public static Vector3i PixelToCube(Vector2 point)
        {
            float q = 0.6667f * point.X * 4;
            float r = (-0.3333f * point.X + SQRT_3_OVER_3 * point.Y) * 4;

            Vector3 cube = new Vector3(q, -q - r, r);

            return CubeRound(ref cube);
        }

        public static Vector3i PixelToCube(ref Vector2 point)
        {
            float q = 0.6667f * point.X * 4;
            float r = (-0.3333f * point.X + SQRT_3_OVER_3 * point.Y) * 4;

            Vector3 cube = new Vector3(q, -q - r, r);

            return CubeRound(ref cube);
        }

        public static Vector3i GetCubeInDirection(float direction, float magnitude)
        {
            Vector2 point = new Vector2();

            point.X = magnitude * (float)MathHelper.Cos(direction) * MeshTile.TILE_WIDTH;
            point.Y = magnitude * (float)MathHelper.Sin(direction) * MeshTile.TILE_HEIGHT;

            return PixelToCube(ref point);
        }

        public static bool FloodFill(HashSet<Cube> walls, Cube anchor, in HashSet<Cube> filledPoints, int maxTiles)
        {
            Cube currentPoint = new Cube(anchor.Point);

            HashSet<Cube> neighbors = new HashSet<Cube>();

            neighbors.Add(currentPoint);

            Vector3i temp = new Vector3i();
            Cube cubeTemp = new Cube();

            int neighborCount;

            while(neighbors.Count > 0 && filledPoints.Count < maxTiles)
            {
                currentPoint = neighbors.First();

                for (int i = 0; i < 6; i++)
                {
                    temp.X = currentPoint.Point.X;
                    temp.Y = currentPoint.Point.Y;
                    temp.Z = currentPoint.Point.Z;

                    CubeNeighborInPlace(ref temp, (Direction)i);
                    cubeTemp.Point = temp;

                    if (!walls.Contains(cubeTemp) && !neighbors.Contains(cubeTemp) && !filledPoints.Contains(cubeTemp))
                    {
                        neighbors.Add(new Cube(temp));
                    }
                }

                filledPoints.Add(currentPoint);
                neighbors.Remove(currentPoint);
            }

            return neighbors.Count == 0;
        }

        

        private const float LINE_DISTANCE_FLEX = 1.5f;
        public static void GetLineLerp(Vector3i startPoint, Vector3i endPoint, in ICollection<Vector3i> outputList)
        {
            //int N = (int)(GetDistanceBetweenPoints(ref startPoint, ref endPoint) * LINE_DISTANCE_FLEX);
            int N = GetDistanceBetweenPoints(startPoint, endPoint);
            float n = 1f / N;

            Vector3 currentCube;
            Vector3i roundedCube;

            Vector3 startVec3 = new Vector3(startPoint);
            Vector3 endVec3 = new Vector3(endPoint);

            for (int i = 0; i <= N; i++)
            {
                currentCube = CubeLerp(ref startVec3, ref endVec3, n * i);
                roundedCube = CubeRound(ref currentCube);

                outputList.Add(roundedCube);
            }
        }

        public static void GetLineLerp(Vector3i startPoint, Vector3i endPoint, in ICollection<Cube> outputList)
        {
            //int N = (int)(GetDistanceBetweenPoints(ref startPoint, ref endPoint) * LINE_DISTANCE_FLEX);
            int N = GetDistanceBetweenPoints(startPoint, endPoint);
            float n = 1f / N;

            Vector3 currentCube;
            Vector3i roundedCube;

            Vector3 startVec3 = new Vector3(startPoint);
            Vector3 endVec3 = new Vector3(endPoint);

            for (int i = 0; i <= N; i++)
            {
                currentCube = CubeLerp(ref startVec3, ref endVec3, n * i);
                roundedCube = CubeRound(ref currentCube);

                outputList.Add(new Cube(roundedCube));
            }
        }

        private static ObjectPool<HashSet<CubeWithParent>> _cubeWParentSetPool = new ObjectPool<HashSet<CubeWithParent>>();
        private static ObjectPool<HashSet<Vector3i>> _cubeSetPool = new ObjectPool<HashSet<Vector3i>>();
        public static void GetLineBetweenPoints(Vector3i start, Vector3i destination, in List<Vector3i> line)
        {
            HashSet<CubeWithParent> tileList = _cubeWParentSetPool.GetObject();

            HashSet<Vector3i> visitedTiles = _cubeSetPool.GetObject();
            HashSet<Vector3i> newNeighbors = _cubeSetPool.GetObject();

            int maximumDepth = 1000;

            //FeaturePoint placeholderPoint = FeaturePoint.FeaturePointPool.GetObject();
            //FeaturePoint currentFeaturePoint = FeaturePoint.FeaturePointPool.GetObject();

            CubeWithParent currentTile;

            Vector3i placeholder = new Vector3i();

            try
            {
                if (maximumDepth <= 0)
                    return;


                if (GetDistanceBetweenPoints(start, destination) > maximumDepth * 1.5f)
                {
                    return;
                }

                currentTile = CubeWithParent.Pool.GetObject();
                currentTile.Initialize(start);
                currentTile.PathCost = GetDistanceBetweenPoints(start, start);
                currentTile.DistanceToEnd = GetDistanceBetweenPoints(start, destination);

                visitedTiles.Add(start);

                tileList.Add(currentTile);



                float tempMinDepth;
                float currMinDepth;

                bool skipNeighbor;
                bool unitOnSpace;
                bool immunityExists;
                float pathCost;
                float distanceToEnd;
                bool tileChanged;
                Direction dir;

                while (true)
                {
                    currentTile.Visited = true;

                    if (currentTile.Point == destination)
                    {
                        #region pathing successful
                        while (currentTile.Parent != null)
                        {
                            line.Add(currentTile.Point);
                            currentTile = currentTile.Parent;
                        }

                        line.Add(currentTile.Point);
                        line.Reverse();
                        return;
                        #endregion
                    }

                    newNeighbors.Clear();

                    for (int i = 0; i < 6; i++)
                    {
                        dir = (Direction)i;

                        newNeighbors.Add(CubeNeighbor(ref currentTile.Point, dir));
                    }

                    foreach (var neighbor in newNeighbors)
                    {
                        if (visitedTiles.Contains(neighbor))
                        {
                            continue;
                        }
                        else
                        {
                            placeholder.X = neighbor.X;
                            placeholder.Y = neighbor.Y;
                            placeholder.Z = neighbor.Z;

                            pathCost = currentTile.PathCost + 1;
                            distanceToEnd = GetDistanceBetweenPoints(placeholder, destination);

                            if (pathCost + distanceToEnd <= maximumDepth)
                            {
                                CubeWithParent tile = CubeWithParent.Pool.GetObject();
                                tile.Initialize(neighbor, currentTile);
                                tile.PathCost = pathCost;
                                tile.DistanceToEnd = distanceToEnd;

                                tileList.Add(tile);

                                if (neighbor == destination)
                                {
                                    currentTile = tile;
                                    break;
                                }
                            }

                            visitedTiles.Add(neighbor);
                        }
                    }

                    if (currentTile.Point == destination)
                    {
                        #region pathing successful
                        while (currentTile.Parent != null)
                        {
                            line.Add(currentTile.Point);
                            currentTile = currentTile.Parent;
                        }

                        line.Add(currentTile.Point);
                        line.Reverse();
                        return;
                        #endregion
                    }

                    currMinDepth = currentTile.GetCurrentMinimumDepth();

                    tileChanged = false;
                    foreach (var tile in tileList)
                    {
                        if (!tile.Visited)
                        {
                            if (!tileChanged)
                            {
                                currentTile = tile;
                                tileChanged = true;
                            }

                            tempMinDepth = tile.GetCurrentMinimumDepth();

                            if ((tempMinDepth < currMinDepth) || (tempMinDepth == currMinDepth && tile.DistanceToEnd < currentTile.DistanceToEnd))
                            {
                                currentTile = tile;
                            }
                        }
                    }

                    if (currentTile.Visited)
                    {
                        return;
                    }

                    if (currMinDepth > maximumDepth)
                    {
                        return;
                    }
                }

            }
            finally
            {
                foreach (var tile in tileList)
                {
                    currentTile = tile;
                    CubeWithParent.Pool.FreeObject(ref currentTile);
                }

                tileList.Clear();
                _cubeWParentSetPool.FreeObject(ref tileList);

                visitedTiles.Clear();
                _cubeSetPool.FreeObject(ref visitedTiles);

                newNeighbors.Clear();
                _cubeSetPool.FreeObject(ref newNeighbors);
            }
        }

        class CubeWithParent
        {
            public CubeWithParent Parent;
            public Vector3i Point;

            public float PathCost = 0; //Path cost
            public float DistanceToEnd = 0; //Distance to end
            public bool Visited = false;

            public static ObjectPool<CubeWithParent> Pool = new ObjectPool<CubeWithParent>(10);

            public void Initialize(Vector3i current, CubeWithParent parent = null)
            {
                Parent = parent;
                Point = current;
                Visited = false;
            }

            public float GetCurrentMinimumDepth()
            {
                return PathCost + DistanceToEnd;
            }
        }

        
    }

    public class Cube
    {
        //X = q
        //Y = s
        //Z = r
        public Vector3i Point;


        public Cube() { }
        public Cube(Vector3i point)
        {
            Point = point;
        }

        public Cube(Axial axial)
        {
            Point.X = axial.q;
            Point.Y = axial.s;
            Point.Z = axial.r;
        }

        public override bool Equals(object obj)
        {
            return obj is Cube cube &&
                   Point.Equals(cube.Point);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Point);
        }

        public override string ToString()
        {
            return Point.ToString();
        }
    }

    public struct Axial
    {
        public int q;
        public int r;
        public int s => -q - r;

        public Axial(int Q, int R)
        {
            q = Q;
            r = R;
        }

        public Axial(Cube cube)
        {
            q = cube.Point.X;
            r = cube.Point.Z;
        }

        public static Axial operator +(Axial a, Axial b) => new Axial(a.q + b.q, a.r + b.r);
        public static Axial operator -(Axial a, Axial b) => new Axial(a.q - b.q, a.r - b.r);
        public static Axial operator *(Axial a, Axial b) => new Axial(a.q * b.q, a.r * b.r);
        public static Axial operator /(Axial a, Axial b) => new Axial(a.q / b.q, a.r / b.r);
    }
}
