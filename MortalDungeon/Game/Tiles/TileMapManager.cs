using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles.TileMaps;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MortalDungeon.Game.Tiles
{
    public static class TileMapManager
    {
        //Keeps a list of loaded tilemaps. These tilemaps are loaded in a radius around a center point.

        //When passing a new center point the tilemaps should be evaluated for maps that don't need to change, maps that need to be removed
        //and maps that need to be added

        //A "visible center" or "camera position" field should be created to track which tilemaps are considered to be visible. When these are changed
        //the tile instanced render data will be updated.

        //When entities are unloaded, save their tilemap position and feature point in a dictionary<TileMapPoint, (entity and position)> then this can be used
        //to readd them to the map when it gets reloaded. (doing this we wouldn't really need to guarantee that the maps around the unit remain loaded necessarily)

        public static Dictionary<TileMapPoint, TileMap> LoadedMaps = new Dictionary<TileMapPoint, TileMap>();

        public static TileMapPoint LoadedCenter = new TileMapPoint(0, 0);

        public static int LOAD_DIAMETER = 15;

        public static CombatScene Scene;

        public static HashSet<TileMap> VisibleMaps = new HashSet<TileMap>();

        public static List<TileMap> ActiveMaps = new List<TileMap>();

        public static Dictionary<TileMapPoint, InstancedRenderData> TilePillarsRenderData = new Dictionary<TileMapPoint, InstancedRenderData>();

        public static readonly Vector2i TILE_MAP_DIMENSIONS = new Vector2i(20, 20);

        public static void SetCenter(TileMapPoint center)
        {
            LoadedCenter = center;
        }

        public static object _loadLock = new object();
        public static void LoadMapsAroundCenter(int loadDiameter = -1, int layer = 0)
        {
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


                Vector2i topleft = new Vector2i(LoadedCenter.X - incrementVal, LoadedCenter.Y - incrementVal);

                List<TileMapPoint> pointsToLoad = new List<TileMapPoint>();

                for (int i = 0; i < loadDiameter; i++)
                {
                    for (int j = 0; j < loadDiameter; j++)
                    {
                        TileMapPoint newPoint = new TileMapPoint(topleft.X + i, topleft.Y + j);

                        if (!LoadedMaps.ContainsKey(newPoint))
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

                UnloadMaps(pointsToRemove);

                foreach (var point in pointsToLoad)
                {
                    TestTileMap newMap = new TestTileMap(default, point, Scene._tileMapController) { Width = TILE_MAP_DIMENSIONS.X, Height = TILE_MAP_DIMENSIONS.Y };
                    newMap.PopulateTileMap();

                    newMap.TileMapCoords = new TileMapPoint(point.X, point.Y);
                    LoadedMaps.TryAdd(newMap.TileMapCoords, newMap);
                    ActiveMaps.Add(newMap);

                    newMap.OnAddedToController();
                }

                TileMapHelpers._topLeftMap = LoadedMaps[new TileMapPoint(LoadedCenter.X - incrementVal, LoadedCenter.Y - incrementVal)];

                PositionTileMaps();

                ApplyLoadedFeaturesToMaps(LoadedMaps.Values.ToList(), pointsToLoad);

                Scene.ContextManager.SetFlag(GeneralContextFlags.TileMapManagerLoading, false);
                Scene.ContextManager.SetFlag(GeneralContextFlags.DisallowCameraMovement, false);

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
                });

                GC.Collect();
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
            

            foreach (var feature in FeatureManager.LoadedFeatures)
            {
                for (int i = 0; i < maps.Count; i++)
                {
                    bool freshGen = true;

                    if (addedMaps != null)
                    {
                        freshGen = addedPoints.Contains(maps[i].TileMapCoords);
                    }

                    if (feature.Value.AffectsMap(maps[i]))
                    {
                        feature.Value.ApplyToMap(maps[i], freshGen);
                    }
                }

                feature.Value.OnAppliedToMaps();
            };
        }

        public static void UnloadMaps(IEnumerable<TileMapPoint> mapPoints)
        {
            foreach (TileMapPoint point in mapPoints)
            {
                if(LoadedMaps.TryGetValue(point, out var map))
                {
                    List<Entity> entitiesToUnload = new List<Entity>();

                    lock (EntityManager.Entities)
                    {
                        foreach (var entity in EntityManager.LoadedEntities)
                        {
                            if (entity.Handle.OnTileMap(map))
                            {
                                entitiesToUnload.Add(entity);
                            }
                        }
                    }
                   
                    Scene.QueueToRenderCycle(() =>
                    {
                        foreach (Entity entity in entitiesToUnload)
                        {
                            EntityManager.UnloadEntity(entity);
                        }

                        //map.CleanUp();
                    });

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

                Scene.CreateStructureInstancedRenderData();
                //Scene.UpdateVisionMap();
            }
        }

        public static void CreateTilePillarsForMap(TileMap map)
        {
            List<GameObject> tilePillars = new List<GameObject>();

            List<InstancedRenderData> data;

            List<BaseTile> neighborList = new List<BaseTile>();

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
