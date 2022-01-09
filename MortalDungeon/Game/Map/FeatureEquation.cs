using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.LuaHandling;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Map
{
    public enum FeatureEquationPointValues
    {
        TileStart = 50000,
        TileEnd = 150000,

        UnitStart = 1000000,
        UnitEnd = 2000000,
        BuildingStart = 2000000,
        BuildingEnd = 2100000
    }

    public class FeatureEquation
    {
        public const int MAP_WIDTH = 50;
        public const int MAP_HEIGHT = 50;

        public Dictionary<FeaturePoint, int> AffectedPoints = new Dictionary<FeaturePoint, int>();
        public HashSet<FeaturePoint> VisitedTiles = new HashSet<FeaturePoint>();

        public HashSet<TileMapPoint> AffectedMaps = new HashSet<TileMapPoint>();

        public List<BoundingPoints> BoundingPoints = new List<BoundingPoints>();

        public Dictionary<FeaturePoint, Dictionary<string, string>> Parameters = new Dictionary<FeaturePoint, Dictionary<string, string>>();

        public List<BuildingSkeleton> BuildingSkeletons = new List<BuildingSkeleton>();

        public List<ClearParamaters> ClearParamaters = new List<ClearParamaters>();

        public FeaturePoint Origin = new FeaturePoint();

        public Random NumberGen;

        public int NameTextEntry = 0;

        /// <summary>
        /// These get applied to the state when the player enters the load radius.
        /// </summary>
        public List<Instructions> Instructions = new List<Instructions>();



        /// <summary>
        /// If the tile map we are loading is within this load radius of maps then we should load and check this feature.
        /// </summary>
        public int LoadRadius = 100;

        //public int FeatureID => _featureID;
        //protected int _featureID = _currentFeatureID++;
        //protected static int _currentFeatureID = 0;

        public long FeatureID = -1;

        public int FeatureTemplate = 0;

        public int Layer = 0;
        /// <summary>
        /// Higher load priority gets applied first
        /// </summary>
        public int LoadPriority = 0;

        public string DescriptiveName = "";

        /// <summary>
        /// Applies any feature effects to a relevant tile.
        /// </summary>
        /// <param name="freshGeneration">Whether this is a newly added map or one that has already been loaded. 
        /// The main use for this is resolving features that span multiple tilemaps and must seem contiguous (such as buildings)</param>
        public virtual void ApplyToTile(BaseTile tile, bool freshGeneration = true)
        {
            FeaturePoint affectedPoint = new FeaturePoint(PointToMapCoords(tile.TilePoint));

            if(Parameters.TryGetValue(affectedPoint, out var param))
            {
                if(param.TryGetValue("GEN_SCRIPT", out var script))
                {
                    LuaManager.ApplyScript(script);
                }
            }

            if(AffectedPoints.TryGetValue(affectedPoint, out int value))
            {
                long pointHash = affectedPoint.GetUniqueHash();

                var unitAlive = FeatureLedger.GetHashBoolean(FeatureID, pointHash, "alive");



                //if(value == 50000)
                //{
                //    tile.Properties.Type = new Random().NextDouble() > 0.3 ? TileType.Water : TileType.AltWater;
                //    tile.Properties.Classification = TileClassification.Water;
                //    tile.Outline = false;
                //    tile.NeverOutline = true;

                //    tile.Update();

                //    return;
                //}

                if (value >= (int)FeatureEquationPointValues.UnitStart && value <= (int)FeatureEquationPointValues.UnitEnd && freshGeneration &&
                    unitAlive != HashBoolean.False &&
                    !tile.GetScene()._units.Exists(u => u.FeatureID == FeatureID && u.ObjectHash == pointHash))
                {
                    var unitInfo = UnitCreationInfoSerializer.LoadUnitCreationInfoFromFile(value - (int)FeatureEquationPointValues.UnitStart);

                    if(unitInfo != null)
                    {
                        Unit unit = unitInfo.CreateUnit(tile.GetScene());

                        if(Parameters.TryGetValue(affectedPoint, out var parameters))
                        {
                            unit.ApplyUnitParameters(parameters);
                        }
                        

                        Entity unitEntity = new Entity(unit);
                        EntityManager.AddEntity(unitEntity);

                        unitEntity.Handle.FeatureID = FeatureID;
                        unitEntity.Handle.ObjectHash = pointHash;

                        unitEntity.DestroyOnUnload = true;

                        unitEntity.Load(affectedPoint);

                        FeatureLedger.SetHashBoolean(FeatureID, pointHash, "alive", true);
                    }
                }

                if(value >= (int)FeatureEquationPointValues.BuildingStart && value <= (int)FeatureEquationPointValues.BuildingEnd)
                {
                    BuildingSkeleton skeleton = BuildingSkeletons[value - (int)FeatureEquationPointValues.BuildingStart];

                    if (skeleton != null && skeleton.Loaded == false)
                    {
                        skeleton.Loaded = true;
                        skeleton._skeletonTouchedThisCycle = true;

                        var building = skeleton.Handle;
                        building.Scene = tile.GetScene();
                        building.InitializeUnitInfo();

                        building.FeatureOrigin = Origin;

                        building.SkeletonReference = skeleton;

                        //building.InitializeVisualComponent();

                        Vector3 tileSize = tile.GetDimensions();
                        Vector3 posDiff = new Vector3();

                        if (affectedPoint != skeleton.IdealCenter)
                        {
                            posDiff.X = tileSize.X * (skeleton.IdealCenter.X + Origin.X - affectedPoint.X);
                            posDiff.Y = tileSize.Y * (skeleton.IdealCenter.Y + Origin.Y - affectedPoint.Y);



                            if (Math.Abs((skeleton.IdealCenter.X + Origin.X)) % 2 == 0)
                            {
                                posDiff.Y += tileSize.Y / 2;
                            }

                            if (Math.Abs((skeleton.IdealCenter.X + Origin.X)) % 2 == 1)
                            {
                                posDiff.Y -= tileSize.Y / 2;
                            }

                            if (affectedPoint.X < skeleton.IdealCenter.X)
                            {
                                posDiff.X -= tileSize.X / 4;
                            }
                            else
                            {
                                posDiff.X += tileSize.X / 4;
                            }
                        }

                        building.SetPosition(tile.Position + posDiff + new Vector3(0, 0, 0.2f));

                        building.SetTileMapPosition(tile);
                    }
                    else if (skeleton != null && skeleton.Handle != null && skeleton.Loaded && !skeleton._skeletonTouchedThisCycle)
                    {
                        skeleton._skeletonTouchedThisCycle = true;

                        skeleton.Handle.TileAction();
                    }
                }
            }

            foreach (var bound in BoundingPoints)
            {
                if (bound.BoundingPointsId == 0)
                    continue;

                if (bound.OffsetPoints.Count > 2 && FeaturePoint.PointInPolygon(bound.OffsetPoints, new Vector2i(affectedPoint.X, affectedPoint.Y)) && freshGeneration)
                {
                    switch (bound.BoundingPointsId)
                    {
                        case 1:
                            double density = 0.9;
                            if(bound.Parameters.TryGetValue("density", out var val))
                            {
                                if(double.TryParse(val, out var d))
                                {
                                    density = d;
                                }
                            }

                            if (NumberGen.NextDouble() > density)
                            {
                                var tree = new Tree(tile.TileMap, tile, NumberGen.Next() % 2);

                                tile.GetScene().Tick -= tree.Tick;
                            }
                            break;
                        case 2:
                            if (NumberGen.NextDouble() > 0.3)
                            {
                                tile.Properties.Type = TileType.Water;
                            }
                            else
                            {
                                tile.Properties.Type = TileType.AltWater;
                            }
                            break;
                        case 3:
                            tile.Properties.Type = TileType.Dirt;
                            break;
                    }

                }
            }
        }

        public virtual bool AffectsMap(TileMap map)
        {
            return AffectedMaps.TryGetValue(map.TileMapCoords, out var _);
        }

        public virtual bool AffectsPoint(TilePoint point)
        {
            return AffectedPoints.TryGetValue(new FeaturePoint(PointToMapCoords(point)), out int feature);
        }
        public virtual bool AffectsPoint(FeaturePoint point)
        {
            return AffectedPoints.TryGetValue(point, out int feature);
        }

        public virtual void ApplyToMap(TileMap map, bool freshGeneration = true)
        {
            map.Tiles.ForEach(t =>
            {
                ApplyToTile(t, freshGeneration);
            });
        }

        public virtual void OnAppliedToMaps() 
        {

        }

        public bool CheckCleared()
        {
            if(FeatureLedger.GetFeatureStateValue(FeatureID, FeatureStateValues.Cleared) != 1)
            {
                foreach(var parameters in ClearParamaters)
                {
                    foreach(var param in parameters.ExpectedValues)
                    {
                        if(FeatureLedger.GetHashData(FeatureID, parameters.ObjectHash, param.Key) != param.Value)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// The AffectedPoints hash set is filled here. This should only be run if the LoadRadius condition is satisfied
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

        public static TileMapPoint FeaturePointToTileMapCoords(FeaturePoint point, int mapWidth = 50, int mapHeight = 50)
        {
            TileMapPoint coords = new TileMapPoint((int)Math.Floor((float)point.X / mapWidth), (int)Math.Floor((float)point.Y / mapHeight));

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

                NumberGen = new ConsistentRandom((int)HashCoordinates(startingPoint.X, startingPoint.Y));
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
                    k = new ConsistentRandom().Next(n + 1);
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

        public static long HashCoordinates(int x, int y)
        {
            long val = ((long)x << 32) + y;
            return val;
        }

        public static Vector2i UnhashCoordinates(long hashedCoords)
        {
            Vector2i coords = new Vector2i();

            coords.X = (int)(hashedCoords >> 32);
            coords.Y = (int)(hashedCoords - coords.X << 32);

            return coords;
        }

        /// <summary>
        /// This is where the point gets associated with a specific feature
        /// </summary>
        public void AddAffectedPoint(FeaturePoint point, int feature)
        {
            AffectedPoints.TryAdd(point, feature);
            AffectedMaps.Add(FeaturePointToTileMapCoords(point));
        }

        public void ClearAffectedPoints() 
        {
            AffectedPoints.Clear();
            AffectedMaps.Clear();
        }



        protected bool GetBit(int num, int bitNumber)
        {
            return (num & (1 << bitNumber)) != 0;
        }


        public delegate void FeatureEnterHandler(FeatureEquation eq, Unit unit);

        public FeatureEnterHandler Enter;
        public FeatureEnterHandler Exit;

        public void OnEnter(Unit unit)
        {
            Enter?.Invoke(this, unit);
        }

        public void OnExit(Unit unit)
        {
            Exit?.Invoke(this, unit);
        }
    }
}
