using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Lighting;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Tiles
{
    public enum Direction
    {
        None = -1,
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

        public static List<TileClassification> AllTileClassifications = new List<TileClassification>()
        { TileClassification.Ground, TileClassification.ImpassableGround, TileClassification.Water, TileClassification.ImpassableAir };

    }

    public class TileMap : GameObject //grid of tiles
    {
        public int Width = 30;
        public int Height = 30;

        public TileMapPoint TileMapCoords;

        public TileMapController Controller;

        public List<Tile> Tiles = new List<Tile>();

        public List<GameObject> Structures = new List<GameObject>();

        public List<TileChunk> TileChunks = new List<TileChunk>();

        public InstancedRenderData TileRenderData = null;
        public InstancedRenderData FogTileRenderData = null;

        public bool _visible = false;
        public bool Visible 
        {
            get => _visible;

            set 
            {
                _visible = value;

                if (value)
                {
                    Window.RenderEnd -= UpdateTileRenderData;
                    Window.RenderEnd += UpdateTileRenderData;
                }
                else {
                    DisposeOfRenderData();
                }
            }
        }

        public TileMap(Vector3 position, TileMapPoint point, TileMapController controller, string name = "TileMap")
        {
            Position = position; //position of the first tile
            Name = name;

            TileMapCoords = new TileMapPoint(point.X, point.Y);
            Controller = controller;
        }

        ~TileMap() 
        {
            //Console.WriteLine($"TileMap {TileMapCoords.X}, {TileMapCoords.Y} disposed");
        }
        
        public Tile this[int x, int y]
        {
            get { return GetTile(x, y); }
        }

        public Tile this[TilePoint point]
        {
            get { return TileMapHelpers.GetTile(point.X, point.Y, point.ParentTileMap); }
        }

        public Tile GetTile(int x, int y) 
        {
            return TileMapHelpers.GetTile(x, y, this);
        }

        public Tile GetLocalTile(int x, int y) 
        {
            return Tiles[x * Height + y];
        }

        public static Random _randomNumberGen = new ConsistentRandom();

        public virtual void PopulateTileMap(float zTilePlacement = 0)
        {
            Tile tile = new Tile();
            Vector3 tilePosition = new Vector3(Position);

            tilePosition.Z += zTilePlacement;

            for (int i = 0; i < Width; i++)
            {
                for (int o = 0; o < Height; o++)
                {
                    tile = new Tile(tilePosition, new TilePoint(i, o, this));
                    tile.Properties.Type = TileType.Grass;
                    tile.TileMap = this;

                    Tiles.Add(tile);

                    tilePosition.Y += tile.TileBounds.TileDimensions.Y;
                }
                tilePosition.X = (i + 1) * tile.TileBounds.TileDimensions.X / 1.34f; //1.29 before outlining changes
                tilePosition.Y = ((i + 1) % 2 == 0 ? 0 : tile.TileBounds.TileDimensions.Y / -2f); //2 before outlining changes
                //tilePosition.Z += 0.0001f;
            }

            tilePosition.Z += 0.03f;
        }


        public virtual void OnAddedToController() 
        {
            InitializeTileChunks();
        }

        public virtual void PopulateFeatures() { }

        public void InitializeTileChunks() 
        {
            TileChunks.Clear();
            FillTileChunks();
        }
        private void FillTileChunks(int width = TileChunk.DefaultChunkWidth, int height = TileChunk.DefaultChunkHeight)
        {
            float yChunkCount = (float)Height / height;
            float xChunkCount = (float)Width / width;


            for (int i = 0; i < xChunkCount; i++)
            {
                for (int j = 0; j < yChunkCount; j++)
                {
                    TileChunk chunk = new TileChunk(width, height);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (y + j * height >= Height) //stop adding tiles when you go past the bounds of the map
                            {
                                y = height;
                                continue;
                            }
                            if (x + i * width >= Width)
                            {
                                x = width;
                                continue;
                            }

                            chunk.AddTile(GetLocalTile(i * width + x, j * height + y));
                            //GetLocalTile(i * width + x, j * height + y).SetColor(new Vector4(i / xChunkCount, j / yChunkCount, 0, 1));
                        }
                    }

                    chunk.CalculateValues();

                    TileChunks.Add(chunk);

                    chunk.OnFilled();
                    chunk.UpdateTile();
                }
            }
        }

        private List<Tile> _fogTiles = new List<Tile>();
        private List<Tile> _visTiles = new List<Tile>();
        public void UpdateTileRenderData()
        {
            Window.RenderEnd -= UpdateTileRenderData;

            if (TileRenderData != null)
            {
                TileRenderData.CleanUp();
            }

            if (FogTileRenderData != null)
            {
                FogTileRenderData.CleanUp();
            }

            _fogTiles.Clear();
            _visTiles.Clear();

            for (int i = 0; i < Tiles.Count; i++)
            {
                if(Tiles[i].InFog(TileMapManager.Scene.VisibleTeam))
                    _fogTiles.Add(Tiles[i]);
                else
                    _visTiles.Add(Tiles[i]);
            }

            //TileRenderData = TileInstancedRenderData.GenerateInstancedRenderData(_visTiles)[0];
            //FogTileRenderData = TileInstancedRenderData.GenerateInstancedRenderData(_fogTiles)[0];

            //TODO, update tile mesh
        }

        public void UpdateTile() 
        {
            if (!TileMapManager.Scene.ContextManager.GetFlag(Engine_Classes.Scenes.GeneralContextFlags.TileMapManagerLoading))
            {
                Window.RenderEnd -= UpdateTileRenderData;
                Window.RenderEnd += UpdateTileRenderData;
            }
        }

        public override void CleanUp()
        {
            base.CleanUp();

            for (int i = 0; i < TileChunks.Count; i++)
            {
                TileChunks[i].ClearChunk();
            }

            if (UnitPositionManager.UnitMapPositions.TryGetValue(TileMapCoords, out var units))
            {
                foreach (var unit in units.ToList())
                {
                    EntityManager.RemoveEntity(unit.EntityHandle);
                }
            }

            TileChunks.Clear();
            Tiles.Clear();

            DisposeOfRenderData();
        }

        protected void DisposeOfRenderData()
        {
            if(TileRenderData != null || FogTileRenderData != null)
            {
                Controller.Scene.SyncToRender(() =>
                {
                    if (TileRenderData != null)
                    {
                        TileRenderData.CleanUp();
                        TileRenderData = null;
                    }

                    if (FogTileRenderData != null)
                    {
                        FogTileRenderData.CleanUp();
                        FogTileRenderData = null;
                    }
                });
            }
        }


        public Vector3 GetTileMapDimensions() 
        {
            if (Tiles.Count > 0) 
            {
                Vector3 tileDim = this[0, 0].TileBounds.TileDimensions;
                Vector3 returnDim = this[0, 0].Position - this[Width - 1, Height - 1].Position;

                //returnDim.X = Math.Abs(returnDim.X) + tileDim.X * 0.69f;
                returnDim.X = Math.Abs(returnDim.X) + tileDim.X * 0.75f;
                //returnDim.Y = Math.Abs(returnDim.Y) + tileDim.Y * 1.495f;
                returnDim.Y = Math.Abs(returnDim.Y) + tileDim.Y * 1.5f;

                return returnDim;
            }

            return new Vector3();
        }

        public override void SetPosition(Vector3 position)
        {
            Vector3 offset = Position - position;

            Tiles.ForEach(t =>
            {
                t.SetPosition(t.Position - offset);

                foreach(var unit in UnitPositionManager.GetUnitsOnTilePoint(t))
                {
                    unit.SetPosition(unit.Position - offset);
                }

                if (t.Structure != null)
                {
                    t.Structure.SetPosition(t.Structure.Position - offset);
                }
            });

            TileChunks.ForEach(chunk =>
            {
                chunk.Center -= offset;
            });

            base.SetPosition(position);
        }

        

        #region Tile validity checks
        public bool IsValidTile(int xIndex, int yIndex)
        {
            return TileMapHelpers.IsValidTile(xIndex, yIndex, this);
        }
        public bool IsValidTile(TilePoint point)
        {
            return TileMapHelpers.IsValidTile(point.X, point.Y, point.ParentTileMap);
            //return point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height;
        }
        #endregion

        #region Tile conversions
        public Vector3i OffsetToCube(TilePoint offset)
        {
            int xMapOffset = (offset.ParentTileMap.TileMapCoords.X - TileMapCoords.X) * Width;
            int yMapOffset = (offset.ParentTileMap.TileMapCoords.Y - TileMapCoords.Y) * Height;

            Vector3i cubeCoord = new Vector3i
            {
                X = offset.X + xMapOffset,
                Z = (offset.Y + yMapOffset) - ((offset.X + xMapOffset) + (offset.X & 1)) / 2
            };
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


        public override void Tick()
        {
            base.Tick();

            TileChunks.ForEach(chunk =>
            {
                if(!chunk.Cull)
                    chunk.Tick();
            });
        }

        #region Distance between points
        public static int GetDistanceBetweenPoints(TilePoint start, TilePoint end)
        {
            Vector3i a = CubeMethods.OffsetToCube(start);
            Vector3i b = CubeMethods.OffsetToCube(end);

            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }

        public static int GetDistanceBetweenPoints(FeaturePoint start, FeaturePoint end)
        {
            Vector3i a = CubeMethods.OffsetToCube(start);
            Vector3i b = CubeMethods.OffsetToCube(end);

            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }

        public static int GetDistanceBetweenPoints(Vector2i start, Vector2i end)
        {
            Vector3i a = CubeMethods.OffsetToCube(start);
            Vector3i b = CubeMethods.OffsetToCube(end);

            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }

        public static int GetDistanceBetweenPoints(Vector3i a, Vector3i b)
        {
            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }
        #endregion

        public void ClearVisitedTiles() 
        {
            TileMapHelpers.ClearAllVisitedTiles();
        }

        public struct TilesInRadiusParameters 
        {
            public TilePoint StartingPoint;
            public float Radius;
            public List<Unit> Units;
            public Unit CastingUnit;
            public AbilityTypes AbilityType;
            public bool CheckTileHigher;
            public bool CheckTileLower;

            public TilesInRadiusParameters(TilePoint startingPoint, float radius) 
            {
                StartingPoint = startingPoint;
                Radius = radius;
                Units = new List<Unit>();
                CastingUnit = null;
                AbilityType = AbilityTypes.Empty;
                CheckTileHigher = true;
                CheckTileLower = false;
            }

        }

        //gets tiles in a radius from a center point by expanding outward to all valid neighbors until the radius is reached.
        public List<Tile> FindValidTilesInRadius(TilesInRadiusParameters param)
        {
            List<Tile> tileList = new List<Tile>();
            List<Tile> neighbors = new List<Tile>();


            ClearVisitedTiles();

            tileList.Add(this[param.StartingPoint]);
            neighbors.Add(this[param.StartingPoint]);

            List<Tile> newNeighbors = new List<Tile>();

            for (int i = 0; i < param.Radius; i++)
            {
                newNeighbors.Clear();
                neighbors.ForEach(n =>
                {
                    GetNeighboringTiles(n, newNeighbors, true, param.CheckTileHigher, param.CheckTileLower);
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

                    if(param.Units != null)
                    {
                        var unitsOnTile = UnitPositionManager.GetUnitsOnTilePoint(neighbors[j]);

                        unitIndex = (int)(param.Units?.FindIndex(u => unitsOnTile.Contains(u)));
                    }

                    //param.Units?.Exists(u =>
                    //{
                    //    count++;
                    //    if (u.Info.TileMapPosition != null && u.Info.TileMapPosition == neighbors[j].TilePoint)
                    //    {
                    //        unitIndex = count;
                    //        return true;
                    //    }
                    //    else
                    //        return false;
                    //});

                    if (unitIndex != -1 && param.CastingUnit != null && param.AbilityType == AbilityTypes.Move) //special cases for ability targeting should go here
                    {
                        if (param.Units[unitIndex].Info.BlocksSpace && !param.CastingUnit.Info.PhasedMovement)
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
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public List<Tile> GetLineOfTiles(TilePoint startPoint, TilePoint endPoint)
        {
            List<Tile> tileList = new List<Tile>();
            Vector3i startCube = OffsetToCube(startPoint);
            Vector3i endCube = OffsetToCube(endPoint);

            int N = GetDistanceBetweenPoints(startCube, endCube);
            float n = 1f / N;

            Vector3 currentCube;
            Vector2i currentOffset;

            for (int i = 0; i <= N; i++)
            {
                currentCube = CubeMethods.CubeLerp(startCube, endCube, n * i);

                currentOffset = CubeMethods.CubeToOffset(CubeMethods.CubeRound(currentCube));
                if (IsValidTile(currentOffset.X, currentOffset.Y))
                {
                    tileList.Add(TileMapHelpers.GetTile(currentOffset.X, currentOffset.Y, this));
                }
            }

            return tileList;
        }


        /// <summary>
        /// Returns the list of points that create a ring with a radius of the passed in parameter
        /// </summary>
        /// <param name="start"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public void GetRingOfTiles(TilePoint startPoint, List<Vector2i> outputList, int radius = 1)
        {
            Vector3i cubePosition = OffsetToCube(startPoint);

            cubePosition += TileMapConstants.CubeDirections[Direction.North] * radius;

            Vector2i tileOffsetCoord;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    tileOffsetCoord = CubeMethods.CubeToOffset(cubePosition);

                    outputList.Add(new Vector2i(tileOffsetCoord.X, tileOffsetCoord.Y));

                    cubePosition += TileMapConstants.CubeDirections[(Direction)i];
                }
            }
        }

        /// <summary>
        /// Returns the list of BaseTiles that create a ring with a radius of the passed in parameter
        /// </summary>
        /// <param name="start"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public void GetRingOfTiles(TilePoint startPoint, List<Tile> outputList, int radius = 1, bool includeEdges = true)
        {
            Vector3i cubePosition = OffsetToCube(startPoint);

            cubePosition += TileMapConstants.CubeDirections[Direction.North] * radius;

            Vector2i tileOffsetCoord;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    tileOffsetCoord = CubeMethods.CubeToOffset(cubePosition);
                    if (IsValidTile(tileOffsetCoord.X, tileOffsetCoord.Y))
                    {
                        outputList.Add(this[new TilePoint(tileOffsetCoord.X, tileOffsetCoord.Y, this)]);
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
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="untraversableTypes">tile types that will block vision</param>
        /// <param name="units"></param>
        /// <returns></returns>
        public void GetVisionLine(TilePoint startPoint, Vector2i endPoint, HashSet<Tile> outputList, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            Vector3i startCube = OffsetToCube(startPoint);
            Vector3i endCube = CubeMethods.OffsetToCube(endPoint);

            int N = GetDistanceBetweenPoints(startCube, endCube);
            float n = 1f / N;

            Vector3 currentCube;
            Vector2i currentOffset;

            Tile currentTile = null;

            if (IsValidTile(startPoint)) 
            {
                currentTile = startPoint.GetBaseTile();
            }

            for (int i = 0; i <= N; i++)
            {
                currentCube = CubeMethods.CubeLerp(startCube, endCube, n * i);
                currentOffset = CubeMethods.CubeToOffset(CubeMethods.CubeRound(currentCube));
                if (IsValidTile(currentOffset.X, currentOffset.Y))
                {
                    _BaseTileTemp = this[currentOffset.X, currentOffset.Y];

                    if (currentTile != null && _BaseTileTemp.Properties.Height - currentTile.Properties.Height > 1)
                    {
                        return; //if the height difference is due to the tile we can't see the tile
                    }

                    outputList.Add(_BaseTileTemp);

                    if (currentTile != null && _BaseTileTemp.GetVisionHeight() - currentTile.GetVisionHeight() > 1)
                    {
                        return; //if the height difference is due to the structure then we can see the tile
                    }

                    if (_BaseTileTemp.Properties.BlocksVision && !ignoreBlockedVision)
                    {
                        return;
                    }
                    
                    if (units?.Count > 0)
                    {
                        if (units.Exists(unit => unit.Info.TileMapPosition == _BaseTileTemp.TilePoint 
                            && (unit.Info.Height + _BaseTileTemp.GetPathableHeight()) - currentTile.GetPathableHeight() > 1)) //&& unit.BlocksVision
                        {
                            return;
                        }
                    }
                }
            }
        }

        public void GetTargetingLine(TilePoint startPoint, Vector3i endPointCube, HashSet<Tile> outputList, List<Unit> units = default, bool ignoreBlockedVision = false) 
        {
            Vector3i startCube = OffsetToCube(startPoint);

            int N = GetDistanceBetweenPoints(startCube, endPointCube);
            float n = 1f / N;

            Vector3 currentCube;
            Vector2i currentOffset;

            Tile currentTile = null;

            if (IsValidTile(startPoint))
            {
                currentTile = startPoint.GetBaseTile();
            }

            for (int i = 0; i <= N; i++)
            {
                currentCube = CubeMethods.CubeLerp(startCube, endPointCube, n * i);
                currentOffset = CubeMethods.CubeToOffset(CubeMethods.CubeRound(currentCube));
                if (IsValidTile(currentOffset.X, currentOffset.Y))
                {
                    _BaseTileTemp = this[currentOffset.X, currentOffset.Y];

                    if (currentTile != null && _BaseTileTemp.GetPathableHeight() - currentTile.GetPathableHeight() > 1)
                    {
                        return; //if the height difference is due to the tile we can't see the tile
                    }

                    outputList.Add(_BaseTileTemp);

                    if (currentTile != null && _BaseTileTemp.GetVisionHeight() - currentTile.GetPathableHeight() > 1)
                    {
                        return; //if the height difference is due to the structure then we can see the tile
                    }

                    if (_BaseTileTemp.Properties.BlocksVision && !ignoreBlockedVision)
                    {
                        return;
                    }

                    if (units?.Count > 0)
                    {
                        if (units.Exists(unit => unit.Info.TileMapPosition == _BaseTileTemp.TilePoint
                            && (unit.Info.Height + _BaseTileTemp.GetPathableHeight()) - currentTile.GetPathableHeight() > 1)) //&& unit.BlocksVision
                        {
                            return;
                        }
                    }
                }
            }
        }
        public void GetTargetingLine(TilePoint startPoint, TilePoint endPoint, HashSet<Tile> outputList, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            GetTargetingLine(startPoint, OffsetToCube(endPoint), outputList, units, ignoreBlockedVision);
        }


        public List<Tile> GetVisionInRadius(TilePoint point, int radius, List<TileClassification> untraversableTypes = default, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            List<Vector2i> ringOfTiles = new List<Vector2i>();
            GetRingOfTiles(point, ringOfTiles, radius);

            HashSet<Tile> outputList = new HashSet<Tile>();

            for (int i = 0; i < ringOfTiles.Count; i++)
            {
                GetVisionLine(point, ringOfTiles[i], outputList, units, ignoreBlockedVision);
            }

            return outputList.ToList();
        }

        public List<Tile> GetTargetsInRadius(TilePoint point, int radius, List<TileClassification> untraversableTypes = default, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            List<Vector2i> ringOfTiles = new List<Vector2i>();
            GetRingOfTiles(point, ringOfTiles, radius);

            HashSet<Tile> outputList = new HashSet<Tile>();

            for (int i = 0; i < ringOfTiles.Count; i++)
            {
                Vector3i endCube = CubeMethods.OffsetToCube(ringOfTiles[i]);
                GetTargetingLine(point, endCube, outputList, units, ignoreBlockedVision);
            }

            return outputList.ToList();
        }



        public struct PathToPointParameters
        {
            public TilePoint StartingPoint;
            public TilePoint EndingPoint;
            public float Depth;
            public List<TileClassification> TraversableTypes;
            public List<Unit> Units;
            public Unit CastingUnit;
            public AbilityTypes AbilityType;
            public bool CheckTileHigher;
            public bool CheckTileLower;
            public bool Shuffle;

            /// <summary>
            /// If a unit is at the endPoint it will not break the path.
            /// </summary>
            public bool IgnoreTargetUnit;

            public PathToPointParameters(TilePoint startingPoint, TilePoint endPoint, float depth)
            {
                StartingPoint = startingPoint;
                EndingPoint = endPoint;
                Depth = depth;
                TraversableTypes = new List<TileClassification>();
                Units = new List<Unit>();
                CastingUnit = null;
                AbilityType = AbilityTypes.Empty;
                CheckTileHigher = true;
                CheckTileLower = false;
                Shuffle = true;

                IgnoreTargetUnit = false;
            }
        }


        public List<Tile> GetPathToPoint(PathToPointParameters param)
        {
            List<TileWithParent> tileList = new List<TileWithParent>();
            List<Tile> returnList = new List<Tile>();

            HashSet<Unit> unitSet = param.Units.ToHashSet();

            if (param.Depth <= 0)
                return returnList;

            ClearVisitedTiles();

            Tile endingTile = this[param.EndingPoint];

            if (!param.TraversableTypes.Exists(c => c == this[param.StartingPoint].Properties.Classification) || !param.TraversableTypes.Exists(c => c == this[param.EndingPoint].Properties.Classification))
            {
                return returnList; //if the starting or ending tile isn't traversable then immediately return
            }

            if (param.Units != null && param.Units.Exists(u => u.Info.TileMapPosition == param.EndingPoint) 
                && param.CastingUnit != null && !param.CastingUnit.Info.PhasedMovement && !param.IgnoreTargetUnit)
            {
                return returnList; //if the ending tile is inside of a unit then immediately return
            }

            if (endingTile.Structure != null && !(endingTile.Structure.Passable || endingTile.Structure.Pathable))
            {
                return returnList; //if the ending tile is inside of a unit then immediately return
            }

            if (GetDistanceBetweenPoints(param.StartingPoint, param.EndingPoint) > param.Depth * 1.5f)
            {
                return returnList;
            }

            TileWithParent currentTile = new TileWithParent(this[param.StartingPoint]) 
            {
                G = GetDistanceBetweenPoints(this[param.StartingPoint].TilePoint, param.StartingPoint),
                H = GetDistanceBetweenPoints(this[param.StartingPoint].TilePoint, param.EndingPoint),
            };

            

            param.StartingPoint.Visited = true;

            tileList.Add(currentTile);

            List<Tile> newNeighbors = new List<Tile>();

            List<Tile> createReturnList() 
            {
                while (currentTile.Parent != null)
                {
                    returnList.Add(currentTile.Tile);
                    currentTile = currentTile.Parent;
                }

                returnList.Add(currentTile.Tile);
                returnList.Reverse();

                return returnList;
            }

            HashSet<TilePoint> visitedPoints = new HashSet<TilePoint>() { currentTile.Tile.TilePoint };

            while(true)
            {
                currentTile.Visited = true;

                if (currentTile.Tile.TilePoint == param.EndingPoint) 
                {
                    return createReturnList();
                }

                newNeighbors.Clear();
                GetNeighboringTiles(currentTile.Tile, newNeighbors, true, true, true);

                for (int j = 0; j < newNeighbors.Count; j++)
                {
                    Unit foundUnit = null;
                    int count = -1;

                    var unitsOnPoint = UnitPositionManager.GetUnitsOnTilePoint(newNeighbors[j].TilePoint);

                    foreach(var unit in unitSet)
                    {
                        if (unitsOnPoint.Contains(unit))
                        {
                            foundUnit = unit;
                            break;
                        }
                    }

                    if (!param.TraversableTypes.Exists(c => c == newNeighbors[j].Properties.Classification))
                    {
                        //if the space contains a tile that is not traversable cut off this branch and continue
                        newNeighbors.RemoveAt(j);
                        j--;
                        continue;
                    }
                    else if (foundUnit != null && param.CastingUnit != null && param.AbilityType == AbilityTypes.Move) //special cases for ability targeting should go here
                    {
                        if (foundUnit.Info.BlocksSpace && !param.CastingUnit.Info.PhasedMovement 
                            && !(param.IgnoreTargetUnit && foundUnit.Info.TileMapPosition.TilePoint == param.EndingPoint)
                            && !foundUnit.Info.Dead)
                        {
                            //if this is a movement ability and the unit using the ability does not have phased movement, the target unit is alive, and the unit is not 
                            //a unit we are attempting to path to we remove this path as a possibility
                            newNeighbors.RemoveAt(j);
                            j--;
                            continue;
                        }
                    }

                    //int tileIndex = tileList.FindIndex(0, t => t.Tile.TilePoint == newNeighbors[j].TilePoint);
                    //if (tileIndex > -1)
                    if (visitedPoints.TryGetValue(newNeighbors[j].TilePoint, out var _))
                    {
                        continue;
                        //tileList[tileIndex].Parent = currentTile;
                        //tileList[tileIndex].G = currentTile.G + tileList[tileIndex].Tile.Properties.MovementCost; //for different cost modifiers (such as height) an enum and switch statement can be employed
                        //tileList[tileIndex].H = GetDistanceBetweenPoints(newNeighbors[j].TilePoint, param.EndingPoint);

                        //if (tileList[tileIndex].Parent.Parent == tileList[tileIndex])
                        //{

                        //}
                    }
                    else 
                    {
                        TileWithParent tile = new TileWithParent(newNeighbors[j], currentTile)
                        {
                            G = currentTile.G + newNeighbors[j].Properties.MovementCost,
                            H = GetDistanceBetweenPoints(newNeighbors[j].TilePoint, param.EndingPoint),
                        };

                        tileList.Add(tile);
                        visitedPoints.Add(tile.Tile.TilePoint);
                    }


                    if (tileList[^1].Tile.TilePoint == param.EndingPoint) 
                    {
                        currentTile = tileList[^1];
                        return createReturnList();
                    }
                }

                bool tileChanged = false;
                for (int j = 0; j < tileList.Count; j++) 
                {
                    if (!tileList[j].Visited) 
                    {
                        if (!tileChanged) 
                        {
                            currentTile = tileList[j];
                            tileChanged = true;
                        }

                        if ((tileList[j].F < currentTile.F) || (tileList[j].F == currentTile.F && tileList[j].H < currentTile.H))
                        {
                            currentTile = tileList[j];
                        }
                    }
                }

                if (currentTile.Visited) 
                {
                    return returnList;
                }

                if (currentTile.F > param.Depth)
                {
                    return returnList;
                }
            }
        }

        /// <summary>
        /// Fills the passed neighborList List with all tiles that surround the tile contained in tilePos position
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="neighborList"></param>
        /// <param name="visitedTiles"></param>
        public void GetNeighboringTiles(Tile tile, List<Tile> neighborList, bool shuffle = true, bool checkTileHigher = false, bool checkTileLower = false, bool attachHeightIndicator = false, bool setVisited = true)
        {
            TilePoint neighborPos = new TilePoint(tile.TilePoint.X, tile.TilePoint.Y, tile.TilePoint.ParentTileMap);
            int yOffset = tile.TilePoint.X % 2 == 0 ? 1 : 0;

            for (int i = 0; i < 6; i++)
            {
                neighborPos.X = tile.TilePoint.X;
                neighborPos.Y = tile.TilePoint.Y;
                switch (i)
                {
                    case (int)Direction.South: //tile below
                        neighborPos.Y += 1;
                        break;
                    case (int)Direction.North: //tile above
                        neighborPos.Y -= 1;
                        break;
                    case (int)Direction.SouthWest: //tile bottom left
                        neighborPos.X -= 1;
                        neighborPos.Y += yOffset;
                        break;
                    case (int)Direction.NorthWest: //tile top left
                        neighborPos.Y -= 1 + -yOffset;
                        neighborPos.X -= 1;
                        break;
                    case (int)Direction.NorthEast: //tile top right
                        neighborPos.Y -= 1 + -yOffset;
                        neighborPos.X += 1;
                        break;
                    case (int)Direction.SouthEast: //tile bottom right
                        neighborPos.X += 1;
                        neighborPos.Y += yOffset;
                        break;
                }

                if (IsValidTile(neighborPos))
                {
                    Tile neighborTile = tile.TileMap[neighborPos];
                    if (neighborTile != null && !neighborTile.TilePoint.Visited)
                    {
                        if (checkTileHigher && (!neighborTile.StructurePathable() || neighborTile.GetPathableHeight() - tile.GetPathableHeight() > 1)) 
                        {
                            if (attachHeightIndicator) 
                            {
                            }
                            continue;
                        }

                        if (checkTileLower && (!neighborTile.StructurePathable() || neighborTile.GetPathableHeight() - tile.GetPathableHeight() < -1))
                        {
                            if (attachHeightIndicator)
                            {
                            }
                            continue;
                        }

                        neighborList.Add(neighborTile);
                        neighborTile.TilePoint.Visited = setVisited ? true : neighborTile.TilePoint.Visited;
                    }
                }
            }

            if (shuffle)
            {
                ShuffleList(neighborList);
            }
        }

        public Tile GetNeighboringTile(Tile tile, Direction direction) 
        {
            TilePoint neighborPos = new TilePoint(tile.TilePoint.X, tile.TilePoint.Y, tile.TilePoint.ParentTileMap); ;
            int yOffset = tile.TilePoint.X % 2 == 0 ? 1 : 0;

            neighborPos.X = tile.TilePoint.X;
            neighborPos.Y = tile.TilePoint.Y;
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

            if (IsValidTile(neighborPos))
            {
                return this[neighborPos];
            }

            return null;
        }

        private Tile _BaseTileTemp = new Tile();
        private readonly List<TileClassification> _EmptyTileClassification = new List<TileClassification>();

        public class TileWithParent
        {
            public TileWithParent Parent = null;
            public Tile Tile;
            public float G = 0; //Path cost
            public float H = 0; //Distance to end
            public float F => G + H;
            public bool Visited = false;

            public TileWithParent(Tile tile, TileWithParent parent = null)
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

        public override bool Equals(object obj)
        {
            return obj is TileMap map &&
                   base.Equals(obj) &&
                   EqualityComparer<TileMapPoint>.Default.Equals(TileMapCoords, map.TileMapCoords);
        }

        public override int GetHashCode()
        {
            return TileMapCoords.GetHashCode();
        }
    }

    public class TileMapPoint
    {
        public int X;
        public int Y;

        public TileMapPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public TileMapPoint(Vector2i coords)
        {
            X = coords.X;
            Y = coords.Y;
        }

        public static bool operator ==(TileMapPoint a, TileMapPoint b) => Equals(a, b);
        public static bool operator !=(TileMapPoint a, TileMapPoint b) => !(a == b);

        public override string ToString()
        {
            return "TileMapPoint {" + X + ", " + Y + "}";
        }
        public override bool Equals(object obj)
        {
            return obj is TileMapPoint point &&
                   X == point.X &&
                   Y == point.Y;
        }

        public long GetUniqueHash()
        {
            return ((long)X << 32) + Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    public class DynamicTextureInfo 
    {
        public bool TextureChanged = false;
        public bool Initialize = true;
    }
}
