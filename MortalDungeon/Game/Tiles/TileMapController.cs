using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles.TileMaps;
using System.Diagnostics;
using MortalDungeon.Game.Units;
using MortalDungeon.Game.Entities;
using System.Linq;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Serializers;
using System.Threading.Tasks;
using System.Threading;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;

namespace MortalDungeon.Game.Tiles
{
    public enum MapPosition
    {
        None = 0,
        Top = 1,
        Left = 2,
        Bot = 4,
        Right = 8,
    }

    public class TileMapController
    {
        public static readonly Texture TileSpritesheet = Texture.LoadFromFile("Resources/TileSpritesheet.png");
        public static readonly Texture TileOverlaySpritesheet = Texture.LoadFromFile("Resources/TileOverlaySpritesheet.png");

        public List<TileMap> TileMaps = new List<TileMap>();
        public HashSet<TileMapPoint> LoadedPoints = new HashSet<TileMapPoint>();

        public static StaticBitmap TileBitmap;

        public int BaseElevation = 0; //base elevation for determining heightmap colors

        public CombatScene Scene;

        public QueuedList<BaseTile> SelectionTiles = new QueuedList<BaseTile>(); //these tiles will be place above the currently selected tiles
        private const int MAX_SELECTION_TILES = 1000;
        public int _amountOfSelectionTiles = 0;
        private readonly List<BaseTile> _selectionTilePool = new List<BaseTile>();

        public BaseTile HoveredTile;
        private List<BaseTile> _hoveredTileList = new List<BaseTile>();

        public Dictionary<TileMapPoint, InstancedRenderData> TilePillarsRenderData = new Dictionary<TileMapPoint, InstancedRenderData>();

        public TileMapController(CombatScene scene = null) 
        {
            Scene = scene;

            InitializeHelperTiles();
        }

        private void InitializeHelperTiles() 
        {
            BaseTile baseTile = new BaseTile();
            Vector3 tilePosition = new Vector3(0, 0, 0.03f);

            for (int i = 0; i < MAX_SELECTION_TILES; i++)
            {
                baseTile = new BaseTile(tilePosition, new TilePoint(i, -1, null));
                baseTile.SetRender(false);

                baseTile.DefaultColor = _Colors.TranslucentBlue;
                baseTile.SetColor(_Colors.TranslucentBlue);

                baseTile.Properties.Type = (TileType)1;

                //baseTile.BaseObject.EnableLighting = false;

                _selectionTilePool.Add(baseTile);
            }

            GameObject.LoadTextures(_selectionTilePool);

            HoveredTile = new BaseTile(new Vector3(0, 0, 0.05f), new TilePoint(-1, -1, null));
            HoveredTile.SetRender(false);

            HoveredTile.Properties.Type = (TileType)1;
            HoveredTile.SetColor(_Colors.TranslucentRed);

            //HoveredTile.BaseObject.EnableLighting = false;

            GameObject.LoadTexture(HoveredTile);

            _hoveredTileList = new List<BaseTile>() { HoveredTile };
        }

        public void AddTileMap(TileMapPoint point, TileMap map) 
        {
            map.TileMapCoords = new TileMapPoint(point.X, point.Y);
            TileMaps.Add(map);
            LoadedPoints.Add(point);

            //PositionTileMaps();
            map.OnAddedToController();
        }

        public void RemoveTileMap(TileMap map) 
        {
            List<Entity> entitiesToUnload = new List<Entity>();

            lock (EntityManager.Entities) 
            {
                for (int i = EntityManager.Entities.Count - 1; i >= 0; i--) 
                {
                    if (EntityManager.Entities[i].Handle.OnTileMap(map))
                    {
                        entitiesToUnload.Add(EntityManager.Entities[i]);
                    }
                }
            }


            Scene.QueueToRenderCycle(() =>
            {
                foreach(Entity entity in entitiesToUnload)
                {
                    EntityManager.UnloadEntity(entity);
                }

                map.CleanUp();
            });
            //map.CleanUp();

            TileMaps.Remove(map);
            LoadedPoints.Remove(map.TileMapCoords);
        }

        public void PositionTileMaps() 
        {
            if (TileMaps.Count == 0)
                return;

            Vector3 tileMapDimensions = TileMaps[0].GetTileMapDimensions();

            int minX = int.MaxValue;
            int maxX = int.MinValue;

            int minY = int.MaxValue;
            int maxY = int.MinValue;

            TileMaps.ForEach(map =>
            {
                if (map.TileMapCoords.X < minX) 
                {
                    minX = map.TileMapCoords.X;
                }
                if (map.TileMapCoords.X > maxX)
                {
                    maxX = map.TileMapCoords.X;
                }
                if (map.TileMapCoords.Y < minY)
                {
                    minY = map.TileMapCoords.Y;
                }
                if (map.TileMapCoords.Y > maxY)
                {
                    maxY = map.TileMapCoords.Y;
                }
            });

            Vector2i centerTileMapCoords = new Vector2i((maxX + minX) / 2, (maxY + minY) / 2);
            //Vector2i centerTileMapCoords = new Vector2i(0, 0);

            Vector3 offset = new Vector3();
            for (int i = 0; i < TileMaps.Count; i++) 
            {
                //Vector3 pos = new Vector3(tileMapDimensions.X * (TileMaps[i].TileMapCoords.X - centerTileMapCoords.X), tileMapDimensions.Y * (TileMaps[i].TileMapCoords.Y - centerTileMapCoords.Y), 0);
                Vector3 pos = new Vector3(tileMapDimensions.X * (TileMaps[i].TileMapCoords.X), tileMapDimensions.Y * (TileMaps[i].TileMapCoords.Y), 0);

                if(i == 0) 
                {
                    offset = new Vector3(pos.X - TileMaps[i].Position.X, pos.Y - TileMaps[i].Position.Y, 0);

                    offset.X /= WindowConstants.ScreenUnits.X;
                    offset.Y /= WindowConstants.ScreenUnits.Y * -1;

                }

                TileMaps[i].SetPosition(pos);
            }
        }

        public void RecreateTileChunks() 
        {
            TileMaps.ForEach(map =>
            {
                map.InitializeTileChunks();
            });
        }

        public void CreateTilePillarsForMap(TileMap map) 
        {
            List<GameObject> tilePillars = new List<GameObject>();

            List<InstancedRenderData> data;

            for (int j = 0; j < map.Tiles.Count; j++)
            {
                GameObject tent1 = new GameObject();
                tent1.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(0, Textures.TentTexture), _3DObjects.TilePillar, default));

                tent1.SetPosition(map.Tiles[j].Position + new Vector3(0, 217, -1.0f));

                tent1.BaseObject.BaseFrame.SetScale(1.64f, 1.64f, 1);

                Renderer.LoadTextureFromGameObj(tent1);

                tilePillars.Add(tent1);

            }

            data = InstancedRenderData.GenerateInstancedRenderData(tilePillars);
            foreach (var item in data)
            {
                TilePillarsRenderData.TryAdd(map.TileMapCoords, item);
            }

            tilePillars.Clear();
        }

        public void ApplyFeatureEquationToMaps(FeatureEquation feature) 
        {
            for (int i = 0; i < TileMaps.Count; i++) 
            {
                if (feature.AffectsMap(TileMaps[i])) 
                {
                    feature.ApplyToMap(TileMaps[i]);
                }
            }
            feature.OnAppliedToMaps();
        }

        //TODO, optimize so that it only loops through every tile once
        public void ApplyLoadedFeaturesToMaps(List<TileMapPoint> points)
        {
            foreach(var feature in FeatureManager.LoadedFeatures)
            {
                for (int i = 0; i < TileMaps.Count; i++)
                {
                    if (points.Count == 0 || points.Exists(p => p == TileMaps[i].TileMapCoords))
                    {
                        if (feature.Value.AffectsMap(TileMaps[i]))
                        {
                            feature.Value.ApplyToMap(TileMaps[i]);
                        }
                    }
                }

                feature.Value.OnAppliedToMaps();
            }
        }

        public void ApplyLoadedFeaturesToMaps(List<TileMap> maps, List<TileMapPoint> addedMaps = null)
        {
            var featureList = FeatureManager.LoadedFeatures.Values.ToList();

            featureList.Sort((a, b) => b.LoadPriority.CompareTo(a.LoadPriority));

            foreach (var feature in FeatureManager.LoadedFeatures)
            {
                for (int i = 0; i < maps.Count; i++)
                {
                    bool freshGen = true;

                    if (addedMaps != null) 
                    {
                        freshGen = addedMaps.Exists(p => p == maps[i].TileMapCoords);
                    }

                    if (feature.Value.AffectsMap(maps[i]))
                    {
                        feature.Value.ApplyToMap(maps[i], freshGen);
                    }
                }

                feature.Value.OnAppliedToMaps();
            };
        }

        public void ApplyLoadedFeaturesToMap(TileMap map, bool freshGeneration = true) 
        {
            foreach (var feature in FeatureManager.LoadedFeatures)
            {
                if (feature.Value.AffectsMap(map))
                {
                    feature.Value.ApplyToMap(map, freshGeneration);
                }
            };
        }

        //public void AddFeature(FeatureEquation feature) 
        //{
        //    LoadedFeatures.Add(feature);
        //}


        const int LOADED_MAP_DIMENSIONS = 3;
        public object _mapLoadLock = new object();

        public void LoadMapsWithFade(TileMapPoint point, bool applyFeatures = true,
            bool forceMapRegeneration = false, Action onFinish = null, int layer = 0)
        {
            void loadMaps()
            {
                RenderFunctions.FadeParameters.FadeComplete -= loadMaps;

                Scene.SyncToRender(() =>
                {
                    LoadSurroundingTileMaps(point, applyFeatures, forceMapRegeneration, onFinish, layer, withFade: false, didFade: true);
                });
            }

            RenderFunctions.FadeParameters.StepSize = 0.05f;
            //RenderFunctions.FadeParameters.TimeDelay = 25;
            RenderFunctions.FadeParameters.TimeDelay = 15;
            RenderFunctions.FadeParameters.StartFade(FadeDirection.Out);

            RenderFunctions.FadeParameters.FadeComplete += loadMaps;
        }

        public void LoadSurroundingTileMaps(TileMapPoint point, bool applyFeatures = true, 
            bool forceMapRegeneration = false, Action onFinish = null, int layer = 0, bool withFade = true, bool didFade = false) 
        {
            if (withFade)
            {
                LoadMapsWithFade(point, applyFeatures, forceMapRegeneration, onFinish, layer);
                return;
            }

            lock (_mapLoadLock)
            {
                TileMapPoint currPoint = new TileMapPoint(point.X - (LOADED_MAP_DIMENSIONS - 1) / 2, point.Y - (LOADED_MAP_DIMENSIONS - 1) / 2);

                HashSet<TileMapPoint> loadedPoints = new HashSet<TileMapPoint>();
                List<TileMapPoint> mapsToAdd = new List<TileMapPoint>();

                Scene.ContextManager.SetFlag(GeneralContextFlags.TileMapLoadInProgress, true);

                FeatureManager.EvaluateLoadedFeatures(new FeaturePoint(point.X * TileMapManager.TILE_MAP_DIMENSIONS.X, point.Y * TileMapManager.TILE_MAP_DIMENSIONS.Y), layer);


                Stopwatch timer = new Stopwatch();
                timer.Start();

                if (forceMapRegeneration)
                {
                    for (int i = TileMaps.Count - 1; i >= 0; i--)
                    {
                        RemoveTileMap(TileMaps[i]);
                    }
                }


                for (int i = 0; i < LOADED_MAP_DIMENSIONS; i++) 
                {
                    for (int j = 0; j < LOADED_MAP_DIMENSIONS; j++) 
                    {
                        TileMap map = TileMaps.Find(m => m.TileMapCoords == currPoint);

                        if (map == null)
                        {
                            mapsToAdd.Add(new TileMapPoint(currPoint.X, currPoint.Y));
                        }

                        TileMapPoint mapPoint = new TileMapPoint(currPoint.X, currPoint.Y);

                        mapPoint.MapPosition = GetMapPosition(i * LOADED_MAP_DIMENSIONS + j);

                        loadedPoints.Add(mapPoint);

                        currPoint.Y++;
                    }

                    currPoint.X++;
                    currPoint.Y = point.Y - (LOADED_MAP_DIMENSIONS - 1) / 2;
                }

                for (int i = TileMaps.Count - 1; i >= 0; i--)
                {
                    if (!loadedPoints.Contains(TileMaps[i].TileMapCoords))
                    {
                        RemoveTileMap(TileMaps[i]);
                    }
                }

                //GC.Collect();

                List<TileMap> addedMaps = new List<TileMap>();

                mapsToAdd.ForEach(p =>
                {
                    TestTileMap newMap = new TestTileMap(default, p, this) { Width = TileMapManager.TILE_MAP_DIMENSIONS.X, Height = TileMapManager.TILE_MAP_DIMENSIONS.Y };
                    newMap.PopulateTileMap();
                    AddTileMap(p, newMap);

                    addedMaps.Add(newMap);
                });

                foreach(var m in TileMaps)
                {
                    loadedPoints.TryGetValue(m.TileMapCoords, out var foundPoint);

                    m.TileMapCoords.MapPosition = foundPoint.MapPosition;

                    if (m.TileMapCoords.MapPosition.HasFlag(MapPosition.Top) && m.TileMapCoords.MapPosition.HasFlag(MapPosition.Left))
                    {
                        _topLeftMap = m;
                    }
                }

                List<TileMapPoint> itemsToRemove = new List<TileMapPoint>();
                foreach(var kvp in TilePillarsRenderData)
                {
                    if (!LoadedPoints.Contains(kvp.Key))
                    {
                        itemsToRemove.Add(kvp.Key);
                    }
                }

                List<InstancedRenderData> renderDataToRemove = new List<InstancedRenderData>();
                foreach(var item in itemsToRemove)
                {
                    var renderData = TilePillarsRenderData[item];
                    renderDataToRemove.Add(renderData);

                    //TilePillarsRenderData[item].CleanUp();
                    TilePillarsRenderData.Remove(item);
                }

                Scene.QueueToRenderCycle(() =>
                {
                    for(int i = 0; i < renderDataToRemove.Count; i++)
                    {
                        renderDataToRemove[i].CleanUp();
                    }
                });

                timer.Restart();
                //ApplyLoadedFeaturesToMaps(mapsToAdd);
                if (applyFeatures)
                {
                    ApplyLoadedFeaturesToMaps(TileMaps, mapsToAdd);
                }


                PositionTileMaps();

                Console.WriteLine("LoadSurroundingTileMaps completed in " + timer.ElapsedMilliseconds + "ms");

                Scene.ContextManager.SetFlag(GeneralContextFlags.TileMapLoadInProgress, false);

                //Scene.UpdateVisionMap(() => Scene.FillInTeamFog());
                //Scene.FillInTeamFog();

                Scene.SyncToRender(() =>
                {
                    if (didFade)
                    {
                        RenderFunctions.FadeParameters.StartFade(FadeDirection.In);

                        static void fadeEnd()
                        {
                            RenderFunctions.FadeParameters.EndFade();
                            RenderFunctions.FadeParameters.FadeComplete -= fadeEnd;
                        }

                        RenderFunctions.FadeParameters.FadeComplete += fadeEnd;
                    }
                });

                foreach (var map in addedMaps)
                {
                    Scene.QueueToRenderCycle(() =>
                    {
                        map.UpdateTile(map.Tiles[0]);
                    });

                    Scene.QueueToRenderCycle(() =>
                    {
                        CreateTilePillarsForMap(map);
                    });
                }

                Scene.Controller.CullObjects();

                Scene.QueueLightObstructionUpdate();

                onFinish?.Invoke();

                Scene.OnCameraMoved();

                Scene.QueueToRenderCycle(() =>
                {
                    foreach (var unit in Scene._units)
                    {
                        unit.VisionGenerator.SetPosition(unit.Info.TileMapPosition);
                        unit.LightObstruction.SetPosition(unit.Info.TileMapPosition);
                    }
                });

                Scene.QueueToRenderCycle(() =>
                {
                    Scene.UnitVisionGenerators.ManuallyIncrementChangeToken();
                    Scene.LightObstructions.ManuallyIncrementChangeToken();

                    Scene.UnitVisionGenerators.HandleQueuedItems();
                    Scene.LightObstructions.HandleQueuedItems();

                    Scene.OnStructureMoved();
                    //Scene.UpdateVisionMap();
                });
            }
        }

        private TileMap _topLeftMap;

        public void UpdateTileMapRenderData()
        {
            foreach (var map in TileMaps)
            {
                map.UpdateTileRenderData();
            }
        }

        //private TileMap _topLeftMap;
        //public Vector2i GetTopLeftTilePosition()
        //{
        //    lock (_mapLoadLock)
        //    {
        //        if (_topLeftMap != null)
        //        {
        //            return FeatureEquation.PointToMapCoords(_topLeftMap.Tiles[0].TilePoint);
        //        }

        //        return new Vector2i(0, 0);
        //    }
        //}

        //public Vector2i PointToClusterPosition(TilePoint point) 
        //{
        //    Vector2i globalPoint = FeatureEquation.PointToMapCoords(point);

        //    Vector2i zeroPoint = GetTopLeftTilePosition();

        //    return globalPoint - zeroPoint;
        //}

        //public TileMap GetTopLeftMap()
        //{
        //    if(_topLeftMap != null)
        //    {
        //        return _topLeftMap;
        //    }

        //    return null;
        //}

        //public BaseTile GetCenterTile()
        //{
        //    for (int i = 0; i < TileMaps.Count; i++)
        //    {
        //        if (TileMaps[i].TileMapCoords.MapPosition == MapPosition.None)
        //        {
        //            return TileMaps[i].Tiles[TileMaps[i].Tiles.Count / 2 + TileMaps[i].Height / 2];
        //        }
        //    }

        //    throw new Exception("Tile not found");
        //}

        //public Vector2i GetCenterMapCoords()
        //{
        //    for (int i = 0; i < TileMaps.Count; i++)
        //    {
        //        if (TileMaps[i].TileMapCoords.MapPosition == MapPosition.None)
        //        {
        //            return new Vector2i(TileMaps[i].TileMapCoords.X, TileMaps[i].TileMapCoords.Y);
        //        }
        //    }

        //    throw new Exception("Map not found");
        //}

        public bool PointAtEdge(TilePoint point) 
        {
            TileMapPoint mapPoint = point.ParentTileMap.TileMapCoords;

            if ((int)(mapPoint.MapPosition & MapPosition.Top) > 0) 
            {
                if (point.Y < 5) 
                {
                    return true;
                }
            }

            if ((int)(mapPoint.MapPosition & MapPosition.Left) > 0)
            {
                if (point.X < 5)
                {
                    return true;
                }
            }

            if ((int)(mapPoint.MapPosition & MapPosition.Bot) > 0)
            {
                if (point.Y >= point.ParentTileMap.Height - 5)
                {
                    return true;
                }
            }

            if ((int)(mapPoint.MapPosition & MapPosition.Right) > 0)
            {
                if (point.X >= point.ParentTileMap.Width - 5)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool PointWillBeUnloaded(TileMapPoint newCenter, TileMapPoint locationToCheck)
        {
            return newCenter.MapPosition.HasFlag(MapPosition.Left) && locationToCheck.MapPosition.HasFlag(MapPosition.Right)
                      || newCenter.MapPosition.HasFlag(MapPosition.Right) && locationToCheck.MapPosition.HasFlag(MapPosition.Left)
                      || newCenter.MapPosition.HasFlag(MapPosition.Top) && locationToCheck.MapPosition.HasFlag(MapPosition.Bot)
                      || newCenter.MapPosition.HasFlag(MapPosition.Bot) && locationToCheck.MapPosition.HasFlag(MapPosition.Top);
        }

        public MapPosition GetMapPosition(int index) 
        {
            MapPosition pos = MapPosition.None;

            if (index % LOADED_MAP_DIMENSIONS == 0) 
            {
                pos |= MapPosition.Top;
            }

            if ((index + 1) % LOADED_MAP_DIMENSIONS == 0)
            {
                pos |= MapPosition.Bot;
            }

            if (index < LOADED_MAP_DIMENSIONS) 
            {
                pos |= MapPosition.Left;
            }

            if (index >= LOADED_MAP_DIMENSIONS * (LOADED_MAP_DIMENSIONS - 1))
            {
                pos |= MapPosition.Right;
            }

            return pos;
        }


        //public bool IsValidTile(int xIndex, int yIndex, TileMap map)
        //{
        //    int currX;
        //    int currY;
        //    for (int i = 0; i < TileMaps.Count; i++) 
        //    {
        //        currX = xIndex + TileMaps[i].Width * (map.TileMapCoords.X - TileMaps[i].TileMapCoords.X);
        //        currY = yIndex + TileMaps[i].Height * (map.TileMapCoords.Y - TileMaps[i].TileMapCoords.Y);

        //        if (currX >= 0 && currY >= 0 && currX < map.Width && currY < map.Height) 
        //        {
        //            return true;
        //        }
                    
        //    }

        //    return false;
        //}

        //public bool IsValidTile(FeaturePoint point) 
        //{
        //    Vector2i topLeftCoord = GetTopLeftTilePosition();
        //    TileMap topleftMap = GetTopLeftMap();

        //    Vector2i botRightCoord = topLeftCoord + new Vector2i(topleftMap.Width * LOADED_MAP_DIMENSIONS, topleftMap.Height * LOADED_MAP_DIMENSIONS);

        //    return point.X >= topLeftCoord.X && point.X <= botRightCoord.X && point.Y >= topLeftCoord.Y && point.Y <= botRightCoord.Y;
        //}

        

        public static Vector2i PointToMapCoords(TilePoint point)
        {
            Vector2i coords = new Vector2i
            {
                X = point.X + point.ParentTileMap.TileMapCoords.X * point.ParentTileMap.Width,
                Y = point.Y + point.ParentTileMap.TileMapCoords.Y * point.ParentTileMap.Height
            };

            return coords;
        }

        //public BaseTile GetTile(int xIndex, int yIndex, TileMap map)
        //{
        //    int currX;
        //    int currY;

        //    int mapX = (int)Math.Floor((float)(map.TileMapCoords.X * TILE_MAP_DIMENSIONS.X + xIndex) / TILE_MAP_DIMENSIONS.X);
        //    int mapY = (int)Math.Floor((float)(map.TileMapCoords.Y * TILE_MAP_DIMENSIONS.Y + xIndex) / TILE_MAP_DIMENSIONS.Y);

        //    TileMapPoint calculatedPoint = new TileMapPoint(mapX, mapY);

        //    if(TileMapManager.LoadedMaps.TryGetValue(calculatedPoint, out var foundMap))
        //    {
        //        if(xIndex < 0)
        //        {
        //            currX = TILE_MAP_DIMENSIONS.X - Math.Abs(xIndex % TILE_MAP_DIMENSIONS.X);
        //        }
        //        else
        //        {
        //            currX = Math.Abs(xIndex % TILE_MAP_DIMENSIONS.X);
        //        }

        //        if (yIndex < 0)
        //        {
        //            currY = TILE_MAP_DIMENSIONS.Y - Math.Abs(yIndex % TILE_MAP_DIMENSIONS.Y);
        //        }
        //        else
        //        {
        //            currY = Math.Abs(yIndex % TILE_MAP_DIMENSIONS.Y);
        //        }

        //        return foundMap.GetLocalTile(currX, currY);
        //    }

        //    //for (int i = 0; i < TileMaps.Count; i++)
        //    //{
        //    //    currX = xIndex + TileMaps[i].Width * (map.TileMapCoords.X - TileMaps[i].TileMapCoords.X);
        //    //    currY = yIndex + TileMaps[i].Height * (map.TileMapCoords.Y - TileMaps[i].TileMapCoords.Y);

        //    //    if (currX >= 0 && currY >= 0 && currX < map.Width && currY < map.Height)
        //    //        return TileMaps[i].GetLocalTile(currX, currY);
        //    //}

        //    return null;
        //}

        //public BaseTile GetTile(int xIndex, int yIndex)
        //{
        //    TileMap map = GetTopLeftMap();

        //    if (map == null)
        //        return null;

        //    try
        //    {
        //        int currX;
        //        int currY;
        //        for (int i = 0; i < TileMaps.Count; i++)
        //        {
        //            currX = xIndex + TileMaps.ElementAt(i).Width * (map.TileMapCoords.X - TileMaps.ElementAt(i).TileMapCoords.X);
        //            currY = yIndex + TileMaps.ElementAt(i).Height * (map.TileMapCoords.Y - TileMaps.ElementAt(i).TileMapCoords.Y);

        //            if (currX >= 0 && currY >= 0 && currX < map.Width && currY < map.Height)
        //            {
        //                return TileMaps.ElementAt(i).GetLocalTile(currX, currY);
        //            }

        //        }
        //    }
        //    catch
        //    {
        //        return null;
        //    }

        //    return null;
        //}

        //public BaseTile GetTile(FeaturePoint point)
        //{
        //    Vector2i topLeftCoord = GetTopLeftTilePosition();

        //    return GetTile(point.X - topLeftCoord.X, point.Y - topLeftCoord.Y);
        //}

        //public void ClearAllVisitedTiles()
        //{
        //    TileMaps.ForEach(m => m.Tiles.ForEach(tile => tile.TilePoint._visited = false)); //clear visited tiles
        //}

        public void ToggleHeightmap()
        {
            Settings.HeightmapEnabled = !Settings.HeightmapEnabled;

            TileMaps.ForEach(map =>
            {
                map.Tiles.ForEach(tile => tile.Update());
            });
        }


        public List<BaseTile> GetSelectionTilePool()
        {
            return _selectionTilePool;
        }

        public void SelectTiles(List<BaseTile> tiles)
        {
            //if (tiles.Count > MAX_SELECTION_TILES)
            //    throw new Exception("Attempted to select " + tiles.Count + " tiles while the maximum was " + MAX_SELECTION_TILES + " in tile map " + ObjectID);

            for (int i = 0; i < tiles.Count; i++)
            {
                SelectTile(tiles[i]);
            }
        }

        public void SelectTile(BaseTile tile)
        {
            //Console.WriteLine(_amountOfSelectionTiles + " tiles in use");

            if (_amountOfSelectionTiles == _selectionTilePool.Count)
                _amountOfSelectionTiles--;

            if (_amountOfSelectionTiles < 0)
            {
                Console.WriteLine("TileMap.SelectTile: Less than 0 selection tiles ");
                _amountOfSelectionTiles = 0;
            }


            Vector3 pos = new Vector3
            {
                X = tile.Position.X,
                Y = tile.Position.Y,
                Z = tile.Position.Z + 0.05f
            };

            _selectionTilePool[_amountOfSelectionTiles].TilePoint.ParentTileMap = tile.TileMap;
            _selectionTilePool[_amountOfSelectionTiles].TileMap = tile.TileMap;
            _selectionTilePool[_amountOfSelectionTiles].SetPosition(pos);
            _selectionTilePool[_amountOfSelectionTiles].SetRender(true);

            if(tile.AttachedTile != null) 
            {
                DeselectTile(tile.AttachedTile);
            }

            _selectionTilePool[_amountOfSelectionTiles].AttachedTile = tile;
            tile.AttachedTile = _selectionTilePool[_amountOfSelectionTiles];


            SelectionTiles.Add(_selectionTilePool[_amountOfSelectionTiles]);

            _amountOfSelectionTiles++;
        }

        public void DeselectTile(BaseTile selectionTile)
        {
            SelectionTiles.Remove(selectionTile);

            if (selectionTile.AttachedTile != null)
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

        public List<BaseTile> GetHoveredTile()
        {
            return _hoveredTileList;
        }

        public void HoverTile(BaseTile tile)
        {
            Vector3 pos = new Vector3
            {
                X = tile.Position.X,
                Y = tile.Position.Y,
                Z = tile.Position.Z + 0.06f
            };

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
                HoveredTile.AttachedTile.OnHoverEnd();
                HoveredTile.AttachedTile = null;
            }

            HoveredTile.SetRender(false);
        }

        //public TileMapPoint GlobalPositionToMapPoint(Vector3 position)
        //{
        //    if (TileMaps.Count == 0 || Scene.ContextManager.GetFlag(GeneralContextFlags.TileMapLoadInProgress))
        //        return null;

        //    Vector3 camPos = WindowConstants.ConvertLocalToScreenSpaceCoordinates(position.Xy);

        //    var map = TileMaps[0];

        //    Vector3 dim = map.GetTileMapDimensions();

        //    Vector3 mapPos = map.Position;

        //    Vector3 offsetPos = camPos - mapPos;

        //    TileMapPoint point = new TileMapPoint((int)Math.Floor(offsetPos.X / dim.X) + map.TileMapCoords.X, (int)Math.Floor(offsetPos.Y / dim.Y) + map.TileMapCoords.Y);

        //    return point;
        //} 
    }
}
