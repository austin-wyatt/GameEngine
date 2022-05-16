using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Rendering;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Combat;
using Empyrean.Game.Entities;
using Empyrean.Game.Ledger.Units;
using Empyrean.Game.Map;
using Empyrean.Game.Objects;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles.Meshes;
using Empyrean.Game.Tiles.TileMaps;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Empyrean.Game.Tiles
{
    public static class TileMapManager
    {
        //public static Texture TEST_BLEND_MAP = Texture.LoadFromFile("Resources/Textures/TestBlendMap.png");
        //public static Texture TEST_GRASS = Texture.LoadFromFile("Resources/Textures/Grass.png");
        //public static Texture TEST_DIRT = Texture.LoadFromFile("Resources/Textures/Dirt.png");
        //public static Texture TEST_STONE = Texture.LoadFromFile("Resources/Textures/Stone_1.png");

        public static Dictionary<TileMapPoint, TileMap> LoadedMaps = new Dictionary<TileMapPoint, TileMap>();

        public static TileMapPoint LoadedCenter = new TileMapPoint(0, 0);

        public static int LOAD_DIAMETER = 15;

        public static CombatScene Scene;

        public static HashSet<TileMap> VisibleMaps = new HashSet<TileMap>();
        public static List<TileMap> VisibleMapsList = new List<TileMap>();

        public static List<TileMap> ActiveMaps = new List<TileMap>();

        public static readonly Vector2i TILE_MAP_DIMENSIONS = new Vector2i(20, 20);

        public static NavMesh NavMesh = new NavMesh();

        public static void SetCenter(TileMapPoint center)
        {
            LoadedCenter = center;
        }


        private static AsyncSignal _featureWaitHandle = new AsyncSignal();

        /// <summary>
        /// A list of tiles that had their type changed during the loading process and need to have
        /// their texture data updated in their chunk's mesh.
        /// </summary>
        public static List<Tile> TilesRequiringTextureUpdates = new List<Tile>();

        public static object _loadLock = new object();
        public static void LoadMapsAroundCenter(int loadDiameter = -1, int layer = 0, bool forceFresh = false)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            lock (_loadLock)
            {
                HashSet<TileMapPoint> pointsToRemove = new HashSet<TileMapPoint>();

                Scene.ContextManager.SetFlag(GeneralContextFlags.TileMapManagerLoading, true);
                Scene.ContextManager.SetFlag(GeneralContextFlags.DisallowCameraMovement, true);
                

                ActiveMaps.Clear();

                foreach (var kvp in LoadedMaps)
                {
                    pointsToRemove.Add(kvp.Key);
                }

                if (loadDiameter < 1)
                {
                    loadDiameter = LOAD_DIAMETER;
                }

                int incrementVal;
                if (loadDiameter % 2 == 0)
                {
                    incrementVal = loadDiameter / 2;
                }
                else
                {
                    incrementVal = (loadDiameter - 1) / 2;
                }

                FeatureManager.EvaluateLoadedFeatures(new FeaturePoint(LoadedCenter.X * TILE_MAP_DIMENSIONS.X, LoadedCenter.Y * TILE_MAP_DIMENSIONS.Y),
                    layer, (incrementVal + 2) * TILE_MAP_DIMENSIONS.X);

                Console.WriteLine("Flag 1: " + stopwatch.ElapsedMilliseconds + "ms");

                Vector2i topleft = new Vector2i(LoadedCenter.X - incrementVal, LoadedCenter.Y - incrementVal);

                List<TileMapPoint> pointsToLoad = new List<TileMapPoint>();

                for (int i = 0; i < loadDiameter; i++)
                {
                    for (int j = 0; j < loadDiameter; j++)
                    {
                        TileMapPoint newPoint = new TileMapPoint(topleft.X + i, topleft.Y + j);

                        if (!LoadedMaps.ContainsKey(newPoint) || forceFresh)
                        {
                            pointsToLoad.Add(newPoint);
                        }
                        else
                        {
                            pointsToRemove.Remove(newPoint);
                            ActiveMaps.Add(LoadedMaps[newPoint]);
                        }
                    }
                }

                Console.WriteLine("Flag 2: " + stopwatch.ElapsedMilliseconds + "ms");

                UnloadMaps(pointsToRemove);

                Console.WriteLine("Flag 2.5: " + stopwatch.ElapsedMilliseconds + "ms");

                Stack<Task> pointLoadTasks = new Stack<Task>();

                for (int i = 0; i < pointsToLoad.Count; i++)
                {
                    int capturedIndex = i;
                    pointLoadTasks.Push(Task.Run(() =>
                    {
                        TestTileMap newMap = new TestTileMap(default, pointsToLoad[capturedIndex], Scene._tileMapController) 
                        { 
                            Width = TILE_MAP_DIMENSIONS.X, Height = TILE_MAP_DIMENSIONS.Y 
                        };
                        newMap.PopulateTileMap();

                        newMap.TileMapCoords = new TileMapPoint(pointsToLoad[capturedIndex].X, pointsToLoad[capturedIndex].Y);
                        lock (LoadedMaps)
                        {
                            LoadedMaps.TryAdd(newMap.TileMapCoords, newMap);
                            ActiveMaps.Add(newMap);
                        }

                        newMap.OnAddedToController();
                    }));
                }

                while(pointLoadTasks.Count > 0)
                    pointLoadTasks.Pop().Wait();

                Console.WriteLine("Flag 3: " + stopwatch.ElapsedMilliseconds + "ms");

                TileMapHelpers._topLeftMap = LoadedMaps[new TileMapPoint(LoadedCenter.X - incrementVal, LoadedCenter.Y - incrementVal)];
                TileMapHelpers._bottomRightMap = LoadedMaps[new TileMapPoint(LoadedCenter.X + incrementVal, LoadedCenter.Y + incrementVal)];

                PositionTileMaps();

                Console.WriteLine("Flag 4: " + stopwatch.ElapsedMilliseconds + "ms");


                _featureWaitHandle.Reset();
                ApplyLoadedFeaturesToMaps(LoadedMaps.Values.ToList(), pointsToLoad);

                
                Console.WriteLine("Flag 5: " + stopwatch.ElapsedMilliseconds + "ms");

                AddLedgeredUnitsToMaps(pointsToLoad);

                Console.WriteLine("Flag 5.5: " + stopwatch.ElapsedMilliseconds + "ms");


                Task navTask = Task.Run(NavMesh.CalculateNavTiles);

                Console.WriteLine("Flag 5.75: " + stopwatch.ElapsedMilliseconds + "ms");


                MeshTileBlender.FillTileHeightMap();
                MeshTileBlender.MajorBlendPass();
                Console.WriteLine("Blend completed: " + stopwatch.ElapsedMilliseconds + "ms");

                if (_blendMapTotalCount > 0)
                    _featureWaitHandle.Wait();

                navTask.Wait();

                GC.Collect();

                Scene.QueueToRenderCycle(() =>
                {
                    Scene.OnStructureMoved();

                    foreach (var unit in Scene._units)
                    {
                        if (unit.Info.TileMapPosition == null)
                            continue;

                        unit.VisionGenerator.SetPosition(unit.Info.TileMapPosition);
                    }

                    for(int i = 0; i < TilesRequiringTextureUpdates.Count; i++)
                    {
                        var tile = TilesRequiringTextureUpdates[i];
                        tile.MeshTileHandle.UpdateTextureInfo();
                        tile.Update(TileUpdateType.Textures);
                    }

                    //MeshTileBlender.MajorBlendPass();

                    TilesRequiringTextureUpdates.Clear();

                    Scene.ContextManager.SetFlag(GeneralContextFlags.TileMapManagerLoading, false);
                    Scene.ContextManager.SetFlag(GeneralContextFlags.DisallowCameraMovement, false);
                });

                Console.WriteLine("Flag 6: " + stopwatch.ElapsedMilliseconds + "ms");
            }
        }

        public static void PositionTileMaps()
        {
            if (LoadedMaps.Values.Count == 0)
                return;

            Vector3 tileMapDimensions = LoadedMaps.Values.First().GetTileMapDimensions();

            //int minX = int.MaxValue;
            //int maxX = int.MinValue;

            //int minY = int.MaxValue;
            //int maxY = int.MinValue;

            //foreach(var map in LoadedMaps.Values)
            //{
            //    if (map.TileMapCoords.X < minX)
            //    {
            //        minX = map.TileMapCoords.X;
            //    }
            //    if (map.TileMapCoords.X > maxX)
            //    {
            //        maxX = map.TileMapCoords.X;
            //    }
            //    if (map.TileMapCoords.Y < minY)
            //    {
            //        minY = map.TileMapCoords.Y;
            //    }
            //    if (map.TileMapCoords.Y > maxY)
            //    {
            //        maxY = map.TileMapCoords.Y;
            //    }
            //}


            foreach(var map in LoadedMaps.Values)
            {
                Vector3 pos = new Vector3(tileMapDimensions.X * (map.TileMapCoords.X), tileMapDimensions.Y * (map.TileMapCoords.Y), 0);

                map.SetPosition(pos);
            }
        }

        private static int _blendMapTotalCount;
        private static int _blendMapCurrentCount;
        public static void ApplyLoadedFeaturesToMaps(List<TileMap> maps, List<TileMapPoint> addedMaps = null)
        {
            _blendMapTotalCount = 0;
            _blendMapCurrentCount = 0;

            var featureList = FeatureManager.LoadedFeatures.Values.ToList();

            featureList.Sort((a, b) => b.LoadPriority.CompareTo(a.LoadPriority));


            HashSet<TileMapPoint> addedPoints;
            if (addedMaps != null)
            {
                addedPoints = addedMaps.ToHashSet();
            }
            else
            {
                addedPoints = new HashSet<TileMapPoint>();
            }

            Console.WriteLine("Starting map update");

            List<BlendControl> blendControls = new List<BlendControl>();

            Stack<Task> mapBrushTaskList = new Stack<Task>();
            const int MAP_BRUSHES_PER_TASK = 20;

            foreach (var feature in featureList)
            {
                List<Action> brushActions = new List<Action>();

                #region apply map brushes
                if (feature.MapBrushes.Count > maps.Count)
                {
                    //Case for when a single feature has a ton of map brushes. 
                    //It should be faster to check each map for a matching brush

                    for (int i = 0; i < maps.Count; i++)
                    {
                        int capturedIndex = i;

                        bool freshGen = true;

                        if (addedMaps != null)
                        {
                            freshGen = addedPoints.Contains(maps[i].TileMapCoords);
                        }

                        if (feature.MapBrushes.TryGetValue(maps[i].TileMapCoords, out var mapBrush) && freshGen)
                        {
                            brushActions.Add(() =>
                            {
                                mapBrush.ApplyToMap(maps[capturedIndex]);
                            });
                        }

                        if(brushActions.Count >= MAP_BRUSHES_PER_TASK)
                        {
                            List<Action> brushActionRef = brushActions;
                            mapBrushTaskList.Push(Task.Run(() =>
                            {
                                foreach(var ac in brushActionRef)
                                {
                                    ac.Invoke();
                                }
                            }));

                            brushActions = new List<Action>();
                        }
                    }

                    if(brushActions.Count > 0)
                    {
                        foreach (var ac in brushActions)
                        {
                            ac.Invoke();
                        }
                    }
                }
                else
                {
                    //Case for when a feature has few map brushes
                    //Here we can directly check the loaded maps

                    foreach (var brush in feature.MapBrushes)
                    {
                        if (LoadedMaps.TryGetValue(brush.Key, out var map))
                        {
                            bool freshGen = true;

                            if (addedMaps != null)
                            {
                                freshGen = addedPoints.Contains(map.TileMapCoords);
                            }

                            brush.Value.ApplyToMap(map);
                        }
                    }
                }
                #endregion

                #region apply bounding points
                for (int i = 0; i < feature.BoundingPoints.Count; i++)
                {
                    if (feature.BoundingPoints[i].ApplyToStaleMaps)
                    {

                    }
                    else
                    {
                        for (int j = 0; j < addedMaps.Count; j++)
                        {
                            if (LoadedMaps.TryGetValue(addedMaps[j], out var map) && feature.BoundingPoints[i].AffectedMaps.Contains(addedMaps[j]))
                            {
                                feature.BoundingPoints[i].ApplyToMap(map);
                            }
                        }
                    }
                }
                #endregion

                #region apply affected points
                foreach (var point in feature.AffectedPoints)
                {
                    var tile = TileMapHelpers.GetTile(point.Key);

                    if(tile != null)
                    {
                        bool freshGen = true;

                        if (addedMaps != null)
                        {
                            freshGen = addedPoints.Contains(tile.TileMap.TileMapCoords);
                        }

                        feature.ApplyToAffectedPoint(tile, freshGen);
                    }
                }
                #endregion

                #region apply units
                foreach (var point in feature.FeatureUnits)
                {
                    var tile = TileMapHelpers.GetTile(point.Key);

                    if (tile != null)
                    {
                        bool freshGen = true;

                        if (addedMaps != null)
                        {
                            freshGen = addedPoints.Contains(tile.TileMap.TileMapCoords);
                        }

                        feature.ApplyUnitToPoint(tile, freshGen);
                    }
                }
                #endregion

                #region apply blend controls
                for(int i = 0; i < feature.BlendControls.Count; i++)
                {
                    _blendMapTotalCount++;
                    blendControls.Add(feature.BlendControls[i]);

                    //Task.Run(() => 
                    //{
                    //    feature.BlendControls[capturedIndex].ApplyControl();
                    //    _blendMapCurrentCount++;

                    //    if (_blendMapCurrentCount == _blendMapTotalCount)
                    //    {
                    //        _featureWaitHandle.Set();
                    //    }
                    //});
                }
                #endregion

                //Console.WriteLine("Feature applied to map in: " + stopwatch.ElapsedMilliseconds + "ms");

                feature.OnAppliedToMaps();
            };

            blendControls.Sort((a, b) => a.LoadOrder.CompareTo(b.LoadOrder));

            Task.Run(() =>
            {
                for (int i = 0; i < blendControls.Count; i++)
                {
                    int capturedIndex = i;

                
                    blendControls[capturedIndex].ApplyControl();
                    _blendMapCurrentCount++;

                    if (_blendMapCurrentCount == _blendMapTotalCount)
                    {
                        _featureWaitHandle.Set();
                    }
                }
            });
        }

        public static void AddLedgeredUnitsToMaps(List<TileMapPoint> addedMaps)
        {
            foreach(var point in addedMaps)
            {
                if(LoadedMaps.TryGetValue(point, out var map))
                {
                    foreach(var unit in UnitPositionLedger.GetLedgeredUnitsOnTileMap(point).ToHashSet())
                    {
                        Entity entity = UnitPositionLedger.CreateEntityFromLedgeredUnit(unit);

                        EntityManager.LoadEntity(entity, unit.UnitInfo.Position);

                        UnitPositionLedger.RemoveUnitFromLedger(unit);
                    }
                }
            }
        }

        public static void UnloadMaps(IEnumerable<TileMapPoint> mapPoints)
        {
            foreach (TileMapPoint point in mapPoints)
            {
                if(LoadedMaps.TryGetValue(point, out var map))
                {
                    map.CleanUp();
                    LoadedMaps.Remove(point);
                }
            }
        }

        public static List<TileMap> GetTileMapsInDiameter(TileMapPoint center, int diameter)
        {
            List<TileMap> maps = TileMap.TileMapListPool.GetObject();

            int incrementVal;
            if(diameter % 2 == 0)
            {
                incrementVal = diameter / 2;
            }
            else
            {
                incrementVal = (diameter - 1) / 2;
            }

            center = new TileMapPoint(center.X - incrementVal, center.Y - incrementVal);

            for(int i = 0; i < diameter; i++)
            {
                for(int j = 0; j < diameter; j++)
                {
                    if (LoadedMaps.TryGetValue(new TileMapPoint(center.X + i, center.Y + j), out var map))
                    {
                        maps.Add(map);
                    }
                }
            }

            return maps;
        }

        public static object _visibleMapLock = new object();
        private static ObjectPool<HashSet<TileMap>> _tileMapSetPool = new ObjectPool<HashSet<TileMap>>();
        public static void SetVisibleMaps(List<TileMap> maps)
        {
            lock (_visibleMapLock)
            {
                var currVisibleMaps = _tileMapSetPool.GetObject();

                foreach(var item in VisibleMaps)
                {
                    currVisibleMaps.Add(item);
                }

                VisibleMaps.Clear();
                VisibleMapsList.Clear();

                for (int i = 0; i < maps.Count; i++)
                {
                    VisibleMaps.Add(maps[i]);
                    VisibleMapsList.Add(maps[i]);

                    if (!currVisibleMaps.Contains(maps[i]))
                    {
                        VisibleMaps.Add(maps[i]);
                        maps[i].Visible = true;

                        maps[i].UpdateChunks(TileUpdateType.Initialize, overrideTileMapLoadBlock: true);

                        for (int j = 0; j < maps[i].TileChunks.Count; j++)
                        {
                            maps[i].TileChunks[j].Cull = false;
                        }
                    }
                    else
                    {
                        //currVisibleMaps will contain all of the maps that need to be removed at the end of this loop
                        currVisibleMaps.Remove(maps[i]);
                    }
                }

                foreach (var map in currVisibleMaps)
                {
                    map.ClearMeshRenderData();
                    map.Visible = false;
                    
                    for(int i = 0; i < map.TileChunks.Count; i++)
                    {
                        map.TileChunks[i].Cull = true;
                        map.TileChunks[i].OnCull();
                    }
                }

                currVisibleMaps.Clear();
                _tileMapSetPool.FreeObject(ref currVisibleMaps);

                //Window.QueueToRenderCycle(() =>
                //{
                //    Scene.CreateStructureInstancedRenderData();
                //});
                Scene.RenderDispatcher.DispatchAction(Scene._structureDispatchObject, Scene.CreateStructureInstancedRenderData);
                //Scene.UpdateVisionMap();
            }
        }
    }
}
