using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Game.Units.AIFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MortalDungeon.Game.Combat
{
    public class NavMesh
    {
        public Dictionary<FeaturePoint, NavTile> NavTiles = new Dictionary<FeaturePoint, NavTile>();

        private object _navTileLock = new object();
        public void CalculateNavTiles()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            lock (_navTileLock)
            {
                NavTiles.Clear();

                for (int i = 0; i < TileMapManager.ActiveMaps.Count; i++)
                {
                    for (int j = 0; j < TileMapManager.ActiveMaps[i].Tiles.Count; j++)
                    {
                        FeaturePoint point = TileMapManager.ActiveMaps[i].Tiles[j].ToFeaturePoint();
                        NavTile navTile = new NavTile(TileMapManager.ActiveMaps[i].Tiles[j]);
                        NavTiles.Add(point, navTile);
                    }
                }
            }

            Console.WriteLine($"NavMesh calculated in {stopwatch.ElapsedMilliseconds}ms");
        }

        public void UpdateNavMeshForTileMap(TileMap map)
        {
            lock (_navTileLock)
            {
                for (int j = 0; j < map.Tiles.Count; j++)
                {
                    FeaturePoint point = map.Tiles[j].ToFeaturePoint();

                    if (NavTiles.TryGetValue(point, out NavTile navTile))
                    {
                        navTile.CalculateNavDirectionMask();
                    }
                }
            }
        }

        public void UpdateNavMeshForTile(Tile tile)
        {
            var point = tile.ToFeaturePoint();

            if (NavTiles.TryGetValue(point, out NavTile navTile))
            {
                navTile.CalculateNavDirectionMask();

                for(int i = 0; i < 6; i++)
                {
                    if(GetNeighboringNavTile(point, (Direction)i, out var neighborPoint, out var neighborNavTile))
                    {
                        neighborNavTile.CalculateNavDirectionMask();
                    }
                }
            }
        }


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
            float maximumDepth = 10, Unit pathingUnit = null, bool considerCaution = false, GroupedMovementParams movementParams = null,
            bool allowEndInUnit = false)
        {
            HashSet<NavTileWithParent> tileList = new HashSet<NavTileWithParent>();
            returnList = new List<Tile>();

            if (maximumDepth <= 0)
                return false;

            if(!NavTiles.ContainsKey(start) || !NavTiles.ContainsKey(destination))
            {
                return false;
            }

            NavTile destinationTile = NavTiles[destination];
            NavTile startTile = NavTiles[start];
            

            if (UnitPositionManager.GetUnitsOnTilePoint(destinationTile.Tile).Count > 0 && !allowEndInUnit)
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

            NavTileWithParent currentTile = new NavTileWithParent(startTile)
            {
                PathCost = FeatureEquation.GetDistanceBetweenPoints(start, start),
                DistanceToEnd = FeatureEquation.GetDistanceBetweenPoints(start, destination),
            };

            HashSet<NavTile> visitedTiles = new HashSet<NavTile>();

            visitedTiles.Add(NavTiles[start]);

            tileList.Add(currentTile);

            HashSet<NavTile> newNeighbors = new HashSet<NavTile>();

            float unitCaution = considerCaution ? pathingUnit?.AI.Feelings.GetFeelingValue(FeelingType.Caution) ?? 0f : 0f;

            bool hasMovementAbilities = movementParams != null && movementParams.Params.Count > 0;

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

                for(int i = 0; i < 6; i++)
                {
                    if (currentTile.NavTile.NavDirectionMask.HasFlag(NavTile.GetNavDirection(navType, (Direction)i)))
                    {
                        //TODO? change this function call to be a field access "currentTile.NavTile.Tile.ToFeaturePoint()"
                        if (GetNeighboringNavTile(currentTile.NavTile.Tile.ToFeaturePoint(), (Direction)i, out var neighborPoint, out NavTile neighborTile))
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
                        bool unitOnSpace = false;

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

                    bool skipNeighbor = false;
                    foreach (var tileEffect in tileEffectsOnPoint)
                    {
                        if (pathingUnit != null)
                        {
                            //if the unit isn't immune then we check their caution
                            if (!tileEffect.Immunities.Exists(immunity => pathingUnit.Info.StatusManager.CheckCondition(immunity)))
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
                        float pathCost = currentTile.PathCost + neighbor.Tile.Properties.MovementCost;
                        float distanceToEnd = FeatureEquation.GetDistanceBetweenPoints(neighbor.Tile.ToFeaturePoint(), destination);

                        if (pathCost + distanceToEnd <= maximumDepth)
                        {
                            NavTileWithParent tile = new NavTileWithParent(neighbor, currentTile)
                            {
                                PathCost = pathCost,
                                DistanceToEnd = distanceToEnd,
                            };

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

                    returnList.Add(TileMapHelpers.GetTile(currentTile.NavTile.Tile));
                    returnList.Reverse();
                    return true;
                    #endregion
                }

                bool tileChanged = false;
                foreach(var tile in tileList)
                {
                    if (!tile.Visited)
                    {
                        if (!tileChanged)
                        {
                            currentTile = tile;
                            tileChanged = true;
                        }

                        if ((tile.CurrentMinimumDepth < currentTile.CurrentMinimumDepth) || (tile.CurrentMinimumDepth == currentTile.CurrentMinimumDepth && tile.DistanceToEnd < currentTile.DistanceToEnd))
                        {
                            currentTile = tile;
                        }
                    }
                }

                if (currentTile.Visited)
                {
                    return false;
                }

                if (currentTile.CurrentMinimumDepth > maximumDepth)
                {
                    return false;
                }
            }
        }

        public bool GetNeighboringNavTile(FeaturePoint point, Direction direction, out FeaturePoint neighborPoint, out NavTile neighborTile)
        {
            var cube = CubeMethods.OffsetToCube(point);
            cube = CubeMethods.CubeNeighbor(cube, direction);

            var featurePoint = CubeMethods.CubeToFeaturePoint(cube);

            if(NavTiles.TryGetValue(featurePoint, out var navTile))
            {
                neighborPoint = featurePoint;
                neighborTile = navTile;
                return true;
            }
            else
            {
                neighborPoint = new FeaturePoint();
                neighborTile = null;
                return false;
            }
        }
    }

    [Flags]
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
        public HashSet<TileEffect> TileEffects => TileEffectManager.GetTileEffectsOnTilePoint(Tile.TilePoint);

        public NavTile(Tile tile)
        {
            Tile = tile;

            CalculateNavDirectionMask();
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

            List<FeaturePoint> neighbors = new List<FeaturePoint>();

            for(int i = 0; i < 6; i++)
            {
                neighbors.Add(CubeMethods.CubeToFeaturePoint(CubeMethods.CubeNeighbor(tilePosCube, (Direction)i)));
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
        public float CurrentMinimumDepth => PathCost + DistanceToEnd;
        public bool Visited = false;

        //Add an aversion field here and include it in the algorithm

        public NavTileWithParent(NavTile navTile, NavTileWithParent parent = null)
        {
            Parent = parent;
            NavTile = navTile;
        }
    }
}
