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
        public List<TileMap> TileMaps = new List<TileMap>();
        public static StaticBitmap TileBitmap;

        public int BaseElevation = 0; //base elevation for determining heightmap colors

        public CombatScene Scene;

        public List<FeatureEquation> LoadedFeatures = new List<FeatureEquation>();

        public TileMapController(CombatScene scene = null) 
        {
            Scene = scene;
        }

        public void AddTileMap(TileMapPoint point, TileMap map) 
        {
            map.TileMapCoords = new TileMapPoint(point.X, point.Y);
            TileMaps.Add(map);

            PositionTileMaps();
            map.OnAddedToController();
        }

        public void RemoveTileMap(TileMap map) 
        {
            map.SetRender(false);
            map.CleanUp();
            TileMaps.Remove(map);
        }

        public void PositionTileMaps() 
        {
            if (TileMaps.Count == 0)
                return;

            Vector3 tileMapDimensions = TileMaps[0].GetTileMapDimensions();

            TileMaps.ForEach(map =>
            {
                Vector3 pos = new Vector3(tileMapDimensions.X * map.TileMapCoords.X, tileMapDimensions.Y * map.TileMapCoords.Y, 0);
                map.SetPosition(pos);
            });
        }

        public void RecreateTileChunks() 
        {
            TileMaps.ForEach(map =>
            {
                map.InitializeTileChunks();
            });
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
        }

        //TODO, optimize so that it only loops through every tile once
        public void ApplyLoadedFeaturesToMaps()
        {
            LoadedFeatures.ForEach(feature =>
            {
                for (int i = 0; i < TileMaps.Count; i++)
                {
                    if (feature.AffectsMap(TileMaps[i]))
                    {
                        feature.ApplyToMap(TileMaps[i]);
                    }
                }
            });
        }

        const int LOADED_MAP_DIMENSIONS = 3;
        public void LoadSurroundingTileMaps(TileMapPoint point) 
        {
            TileMapPoint currPoint = new TileMapPoint(point.X - 1, point.Y - 1);

            List<TileMapPoint> loadedPoints = new List<TileMapPoint>();
            List<TileMapPoint> mapsToAdd = new List<TileMapPoint>();

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
                currPoint.Y = point.Y - 1;
            }

            Scene.PostTickAction = () =>
            {
                for (int i = TileMaps.Count - 1; i >= 0; i--)
                {
                    if (loadedPoints.Find(p => p == TileMaps[i].TileMapCoords) == null)
                    {
                        RemoveTileMap(TileMaps[i]);
                    }
                }

                mapsToAdd.ForEach(p =>
                {
                    TestTileMap newMap = new TestTileMap(default, p, this) { Width = 50, Height = 50 };
                    newMap.PopulateTileMap();
                    AddTileMap(p, newMap);
                });

                ApplyLoadedFeaturesToMaps();

                TileMaps.ForEach(m =>
                {
                    m.TileMapCoords.MapPosition = loadedPoints.Find(p => p == m.TileMapCoords).MapPosition;
                });

                Scene.PostTickAction = null;

                Scene.FillInTeamFog();
            };
        }

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

        internal MapPosition GetMapPosition(int index) 
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


        internal bool IsValidTile(int xIndex, int yIndex, TileMap map)
        {
            int currX;
            int currY;
            for (int i = 0; i < TileMaps.Count; i++) 
            {
                currX = xIndex + TileMaps[i].Width * (map.TileMapCoords.X - TileMaps[i].TileMapCoords.X);
                currY = yIndex + TileMaps[i].Height * (map.TileMapCoords.Y - TileMaps[i].TileMapCoords.Y);

                if (currX >= 0 && currY >= 0 && currX < map.Width && currY < map.Height) 
                {
                    return true;
                }
                    
            }

            return false;
        }

        internal BaseTile GetTile(int xIndex, int yIndex, TileMap map)
        {
            int currX;
            int currY;
            for (int i = 0; i < TileMaps.Count; i++)
            {
                currX = xIndex + TileMaps[i].Width * (map.TileMapCoords.X - TileMaps[i].TileMapCoords.X);
                currY = yIndex + TileMaps[i].Height * (map.TileMapCoords.Y - TileMaps[i].TileMapCoords.Y);

                if (currX >= 0 && currY >= 0 && currX < map.Width && currY < map.Height)
                    return TileMaps[i].GetLocalTile(currX, currY);
            }

            throw new NotImplementedException();
        }

        internal void ClearAllVisitedTiles()
        {
            TileMaps.ForEach(m => m.Tiles.ForEach(tile => tile.TilePoint._visited = false)); //clear visited tiles
        }

        internal void ToggleHeightmap()
        {
            Settings.HeightmapEnabled = !Settings.HeightmapEnabled;

            TileMaps.ForEach(map =>
            {
                map.Tiles.ForEach(tile => tile.Update());
            });
        }
    }
}
