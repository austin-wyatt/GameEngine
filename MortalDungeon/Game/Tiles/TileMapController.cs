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

namespace MortalDungeon.Game.Tiles
{
    public class TileMapController
    {
        public List<TileMap> TileMaps = new List<TileMap>();
        public static StaticBitmap TileBitmap;

        public int BaseElevation = 0; //base elevation for determining heightmap colors

        public CombatScene Scene;

        public List<FeatureEquation> LoadedFeatures = new List<FeatureEquation>();

        public TileMapController(CombatScene scene = null) 
        {
            //Bitmap tempMap = new Bitmap("Resources/TileSpritesheet.png");

            //TileBitmap = new StaticBitmap(tempMap.Width, tempMap.Height);

            //for (int y = 0; y < tempMap.Height; y++) 
            //{
            //    for (int x = 0; x < tempMap.Width; x++)
            //    {
            //        TileBitmap.SetPixel(x, y, tempMap.GetPixel(x, y));
            //    }
            //}

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

        public void LoadSurroundingTileMaps(TileMapPoint point, int width = 3) 
        {
            TileMapPoint currPoint = new TileMapPoint(point.X - 1, point.Y - 1);

            List<TileMapPoint> loadedPoints = new List<TileMapPoint>();
            List<TileMapPoint> mapsToAdd = new List<TileMapPoint>();

            for (int i = 0; i < width; i++) 
            {
                for (int j = 0; j < width; j++) 
                {
                    TileMap map = TileMaps.Find(m => m.TileMapCoords == currPoint);

                    if (map == null)
                    {
                        mapsToAdd.Add(new TileMapPoint(currPoint.X, currPoint.Y));
                        
                    }

                    loadedPoints.Add(new TileMapPoint(currPoint.X, currPoint.Y));

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

                Scene.PostTickAction = null;
            };
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
