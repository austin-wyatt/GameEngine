using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Game.Abilities;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Combat
{
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

        public NavTile()
        {

        }
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
                            return NavDirections.Base_North | NavDirections.Aquatic_North;
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
                            return NavDirections.Base_NorthWest | NavDirections.Aquatic_NorthWest;
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
                            return NavDirections.Base_NorthEast | NavDirections.Aquatic_NorthEast;
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
                            return NavDirections.Base_SouthWest | NavDirections.Aquatic_SouthWest;
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
                            return NavDirections.Base_South | NavDirections.Aquatic_South;
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
                            return NavDirections.Base_SouthEast | NavDirections.Aquatic_SouthEast;
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

            for (int i = 0; i < 6; i++)
            {
                neighbors.Add(CubeMethods.CubeToFeaturePoint(CubeMethods.CubeNeighbor(ref tilePosCube, (Direction)i)));
            }

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (TileMapHelpers.IsValidTile(neighbors[i]))
                {
                    var tile = TileMapHelpers.GetTile(neighbors[i]);
                    if (tile != null)
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
                                    NavDirectionMask |= NavDirections.Aquatic_SouthWest;
                                    NavDirectionMask |= NavDirections.Base_SouthWest;
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
                                    NavDirectionMask |= NavDirections.Aquatic_South;
                                    NavDirectionMask |= NavDirections.Base_South;
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
                                    NavDirectionMask |= NavDirections.Aquatic_SouthEast;
                                    NavDirectionMask |= NavDirections.Base_SouthEast;
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
                                    NavDirectionMask |= NavDirections.Aquatic_NorthEast;
                                    NavDirectionMask |= NavDirections.Base_NorthEast;
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
                                    NavDirectionMask |= NavDirections.Aquatic_North;
                                    NavDirectionMask |= NavDirections.Base_North;
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
                                    NavDirectionMask |= NavDirections.Aquatic_NorthWest;
                                    NavDirectionMask |= NavDirections.Base_NorthWest;
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
            if (Math.Abs(source.GetPathableHeight() - destination.GetPathableHeight()) > 1)
            {
                return false;
            }

            if (destination.Properties.Classification != TileClassification.Ground)
            {
                return false;
            }

            if (destination.Structure != null && !destination.Structure.Pathable)
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

        public override bool Equals(object obj)
        {
            return obj is NavTileWithParent parent &&
                   EqualityComparer<NavTile>.Default.Equals(NavTile, parent.NavTile);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NavTile);
        }
    }
}
