using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Lighting;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles.HelperTiles;
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
    internal enum Direction
    {
        None = -1,
        SouthWest,
        South,
        SouthEast,
        NorthEast,
        North,
        NorthWest,
    }

    internal static class TileMapConstants
    {
        internal static Dictionary<Direction, Vector3i> CubeDirections = new Dictionary<Direction, Vector3i>
        {
            { Direction.SouthWest, new Vector3i(-1, 0, 1) },
            { Direction.South, new Vector3i(0, -1, 1) },
            { Direction.SouthEast, new Vector3i(1, -1, 0) },
            { Direction.NorthEast, new Vector3i(1, 0, -1) },
            { Direction.North, new Vector3i(0, 1, -1) },
            { Direction.NorthWest, new Vector3i(-1, 1, 0) },
        };

        internal static List<TileClassification> AllTileClassifications = new List<TileClassification>()
        { TileClassification.Ground, TileClassification.AttackableTerrain, TileClassification.Terrain, TileClassification.Water };

    }

    internal class TileMap : GameObject //grid of tiles
    {
        internal int Width = 30;
        internal int Height = 30;

        internal TileMapPoint TileMapCoords;

        internal TileMapController Controller;

        internal List<BaseTile> Tiles = new List<BaseTile>();

        internal List<GameObject> Structures = new List<GameObject>();

        internal List<TileChunk> TileChunks = new List<TileChunk>();

        internal Texture DynamicTexture;
        internal FrameBufferObject FrameBuffer;
        internal DynamicTextureInfo DynamicTextureInfo = new DynamicTextureInfo();
        internal ConcurrentBag<BaseTile> TilesToUpdate = new ConcurrentBag<BaseTile>();

        private const int TILE_QUEUES = 5;
        protected int _currentTileQueue = 0;
        protected List<ConcurrentBag<BaseTile>> _tilesToUpdate = new List<ConcurrentBag<BaseTile>>();

        internal GameObject TexturedQuad;

        internal TileMap(Vector3 position, TileMapPoint point, TileMapController controller, string name = "TileMap")
        {
            Position = position; //position of the first tile
            Name = name;

            TileMapCoords = new TileMapPoint(point.X, point.Y);
            Controller = controller;

            for (int i = 0; i < TILE_QUEUES; i++) 
            {
                _tilesToUpdate.Add(new ConcurrentBag<BaseTile>());
            }
        }

        ~TileMap() 
        {
            Console.WriteLine($"TileMap {TileMapCoords.X}, {TileMapCoords.Y} disposed");
        }
        
        internal BaseTile this[int x, int y]
        {
            get { return GetTile(x, y); }
        }

        internal BaseTile this[TilePoint point]
        {
            get { return Controller.GetTile(point.X, point.Y, point.ParentTileMap); }
        }

        private BaseTile GetTile(int x, int y) 
        {
            return Controller.GetTile(x, y, this);
        }

        internal BaseTile GetLocalTile(int x, int y) 
        {
            return Tiles[x * Height + y];
        }

        internal static Random _randomNumberGen = new Random();

        internal virtual void PopulateTileMap(float zTilePlacement = 0)
        {
            BaseTile baseTile = new BaseTile();
            Vector3 tilePosition = new Vector3(Position);

            tilePosition.Z += zTilePlacement;

            for (int i = 0; i < Width; i++)
            {
                for (int o = 0; o < Height; o++)
                {
                    baseTile = new BaseTile(tilePosition, new TilePoint(i, o, this)) { Clickable = true };
                    baseTile.SetAnimation(BaseTileAnimationType.Grass);
                    baseTile.DefaultAnimation = BaseTileAnimationType.Grass;
                    baseTile.Properties.Type = TileType.Grass;
                    baseTile.TileMap = this;

                    Tiles.Add(baseTile);

                    tilePosition.Y += baseTile.BaseObjects[0].Dimensions.Y;
                }
                tilePosition.X = (i + 1) * baseTile.BaseObjects[0].Dimensions.X / 1.34f; //1.29 before outlining changes
                tilePosition.Y = ((i + 1) % 2 == 0 ? 0 : baseTile.BaseObjects[0].Dimensions.Y / -2f); //2 before outlining changes
                //tilePosition.Z += 0.0001f;
            }

            tilePosition.Z += 0.03f;

            SetDefaultTileValues();
            //InitializeTexturedQuad();
        }


        internal virtual void OnAddedToController() 
        {
            InitializeTileChunks();
        }

        internal virtual void PopulateFeatures() { }

        internal void InitializeTileChunks() 
        {
            TileChunks.Clear();
            FillTileChunks(10, 10);
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
                }
            }
        }

        internal void InitializeTexturedQuad() 
        {
            TileTexturer.InitializeTexture(this);

            RenderableObject obj = new RenderableObject(new SpritesheetObject(0, Spritesheets.TestSheet, 10, 10).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

            obj.Material = new Material() { Diffuse = DynamicTexture };

            obj.Material.Diffuse.TextureName = TextureName.DynamicTexture;
            obj.Textures.Textures[0] = TextureName.DynamicTexture;

            Renderer.LoadTextureFromTextureObj(obj.Material.Diffuse, TextureName.DynamicTexture);

            Animation Idle = new Animation()
            {
                Frames = new List<RenderableObject>() { obj },
                Frequency = -1,
                Repeats = -1
            };

            BaseObject baseObj = new BaseObject(new List<Animation>() { Idle }, 0, "", new Vector3());

            GameObject temp = new GameObject();
            temp.AddBaseObject(baseObj);

            temp.BaseObjects[0].BaseFrame.CameraPerspective = true;

            temp.BaseObjects[0].BaseFrame.ScaleX(0.753f);
            temp.BaseObjects[0].BaseFrame.ScaleY(0.8727f);
            temp.BaseObjects[0].BaseFrame.ScaleAll(Width);

            temp.TextureLoaded = true;

            TexturedQuad = temp;
            
            UpdateQuadPosition();
        }

        internal void UpdateQuadPosition() 
        {
            Vector3 tileMapPos = (Tiles[0].Position + Tiles[Tiles.Count - 1].Position) / 2;


            //the most magic of magic numbers (I'll probably need to redo tiling in some fashion if I want to make a general solution for this)

            if (TexturedQuad != null) 
            {
                TexturedQuad.SetPosition(tileMapPos + new Vector3(58, -105, 0));
            }
        }

        internal void UpdateDynamicTexture() 
        {
            //DynamicTexture.UpdateTextureArray(DynamicTextureInfo.MinChangedBounds, DynamicTextureInfo.MaxChangedBounds, this);
            TilesToUpdate = _tilesToUpdate[_currentTileQueue];

            _currentTileQueue++;
            _currentTileQueue %= _tilesToUpdate.Count;

            TileTexturer.UpdateTexture(this);
            TilesToUpdate.Clear();
            //DynamicTextureInfo.TextureChanged = false;
            DynamicTextureInfo.TextureChanged = _tilesToUpdate[_currentTileQueue].Count > 0;
        }

        private ActionQueue _tileActionQueue = new ActionQueue();
        internal void UpdateTile(BaseTile tile) 
        {
            _tilesToUpdate[_currentTileQueue].Add(tile);
        }

        internal override void CleanUp()
        {
            base.CleanUp();

            if (FrameBuffer != null) 
            {
                FrameBuffer.Dispose();
            }

            for (int i = 0; i < TileChunks.Count; i++)
            {
                TileChunks[i].ClearChunk();
            }

            TileChunks.Clear();
            Tiles.Clear();
        }


        internal void GenerateCliffs() 
        {
            BaseTile neighborTile;
            for (int i = 0; i < Tiles.Count; i++) 
            {
                short cliffBitArray = 0;

                Tiles[i].ClearCliff();

                for (int j = 0; j < 6; j++) 
                {
                    neighborTile = GetNeighboringTile(Tiles[i], (Direction)j);

                    if (neighborTile != null) 
                    {
                        if (Tiles[i].Properties.Height - neighborTile.Properties.Height < -1)
                        {
                            cliffBitArray += (short)Cliff.DirectionToCliffFace((Direction)j);
                        }
                    }
                }

                if (cliffBitArray != 0) 
                {
                    new Cliff(Controller.Scene, Tiles[i], cliffBitArray);
                }
            }
        }

        internal void GenerateCliff(BaseTile tile) 
        {
            BaseTile neighborTile;
            short cliffBitArray = 0;

            tile.ClearCliff();

            for (int j = 0; j < 6; j++)
            {
                neighborTile = GetNeighboringTile(tile, (Direction)j);

                if (neighborTile != null)
                {
                    if (tile.Properties.Height - neighborTile.Properties.Height > 1)
                    {
                        cliffBitArray += (short)Cliff.DirectionToCliffFace((Direction)j);
                    }
                }
            }

            if (cliffBitArray != 0)
            {
                new Cliff(Controller.Scene, tile, cliffBitArray);
            }
        }

        internal Vector3 GetTileMapDimensions() 
        {
            if (Tiles.Count > 0) 
            {
                Vector3 tileDim = this[0, 0].GetDimensions();
                Vector3 returnDim = this[0, 0].Position - this[Width - 1, Height - 1].Position;

                returnDim.X = Math.Abs(returnDim.X) + tileDim.X * 0.725f;
                returnDim.Y = Math.Abs(returnDim.Y) + tileDim.Y * 1.50f;

                return returnDim;
            }

            return new Vector3();
        }

        internal override void SetPosition(Vector3 position)
        {
            Vector3 offset = Position - position;

            Tiles.ForEach(t =>
            {
                t.SetPosition(t.Position - offset);

                if (t.UnitOnTile != null)
                {
                    t.UnitOnTile.SetPosition(t.UnitOnTile.Position - offset);
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

            UpdateQuadPosition();
        }

        

        #region Tile validity checks
        internal bool IsValidTile(int xIndex, int yIndex)
        {
            return Controller.IsValidTile(xIndex, yIndex, this);
        }
        internal bool IsValidTile(TilePoint point)
        {
            return Controller.IsValidTile(point.X, point.Y, point.ParentTileMap);
            //return point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height;
        }
        #endregion

        #region Tile conversions
        internal Vector3i OffsetToCube(TilePoint offset)
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
        internal Vector2i ClampTileCoordsToMapValues(Vector2i offsetCoord)
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

        

        internal void AddHeightIndicator(BaseTile attachedTile, bool up = true) 
        {
            if (attachedTile.TilePoint == null)
                return;

            HeightIndicatorTile tile = new HeightIndicatorTile(attachedTile);

            if (up)
            {
                tile.BaseObjects[0].SetAnimation((int)HeightIndicatorTile.Animations.Up);
                tile.SetColor(Colors.MoreTranslucentRed);
            }
            else 
            {
                tile.BaseObjects[0].SetAnimation((int)HeightIndicatorTile.Animations.Down);
                tile.SetColor(Colors.MoreTranslucentBlue);
            }

            Controller.Scene._genericObjects.Add(tile);
        }

        internal void RemoveHeightIndicatorTile(BaseTile attachedTile) 
        {
            if (attachedTile.HeightIndicator != null) 
            {
                Controller.Scene._genericObjects.Remove(attachedTile.HeightIndicator);
            }
        }


        internal void SetDefaultTileValues()
        {
            Tiles.ForEach(tile =>
            {
                tile.SetColor(tile.DefaultColor);
                tile.SetAnimation(tile.DefaultAnimation);
            });
        }

        internal override void Tick()
        {
            base.Tick();

            TileChunks.ForEach(chunk =>
            {
                if(!chunk.Cull)
                    chunk.Tick();
            });
        }

        #region Distance between points
        internal int GetDistanceBetweenPoints(TilePoint start, TilePoint end)
        {
            Vector3i a = OffsetToCube(start);
            Vector3i b = OffsetToCube(end);

            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }
        internal int GetDistanceBetweenPoints(Vector3i a, Vector3i b)
        {
            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }
        #endregion

        internal void ClearVisitedTiles() 
        {
            Controller.ClearAllVisitedTiles();
        }

        internal struct TilesInRadiusParameters 
        {
            internal TilePoint StartingPoint;
            internal float Radius;
            internal List<TileClassification> TraversableTypes;
            internal List<Unit> Units;
            internal Unit CastingUnit;
            internal AbilityTypes AbilityType;
            internal bool CheckTileHigher;
            internal bool CheckTileLower;

            internal TilesInRadiusParameters(TilePoint startingPoint, float radius) 
            {
                StartingPoint = startingPoint;
                Radius = radius;
                TraversableTypes = new List<TileClassification>();
                Units = new List<Unit>();
                CastingUnit = null;
                AbilityType = AbilityTypes.Empty;
                CheckTileHigher = true;
                CheckTileLower = false;
            }

        }

        //gets tiles in a radius from a center point by expanding outward to all valid neighbors until the radius is reached.
        internal List<BaseTile> FindValidTilesInRadius(TilesInRadiusParameters param)
        {
            List<BaseTile> tileList = new List<BaseTile>();
            List<BaseTile> neighbors = new List<BaseTile>();

            if (!param.TraversableTypes.Exists(c => c == this[param.StartingPoint].Properties.Classification))
            {
                return tileList;
            }

            ClearVisitedTiles();

            tileList.Add(this[param.StartingPoint]);
            neighbors.Add(this[param.StartingPoint]);

            List<BaseTile> newNeighbors = new List<BaseTile>();

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
                    param.Units?.Exists(u =>
                    {
                        count++;
                        if (u.Info.TileMapPosition == neighbors[j].TilePoint)
                        {
                            unitIndex = count;
                            return true;
                        }
                        else
                            return false;
                    });

                    if (!param.TraversableTypes.Exists(c => c == neighbors[j].Properties.Classification))
                    {
                        neighbors.RemoveAt(j);
                        j--;
                    }
                    else if (unitIndex != -1 && param.CastingUnit != null && param.AbilityType == AbilityTypes.Move) //special cases for ability targeting should go here
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
        internal List<BaseTile> GetLineOfTiles(TilePoint startPoint, TilePoint endPoint)
        {
            List<BaseTile> tileList = new List<BaseTile>();
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
                    tileList.Add(this[currentOffset.X, currentOffset.Y]);
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
        internal void GetRingOfTiles(TilePoint startPoint, List<Vector2i> outputList, int radius = 1)
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
        internal void GetRingOfTiles(TilePoint startPoint, List<BaseTile> outputList, int radius = 1, bool includeEdges = true)
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
        internal void GetVisionLine(TilePoint startPoint, Vector2i endPoint, HashSet<BaseTile> outputList, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            Vector3i startCube = OffsetToCube(startPoint);
            Vector3i endCube = CubeMethods.OffsetToCube(endPoint);

            int N = GetDistanceBetweenPoints(startCube, endCube);
            float n = 1f / N;

            Vector3 currentCube;
            Vector2i currentOffset;

            BaseTile currentTile = null;

            if (IsValidTile(startPoint)) 
            {
                currentTile = startPoint.GetTile();
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

        internal void GetTargetingLine(TilePoint startPoint, Vector3i endPointCube, HashSet<BaseTile> outputList, List<Unit> units = default, bool ignoreBlockedVision = false) 
        {
            Vector3i startCube = OffsetToCube(startPoint);

            int N = GetDistanceBetweenPoints(startCube, endPointCube);
            float n = 1f / N;

            Vector3 currentCube;
            Vector2i currentOffset;

            BaseTile currentTile = null;

            if (IsValidTile(startPoint))
            {
                currentTile = startPoint.GetTile();
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
        internal void GetTargetingLine(TilePoint startPoint, TilePoint endPoint, HashSet<BaseTile> outputList, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            GetTargetingLine(startPoint, OffsetToCube(endPoint), outputList, units, ignoreBlockedVision);
        }


        internal List<BaseTile> GetVisionInRadius(TilePoint point, int radius, List<TileClassification> untraversableTypes = default, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            List<Vector2i> ringOfTiles = new List<Vector2i>();
            GetRingOfTiles(point, ringOfTiles, radius);

            HashSet<BaseTile> outputList = new HashSet<BaseTile>();

            for (int i = 0; i < ringOfTiles.Count; i++)
            {
                GetVisionLine(point, ringOfTiles[i], outputList, units, ignoreBlockedVision);
            }

            return outputList.ToList();
        }

        internal List<BaseTile> GetTargetsInRadius(TilePoint point, int radius, List<TileClassification> untraversableTypes = default, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            List<Vector2i> ringOfTiles = new List<Vector2i>();
            GetRingOfTiles(point, ringOfTiles, radius);

            HashSet<BaseTile> outputList = new HashSet<BaseTile>();

            for (int i = 0; i < ringOfTiles.Count; i++)
            {
                Vector3i endCube = CubeMethods.OffsetToCube(ringOfTiles[i]);
                GetTargetingLine(point, endCube, outputList, units, ignoreBlockedVision);
            }

            return outputList.ToList();
        }



        internal struct PathToPointParameters
        {
            internal TilePoint StartingPoint;
            internal TilePoint EndingPoint;
            internal float Depth;
            internal List<TileClassification> TraversableTypes;
            internal List<Unit> Units;
            internal Unit CastingUnit;
            internal AbilityTypes AbilityType;
            internal bool CheckTileHigher;
            internal bool CheckTileLower;
            internal bool Shuffle;

            /// <summary>
            /// If a unit is at the endPoint it will not break the path.
            /// </summary>
            internal bool IgnoreTargetUnit;

            internal PathToPointParameters(TilePoint startingPoint, TilePoint endPoint, float depth)
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


        internal List<BaseTile> GetPathToPoint(PathToPointParameters param)
        {
            List<TileWithParent> tileList = new List<TileWithParent>();
            List<BaseTile> returnList = new List<BaseTile>();

            if (param.Depth <= 0)
                return returnList;

            ClearVisitedTiles();

            BaseTile endingTile = this[param.EndingPoint];

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

            

            param.StartingPoint._visited = true;

            tileList.Add(currentTile);

            List<BaseTile> newNeighbors = new List<BaseTile>();

            List<BaseTile> createReturnList() 
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
                    int unitIndex = -1;
                    int count = -1;


                    //find if there's a unit in this space
                    param.Units?.Exists(u =>
                    {
                        count++;
                        if (u.Info.TileMapPosition == newNeighbors[j].TilePoint)
                        {
                            unitIndex = count;
                            return true;
                        }
                        else
                            return false;
                    });

                    if (!param.TraversableTypes.Exists(c => c == newNeighbors[j].Properties.Classification))
                    {
                        //if the space contains a tile that is not traversable cut off this branch and continue
                        newNeighbors.RemoveAt(j);
                        j--;
                        continue;
                    }
                    else if (unitIndex != -1 && param.CastingUnit != null && param.AbilityType == AbilityTypes.Move) //special cases for ability targeting should go here
                    {
                        if (param.Units[unitIndex].Info.BlocksSpace && !param.CastingUnit.Info.PhasedMovement 
                            && !(param.IgnoreTargetUnit && param.Units[unitIndex].Info.TileMapPosition.TilePoint == param.EndingPoint)
                            && !param.Units[unitIndex].Info.Dead)
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

                if (currentTile.F > param.Depth * 1.5f)
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
        internal void GetNeighboringTiles(BaseTile tile, List<BaseTile> neighborList, bool shuffle = true, bool checkTileHigher = false, bool checkTileLower = false, bool attachHeightIndicator = false)
        {
            TilePoint neighborPos = new TilePoint(tile.TilePoint.X, tile.TilePoint.Y, tile.TilePoint.ParentTileMap);
            int yOffset = tile.TilePoint.X % 2 == 0 ? 1 : 0;

            for (int i = 0; i < 6; i++)
            {
                neighborPos.X = tile.TilePoint.X;
                neighborPos.Y = tile.TilePoint.Y;
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

                if (IsValidTile(neighborPos))
                {
                    BaseTile neighborTile = this[neighborPos];
                    if (!neighborTile.TilePoint._visited)
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
                        neighborTile.TilePoint._visited = true;
                    }
                }
            }

            if (shuffle)
            {
                ShuffleList(neighborList);
            }
        }

        internal BaseTile GetNeighboringTile(BaseTile tile, Direction direction) 
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

        private BaseTile _BaseTileTemp = new BaseTile();
        private readonly List<TileClassification> _EmptyTileClassification = new List<TileClassification>();

        internal class TileWithParent
        {
            internal TileWithParent Parent = null;
            internal BaseTile Tile;
            internal float G = 0; //Path cost
            internal float H = 0; //Distance to end
            internal float F => G + H;
            internal bool Visited = false;

            internal TileWithParent(BaseTile tile, TileWithParent parent = null)
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

    internal class TileMapPoint
    {
        internal int X;
        internal int Y;

        internal MapPosition MapPosition = MapPosition.None;

        internal TileMapPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        internal TileMapPoint(Vector2i coords)
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

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    internal class DynamicTextureInfo 
    {
        internal bool TextureChanged = false;
        internal bool Initialize = true;
    }
}
