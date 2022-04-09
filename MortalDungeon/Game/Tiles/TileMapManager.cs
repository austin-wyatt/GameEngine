using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Combat;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.Ledger.Units;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles.TileMaps;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Tiles
{
    public static class TileMapManager
    {
        public static Dictionary<TileMapPoint, TileMap> LoadedMaps = new Dictionary<TileMapPoint, TileMap>();

        public static TileMapPoint LoadedCenter = new TileMapPoint(0, 0);

        public static int LOAD_DIAMETER = 15;

        public static CombatScene Scene;

        public static HashSet<TileMap> VisibleMaps = new HashSet<TileMap>();

        public static List<TileMap> ActiveMaps = new List<TileMap>();

        public static Dictionary<TileMapPoint, InstancedRenderData> TilePillarsRenderData = new Dictionary<TileMapPoint, InstancedRenderData>();

        public static readonly Vector2i TILE_MAP_DIMENSIONS = new Vector2i(20, 20);

        public static NavMesh NavMesh = new NavMesh();

        public static void SetCenter(TileMapPoint center)
        {
            LoadedCenter = center;
        }

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

                foreach (var point in pointsToLoad)
                {
                    TestTileMap newMap = new TestTileMap(default, point, Scene._tileMapController) { Width = TILE_MAP_DIMENSIONS.X, Height = TILE_MAP_DIMENSIONS.Y };
                    newMap.PopulateTileMap();

                    newMap.TileMapCoords = new TileMapPoint(point.X, point.Y);
                    LoadedMaps.TryAdd(newMap.TileMapCoords, newMap);
                    ActiveMaps.Add(newMap);

                    newMap.OnAddedToController();
                }

                Console.WriteLine("Flag 3: " + stopwatch.ElapsedMilliseconds + "ms");

                TileMapHelpers._topLeftMap = LoadedMaps[new TileMapPoint(LoadedCenter.X - incrementVal, LoadedCenter.Y - incrementVal)];

                PositionTileMaps();

                Console.WriteLine("Flag 4: " + stopwatch.ElapsedMilliseconds + "ms");

                ApplyLoadedFeaturesToMaps(LoadedMaps.Values.ToList(), pointsToLoad);

                Console.WriteLine("Flag 5: " + stopwatch.ElapsedMilliseconds + "ms");

                AddLedgeredUnitsToMaps(pointsToLoad);

                Console.WriteLine("Flag 5.5: " + stopwatch.ElapsedMilliseconds + "ms");

                NavMesh.CalculateNavTiles();

                Console.WriteLine("Flag 5.75: " + stopwatch.ElapsedMilliseconds + "ms");

                Scene.ContextManager.SetFlag(GeneralContextFlags.TileMapManagerLoading, false);
                Scene.ContextManager.SetFlag(GeneralContextFlags.DisallowCameraMovement, false);

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
                });

                Console.WriteLine("Flag 6: " + stopwatch.ElapsedMilliseconds + "ms");
            }
        }

        public static void PositionTileMaps()
        {
            if (LoadedMaps.Values.Count == 0)
                return;

            Vector3 tileMapDimensions = LoadedMaps.Values.First().GetTileMapDimensions();

            int minX = int.MaxValue;
            int maxX = int.MinValue;

            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach(var map in LoadedMaps.Values)
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
            }


            foreach(var map in LoadedMaps.Values)
            {
                Vector3 pos = new Vector3(tileMapDimensions.X * (map.TileMapCoords.X), tileMapDimensions.Y * (map.TileMapCoords.Y), 0);

                map.SetPosition(pos);
            }
        }


        public static void ApplyLoadedFeaturesToMaps(List<TileMap> maps, List<TileMapPoint> addedMaps = null)
        {
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

            foreach (var feature in featureList)
            {
                #region apply map brushes
                if (feature.MapBrushes.Count > maps.Count)
                {
                    //Case for when a single feature has a ton of map brushes. 
                    //It should be faster to check each map for a matching brush

                    for (int i = 0; i < maps.Count; i++)
                    {
                        bool freshGen = true;

                        if (addedMaps != null)
                        {
                            freshGen = addedPoints.Contains(maps[i].TileMapCoords);
                        }

                        if (feature.MapBrushes.TryGetValue(maps[i].TileMapCoords, out var mapBrush) && freshGen)
                        {
                            mapBrush.ApplyToMap(maps[i]);
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

                //Console.WriteLine("Feature applied to map in: " + stopwatch.ElapsedMilliseconds + "ms");

                feature.OnAppliedToMaps();
            };
        }

        public static void AddLedgeredUnitsToMaps(List<TileMapPoint> addedMaps)
        {
            foreach(var point in addedMaps)
            {
                if(LoadedMaps.TryGetValue(point, out var map))
                {
                    foreach(var unit in UnitLedger.GetLedgeredUnitsOnTileMap(point).ToHashSet())
                    {
                        Entity entity = UnitLedger.CreateEntityFromLedgeredUnit(unit);

                        EntityManager.LoadEntity(entity, unit.UnitInfo.Position);

                        UnitLedger.RemoveUnitFromLedger(unit);
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
            List<TileMap> maps = new List<TileMap>();

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
        public static void SetVisibleMaps(List<TileMap> maps)
        {
            lock (_visibleMapLock)
            {
                var currVisibleMaps = new HashSet<TileMap>(VisibleMaps);

                VisibleMaps.Clear();

                for (int i = 0; i < maps.Count; i++)
                {
                    VisibleMaps.Add(maps[i]);

                    if (!currVisibleMaps.Contains(maps[i]))
                    {
                        VisibleMaps.Add(maps[i]);
                        maps[i].Visible = true;

                        CreateTilePillarsForMap(maps[i]);
                    }
                    else
                    {
                        //currVisibleMaps will contain all of the maps that need to be removed at the end of this loop
                        currVisibleMaps.Remove(maps[i]);
                    }
                }

                foreach (var map in currVisibleMaps)
                {
                    if (TilePillarsRenderData.TryGetValue(map.TileMapCoords, out var renderData))
                    {
                        renderData.CleanUp();
                        TilePillarsRenderData.Remove(map.TileMapCoords);
                    }

                    map.Visible = false;
                }

                //Window.QueueToRenderCycle(() =>
                //{
                //    Scene.CreateStructureInstancedRenderData();
                //});
                Scene.RenderDispatcher.DispatchAction(Scene._structureDispatchObject, Scene.CreateStructureInstancedRenderData);
                //Scene.UpdateVisionMap();
            }
        }


        private static HashSet<TileMap> _mapsToUpdatePillars = new HashSet<TileMap>();
        private static object _pillarLock = new object();
        public static void DispatchTilePillarUpdate(TileMap map)
        {
            lock (_pillarLock)
            {
                if (TilePillarsRenderData.TryGetValue(map.TileMapCoords, out var renderData))
                {
                    renderData.CleanUp();
                    TilePillarsRenderData.Remove(map.TileMapCoords);
                }

                _mapsToUpdatePillars.Add(map);
                Window.QueueToRenderCycle(BatchPillarUpdate);
            }
        }

        private static void BatchPillarUpdate()
        {
            lock (_pillarLock)
            {
                foreach(var map in _mapsToUpdatePillars)
                {
                    CreateTilePillarsForMap(map);
                }

                _mapsToUpdatePillars.Clear();
            }
        }

        public static void CreateTilePillarsForMap(TileMap map)
        {
            List<GameObject> tilePillars = new List<GameObject>();

            List<InstancedRenderData> data;

            List<Tile> neighborList = new List<Tile>();

            for (int j = 0; j < map.Tiles.Count; j++)
            {
                var tile = map.Tiles[j];

                neighborList.Clear();
                map.GetNeighboringTiles(tile, neighborList, shuffle: false, setVisited: false);

                bool createPillar = false;

                for (int i = 0; i < neighborList.Count; i++)
                {
                    if (tile.Properties.Height > neighborList[i].Properties.Height)
                    {
                        createPillar = true;
                        break;
                    }
                }

                if (createPillar)
                {
                    GameObject pillar = new GameObject();
                    pillar.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(0, Textures.TentTexture), _3DObjects.TilePillar, default));

                    pillar.SetPosition(map.Tiles[j].Position + new Vector3(0, 217, -1.0f));

                    pillar.BaseObject.BaseFrame.SetScale(1.64f, 1.64f, 1);

                    Renderer.LoadTextureFromGameObj(pillar);

                    tilePillars.Add(pillar);
                }
            }

            data = InstancedRenderData.GenerateInstancedRenderData(tilePillars);
            foreach (var item in data)
            {
                TilePillarsRenderData.TryAdd(map.TileMapCoords, item);
            }

            tilePillars.Clear();
        }
    }
}
