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

                        TileMapManager.ActiveMaps[i].Tiles[j].ToFeaturePoint(ref point);

                        NavTile navTile = new NavTile(TileMapManager.ActiveMaps[i].Tiles[j]);

                        offset = GetOffsetFromFeaturePoint(ref point);

                        NavTilesArr[offset] = navTile;
                    }
                }
            }

            Console.WriteLine($"NavMesh calculated in {stopwatch.ElapsedMilliseconds}ms");
        }


        private Vector2i _offsetInfo = new Vector2i();
        public int GetOffsetFromFeaturePoint(ref FeaturePoint point)
        {
            _offsetInfo.X = point.X - TileMapHelpers._topLeftMap.TileMapCoords.X * TileMapManager.TILE_MAP_DIMENSIONS.X;
            _offsetInfo.Y = point.Y - TileMapHelpers._topLeftMap.TileMapCoords.Y * TileMapManager.TILE_MAP_DIMENSIONS.Y;

            return _offsetInfo.X * COLUMN_SIZE + _offsetInfo.Y;
        }

        public NavTile GetNavTileAtFeaturePoint(ref FeaturePoint point)
        {
            return NavTilesArr[GetOffsetFromFeaturePoint(ref point)];
        }

        public bool CheckFeaturePointInBounds(ref FeaturePoint point)
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
                    map.Tiles[j].ToFeaturePoint(ref point);

                    NavTile navTile = GetNavTileAtFeaturePoint(ref point);

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
            tile.ToFeaturePoint(ref point);

            FeaturePoint featurePoint = FeaturePoint.FeaturePointPool.GetObject();

            Direction dir;

            NavTile navTile = GetNavTileAtFeaturePoint(ref point);

            if (navTile != null)
            {
                navTile.CalculateNavDirectionMask();

                for(int i = 0; i < 6; i++)
                {
                    dir = (Direction)i;

                    if (GetNeighboringNavTile(ref point, ref dir, ref featurePoint, out var neighborNavTile))
                    {
                        neighborNavTile.CalculateNavDirectionMask();
                    }
                }
            }

            FeaturePoint.FeaturePointPool.FreeObject(ref featurePoint);
        }


        private static ObjectPool<HashSet<NavTileWithParent>> _navTileWParentSetPool = new ObjectPool<HashSet<NavTileWithParent>>();
        private static ObjectPool<HashSet<NavTile>> _navTileSetPool = new ObjectPool<HashSet<NavTile>>();

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
            HashSet<NavTile> newNeighbors = _navTileSetPool.GetObject();

            FeaturePoint placeholderPoint = FeaturePoint.FeaturePointPool.GetObject();
            FeaturePoint currentFeaturePoint = FeaturePoint.FeaturePointPool.GetObject();

            NavTileWithParent currentTile;

            List<Direction> directions = new List<Direction>()
            { 
                Direction.South, Direction.SouthEast, Direction.SouthWest, 
                Direction.NorthEast, Direction.NorthWest, Direction.North
            };

            try
            {
                if (maximumDepth <= 0)
                    return false;

                if (!CheckFeaturePointInBounds(ref start) || !CheckFeaturePointInBounds(ref destination))
                {
                    return false;
                }

                NavTile destinationTile = GetNavTileAtFeaturePoint(ref destination);
                NavTile startTile = GetNavTileAtFeaturePoint(ref start);

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

                    currentTile.NavTile.Tile.ToFeaturePoint(ref currentFeaturePoint);

                    directions.Randomize();

                    for (int i = 0; i < 6; i++)
                    {
                        dir = directions[i];

                        if ((currentTile.NavTile.NavDirectionMask & NavTile.GetNavDirection(navType, dir)) > 0)
                        {
                            if (GetNeighboringNavTile(ref currentFeaturePoint, ref dir, ref placeholderPoint, out NavTile neighborTile))
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

                        if (visitedTiles.Contains(neighbor))
                        {
                            continue;
                        }
                        else
                        {
                            neighbor.Tile.ToFeaturePoint(ref placeholderPoint);

                            pathCost = currentTile.PathCost + neighbor.Tile.Properties.MovementCost;
                            distanceToEnd = FeatureEquation.GetDistanceBetweenPoints(placeholderPoint, destination);

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
                    currentTile = tile;
                    NavTileWithParent.Pool.FreeObject(ref currentTile);
                }

                tileList.Clear();
                _navTileWParentSetPool.FreeObject(ref tileList);

                visitedTiles.Clear();
                _navTileSetPool.FreeObject(ref visitedTiles);

                newNeighbors.Clear();
                _navTileSetPool.FreeObject(ref newNeighbors);

                FeaturePoint.FeaturePointPool.FreeObject(ref placeholderPoint);
                FeaturePoint.FeaturePointPool.FreeObject(ref currentFeaturePoint);
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

                    currNavTile = GetNavTileAtFeaturePoint(ref currFeaturePoint);

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

        private static ObjectPool<Vector3i> _vector3iPool = new ObjectPool<Vector3i>();
        public bool GetNeighboringNavTile(ref FeaturePoint point, ref Direction direction, ref FeaturePoint neighborPoint, out NavTile neighborTile)
        {
            var cube = _vector3iPool.GetObject();
            CubeMethods.OffsetToCube(point, ref cube);

            CubeMethods.CubeNeighborInPlace(ref cube, direction);

            CubeMethods.CubeToFeaturePoint(cube, ref neighborPoint);

            _vector3iPool.FreeObject(ref cube);

            if (!CheckFeaturePointInBounds(ref neighborPoint))
            {
                neighborTile = null;
                return false;
            }

            NavTile navTile = GetNavTileAtFeaturePoint(ref neighborPoint);

            neighborTile = navTile;
            return navTile != null;
        }
    }

    public enum NavDirections
    {
        None = 0,

        //-------Base-------
        Base_SouthWest = 1,
        Base_South = 2,
        Base_SouthEast = 4,
        Base_NorthEast = 8,
        Base_North = 16,
        Base_NorthWest = 32,

        //------Flying------
        Flying_SouthWest = 64,
        Flying_South = 128,
        Flying_SouthEast = 256,
        Flying_NorthEast = 512,
        Flying_North = 1024,
        Flying_NorthWest = 2048,

        //-----Aquatic------
        Aquatic_SouthWest = 4096,
        Aquatic_South = 8192,
        Aquatic_SouthEast = 16384,
        Aquatic_NorthEast = 32768,
        Aquatic_North = 65536,
        Aquatic_NorthWest = 131072,

        //---Semi-Aquatic---
        SemiAquatic_SouthWest = 262144,
        SemiAquatic_South = 524288,
        SemiAquatic_SouthEast = 1048576,
        SemiAquatic_NorthEast = 2097152,
        SemiAquatic_North = 4194304,
        SemiAquatic_NorthWest = 8388608,
    }

    public enum NavType
    {
        Base,
        Flying,
        Aquatic,
        Semi_Aquatic
    }

    public class NavTile
    {
        public Tile Tile;
        public NavDirections NavDirectionMask = NavDirections.None;
        public NavTile(Tile tile)
        {
            Tile = tile;

            CalculateNavDirectionMask();
        }

        public HashSet<TileEffect> GetTileEffects()
        {
            return TileEffectManager.GetTileEffectsOnTilePoint(Tile.TilePoint);
        }

        public static NavDirections GetNavDirection(NavType type, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    switch (type)
                    {
                        case NavType.Base:
                            return NavDirections.Base_North;
                        case NavType.Flying:
                            return NavDirections.Flying_North;
                        case NavType.Aquatic:
                            return NavDirections.Aquatic_North;
                        case NavType.Semi_Aquatic:
                            return NavDirections.SemiAquatic_North;
                    }
                    break;
                case Direction.NorthWest:
                    switch (type)
                    {
                        case NavType.Base:
                            return NavDirections.Base_NorthWest;
                        case NavType.Flying:
                            return NavDirections.Flying_NorthWest;
                        case NavType.Aquatic:
                            return NavDirections.Aquatic_NorthWest;
                        case NavType.Semi_Aquatic:
                            return NavDirections.SemiAquatic_NorthWest;
                    }
                    break;
                case Direction.NorthEast:
                    switch (type)
                    {
                        case NavType.Base:
                            return NavDirections.Base_NorthEast;
                        case NavType.Flying:
                            return NavDirections.Flying_NorthEast;
                        case NavType.Aquatic:
                            return NavDirections.Aquatic_NorthEast;
                        case NavType.Semi_Aquatic:
                            return NavDirections.SemiAquatic_NorthEast;
                    }
                    break;
                case Direction.SouthWest:
                    switch (type)
                    {
                        case NavType.Base:
                            return NavDirections.Base_SouthWest;
                        case NavType.Flying:
                            return NavDirections.Flying_SouthWest;
                        case NavType.Aquatic:
                            return NavDirections.Aquatic_SouthWest;
                        case NavType.Semi_Aquatic:
                            return NavDirections.SemiAquatic_SouthWest;
                    }
                    break;
                case Direction.South:
                    switch (type)
                    {
                        case NavType.Base:
                            return NavDirections.Base_South;
                        case NavType.Flying:
                            return NavDirections.Flying_South;
                        case NavType.Aquatic:
                            return NavDirections.Aquatic_South;
                        case NavType.Semi_Aquatic:
                            return NavDirections.SemiAquatic_South;
                    }
                    break;
                case Direction.SouthEast:
                    switch (type)
                    {
                        case NavType.Base:
                            return NavDirections.Base_SouthEast;
                        case NavType.Flying:
                            return NavDirections.Flying_SouthEast;
                        case NavType.Aquatic:
                            return NavDirections.Aquatic_SouthEast;
                        case NavType.Semi_Aquatic:
                            return NavDirections.SemiAquatic_SouthEast;
                    }
                    break;
            }

            return NavDirections.None;
        }

        public void CalculateNavDirectionMask()
        {
            NavDirectionMask = NavDirections.None;

            var tilePosCube = CubeMethods.OffsetToCube(Tile.ToFeaturePoint());

            List<FeaturePoint> neighbors = FeaturePoint.FeaturePointListPool.GetObject();

            for(int i = 0; i < 6; i++)
            {
                neighbors.Add(CubeMethods.CubeToFeaturePoint(CubeMethods.CubeNeighbor(ref tilePosCube, (Direction)i)));
            }

            for(int i = 0; i < neighbors.Count; i++)
            {
                if (TileMapHelpers.IsValidTile(neighbors[i]))
                {
                    var tile = TileMapHelpers.GetTile(neighbors[i]);
                    if(tile != null)
                    {
                        //check if it's valid and for which types

                        bool validGround = CheckTileGround(Tile, tile);
                        bool validAir = CheckTileFlying(Tile, tile);
                        bool validAquatic = CheckTileAquatic(Tile, tile);
                        bool validSemiAquatic = CheckTileSemiAquatic(Tile, tile);

                        switch (i)
                        {
                            case (int)Direction.SouthWest:
                                #region SouthWest
                                if (validGround)
                                {
                                    NavDirectionMask |= NavDirections.Base_SouthWest;
                                }

                                if (validAir)
                                {
                                    NavDirectionMask |= NavDirections.Flying_SouthWest;
                                }

                                if (validAquatic)
                                {
                                    NavDirectionMask |= NavDirections.Aquatic_SouthWest;
                                }

                                if (validSemiAquatic)
                                {
                                    NavDirectionMask |= NavDirections.SemiAquatic_SouthWest;
                                }
                                #endregion
                                break;
                            case (int)Direction.South:
                                #region South
                                if (validGround)
                                {
                                    NavDirectionMask |= NavDirections.Base_South;
                                }

                                if (validAir)
                                {
                                    NavDirectionMask |= NavDirections.Flying_South;
                                }

                                if (validAquatic)
                                {
                                    NavDirectionMask |= NavDirections.Aquatic_South;
                                }

                                if (validSemiAquatic)
                                {
                                    NavDirectionMask |= NavDirections.SemiAquatic_South;
                                }
                                #endregion
                                break;
                            case (int)Direction.SouthEast:
                                #region SouthEast
                                if (validGround)
                                {
                                    NavDirectionMask |= NavDirections.Base_SouthEast;
                                }

                                if (validAir)
                                {
                                    NavDirectionMask |= NavDirections.Flying_SouthEast;
                                }

                                if (validAquatic)
                                {
                                    NavDirectionMask |= NavDirections.Aquatic_SouthEast;
                                }

                                if (validSemiAquatic)
                                {
                                    NavDirectionMask |= NavDirections.SemiAquatic_SouthEast;
                                }
                                #endregion
                                break;
                            case (int)Direction.NorthEast:
                                #region NorthEast
                                if (validGround)
                                {
                                    NavDirectionMask |= NavDirections.Base_NorthEast;
                                }

                                if (validAir)
                                {
                                    NavDirectionMask |= NavDirections.Flying_NorthEast;
                                }

                                if (validAquatic)
                                {
                                    NavDirectionMask |= NavDirections.Aquatic_NorthEast;
                                }

                                if (validSemiAquatic)
                                {
                                    NavDirectionMask |= NavDirections.SemiAquatic_NorthEast;
                                }
                                #endregion
                                break;
                            case (int)Direction.North:
                                #region North
                                if (validGround)
                                {
                                    NavDirectionMask |= NavDirections.Base_North;
                                }

                                if (validAir)
                                {
                                    NavDirectionMask |= NavDirections.Flying_North;
                                }

                                if (validAquatic)
                                {
                                    NavDirectionMask |= NavDirections.Aquatic_North;
                                }

                                if (validSemiAquatic)
                                {
                                    NavDirectionMask |= NavDirections.SemiAquatic_North;
                                }
                                #endregion
                                break;
                            case (int)Direction.NorthWest:
                                #region NorthWest
                                if (validGround)
                                {
                                    NavDirectionMask |= NavDirections.Base_NorthWest;
                                }

                                if (validAir)
                                {
                                    NavDirectionMask |= NavDirections.Flying_NorthWest;
                                }

                                if (validAquatic)
                                {
                                    NavDirectionMask |= NavDirections.Aquatic_NorthWest;
                                }

                                if (validSemiAquatic)
                                {
                                    NavDirectionMask |= NavDirections.SemiAquatic_NorthWest;
                                }
                                #endregion
                                break;
                        }
                    }
                }
            }

            neighbors.Clear();
            FeaturePoint.FeaturePointListPool.FreeObject(ref neighbors);
        }

        private bool CheckTileGround(Tile source, Tile destination)
        {
            if(Math.Abs(source.GetPathableHeight() - destination.GetPathableHeight()) > 1)
            {
                return false;
            }

            if (destination.Properties.Classification != TileClassification.Ground)
            {
                return false;
            }

            if(destination.Structure != null && !destination.Structure.Pathable)
            {
                return false;
            }

            return true;
        }

        private bool CheckTileFlying(Tile source, Tile destination)
        {
            if (Math.Abs(source.GetPathableHeight() - destination.GetPathableHeight()) > 5)
            {
                return false;
            }

            if (destination.Properties.Classification == TileClassification.ImpassableAir)
            {
                return false;
            }

            return true;
        }

        private bool CheckTileAquatic(Tile source, Tile destination)
        {
            if (Math.Abs(source.GetPathableHeight() - destination.GetPathableHeight()) > 1)
            {
                return false;
            }

            if (destination.Properties.Classification != TileClassification.Water)
            {
                return false;
            }

            if (destination.Structure != null && !destination.Structure.Pathable)
            {
                return false;
            }

            return true;
        }

        private bool CheckTileSemiAquatic(Tile source, Tile destination)
        {
            if (Math.Abs(source.GetPathableHeight() - destination.GetPathableHeight()) > 1)
            {
                return false;
            }

            if (!(destination.Properties.Classification == TileClassification.Water ||
                destination.Properties.Classification == TileClassification.Ground))
            {
                return false;
            }

            if (destination.Structure != null && !destination.Structure.Pathable)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is NavTile tile &&
                   EqualityComparer<Tile>.Default.Equals(Tile, tile.Tile);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tile);
        }
    }

    public class NavTileWithParent
    {
        public NavTileWithParent Parent = null;
        public NavTile NavTile;
        public float PathCost = 0; //Path cost
        public float DistanceToEnd = 0; //Distance to end
        public bool Visited = false;

        public static ObjectPool<NavTileWithParent> Pool = new ObjectPool<NavTileWithParent>();
        //Add an aversion field here and include it in the algorithm

        public NavTileWithParent() { }
        public NavTileWithParent(NavTile navTile, NavTileWithParent parent = null)
        {
            Parent = parent;
            NavTile = navTile;
        }

        public void Initialize(NavTile navTile, NavTileWithParent parent = null)
        {
            Parent = parent;
            NavTile = navTile;
            Visited = false;
        }

        public float GetCurrentMinimumDepth()
        {
            return PathCost + DistanceToEnd;
        }
    }
}
