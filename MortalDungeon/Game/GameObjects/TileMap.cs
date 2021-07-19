using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MortalDungeon.Game.GameObjects
{
    public enum Direction
    {
        SouthWest,
        South,
        SouthEast,
        NorthEast,
        North,
        NorthWest,
    }

    public static class TileMapConstants 
    {
        public static Dictionary<Direction, Vector3i> CubeDirections = new Dictionary<Direction, Vector3i>
        {
            { Direction.SouthWest, new Vector3i(-1, 0, 1) },
            { Direction.South, new Vector3i(0, -1, 1) },
            { Direction.SouthEast, new Vector3i(1, -1, 0) },
            { Direction.NorthEast, new Vector3i(1, 0, -1) },
            { Direction.North, new Vector3i(0, 1, -1) },
            { Direction.NorthWest, new Vector3i(-1, 1, 0) },
        };

        /// <summary>
        /// Linear interpolation between 2 values
        /// </summary>
        /// <param name="a">first value</param>
        /// <param name="b">second value</param>
        /// <param name="t">value between 0 and 1.0</param>
        /// <returns>the interpolation between a and b</returns>
        public static float lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static Vector3 cube_lerp(Vector3 start, Vector3 end, float t)
        {
            return new Vector3(lerp(start.X, end.X, t), lerp(start.Y, end.Y, t), lerp(start.Z, end.Z, t));
        }
        public static Vector3i cube_round(Vector3 cube, bool reverse = false) 
        {
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
            else if (y_diff > (z_diff - 0.0001f))
            {
                ry = -rx - rz;
            }
            else 
            {
                rz = -rx - ry;
            }

            return new Vector3i((int)rx, (int)ry, (int)rz);
        }
        public static Vector3i cube_neighbor(Vector3i cube, Direction direction) 
        {
            return cube + CubeDirections[direction];
        }
    }

    public class TileMap : GameObject //grid of tiles
    {
        public int Width = 30;
        public int Height = 30;

        public List<BaseTile> Tiles = new List<BaseTile>();
        public TileMap(Vector3 position, int id = 0, string name = "TileMap")
        {
            Position = position; //position of the first tile
            Name = name;
        }

        public BaseTile this[int i]
        {
            get { return Tiles[i]; }
        }

        public BaseTile this[int i, int n]
        {
            get { return Tiles[i * Height + n]; }
        }

        private static Random _randomNumberGen = new Random();

        public void PopulateTileMap(float zTilePlacement = 0)
        {
            BaseTile baseTile = new BaseTile();
            Vector3 tilePosition = new Vector3(Position);

            tilePosition.Z += zTilePlacement;

            for (int i = 0; i < Width; i++)
            {
                for (int o = 0; o < Height; o++)
                {
                    baseTile = new BaseTile(tilePosition, i * Width + o) { Clickable = true };

                    Tiles.Add(baseTile);

                    if (_randomNumberGen.NextDouble() < 0.2d && baseTile.TileIndex != 0) //add a bit on randomness to tile gen
                    {
                        baseTile.TileClassification = TileClassification.Terrain;
                        baseTile.DefaultAnimation = Objects.BaseTileAnimationType.SolidWhite;
                        baseTile.DefaultColor = new Vector4(0.25f, 0.25f, 0.25f, 1);
                    }

                    tilePosition.Y += baseTile.BaseObjects[0].Dimensions.Y;
                }
                tilePosition.X = (i + 1) * baseTile.BaseObjects[0].Dimensions.X / 1.29f;
                tilePosition.Y = ((i + 1) % 2 == 0 ? 0 : baseTile.BaseObjects[0].Dimensions.Y / -2);
                tilePosition.Z += 0.0001f;
            }

            SetDefaultTileValues();
        }

        public Vector3 GetPositionOfTile(int index)
        {
            Vector3 position = new Vector3();
            if (index < Width * Height && index >= 0)
            {
                position = Tiles[index].Position;
            }
            else if (index < 0 && Tiles.Count > 0)
            {
                position = Tiles[0].Position;
            }
            else if (index >= Tiles.Count)
            {
                position = Tiles[Tiles.Count - 1].Position;
            }

            return position;
        }

        public List<BaseTile> GetTilesInRadius(int index, int radius)
        {
            List<BaseTile> tileList = new List<BaseTile>();

            int x = index / Height;
            int y = index - (x * Height);

            if (radius > 0 && IsValidTile(index))
            {
                //center
                tileList.Add(Tiles[index]);

                for (int i = 0; i < radius; i++)
                {
                    if (IsValidTile(x, y + i + 1))
                    {
                        tileList.Add(this[x, y + i + 1]); //tiles above center
                    }
                    if (IsValidTile(x, y - i - 1))
                    {
                        tileList.Add(this[x, y - i - 1]); //tiles below center
                    }

                    int sideHeight = radius * 2 - i;
                    int sideX = i + 1;
                    int sideY = -sideHeight / 2 - (-(i + 1) % 2 * (x + 1) % 2);

                    for (int j = 0; j < sideHeight; j++)
                    {
                        if (IsValidTile(x + sideX, y + sideY))
                        {
                            tileList.Add(this[x + sideX, y + sideY]); //tiles in layers to the right of the center
                        }

                        if (IsValidTile(x - sideX, y + sideY))
                        {
                            tileList.Add(this[x - sideX, y + sideY]); //tiles in layers to the left of the center
                        }
                        sideY++;
                    }
                }
            }

            return tileList;
        }

        #region Tile validity checks
        public bool IsValidTile(int tileIndex)
        {
            return tileIndex >= 0 && tileIndex < Width * Height;
        }
        public bool IsValidTile(int xIndex, int yIndex)
        {
            return xIndex >= 0 && yIndex >= 0 && xIndex < Width && yIndex < Height;
        }
        public bool IsValidTile(Vector2i offsetCoord)
        {
            return IsValidTile(offsetCoord.X, offsetCoord.Y);
        }
        public bool IsValidTile(Vector3i cube) 
        {
            return IsValidTile(CubeToOffset(cube));
        }
        #endregion

        #region Tile conversions
        public Vector2i ConvertIndexToCoord(int index) //assumes the index is valid
        {
            int x = index / Height;
            int y = index - (x * Height);
            return new Vector2i(x, y);
        }
        public int ConvertCoordToIndex(Vector2i coord) //assumes the index is valid
        {
            return coord.X * Height + coord.Y;
        }
        public Vector2i CubeToOffset(Vector3i cube)
        {
            Vector2i offsetCoord = new Vector2i();
            offsetCoord.X = cube.X;
            offsetCoord.Y = cube.Z + (cube.X + (cube.X & 1)) / 2;

            return offsetCoord;
        }
        public Vector3i OffsetToCube(Vector2i offset)
        {
            Vector3i cubeCoord = new Vector3i();
            cubeCoord.X = offset.X;
            cubeCoord.Z = offset.Y - (offset.X + (offset.X & 1)) / 2;
            cubeCoord.Y = -cubeCoord.X - cubeCoord.Z;

            return cubeCoord;
        }
        /// <summary>
        /// Sets any coordinate values below 0 to 0 and any above the width/height to the width/height
        /// </summary>
        /// <param name="offsetCoord"></param>
        /// <returns></returns>
        public Vector2i ClampTileCoordsToMapValues(Vector2i offsetCoord) 
        {
            Vector2i returnValue = new Vector2i(offsetCoord.X, offsetCoord.Y);

            if (offsetCoord.X < 0)
                returnValue.X = 0;
            if (offsetCoord.Y < 0)
                returnValue.Y = 0;
            if (offsetCoord.X >= Width)
                returnValue.X = Width - 1;
            if (offsetCoord.Y >= Height)
                returnValue.Y = Height - 1;

            return returnValue;
        }
        #endregion

        public void SetDefaultTileValues()
        {
            Tiles.ForEach(tile =>
            {
                if (tile.InFog)
                {
                    tile.SetFogColor();
                }
                else 
                {
                    tile.SetColor(tile.DefaultColor);
                }
                tile.SetAnimation(tile.DefaultAnimation);
            });
        }

        public override void Tick()
        {
            base.Tick();

            Tiles.ForEach(tile =>
            {
                tile.Tick();
            });
        }

        #region Distance between points
        public int GetDistanceBetweenPoints(Vector2i start, Vector2i end)
        {
            Vector3i a = OffsetToCube(start);
            Vector3i b = OffsetToCube(end);

            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }
        public int GetDistanceBetweenPoints(int startIndex, int endIndex)
        {
            Vector3i a = OffsetToCube(ConvertIndexToCoord(startIndex));
            Vector3i b = OffsetToCube(ConvertIndexToCoord(endIndex));

            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }
        public int GetDistanceBetweenPoints(Vector3i a, Vector3i b)
        {
            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }
        #endregion

        //gets tiles in a radius from a center point by expanding outward to all valid neighbors until the radius is reached.
        public List<BaseTile> FindValidTilesInRadius(int index, int radius, List<TileClassification> traversableTypes, List<Unit> units = default, Unit castingUnit = null, AbilityTypes abilityType = AbilityTypes.Empty) 
        {
            List<BaseTile> tileList = new List<BaseTile>();
            List<BaseTile> neighbors = new List<BaseTile>();

            Vector2i tilePos = ConvertIndexToCoord(index);
            bool[] visitedTiles = new bool[Tiles.Count];

            if (!traversableTypes.Exists(c => c == this[index].TileClassification))
            {
                return tileList;
            }

            tileList.Add(this[index]);
            neighbors.Add(this[index]);
            visitedTiles[index] = true;

            List<BaseTile> newNeighbors = new List<BaseTile>();

            for (int i = 0; i < radius; i++) 
            {
                newNeighbors.Clear();
                neighbors.ForEach(n =>
                {
                    GetNeighboringTiles(ConvertIndexToCoord(n.TileIndex), newNeighbors, visitedTiles);
                });

                neighbors.Clear();
                for (int j = 0; j < newNeighbors.Count; j++) 
                {
                    neighbors.Add(newNeighbors[j]);
                }

                for (int j = 0; j < neighbors.Count; j++)
                {
                    int unitIndex = -1;
                    int count = -1;
                    units?.Exists(u => 
                    {
                        count++;
                        if (u.TileMapPosition == neighbors[j].TileIndex) 
                        {
                            unitIndex = count;
                            return true;
                        }
                        else
                            return false;
                    });

                    if (!traversableTypes.Exists(c => c == neighbors[j].TileClassification))
                    {
                        neighbors.RemoveAt(j);
                        j--;
                    }
                    else if (unitIndex != -1 && castingUnit != null && abilityType == AbilityTypes.Move) //special cases for ability targeting should go here
                    {
                        if (units[unitIndex].BlocksSpace && !castingUnit.PhasedMovement) 
                        {
                            neighbors.RemoveAt(j);
                            j--;
                        }
                    }
                    else
                    {
                        tileList.Add(neighbors[j]);
                    }
                }
            }


            return tileList;
        }

        /// <summary>
        /// Gets a line of tiles that begins at the startIndex and ends and the endIndex
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public List<BaseTile> GetLineOfTiles(int startIndex, int endIndex) 
        {
            List<BaseTile> tileList = new List<BaseTile>();
            Vector2i start = ConvertIndexToCoord(startIndex);
            Vector2i end = ConvertIndexToCoord(endIndex);

            Vector3i startCube = OffsetToCube(start);
            Vector3i endCube = OffsetToCube(end);

            int N = GetDistanceBetweenPoints(startCube, endCube);
            float n = 1f / N;

            Vector3 currentCube;
            Vector2i currentOffset;

            for (int i = 0; i <= N; i++) 
            {
                currentCube = TileMapConstants.cube_lerp(startCube, endCube, n * i);

                currentOffset = CubeToOffset(TileMapConstants.cube_round(currentCube));
                if (IsValidTile(currentOffset.X, currentOffset.Y)) 
                {
                    tileList.Add(this[currentOffset.X, currentOffset.Y]);
                }
            }

            return tileList;
        }

        public void GetRingOfTiles(int startIndex, List<BaseTile> outputList, int radius = 1, bool includeEdges = true) 
        {
            GetRingOfTiles(ConvertIndexToCoord(startIndex), outputList, radius, includeEdges);
        }

        /// <summary>
        /// Returns the list of BaseTiles that create a ring with a radius of the passed in parameter
        /// </summary>
        /// <param name="start"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public void GetRingOfTiles(Vector2i start, List<BaseTile> outputList, int radius = 1, bool includeEdges = true)
        {
            Vector3i cubePosition = OffsetToCube(start);

            cubePosition += TileMapConstants.CubeDirections[Direction.North] * radius;

            Vector2i tileOffsetCoord;

            for (int i = 0; i < 6; i++) 
            {
                for (int j = 0; j < radius; j++) 
                {
                    tileOffsetCoord = CubeToOffset(cubePosition);
                    if (IsValidTile(tileOffsetCoord))
                    {
                        outputList.Add(this[tileOffsetCoord.X, tileOffsetCoord.Y]);
                    }
                    else if (includeEdges) 
                    {
                        tileOffsetCoord = ClampTileCoordsToMapValues(tileOffsetCoord);
                        outputList.Add(this[tileOffsetCoord.X, tileOffsetCoord.Y]);
                    }

                    cubePosition += TileMapConstants.CubeDirections[(Direction)i];
                }
            }
        }

        /// <summary>
        /// Returns a list of BaseTiles in a line from startIndex to endIndex or from startIndex to the first instance of a tile that blocks
        /// vision or has a TileClassification from untraversableTypes or contains a unit from the units parameter.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="untraversableTypes">tile types that will block vision</param>
        /// <param name="units"></param>
        /// <returns></returns>
        public void GetVisionLine(int startIndex, int endIndex, List<BaseTile> outputList, List<TileClassification> untraversableTypes = default, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            Vector2i start = ConvertIndexToCoord(startIndex);
            Vector2i end = ConvertIndexToCoord(endIndex);

            Vector3i startCube = OffsetToCube(start);
            Vector3i endCube = OffsetToCube(end);

            int N = GetDistanceBetweenPoints(startCube, endCube);
            float n = 1f / N;

            Vector3 currentCube;
            Vector2i currentOffset;

            for (int i = 0; i <= N; i++)
            {
                currentCube = TileMapConstants.cube_lerp(startCube, endCube, n * i);
                currentOffset = CubeToOffset(TileMapConstants.cube_round(currentCube));
                if (IsValidTile(currentOffset.X, currentOffset.Y))
                {
                    _BaseTileTemp = this[currentOffset.X, currentOffset.Y];
                    outputList.Add(_BaseTileTemp);

                    if ((_BaseTileTemp.BlocksVision && !ignoreBlockedVision) || (untraversableTypes != null && untraversableTypes.Contains(_BaseTileTemp.TileClassification)))
                    {
                        return;
                    }

                    if (units?.Count > 0) 
                    {
                        int tileMapIndex = ConvertCoordToIndex(currentOffset);
                        if (units.Exists(unit => unit.TileMapPosition == tileMapIndex)) 
                        {
                            return;
                        }
                    }
                }
                
            }
        }

        /// <summary>
        /// Returns a list of BaseTiles that are not in the fog of war in a radius around an index
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="untraversableTypes"></param>
        /// <param name="units"></param>
        /// <param name="ignoreBlockedVision"></param>
        /// <returns></returns>
        public List<BaseTile> GetVisionInRadius(int originIndex, int radius, List<TileClassification> untraversableTypes = default, List<Unit> units = default, bool ignoreBlockedVision = false) 
        {
            List<BaseTile> ringOfTiles = new List<BaseTile>();
            GetRingOfTiles(originIndex, ringOfTiles, radius);

            List<BaseTile> outputList = new List<BaseTile>();

            for (int i = 0; i < ringOfTiles.Count; i++) 
            {
                GetVisionLine(originIndex, ringOfTiles[i].TileIndex, outputList, untraversableTypes, units, ignoreBlockedVision);
            }

            return outputList.Distinct().ToList();
        }

        /// <summary>
        /// Get a list of tiles that leads from the start index to the end index.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="depth">The maximum length of the path</param>
        /// <returns></returns>
        public List<BaseTile> GetPathToPoint(int startIndex, int endIndex, int depth, List<TileClassification> traversableTypes, List<Unit> units = default, Unit castingUnit = null, AbilityTypes abilityType = AbilityTypes.Empty) 
        {
            List<TileWithParent> tileList = new List<TileWithParent>();
            List<BaseTile> returnList = new List<BaseTile>();

            List<BaseTile> neighbors = new List<BaseTile>();

            Vector2i startingPos = ConvertIndexToCoord(startIndex);
            Vector2i endingPos = ConvertIndexToCoord(endIndex);

            bool[] visitedTiles = new bool[Tiles.Count]; //this assumption might need to be revisited if multiple maps can be placed side by side

            if (!traversableTypes.Exists(c => c == this[startIndex].TileClassification) || !traversableTypes.Exists(c => c == this[endIndex].TileClassification))
            {
                return new List<BaseTile>(); //if the starting or ending tile isn't traversable then immediately return
            }



            neighbors.Add(this[startIndex]);
            visitedTiles[startIndex] = true;

            tileList.Add(new TileWithParent(this[startIndex]));

            List<BaseTile> newNeighbors = new List<BaseTile>();


            for (int i = 0; i < depth; i++)
            {
                newNeighbors.Clear();
                neighbors.ForEach(p =>
                {
                    GetNeighboringTiles(ConvertIndexToCoord(p.TileIndex), newNeighbors, visitedTiles);

                    newNeighbors.ForEach(neighbor =>
                    {
                        tileList.Add(new TileWithParent(neighbor, p));
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
                    int unitIndex = -1;
                    int count = -1;

                    if (neighbors[j].TileIndex == endIndex) 
                    {
                        //if we found the destination tile then fill the returnList and return
                        TileWithParent finalTile = tileList.Find(t => t.Tile.TileIndex == endIndex);

                        returnList.Add(finalTile.Tile);

                        BaseTile parent = finalTile.Parent;
                        
                        while (parent != null) 
                        {
                            TileWithParent currentTile = tileList.Find(t => t.Tile.TileIndex == parent.TileIndex);
                            returnList.Add(currentTile.Tile);

                            parent = currentTile.Parent;
                        }

                        returnList.Reverse();
                        return returnList;
                    }

                    int distanceToDestination = GetDistanceBetweenPoints(neighbors[j].TileIndex, endIndex);

                    if (distanceToDestination + i > depth) 
                    {
                        //if the best case path to the end point is longer than our remaining movement cut off this branch and continue
                        neighbors.RemoveAt(j);
                        j--;
                        continue;
                    }

                    //find if there's a unit in this space
                    units?.Exists(u =>
                    {
                        count++;
                        if (u.TileMapPosition == neighbors[j].TileIndex)
                        {
                            unitIndex = count;
                            return true;
                        }
                        else
                            return false;
                    });

                    if (!traversableTypes.Exists(c => c == neighbors[j].TileClassification))
                    {
                        //if the space contains a tile that is not traversable cut off this branch and continue
                        neighbors.RemoveAt(j);
                        j--;
                        continue;
                    }
                    else if (unitIndex != -1 && castingUnit != null && abilityType == AbilityTypes.Move) //special cases for ability targeting should go here
                    {
                        if (units[unitIndex].BlocksSpace && !castingUnit.PhasedMovement)
                        {
                            //if this is a movement ability and the unit using the ability does not have phased movement cut off this branch and continue
                            neighbors.RemoveAt(j);
                            j--;
                            continue;
                        }
                    }
                }
            }

            return returnList;
        }

        /// <summary>
        /// Fills the passed neighborList List with all tiles that surround the tile contained in tilePos position
        /// </summary>
        /// <param name="tilePos"></param>
        /// <param name="neighborList"></param>
        /// <param name="visitedTiles"></param>
        private void GetNeighboringTiles(Vector2i tilePos, List<BaseTile> neighborList, bool[] visitedTiles, bool shuffle = true) 
        {
            Vector2i neighborPos = new Vector2i(tilePos.X, tilePos.Y); 
            int neighborIndex = ConvertCoordToIndex(tilePos);
            int yOffset = tilePos.X % 2 == 0 ? -1 : 0;

            for (int i = 0; i < 6; i++) 
            {
                switch (i) 
                {
                    case 0: //tile above
                        neighborPos.Y += 1;
                        neighborIndex += 1;
                        break;
                    case 1: //tile below
                        neighborPos.Y -= 2;
                        neighborIndex -= 2;
                        break;
                    case 2: //tile bottom left
                        neighborPos.X -= 1;
                        neighborPos.Y += 1 - yOffset;
                        neighborIndex += 1 - yOffset - Height;
                        break;
                    case 3: //tile top left
                        neighborPos.Y -= 1;
                        neighborIndex -= 1;
                        break;
                    case 4: //tile top right
                        neighborPos.X += 2;
                        neighborIndex += Height * 2;
                        break;
                    case 5: //tile top right
                        neighborPos.Y += 1;
                        neighborIndex += 1;
                        break;
                }

                if (IsValidTile(neighborPos.X, neighborPos.Y)) 
                {
                    if (!visitedTiles[neighborIndex])
                    {
                        neighborList.Add(this[neighborIndex]);
                        visitedTiles[neighborIndex] = true;
                    }
                }
            }

            if (shuffle) 
            {
                ShuffleList(neighborList);
            }
        }

        private BaseTile _BaseTileTemp = new BaseTile();
        private List<TileClassification> _EmptyTileClassification = new List<TileClassification>();

        internal class TileWithParent 
        {
            public BaseTile Parent = null;
            public BaseTile Tile;

            public TileWithParent(BaseTile tile, BaseTile parent = null) 
            {
                Parent = parent;
                Tile = tile;
            }
        }

        static void ShuffleList<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _randomNumberGen.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
