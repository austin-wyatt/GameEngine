using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MortalDungeon.Game.Map
{
    public class FeatureEquation
    {
        internal const int MAP_WIDTH = 50;
        internal const int MAP_HEIGHT = 50;

        public Dictionary<FeaturePoint, int> AffectedPoints = new Dictionary<FeaturePoint, int>();
        public HashSet<FeaturePoint> VisitedTiles = new HashSet<FeaturePoint>();

        public virtual void ApplyToTile(BaseTile tile)
        {

        }

        public virtual bool AffectsMap(TileMap map)
        {
            for (int i = 0; i < map.Tiles.Count; i++) 
            {
                FeaturePoint point = new FeaturePoint(map.Tiles[i]);

                if (AffectedPoints.ContainsKey(point)) 
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool AffectsPoint(TilePoint point)
        {
            return AffectedPoints.TryGetValue(new FeaturePoint(PointToMapCoords(point)), out int feature);
        }
        public virtual bool AffectsPoint(FeaturePoint point)
        {
            return AffectedPoints.TryGetValue(point, out int feature);
        }

        public virtual void ApplyToMap(TileMap map)
        {
            map.Tiles.ForEach(t =>
            {
                ApplyToTile(t);
            });
        }

        public virtual void OnAppliedToMaps() 
        {

        }

        /// <summary>
        /// The AffectedPoints hash set is filled here
        /// </summary>
        public virtual void GenerateFeature() { }

        public static Vector2i PointToMapCoords(TilePoint point)
        {
            Vector2i coords = new Vector2i
            {
                X = point.X + point.ParentTileMap.TileMapCoords.X * point.ParentTileMap.Width,
                Y = point.Y + point.ParentTileMap.TileMapCoords.Y * point.ParentTileMap.Height
            };

            return coords;
        }


        public struct FeaturePathToPointParameters
        {
            public FeaturePoint StartingPoint;
            public FeaturePoint EndingPoint;
            public Random NumberGen;


            public FeaturePathToPointParameters(FeaturePoint startingPoint, FeaturePoint endPoint)
            {
                StartingPoint = startingPoint;
                EndingPoint = endPoint;

                NumberGen = new Random(HashCoordinates(startingPoint.X, startingPoint.Y));
            }
        }

        public List<FeaturePoint> GetPathToPoint(FeaturePathToPointParameters param)
        {
            List<FeaturePointWithParent> pointList = new List<FeaturePointWithParent>();
            List<FeaturePoint> returnList = new List<FeaturePoint>();

            List<FeaturePoint> neighbors = new List<FeaturePoint>
            {
                param.StartingPoint
            };

            VisitedTiles.Clear();

            VisitedTiles.Add(param.StartingPoint);

            pointList.Add(new FeaturePointWithParent(param.StartingPoint, param.StartingPoint, true));

            List<FeaturePoint> newNeighbors = new List<FeaturePoint>();


            for (int i = 0; i < MAP_HEIGHT * MAP_WIDTH; i++)
            {
                newNeighbors.Clear();
                neighbors.ForEach(p =>
                {
                    GetNeighboringTiles(p, newNeighbors, true, param.NumberGen);

                    newNeighbors.ForEach(neighbor =>
                    {
                        pointList.Add(new FeaturePointWithParent(neighbor, p));
                    });
                });

                neighbors.Clear();
                for (int j = 0; j < newNeighbors.Count; j++)
                {
                    neighbors.Add(newNeighbors[j]);
                }

                if (neighbors.Count == 0) //if there are no more tiles to traverse return the empty list
                {
                    return returnList;
                }

                //same basic logic used in FindValidTilesInRadius
                for (int j = 0; j < neighbors.Count; j++)
                {
                    if (neighbors[j] == param.EndingPoint)
                    {
                        //if we found the destination tile then fill the returnList and return
                        FeaturePointWithParent finalPoint = pointList.Find(t => t.Point == param.EndingPoint);

                        returnList.Add(finalPoint.Point);

                        FeaturePoint parent = finalPoint.Parent;

                        while (true)
                        {
                            FeaturePointWithParent currentPoint = pointList.Find(t => t.Point == parent);
                            returnList.Add(currentPoint.Point);

                            parent = currentPoint.Parent;

                            if (currentPoint.IsRoot)
                                break;
                        }

                        returnList.Reverse();
                        return returnList;
                    }
                }
            }

            return returnList;
        }

        public void GetRingOfTiles(FeaturePoint startPoint, List<FeaturePoint> outputList, int radius = 1)
        {
            Vector3i cubePosition = CubeMethods.OffsetToCube(startPoint);

            cubePosition += TileMapConstants.CubeDirections[Direction.North] * radius;

            FeaturePoint tileOffsetCoord;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    tileOffsetCoord = new FeaturePoint(CubeMethods.CubeToOffset(cubePosition));

                    outputList.Add(tileOffsetCoord);

                    cubePosition += TileMapConstants.CubeDirections[(Direction)i];
                }
            }
        }

        public void GetNeighboringTiles(FeaturePoint tile, List<FeaturePoint> neighborList, bool shuffle = true, Random numberGen = null)
        {
            FeaturePoint neighborPos = new FeaturePoint(tile.X, tile.Y);
            int yOffset = tile.X % 2 == 0 ? 1 : 0;

            for (int i = 0; i < 6; i++)
            {
                neighborPos.X = tile.X;
                neighborPos.Y = tile.Y;
                switch (i)
                {
                    case 0: //tile below
                        neighborPos.Y += 1;
                        break;
                    case 1: //tile above
                        neighborPos.Y -= 1;
                        break;
                    case 2: //tile bottom left
                        neighborPos.X -= 1;
                        neighborPos.Y += yOffset;
                        break;
                    case 3: //tile top left
                        neighborPos.Y -= 1 + -yOffset;
                        neighborPos.X -= 1;
                        break;
                    case 4: //tile top right
                        neighborPos.Y -= 1 + -yOffset;
                        neighborPos.X += 1;
                        break;
                    case 5: //tile bottom right
                        neighborPos.X += 1;
                        neighborPos.Y += yOffset;
                        break;
                }

                if (!VisitedTiles.Contains(neighborPos))
                {
                    neighborList.Add(neighborPos);
                    VisitedTiles.Add(neighborPos);
                }
            }

            if (shuffle)
            {
                ShuffleList(neighborList, numberGen);
            }
        }

        public FeaturePoint GetNeighboringTile(FeaturePoint point, Direction direction)
        {
            FeaturePoint neighborPos = new FeaturePoint(point.X, point.Y);
            int yOffset = point.X % 2 == 0 ? 1 : 0;

            neighborPos.X = point.X;
            neighborPos.Y = point.Y;
            switch (direction)
            {
                case Direction.South: //tile below
                    neighborPos.Y += 1;
                    break;
                case Direction.North: //tile above
                    neighborPos.Y -= 1;
                    break;
                case Direction.SouthWest: //tile bottom left
                    neighborPos.X -= 1;
                    neighborPos.Y += yOffset;
                    break;
                case Direction.NorthWest: //tile top left
                    neighborPos.Y -= 1 + -yOffset;
                    neighborPos.X -= 1;
                    break;
                case Direction.NorthEast: //tile top right
                    neighborPos.Y -= 1 + -yOffset;
                    neighborPos.X += 1;
                    break;
                case Direction.SouthEast: //tile bottom right
                    neighborPos.X += 1;
                    neighborPos.Y += yOffset;
                    break;
            }

            return neighborPos;
        }

        public void GetLine(FeaturePoint startPoint, FeaturePoint endPoint, List<FeaturePoint> outputList)
        {
            Vector3i startCube = CubeMethods.OffsetToCube(startPoint);
            Vector3i endCube = CubeMethods.OffsetToCube(endPoint);

            int N = CubeMethods.GetDistanceBetweenPoints(startCube, endCube);
            float n = 1f / N;

            Vector3 currentCube;
            Vector2i currentOffset;

            FeaturePoint currentPoint = startPoint;
            FeaturePoint temp;

            for (int i = 0; i <= N; i++)
            {
                currentCube = CubeMethods.CubeLerp(startCube, endCube, n * i);
                currentOffset = CubeMethods.CubeToOffset(CubeMethods.CubeRound(currentCube));

                temp = new FeaturePoint(currentOffset);

                outputList.Add(temp);
            }
        }

        public static Direction DirectionBetweenTiles(TilePoint startPoint, TilePoint endPoint)
        {
            Direction direction = Direction.None;

            Vector2i start = PointToMapCoords(startPoint);
            Vector2i end = PointToMapCoords(endPoint);

            int yOffset = start.X % 2 == 0 ? 0 : -1;

            if (start.Y - end.Y == 1 && start.X == end.X)
            {
                direction = Direction.North;
            }
            else if (start.Y - end.Y == -1 && start.X == end.X)
            {
                direction = Direction.South;
            }
            else if (start.X - end.X == -1 && start.Y + yOffset - end.Y == 0)
            {
                direction = Direction.NorthEast;
            }
            else if (start.X - end.X == -1 && start.Y + yOffset - end.Y == -1)
            {
                direction = Direction.SouthEast;
            }
            else if (start.X - end.X == 1 && start.Y + yOffset - end.Y == 0)
            {
                direction = Direction.NorthWest;
            }
            else if (start.X - end.X == 1 && start.Y + yOffset - end.Y == -1)
            {
                direction = Direction.SouthWest;
            }

            return direction;
        }
        public static Direction DirectionBetweenTiles(FeaturePoint startPoint, FeaturePoint endPoint)
        {
            Direction direction = Direction.None;

            int yOffset = startPoint.X % 2 == 0 ? 0 : -1;

            if (startPoint.Y - endPoint.Y == 1 && startPoint.X == endPoint.X)
            {
                direction = Direction.North;
            }
            else if (startPoint.Y - endPoint.Y == -1 && startPoint.X == endPoint.X)
            {
                direction = Direction.South;
            }
            else if (startPoint.X - endPoint.X == -1 && startPoint.Y + yOffset - endPoint.Y == 0)
            {
                direction = Direction.NorthEast;
            }
            else if (startPoint.X - endPoint.X == -1 && startPoint.Y + yOffset - endPoint.Y == -1)
            {
                direction = Direction.SouthEast;
            }
            else if (startPoint.X - endPoint.X == 1 && startPoint.Y + yOffset - endPoint.Y == 0)
            {
                direction = Direction.NorthWest;
            }
            else if (startPoint.X - endPoint.X == 1 && startPoint.Y + yOffset - endPoint.Y == -1)
            {
                direction = Direction.SouthWest;
            }

            return direction;
        }
        public static int AngleBetweenDirections(Direction a, Direction b)
        {
            int temp = (int)a - (int)b;

            int angle;
            if (temp < 0)
            {
                temp += 6;
                angle = 60 * temp;
            }
            else
            {
                angle = 60 * temp;
            }

            return angle;
        }

        public static int GetDistanceBetweenPoints(FeaturePoint pointA, FeaturePoint pointB)
        {
            Vector3i a = CubeMethods.OffsetToCube(pointA);
            Vector3i b = CubeMethods.OffsetToCube(pointB);

            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }

        public static int AngleOfDirection(Direction dir)
        {
            return ((int)dir + 2) * 60; //subtract 2 from the direction so that the default direction is north
        }

        static void ShuffleList<T>(IList<T> list, Random numberGen = null)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k;
                if (numberGen == null)
                {
                    k = new Random().Next(n + 1);
                }
                else
                {
                    k = numberGen.Next(n + 1);
                }

                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        internal static int HashCoordinates(int x, int y)
        {
            int h = x * 374761393 + y * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            return h ^ (h >> 16);
        }

        /// <summary>
        /// This is where the point gets associated with a specific feature
        /// </summary>
        internal virtual void UpdatePoint(FeaturePoint point)
        {
            AffectedPoints.TryAdd(point, (int)Feature.None);
        }

        protected bool GetBit(int num, int bitNumber)
        {
            return (num & (1 << bitNumber)) != 0;
        }
    }

    public struct FeaturePoint
    {
        public int X;
        public int Y;

        public bool _visited;

        public FeaturePoint(int x, int y) 
        {
            X = x;
            Y = y;

            _visited = false;
        }

        public FeaturePoint(TilePoint tilePoint) 
        {
            Vector2i coords = FeatureEquation.PointToMapCoords(tilePoint);

            X = coords.X;
            Y = coords.Y;

            _visited = false;
        }

        public FeaturePoint(Vector2i coords)
        {
            X = coords.X;
            Y = coords.Y;

            _visited = false;
        }

        public FeaturePoint(FeaturePoint coords)
        {
            X = coords.X;
            Y = coords.Y;

            _visited = false;
        }

        public static bool operator ==(FeaturePoint a, FeaturePoint b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(FeaturePoint a, FeaturePoint b) => !(a == b);

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"{{{X}, {Y}}}";
        }
    }

    public class FeaturePointWithParent 
    {
        public FeaturePoint Point;
        public FeaturePoint Parent;
        public bool IsRoot;

        public FeaturePointWithParent(FeaturePoint point, FeaturePoint parent, bool isRoot = false) 
        {
            Point = point;
            Parent = parent;

            IsRoot = isRoot;
        }
    }

    public enum Feature 
    {
        None,
        Grass,
        Water_1,
        Water_2,
        Tree_1,
        Tree_2,
        StonePath
    }
}
