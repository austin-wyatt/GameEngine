using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Game.Abilities;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Game.Units.AIFunctions;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Empyrean.Game.Combat
{
    public enum PathingResult
    {
        Failed,
        Succeeded,
        Partial
    }

    public class NavMesh
    {
        public NavTile[] NavTilesArr = new NavTile[TileMapManager.LOAD_DIAMETER * TileMapManager.LOAD_DIAMETER 
            * TileMapManager.TILE_MAP_DIMENSIONS.X * TileMapManager.TILE_MAP_DIMENSIONS.Y];

        public int COLUMN_SIZE = TileMapManager.LOAD_DIAMETER * TileMapManager.TILE_MAP_DIMENSIONS.X;

        private object _navTileLock = new object();
        public void CalculateNavTiles()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            lock (_navTileLock)
            {
                for(int i = 0; i < NavTilesArr.Length; i++)
                {
                    NavTilesArr[i] = null;
                }

                FeaturePoint point = new FeaturePoint();

                Vector2i firstTileOffset = new Vector2i();

                int offset;

                for (int i = 0; i < TileMapManager.ActiveMaps.Count; i++)
                {
                    firstTileOffset.X = (TileMapManager.ActiveMaps[i].TileMapCoords.X - TileMapHelpers._topLeftMap.TileMapCoords.X) * TileMapManager.TILE_MAP_DIMENSIONS.X;
                    firstTileOffset.Y = (TileMapManager.ActiveMaps[i].TileMapCoords.Y - TileMapHelpers._topLeftMap.TileMapCoords.Y) * TileMapManager.TILE_MAP_DIMENSIONS.Y;

                    for (int j = 0; j < TileMapManager.ActiveMaps[i].Tiles.Count; j++)
                    {
                        //base offset
                        //tileOffset = firstTileOffset.X * COLUMN_SIZE + firstTileOffset.Y;

                        point.Initialize(TileMapManager.ActiveMaps[i].Tiles[j]);

                        NavTile navTile = new NavTile(TileMapManager.ActiveMaps[i].Tiles[j]);

                        offset = GetOffsetFromFeaturePoint(point);

                        NavTilesArr[offset] = navTile;
                    }
                }
            }

            Console.WriteLine($"NavMesh calculated in {stopwatch.ElapsedMilliseconds}ms");
        }


        private Vector2i _offsetInfo = new Vector2i();
        public int GetOffsetFromFeaturePoint(FeaturePoint point)
        {
            _offsetInfo.X = point.X - TileMapHelpers._topLeftMap.TileMapCoords.X * TileMapManager.TILE_MAP_DIMENSIONS.X;
            _offsetInfo.Y = point.Y - TileMapHelpers._topLeftMap.TileMapCoords.Y * TileMapManager.TILE_MAP_DIMENSIONS.Y;

            return _offsetInfo.X * COLUMN_SIZE + _offsetInfo.Y;
        }

        public NavTile GetNavTileAtFeaturePoint(FeaturePoint point)
        {
            return NavTilesArr[GetOffsetFromFeaturePoint(point)];
        }

        public bool CheckFeaturePointInBounds(FeaturePoint point)
        {
            return !((point.X < TileMapHelpers._topLeftMap.TileMapCoords.X * TileMapManager.TILE_MAP_DIMENSIONS.X) ||
                (point.X > TileMapHelpers._topLeftMap.TileMapCoords.X * TileMapManager.TILE_MAP_DIMENSIONS.X + TileMapManager.LOAD_DIAMETER * TileMapManager.TILE_MAP_DIMENSIONS.X) ||
                (point.Y < TileMapHelpers._topLeftMap.TileMapCoords.Y * TileMapManager.TILE_MAP_DIMENSIONS.Y) ||
                (point.Y > TileMapHelpers._topLeftMap.TileMapCoords.Y * TileMapManager.TILE_MAP_DIMENSIONS.Y + TileMapManager.LOAD_DIAMETER * TileMapManager.TILE_MAP_DIMENSIONS.Y));
        }

        public void UpdateNavMeshForTileMap(TileMap map)
        {
            lock (_navTileLock)
            {
                FeaturePoint point = new FeaturePoint();

                for (int j = 0; j < map.Tiles.Count; j++)
                {
                    point.Initialize(map.Tiles[j]);

                    NavTile navTile = GetNavTileAtFeaturePoint(point);

                    if (navTile != null)
                    {
                        navTile.CalculateNavDirectionMask();
                    }
                }
            }
        }

        public void UpdateNavMeshForTile(Tile tile)
        {
            FeaturePoint point = new FeaturePoint();
            point.Initialize(tile);

            Direction dir;

            NavTile navTile = GetNavTileAtFeaturePoint(point);

            if (navTile != null)
            {
                navTile.CalculateNavDirectionMask();

                for(int i = 0; i < 6; i++)
                {
                    dir = (Direction)i;

                    if (GetNeighboringNavTile(point, dir, out var neighborNavTile))
                    {
                        neighborNavTile.CalculateNavDirectionMask();
                    }
                }
            }
        }


        private static ObjectPool<HashSet<NavTileWithParent>> _navTileWParentSetPool = new ObjectPool<HashSet<NavTileWithParent>>();
        private static ObjectPool<HashSet<NavTile>> _navTileSetPool = new ObjectPool<HashSet<NavTile>>();
        private static ObjectPool<List<NavTile>> _navTileListPool = new ObjectPool<List<NavTile>>();

        /// <summary>
        /// Attempts to find a path from the start point to the destination point. This is intended specifically for movement. <para/>
        /// Returns a bool indicating whether the path was found successfully.
        /// </summary>
        /// <param name="maximumDepth">
        /// The maximum amount of energy that the path can consume
        /// </param>
        /// <param name="maximumAversion">
        /// The maximum combined weight of negative obstacles on a path before a unit does not want to use the path
        /// </param>
        public bool GetPathToPoint(FeaturePoint start, FeaturePoint destination, NavType navType, out List<Tile> returnList,
            float maximumDepth = 10, Unit pathingUnit = null, bool considerCaution = false,
            bool allowEndInUnit = false)
        {
            HashSet<NavTileWithParent> tileList = _navTileWParentSetPool.GetObject();
            returnList = Tile.TileListPool.GetObject();

            HashSet<NavTile> visitedTiles = _navTileSetPool.GetObject();
            List<NavTile> newNeighbors = _navTileListPool.GetObject();

            FeaturePoint placeholderPoint = new FeaturePoint();
            FeaturePoint currentFeaturePoint = new FeaturePoint();

            NavTileWithParent currentTile;

            Stopwatch timer = Stopwatch.StartNew();

            List<Direction> directions = new List<Direction>()
            { 
                Direction.South, Direction.SouthEast, Direction.SouthWest, 
                Direction.NorthEast, Direction.NorthWest, Direction.North
            };

            NavTileWithParent placeholderNavTile = new NavTileWithParent();

            try
            {
                if (maximumDepth <= 0)
                    return false;

                if (!CheckFeaturePointInBounds(start) || !CheckFeaturePointInBounds(destination))
                {
                    return false;
                }

                NavTile destinationTile = GetNavTileAtFeaturePoint(destination);
                NavTile startTile = GetNavTileAtFeaturePoint(start);

                if (destinationTile == null || startTile == null)
                    return false;

                if (UnitPositionManager.TilePointHasUnits(destinationTile.Tile) && !allowEndInUnit)
                {
                    return false; //if the ending tile is inside of a unit then immediately return
                }

                if (destinationTile.Tile.Structure != null && !(destinationTile.Tile.Structure.Passable || destinationTile.Tile.Structure.Pathable))
                {
                    return false;
                }

                if (FeatureEquation.GetDistanceBetweenPoints(start, destination) > maximumDepth * 1.5f)
                {
                    return false;
                }

                currentTile = NavTileWithParent.Pool.GetObject();
                currentTile.Initialize(startTile);
                currentTile.PathCost = FeatureEquation.GetDistanceBetweenPoints(start, start);
                currentTile.DistanceToEnd = FeatureEquation.GetDistanceBetweenPoints(start, destination);

                visitedTiles.Add(startTile);

                tileList.Add(currentTile);


                float unitCaution = considerCaution ? pathingUnit?.AI.Feelings.GetFeelingValue(FeelingType.Caution) ?? 0f : 0f;

                //bool hasMovementAbilities = movementParams != null && movementParams.Params.Count > 0;
                bool hasMovementAbilities = false;

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

                    if (currentTile.NavTile == destinationTile)
                    {
                        #region pathing successful
                        while (currentTile.Parent != null)
                        {
                            returnList.Add(currentTile.NavTile.Tile);
                            currentTile = currentTile.Parent;
                        }

                        returnList.Add(TileMapHelpers.GetTile(currentTile.NavTile.Tile));
                        returnList.Reverse();
                        return true;
                        #endregion
                    }

                    newNeighbors.Clear();

                    currentFeaturePoint.Initialize(currentTile.NavTile.Tile);

                    directions.Randomize();

                    for (int i = 0; i < 6; i++)
                    {
                        dir = directions[i];

                        if ((currentTile.NavTile.NavDirectionMask & NavTile.GetNavDirection(navType, dir)) > 0)
                        {
                            if (GetNeighboringNavTile(currentFeaturePoint, dir, out NavTile neighborTile))
                            {
                                newNeighbors.Add(neighborTile);
                            }
                        }
                    }

                    if (hasMovementAbilities)
                    {
                        //TODO
                        //We need to change the output from a list of basetiles to a list of movement actions
                        //These tile actions will contain the ability that was used (or if it's a basic move then just a list of tiles)
                        //If an ability was used then the destination tile will be included.
                        //This list of movement actions then needs to be unrolled and applied where necessary
                    }

                    foreach (var neighbor in newNeighbors)
                    {
                        var unitsOnPoint = UnitPositionManager.GetUnitsOnTilePoint(neighbor.Tile.TilePoint);

                        if (unitsOnPoint.Count > 0 && !(pathingUnit?.Info.PhasedMovement == true)) //special cases for ability targeting should go here
                        {
                            unitOnSpace = false;

                            foreach (var unit in unitsOnPoint)
                            {
                                if (unit.Info.BlocksSpace)
                                {
                                    unitOnSpace = true;
                                    break;
                                }
                            }

                            if (unitOnSpace && !(allowEndInUnit && neighbor == destinationTile))
                                continue;
                        }

                        var tileEffectsOnPoint = TileEffectManager.GetTileEffectsOnTilePoint(neighbor.Tile.TilePoint);

                        skipNeighbor = false;

                        foreach (var tileEffect in tileEffectsOnPoint)
                        {
                            if (pathingUnit != null)
                            {
                                //if the unit isn't immune then we check their caution
                                immunityExists = false;
                                for(int i = 0; i < tileEffect.Immunities.Count; i++)
                                {
                                    if (pathingUnit.Info.StatusManager.CheckCondition(tileEffect.Immunities[i]))
                                    {
                                        immunityExists = true;
                                        break;
                                    }
                                }

                                if (!immunityExists)
                                {
                                    if (unitCaution > (1 - tileEffect.Danger))
                                    {
                                        skipNeighbor = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (skipNeighbor)
                        {
                            continue;
                        }

                        placeholderPoint.Initialize(neighbor.Tile);
                        pathCost = currentTile.PathCost + neighbor.Tile.Properties.MovementCost;
                        distanceToEnd = FeatureEquation.GetDistanceBetweenPoints(placeholderPoint, destination);

                        if (visitedTiles.Contains(neighbor))
                        {
                            placeholderNavTile.NavTile = neighbor;

                            if (tileList.TryGetValue(placeholderNavTile, out var foundTileWithParent) &&
                                foundTileWithParent.PathCost > pathCost && foundTileWithParent.DistanceToEnd >= distanceToEnd)
                            {
                                foundTileWithParent.PathCost = pathCost;
                                foundTileWithParent.DistanceToEnd = distanceToEnd;
                                foundTileWithParent.Initialize(neighbor, currentTile);
                            }

                            continue;
                        }
                        else
                        {
                            if (pathCost + distanceToEnd <= maximumDepth)
                            {
                                NavTileWithParent tile = NavTileWithParent.Pool.GetObject();
                                tile.Initialize(neighbor, currentTile);
                                tile.PathCost = pathCost;
                                tile.DistanceToEnd = distanceToEnd;

                                tileList.Add(tile);

                                if (neighbor == destinationTile)
                                {
                                    currentTile = tile;
                                    break;
                                }
                            }

                            visitedTiles.Add(neighbor);
                        }
                    }

                    if (currentTile.NavTile == destinationTile)
                    {
                        #region pathing successful
                        while (currentTile.Parent != null)
                        {
                            returnList.Add(currentTile.NavTile.Tile);
                            currentTile = currentTile.Parent;
                        }

                        returnList.Add(TileMapHelpers.GetTile(currentTile.NavTile.Tile.TilePoint));
                        returnList.Reverse();
                        return true;
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
                        return false;
                    }

                    if (currMinDepth > maximumDepth)
                    {
                        return false;
                    }
                }

            }
            finally
            {
                foreach(var tile in tileList)
                {
                    //tile.NavTile.Tile.SetColor(_Colors.Red);

                    currentTile = tile;
                    NavTileWithParent.Pool.FreeObject(ref currentTile);
                }

                tileList.Clear();
                _navTileWParentSetPool.FreeObject(ref tileList);

                visitedTiles.Clear();
                _navTileSetPool.FreeObject(ref visitedTiles);

                newNeighbors.Clear();
                _navTileListPool.FreeObject(ref newNeighbors);

                Console.WriteLine("Pathing completed in " + timer.Elapsed.TotalMilliseconds + "ms");
            }
        }

        private static ObjectPool<List<Vector3i>> _vector3iListPool = new ObjectPool<List<Vector3i>>();
        public PathingResult GetLineToPoint(FeaturePoint start, FeaturePoint destination, NavType navType, out List<Tile> returnList, Unit pathingUnit = null)
        {
            List<Vector3i> cubeList = _vector3iListPool.GetObject();
            returnList = Tile.TileListPool.GetObject();

            Vector3i cubeStart = CubeMethods.OffsetToCube(start);
            Vector3i cubeDest = CubeMethods.OffsetToCube(destination);

            Vector3i tempCube = new Vector3i();

            NavTile currNavTile;

            Direction dir;

            bool addTileAndReturnPartial = false;

            try
            {
                CubeMethods.GetLineLerp(cubeStart, cubeDest, cubeList);

                for(int i = 0; i < cubeList.Count; i++)
                {
                    FeaturePoint currFeaturePoint = CubeMethods.CubeToFeaturePoint(cubeList[i]);

                    currNavTile = GetNavTileAtFeaturePoint(currFeaturePoint);

                    if (i < cubeList.Count - 1)
                    {
                        tempCube.X = cubeList[i + 1].X - cubeList[i].X;
                        tempCube.Y = cubeList[i + 1].Y - cubeList[i].Y;
                        tempCube.Z = cubeList[i + 1].Z - cubeList[i].Z;

                        dir = CubeMethods.CubeDirectionsInverted[tempCube];
                        if ((currNavTile.NavDirectionMask & NavTile.GetNavDirection(navType, dir)) == 0)
                        {
                            addTileAndReturnPartial = true;
                        }
                    }

                    if(i > 0)
                    {
                        var unitsOnPoint = UnitPositionManager.GetUnitsOnTilePoint(currNavTile.Tile.TilePoint);

                        if (unitsOnPoint.Count > 0 && !(pathingUnit?.Info.PhasedMovement == true)) //special cases for ability targeting should go here
                        {
                            foreach (var unit in unitsOnPoint)
                            {
                                if (unit.Info.BlocksSpace)
                                {
                                    return PathingResult.Partial;
                                }
                            }
                        }

                        //var tileEffectsOnPoint = TileEffectManager.GetTileEffectsOnTilePoint(currNavTile.Tile.TilePoint);

                        //foreach (var tileEffect in tileEffectsOnPoint)
                        //{
                        //    //if tile effects can block the path then check that here
                        //}
                    }

                    returnList.Add(currNavTile.Tile);

                    if (addTileAndReturnPartial)
                    {
                        return PathingResult.Partial;
                    }
                }

                return PathingResult.Succeeded;
            }
            finally
            {
                cubeList.Clear();
                _vector3iListPool.FreeObject(cubeList);
            }
        }



        //private static ObjectPool<List<List<Tile>>> _tileListListPool = new ObjectPool<List<List<Tile>>>();
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sampleCenter">The center point of the radius to sample</param>
        ///// <param name="startPoint">The point from which the sample pathing will begin</param>
        //public void SamplePathsInRadiusAroundPoint(Tile sampleCenter, Tile startPoint, float maximumDepth, float radius, Unit pathingUnit = null)
        //{
        //    HashSet<Tile> tilesInRadius = Tile.TileSetPool.GetObject();
        //    sampleCenter.TileMap.GetTilesInRadius(sampleCenter, (int)radius, tilesInRadius);

        //    List<List<Tile>> paths = _tileListListPool.GetObject();

        //    //sample random points from the tiles in radius. Attempt to path to each of those points and save it to the paths list

        //    tilesInRadius.Clear();
        //    Tile.TileSetPool.FreeObject(tilesInRadius);

        //    paths.Clear();
        //    _tileListListPool.FreeObject(paths);
        //}

        private static ObjectPool<Vector3i> _vector3iPool = new ObjectPool<Vector3i>();
        public bool GetNeighboringNavTile(FeaturePoint point, Direction direction, out NavTile neighborTile)
        {
            var cube = _vector3iPool.GetObject();
            CubeMethods.OffsetToCube(point, ref cube);

            CubeMethods.CubeNeighborInPlace(ref cube, direction);

            FeaturePoint neighborPoint = CubeMethods.CubeToFeaturePoint(cube);

            _vector3iPool.FreeObject(ref cube);

            if (!CheckFeaturePointInBounds(neighborPoint))
            {
                neighborTile = null;
                return false;
            }

            NavTile navTile = GetNavTileAtFeaturePoint(neighborPoint);

            neighborTile = navTile;
            return navTile != null;
        }

        public void NavFloodFill(FeaturePoint start, NavType navType, ref HashSet<NavTileWithParent> returnList, float maximumDepth = 10, Unit pathingUnit = null)
        {
            Queue<NavTileWithParent> tilesToCheck = new Queue<NavTileWithParent>();

            //a placeholder NavTileWithParent that can be used to search the returnList
            NavTileWithParent placeholder = new NavTileWithParent();

            FeaturePoint placeholderPoint = new FeaturePoint();

            NavTile navTile = GetNavTileAtFeaturePoint(start);
            tilesToCheck.Enqueue(new NavTileWithParent(navTile) { PathCost = 0 });
            returnList.Add(tilesToCheck.Peek());


            Direction dir;

            NavTileWithParent currentTile;

            float pathCost;
            bool unitOnSpace;
            bool skipNeighbor;

            while (tilesToCheck.Count > 0)
            {
                currentTile = tilesToCheck.Dequeue();

                for (int i = 0; i < 6; i++)
                {
                    dir = (Direction)i;

                    if ((currentTile.NavTile.NavDirectionMask & NavTile.GetNavDirection(navType, dir)) > 0)
                    {
                        placeholderPoint.Initialize(currentTile.NavTile.Tile);

                        if (!GetNeighboringNavTile(placeholderPoint, dir, out NavTile neighbor))
                        {
                            continue;
                        }

                        placeholder.NavTile = neighbor;
                        placeholder.Parent = currentTile;

                        var unitsOnPoint = UnitPositionManager.GetUnitsOnTilePoint(neighbor.Tile.TilePoint);

                        if (unitsOnPoint.Count > 0 && !(pathingUnit?.Info.PhasedMovement == true))
                        {
                            unitOnSpace = false;

                            foreach (var unit in unitsOnPoint)
                            {
                                if (unit.Info.BlocksSpace)
                                {
                                    unitOnSpace = true;
                                    break;
                                }
                            }

                            if (unitOnSpace)
                                continue;
                        }

                        //var tileEffectsOnPoint = TileEffectManager.GetTileEffectsOnTilePoint(neighbor.Tile.TilePoint);

                        //skipNeighbor = false;

                        //foreach (var tileEffect in tileEffectsOnPoint)
                        //{
                        //    if (pathingUnit != null)
                        //    {
                        //        //if the unit isn't immune then we check their caution
                        //        immunityExists = false;
                        //        for (int i = 0; i < tileEffect.Immunities.Count; i++)
                        //        {
                        //            if (pathingUnit.Info.StatusManager.CheckCondition(tileEffect.Immunities[i]))
                        //            {
                        //                immunityExists = true;
                        //                break;
                        //            }
                        //        }

                        //        if (!immunityExists)
                        //        {
                        //            if (unitCaution > (1 - tileEffect.Danger))
                        //            {
                        //                skipNeighbor = true;
                        //                break;
                        //            }
                        //        }
                        //    }
                        //}

                        //if (skipNeighbor)
                        //{
                        //    continue;
                        //}

                        pathCost = currentTile.PathCost + neighbor.Tile.Properties.MovementCost;

                        if (returnList.TryGetValue(placeholder, out NavTileWithParent foundVal))
                        {
                            if(pathCost < foundVal.PathCost)
                            {
                                foundVal.PathCost = pathCost;
                                foundVal.Initialize(neighbor, currentTile);
                            }

                            continue;
                        }
                        else
                        {
                            if (pathCost <= maximumDepth)
                            {
                                NavTileWithParent tileWithParent = new NavTileWithParent();
                                tileWithParent.Initialize(neighbor, currentTile);
                                tileWithParent.PathCost = pathCost;

                                tilesToCheck.Enqueue(tileWithParent);
                                returnList.Add(tileWithParent);
                            }
                        }
                    }
                }
            }
        }
    }
}
