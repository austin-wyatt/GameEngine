using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Map.FeatureEquations
{
    public class River_1 : FeatureEquation
    {
        public int XOrigin;
        public int Height;
        public int Width;
        public float TileDrop;

        List<RiverParams> RiverParams;

        public River_1(int height, int width, float drop = 0, int xOrigin = 0) 
        {
            Height = height;
            Width = width;
            TileDrop = drop;
            XOrigin = xOrigin;
        }
        public River_1(List<RiverParams> riverParams)
        {
            RiverParams = riverParams;
        }

        //int _wiggle = 4;
        //int yRamp = 0;
        public override void GenerateAtPoint(BaseTile tile)
        {
            //Vector2i coords = PointToMapCoords(tile.TilePoint);


            //yRamp = Math.Abs(coords.X % (_wiggle * 2)) >= _wiggle ? 0 : 1;


            //int y = coords.Y;

            ////if (coords.Y + TileDrop * (coords.X - XOrigin) >= Height && coords.Y + TileDrop * (coords.X - XOrigin) < Height + Width) 
            //if (y + yRamp >= Height && y + yRamp < Height + Width)
            //{
            //    //double sin = Math.Sin((double)coords.Y / (Math.Abs(coords.Y) + Width) * Math.PI / 2);
            //    //double sinX = Math.Sin(coords.X);
            //    //if ((sin > 0.5 || sin < -0.5) && (sinX > 0.5 || sinX < -0.5))
            //    //    return;

            //    //if (TileMap._randomNumberGen.NextDouble() < WiggleFactor)
            //    //{
            //    //    _wiggle++;
            //    //}

            //    tile.Properties.Type = TileMap._randomNumberGen.NextDouble() > 0.3 ? TileType.Water : TileType.AltWater;
            //    tile.Properties.Classification = TileClassification.Water;
            //    tile.Outline = false;
            //    tile.NeverOutline = true;

            //    tile.Update();
            //}

            //for a river equation specify an inlet, an outlet, a width, and maybe a wiggle factor for each tilemap that the river should affect 
            //this way its easier to determine which tile maps should be affected then it's also much easier to create consistent seamless features
            //create a modified version of GetPathToPoint to create connections from the inlet to the outlet
        }

        public override bool AffectsMap(TileMap map)
        {
            return RiverParams.Exists(p => p.MapCoords == map.TileMapCoords);
        }

        public override void ApplyToMap(TileMap map)
        {
            RiverParams riverParams = RiverParams.Find(p => p.MapCoords == map.TileMapCoords);
            Vector2i startPoint = riverParams.Inlet;

            List<BaseTile> neighborTiles = new List<BaseTile>();
            List<BaseTile> origTiles = new List<BaseTile>();

            for (int i = 0; i < riverParams.Stops.Count; i++) 
            {
                FeaturePathToPointParameters param = new FeaturePathToPointParameters(map.GetLocalTile(startPoint.X, startPoint.Y).TilePoint, map.GetLocalTile(riverParams.Stops[i].X, riverParams.Stops[i].Y).TilePoint);

                List<BaseTile> path = GetPathToPoint(param);

                param.Map.Controller.ClearAllVisitedTiles();

                path.ForEach(tile =>
                {
                    int width = riverParams.Width / 2;

                    origTiles.Add(tile);
                    for (int i = 0; i < width; i++) 
                    {
                        origTiles.ForEach(origTile =>
                        {
                            GetNeighboringTiles(origTile, neighborTiles);
                        });

                        origTiles.Clear();

                        neighborTiles.ForEach(neighborTile =>
                        {
                            UpdateTile(neighborTile);
                            origTiles.Add(neighborTile);
                        });

                        neighborTiles.Clear();
                    }

                    origTiles.Clear();
                    neighborTiles.Clear();

                    UpdateTile(tile);
                });

                startPoint = riverParams.Stops[i];
            }
        }

        private void UpdateTile(BaseTile tile) 
        {
            tile.Properties.Type = TileMap._randomNumberGen.NextDouble() > 0.3 ? TileType.Water : TileType.AltWater;
            tile.Properties.Classification = TileClassification.Water;
            tile.Outline = false;
            tile.NeverOutline = true;

            tile.Update();
        }
    }

    public struct RiverParams 
    {
        public Vector2i Inlet;
        public List<Vector2i> Stops;
        public TileMapPoint MapCoords;
        public int Width;

        public RiverParams(TileMapPoint map, Vector2i inlet, Vector2i outlet, int width = 1) 
        {
            MapCoords = map;
            Inlet = inlet;
            Stops = new List<Vector2i>() { outlet };

            Width = width;
        }

        public void AddStop(Vector2i stop) 
        {
            Stops.Insert(Stops.Count - 1, stop);
        }
    }
}
