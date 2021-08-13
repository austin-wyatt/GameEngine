using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles.HelperTiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Tiles
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

        public static List<TileClassification> AllTileClassifications = new List<TileClassification>()
        { TileClassification.Ground, TileClassification.AttackableTerrain, TileClassification.Terrain, TileClassification.Water };


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

        public TileMapPoint TileMapCoords;

        public TileMapController Controller;

        public List<BaseTile> Tiles = new List<BaseTile>();
        public List<BaseTile> SelectionTiles = new List<BaseTile>(); //these tiles will be place above the currently selected tiles
        public BaseTile HoveredTile;

        public List<TileChunk> TileChunks = new List<TileChunk>();

        public Texture DynamicTexture;
        public FrameBufferObject FrameBuffer;
        public DynamicTextureInfo DynamicTextureInfo = new DynamicTextureInfo();
        public HashSet<BaseTile> TilesToUpdate = new HashSet<BaseTile>();
        public GameObject TexturedQuad;

        private int _maxSelectionTiles = 1000;
        public int _amountOfSelectionTiles = 0;
        private List<BaseTile> _selectionTilePool = new List<BaseTile>();

        private List<BaseTile> _hoveredTileList = new List<BaseTile>();
        public TileMap(Vector3 position, TileMapPoint point, TileMapController controller, string name = "TileMap")
        {
            Position = position; //position of the first tile
            Name = name;

            TileMapCoords = point;
            Controller = controller;
        }

        public BaseTile this[int x, int y]
        {
            get { return GetTile(x, y); }
        }

        public BaseTile this[TilePoint point]
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

        public virtual void PopulateTileMap(float zTilePlacement = 0)
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
            InitializeHelperTiles(tilePosition);

            SetDefaultTileValues();
            InitializeTexturedQuad();
        }

        internal void InitializeHelperTiles(Vector3 tilePosition)
        {
            BaseTile baseTile = new BaseTile();
            for (int i = 0; i < _maxSelectionTiles; i++)
            {
                baseTile = new BaseTile(tilePosition, new TilePoint(i, -1,this));
                baseTile.SetRender(false);
                baseTile._tileObject.OutlineParameters.OutlineColor = Colors.TranslucentBlue;
                baseTile._tileObject.OutlineParameters.InlineColor = Colors.TranslucentBlue;
                //baseTile._tileObject.OutlineParameters.OutlineThickness = 2;
                baseTile._tileObject.OutlineParameters.SetAllInline(4);
                baseTile.SetAnimation(BaseTileAnimationType.SolidWhite);
                baseTile.DefaultAnimation = BaseTileAnimationType.SolidWhite;

                //baseTile.DefaultColor = Colors.TranslucentBlue;
                //baseTile.SetColor(Colors.TranslucentBlue);

                baseTile.DefaultColor = Colors.Transparent;
                baseTile.SetColor(Colors.Transparent);

                //SelectionTiles.Add(baseTile);
                _selectionTilePool.Add(baseTile);
            }

            tilePosition.Z += 0.001f;
            HoveredTile = new BaseTile(tilePosition, new TilePoint(-1, -1, this));
            HoveredTile.SetRender(false);
            //HoveredTile._tileObject.OutlineParameters.OutlineColor = Colors.Red;
            //HoveredTile._tileObject.OutlineParameters.InlineColor = Colors.Red;
            //HoveredTile._tileObject.OutlineParameters.SetAllOutline(0);
            //HoveredTile._tileObject.OutlineParameters.SetAllInline(10);
            
            HoveredTile.SetAnimation(BaseTileAnimationType.Transparent);
            HoveredTile.DefaultAnimation = BaseTileAnimationType.Transparent;

            HoveredTile.SetColor(Colors.Red);
            HoveredTile._tileObject.OutlineParameters.SetAllInline(0);

            _hoveredTileList = new List<BaseTile>() { HoveredTile };
        }

        public virtual void OnAddedToController() 
        {
            InitializeTileChunks();
        }

        public void InitializeTileChunks() 
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

            obj.TextureReference = DynamicTexture;
            obj.TextureReference.TextureName = TextureName.DynamicTexture;
            obj.Textures.Textures[0] = TextureName.DynamicTexture;

            Renderer.LoadTextureFromTextureObj(obj.TextureReference, TextureName.DynamicTexture);

            Animation Idle = new Animation()
            {
                Frames = new List<RenderableObject>() { obj },
                Frequency = -1,
                Repeats = -1
            };

            BaseObject baseObj = new BaseObject(new List<Animation>() { Idle }, 0, "", new Vector3());

            GameObject temp = new GameObject();
            temp.BaseObjects.Add(baseObj);

            temp.BaseObjects[0].BaseFrame.CameraPerspective = true;
            temp.BaseObjects[0].BaseFrame.ScaleX(0.7783f);
            temp.BaseObjects[0].BaseFrame.ScaleY(1.035f);
            temp.BaseObjects[0].BaseFrame.ScaleAll(Width);
            //temp.BaseObjects[0].BaseFrame.ScaleY(1.065f); //10x10
            //temp.BaseObjects[0].BaseFrame.ScaleY(1.04f); //20x20
            //temp.BaseObjects[0].BaseFrame.ScaleY(1.03f); //30x30
            //temp.BaseObjects[0].BaseFrame.ScaleY(1.027f); //40x40

            //temp.BaseObjects[0].BaseFrame.ScaleY(1.0237f); //50x50
            //temp.BaseObjects[0].BaseFrame.ScaleX(1.0206f);

            //temp.BaseObjects[0].BaseFrame.ScaleY(1.019f); //100x100
            //temp.BaseObjects[0].BaseFrame.ScaleX(1.0206f);

            TexturedQuad = temp;
            
            UpdateQuadPosition();
        }

        internal void UpdateQuadPosition() 
        {
            Vector3 tileMapPos = (Tiles[0].Position + Tiles[Tiles.Count - 1].Position) / 2;


            //the most magic of magic numbers (I'll probably need to redo tiling in some fashion if I want to make a general solution for this)

            //TexturedQuad.SetPosition(new Vector3(tileMapPos.X + 7110, tileMapPos.Y + 3720, tileMapPos.Z)); //100x100

            //TexturedQuad.SetPosition(new Vector3(tileMapPos.X + 3520, tileMapPos.Y + 1740, tileMapPos.Z)); //50x50

            //TexturedQuad.SetPosition(new Vector3(tileMapPos.X + 2800, tileMapPos.Y + 1360, tileMapPos.Z)); //40x40


            TexturedQuad.SetPosition(tileMapPos + new Vector3(363, 1800, 0));
            //TexturedQuad.SetPosition(new Vector3(0, 0, 0));
        }

        internal void UpdateDynamicTexture() 
        {
            //DynamicTexture.UpdateTextureArray(DynamicTextureInfo.MinChangedBounds, DynamicTextureInfo.MaxChangedBounds, this);
            TileTexturer.UpdateTexture(this);
            TilesToUpdate.Clear();
            DynamicTextureInfo.TextureChanged = false;
        }

        public override void CleanUp()
        {
            base.CleanUp();

            if (FrameBuffer != null) 
            {
                FrameBuffer.Dispose();
            }
        }

        public Vector3 GetTileMapDimensions() 
        {
            if (Tiles.Count > 0) 
            {
                Vector3 tileDim = this[0, 0].GetDimensions();
                Vector3 returnDim = this[0, 0].Position - this[Width - 1, Height - 1].Position;

                returnDim.X = Math.Abs(returnDim.X) + tileDim.X * 0.731f;
                returnDim.Y = Math.Abs(returnDim.Y) + tileDim.Y * 1.475f;

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
            });

            base.SetPosition(position);

            UpdateQuadPosition();
        }

        public List<BaseTile> GetHoveredTile()
        {
            return _hoveredTileList;
        }

        #region Tile validity checks
        public bool IsValidTile(int xIndex, int yIndex)
        {
            return Controller.IsValidTile(xIndex, yIndex, this);
        }
        public bool IsValidTile(TilePoint point)
        {
            return Controller.IsValidTile(point.X, point.Y, point.ParentTileMap);
            //return point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height;
        }
        #endregion

        #region Tile conversions
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
            cubeCoord.X = offset.X ;
            cubeCoord.Z = offset.Y - (offset.X + (offset.X & 1)) / 2;
            cubeCoord.Y = -cubeCoord.X - cubeCoord.Z;

            return cubeCoord;
        }
        public Vector3i OffsetToCube(TilePoint offset)
        {
            int xMapOffset = (offset.ParentTileMap.TileMapCoords.X - TileMapCoords.X) * Width; 
            int yMapOffset = (offset.ParentTileMap.TileMapCoords.Y - TileMapCoords.Y) * Height; 

            Vector3i cubeCoord = new Vector3i();
            cubeCoord.X = offset.X + xMapOffset;
            cubeCoord.Z = (offset.Y + yMapOffset) - ((offset.X + xMapOffset) + (offset.X & 1)) / 2;
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

        public List<BaseTile> GetSelectionTilePool()
        {
            return _selectionTilePool;
        }

        public void SelectTiles(List<BaseTile> tiles)
        {
            if (tiles.Count > _maxSelectionTiles)
                throw new Exception("Attempted to select " + tiles.Count + " tiles while the maximum was " + _maxSelectionTiles + " in tile map " + ObjectID);

            for (int i = 0; i < tiles.Count; i++)
            {
                SelectTile(tiles[i]);
            }
        }

        public void SelectTile(BaseTile tile)
        {
            if (_amountOfSelectionTiles == _selectionTilePool.Count)
                _amountOfSelectionTiles--;

            Vector3 pos = new Vector3();
            pos.X = tile.Position.X;
            pos.Y = tile.Position.Y;
            pos.Z = _selectionTilePool[_amountOfSelectionTiles].Position.Z;

            _selectionTilePool[_amountOfSelectionTiles].SetPosition(pos);
            _selectionTilePool[_amountOfSelectionTiles].SetRender(true);

            SelectionTiles.Add(_selectionTilePool[_amountOfSelectionTiles]);

            _selectionTilePool[_amountOfSelectionTiles].AttachedTile = tile;
            tile.AttachedTile = _selectionTilePool[_amountOfSelectionTiles];

            _amountOfSelectionTiles++;
        }

        public void DeselectTile(BaseTile selectionTile)
        {
            if (SelectionTiles.Remove(selectionTile))
            {
                selectionTile.SetRender(false);
                selectionTile.AttachedTile.AttachedTile = null;
                selectionTile.AttachedTile = null;
                _amountOfSelectionTiles--;
            }
        }

        public void DeselectTiles()
        {
            for (int i = 0; i < _amountOfSelectionTiles; i++)
            {
                _selectionTilePool[i].SetRender(false);
                if (_selectionTilePool[i].AttachedTile != null)
                {
                    _selectionTilePool[i].AttachedTile.AttachedTile = null;
                    _selectionTilePool[i].AttachedTile = null;
                }
            }

            SelectionTiles.Clear();

            _amountOfSelectionTiles = 0;
        }

        public void AddHeightIndicator(BaseTile attachedTile, bool up = true) 
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

        public void RemoveHeightIndicatorTile(BaseTile attachedTile) 
        {
            if (attachedTile.HeightIndicator != null) 
            {
                Controller.Scene._genericObjects.Remove(attachedTile.HeightIndicator);
            }
        }

        public void HoverTile(BaseTile tile)
        {
            Vector3 pos = new Vector3();
            pos.X = tile.Position.X;
            pos.Y = tile.Position.Y;
            pos.Z = HoveredTile.Position.Z;

            if (!tile.Hovered) 
            {
                EndHover();
                HoveredTile.AttachedTile = tile;
            }

            tile.OnHover();

            HoveredTile.SetPosition(pos);
            HoveredTile.SetRender(true);
        }

        public void EndHover()
        {
            if (HoveredTile.AttachedTile != null) 
            {
                HoveredTile.AttachedTile.HoverEnd();
                HoveredTile.AttachedTile = null;
            }

            HoveredTile.SetRender(false);
        }


        public void SetDefaultTileValues()
        {
            Tiles.ForEach(tile =>
            {
                tile.SetColor(tile.DefaultColor);
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
        public int GetDistanceBetweenPoints(TilePoint start, TilePoint end)
        {
            Vector3i a = OffsetToCube(start);
            Vector3i b = OffsetToCube(end);

            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }
        public int GetDistanceBetweenPoints(Vector3i a, Vector3i b)
        {
            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }
        #endregion

        internal void ClearVisitedTiles() 
        {
            Controller.ClearAllVisitedTiles();
        }

        public struct TilesInRadiusParameters 
        {
            public TilePoint StartingPoint;
            public int Radius;
            public List<TileClassification> TraversableTypes;
            public List<Unit> Units;
            public Unit CastingUnit;
            public AbilityTypes AbilityType;
            public bool CheckHeight;

            public TilesInRadiusParameters(TilePoint startingPoint, int radius) 
            {
                StartingPoint = startingPoint;
                Radius = radius;
                TraversableTypes = new List<TileClassification>();
                Units = new List<Unit>();
                CastingUnit = null;
                AbilityType = AbilityTypes.Empty;
                CheckHeight = true;
            }

        }

        //gets tiles in a radius from a center point by expanding outward to all valid neighbors until the radius is reached.
        public List<BaseTile> FindValidTilesInRadius(TilesInRadiusParameters param)
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
                    GetNeighboringTiles(n, newNeighbors, default, param.CheckHeight);
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
                        if (u.TileMapPosition == neighbors[j].TilePoint)
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
                        if (param.Units[unitIndex].BlocksSpace && !param.CastingUnit.PhasedMovement)
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
        public List<BaseTile> GetLineOfTiles(TilePoint startPoint, TilePoint endPoint)
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
                currentCube = TileMapConstants.cube_lerp(startCube, endCube, n * i);

                currentOffset = CubeToOffset(TileMapConstants.cube_round(currentCube));
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
        public void GetRingOfTiles(TilePoint startPoint, List<Vector2i> outputList, int radius = 1, bool includeEdges = true)
        {
            Vector3i cubePosition = OffsetToCube(startPoint);

            cubePosition += TileMapConstants.CubeDirections[Direction.North] * radius;

            Vector2i tileOffsetCoord;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    tileOffsetCoord = CubeToOffset(cubePosition);

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
        public void GetRingOfTiles(TilePoint startPoint, List<BaseTile> outputList, int radius = 1, bool includeEdges = true)
        {
            Vector3i cubePosition = OffsetToCube(startPoint);

            cubePosition += TileMapConstants.CubeDirections[Direction.North] * radius;

            Vector2i tileOffsetCoord;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    tileOffsetCoord = CubeToOffset(cubePosition);
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
        public void GetVisionLine(TilePoint startPoint, Vector2i endPoint, List<BaseTile> outputList, List<TileClassification> untraversableTypes = default, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            Vector3i startCube = OffsetToCube(startPoint);
            Vector3i endCube = OffsetToCube(endPoint);

            int N = GetDistanceBetweenPoints(startCube, endCube);
            float n = 1f / N;

            Vector3 currentCube;
            Vector2i currentOffset;

            BaseTile currentTile = null;

            if (IsValidTile(startPoint.X, startPoint.Y)) 
            {
                currentTile = this[startPoint.X, startPoint.Y];
            }

            for (int i = 0; i <= N; i++)
            {
                currentCube = TileMapConstants.cube_lerp(startCube, endCube, n * i);
                currentOffset = CubeToOffset(TileMapConstants.cube_round(currentCube));
                if (IsValidTile(currentOffset.X, currentOffset.Y))
                {
                    _BaseTileTemp = this[currentOffset.X, currentOffset.Y];

                    if (currentTile != null && _BaseTileTemp.Properties.Height - currentTile.Properties.Height > 1)
                    {
                        //_BaseTileTemp.AttachHeightIndicator(outputList[outputList.Count - 2]) or something (pass the previous tile so the indicator can be specific to the direction)
                        AddHeightIndicator(_BaseTileTemp);
                        return;
                    }

                    outputList.Add(_BaseTileTemp);

                    if ((_BaseTileTemp.Properties.BlocksVision && !ignoreBlockedVision) || (untraversableTypes != null && untraversableTypes.Contains(_BaseTileTemp.Properties.Classification)))
                    {
                        return;
                    }
                    
                    if (units?.Count > 0)
                    {
                        if (units.Exists(unit => unit.TileMapPosition == this[currentOffset.X, currentOffset.Y].TilePoint && unit.BlocksVision))
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
        public List<BaseTile> GetVisionInRadius(TilePoint point, int radius, List<TileClassification> untraversableTypes = default, List<Unit> units = default, bool ignoreBlockedVision = false)
        {
            List<Vector2i> ringOfTiles = new List<Vector2i>();
            GetRingOfTiles(point, ringOfTiles, radius);

            List<BaseTile> outputList = new List<BaseTile>();

            for (int i = 0; i < ringOfTiles.Count; i++)
            {
                GetVisionLine(point, ringOfTiles[i], outputList, untraversableTypes, units, ignoreBlockedVision);
            }

            return outputList.Distinct().ToList();
        }



        public struct PathToPointParameters
        {
            public TilePoint StartingPoint;
            public TilePoint EndingPoint;
            public int Depth;
            public List<TileClassification> TraversableTypes;
            public List<Unit> Units;
            public Unit CastingUnit;
            public AbilityTypes AbilityType;
            public bool CheckHeight;

            public PathToPointParameters(TilePoint startingPoint, TilePoint endPoint, int depth)
            {
                StartingPoint = startingPoint;
                EndingPoint = endPoint;
                Depth = depth;
                TraversableTypes = new List<TileClassification>();
                Units = new List<Unit>();
                CastingUnit = null;
                AbilityType = AbilityTypes.Empty;
                CheckHeight = true;
            }
        }

        /// <summary>
        /// Get a list of tiles that leads from the start index to the end index.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="depth">The maximum length of the path</param>
        /// <returns></returns>
        public List<BaseTile> GetPathToPoint(PathToPointParameters param)
        {
            List<TileWithParent> tileList = new List<TileWithParent>();
            List<BaseTile> returnList = new List<BaseTile>();

            if (param.Depth <= 0)
                return returnList;

            List<BaseTile> neighbors = new List<BaseTile>();

            ClearVisitedTiles();

            if (!param.TraversableTypes.Exists(c => c == this[param.StartingPoint].Properties.Classification) || !param.TraversableTypes.Exists(c => c == this[param.EndingPoint].Properties.Classification))
            {
                return returnList; //if the starting or ending tile isn't traversable then immediately return
            }

            if (param.Units != null && param.Units.Exists(u => u.TileMapPosition == param.EndingPoint) && param.CastingUnit != null && !param.CastingUnit.PhasedMovement)
            {
                return returnList; //if the ending tile is inside of a unit then immediately return
            }



            neighbors.Add(this[param.StartingPoint]);
            param.StartingPoint._visited = true;

            tileList.Add(new TileWithParent(this[param.StartingPoint]));

            List<BaseTile> newNeighbors = new List<BaseTile>();


            for (int i = 0; i < param.Depth; i++)
            {
                newNeighbors.Clear();
                neighbors.ForEach(p =>
                {
                    GetNeighboringTiles(p, newNeighbors, default, true);

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

                    if (neighbors[j].TilePoint == param.EndingPoint)
                    {
                        //if we found the destination tile then fill the returnList and return
                        TileWithParent finalTile = tileList.Find(t => t.Tile.TilePoint == param.EndingPoint);

                        returnList.Add(finalTile.Tile);

                        BaseTile parent = finalTile.Parent;

                        while (parent != null)
                        {
                            TileWithParent currentTile = tileList.Find(t => t.Tile.TilePoint == parent.TilePoint);
                            returnList.Add(currentTile.Tile);

                            parent = currentTile.Parent;
                        }

                        returnList.Reverse();
                        return returnList;
                    }

                    int distanceToDestination = GetDistanceBetweenPoints(neighbors[j].TilePoint, param.EndingPoint);

                    if (distanceToDestination + i > param.Depth)
                    {
                        //if the best case path to the end point is longer than our remaining movement cut off this branch and continue
                        neighbors.RemoveAt(j);
                        j--;
                        continue;
                    }

                    //find if there's a unit in this space
                    param.Units?.Exists(u =>
                    {
                        count++;
                        if (u.TileMapPosition == neighbors[j].TilePoint)
                        {
                            unitIndex = count;
                            return true;
                        }
                        else
                            return false;
                    });

                    if (!param.TraversableTypes.Exists(c => c == neighbors[j].Properties.Classification))
                    {
                        //if the space contains a tile that is not traversable cut off this branch and continue
                        neighbors.RemoveAt(j);
                        j--;
                        continue;
                    }
                    else if (unitIndex != -1 && param.CastingUnit != null && param.AbilityType == AbilityTypes.Move) //special cases for ability targeting should go here
                    {
                        if (param.Units[unitIndex].BlocksSpace && !param.CastingUnit.PhasedMovement)
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
        /// <param name="tile"></param>
        /// <param name="neighborList"></param>
        /// <param name="visitedTiles"></param>
        public void GetNeighboringTiles(BaseTile tile, List<BaseTile> neighborList, bool shuffle = true, bool checkHeight = false, bool attachHeightIndicator = false)
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
                        if (checkHeight && neighborTile.Properties.Height - tile.Properties.Height > 1) 
                        {
                            //attach tile height indicator here
                            if (attachHeightIndicator) 
                            {
                                AddHeightIndicator(neighborTile);
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

        public static bool operator ==(TileMapPoint a, TileMapPoint b) => a.Equals(b);
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

    public class DynamicTextureInfo 
    {
        public bool TextureChanged = false;
    }
}
