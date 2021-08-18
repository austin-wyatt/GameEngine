using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MortalDungeon.Game.Map
{
    public class FeatureEquation
    {
        internal const int MAP_WIDTH = 50;
        internal const int MAP_HEIGHT = 50;

        public virtual void GenerateAtPoint(BaseTile tile) 
        {

        }

        public virtual bool AffectsMap(TileMap map) 
        {
            return true;
        }

        public virtual void ApplyToMap(TileMap map) 
        {
            
        }

        public static Vector2i PointToMapCoords(TilePoint point) 
        {
            Vector2i coords = new Vector2i();

            coords.X = point.X + point.ParentTileMap.TileMapCoords.X * point.ParentTileMap.Width;
            coords.Y = point.Y + point.ParentTileMap.TileMapCoords.Y * point.ParentTileMap.Height;

            return coords;
        }




        public struct FeaturePathToPointParameters
        {
            public TilePoint StartingPoint;
            public TilePoint EndingPoint;
            public TileMap Map;
            public Random NumberGen;


            public FeaturePathToPointParameters(TilePoint startingPoint, TilePoint endPoint)
            {
                StartingPoint = startingPoint;
                EndingPoint = endPoint;
                Map = startingPoint.ParentTileMap;

                NumberGen = new Random(HashCoordinates(Map.TileMapCoords.X, Map.TileMapCoords.Y));
            }
        }

        public static List<BaseTile> GetPathToPoint(FeaturePathToPointParameters param)
        {
            List<TileMap.TileWithParent> tileList = new List<TileMap.TileWithParent>();
            List<BaseTile> returnList = new List<BaseTile>();

            List<BaseTile> neighbors = new List<BaseTile>();

            param.Map.Controller.ClearAllVisitedTiles();


            neighbors.Add(param.StartingPoint.GetTile());
            param.StartingPoint._visited = true;

            tileList.Add(new TileMap.TileWithParent(param.StartingPoint.GetTile()));

            List<BaseTile> newNeighbors = new List<BaseTile>();


            for (int i = 0; i < MAP_HEIGHT * MAP_WIDTH; i++)
            {
                newNeighbors.Clear();
                neighbors.ForEach(p =>
                {
                    GetNeighboringTiles(p, newNeighbors, true, param.NumberGen);

                    newNeighbors.ForEach(neighbor =>
                    {
                        tileList.Add(new TileMap.TileWithParent(neighbor, p));
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
                    if (neighbors[j].TilePoint == param.EndingPoint)
                    {
                        //if we found the destination tile then fill the returnList and return
                        TileMap.TileWithParent finalTile = tileList.Find(t => t.Tile.TilePoint == param.EndingPoint);

                        returnList.Add(finalTile.Tile);

                        BaseTile parent = finalTile.Parent;

                        while (parent != null)
                        {
                            TileMap.TileWithParent currentTile = tileList.Find(t => t.Tile.TilePoint == parent.TilePoint);
                            returnList.Add(currentTile.Tile);

                            parent = currentTile.Parent;
                        }

                        returnList.Reverse();
                        return returnList;
                    }
                }
            }

            return returnList;
        }

        public static void GetNeighboringTiles(BaseTile tile, List<BaseTile> neighborList, bool shuffle = true, Random numberGen = null)
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

                if (tile.TilePoint.ParentTileMap.IsValidTile(neighborPos))
                {
                    BaseTile neighborTile = tile.TilePoint.ParentTileMap[neighborPos];
                    if (!neighborTile.TilePoint._visited)
                    {
                        neighborList.Add(neighborTile);
                        neighborTile.TilePoint._visited = true;
                    }
                }
            }

            if (shuffle)
            {
                ShuffleList(neighborList, numberGen);
            }
        }

        public static Direction DirectionBetweenTiles(TilePoint startPoint, TilePoint endPoint) 
        {
            Direction direction = Direction.None;

            Vector2i start = PointToMapCoords(startPoint);
            Vector2i end = PointToMapCoords(endPoint);

            int yOffset = start.X % 2 == 0 ? 0 : -1;

            if (start.Y - end.Y == 1 && start.X == end.X)
            {
                direction = Direction.North;
            }
            else if (start.Y - end.Y == -1 && start.X == end.X)
            {
                direction = Direction.South;
            }
            else if (start.X - end.X == -1 && start.Y + yOffset - end.Y == 0) 
            {
                direction = Direction.NorthEast;
            }
            else if (start.X - end.X == -1 && start.Y + yOffset - end.Y == -1)
            {
                direction = Direction.SouthEast;
            }
            else if (start.X - end.X == 1 && start.Y + yOffset - end.Y == 0)
            {
                direction = Direction.NorthWest;
            }
            else if (start.X - end.X == 1 && start.Y + yOffset - end.Y == -1)
            {
                direction = Direction.SouthWest;
            }

            return direction;
        }

        public static int AngleBetweenDirections(Direction a, Direction b) 
        {
            int temp = (int)a - (int)b;

            int angle;
            if (temp < 0)
            {
                temp += 6;
                angle = 60 * temp;
            }
            else 
            {
                angle = 60 * temp;
            }

            return angle;
        }

        public static int AngleOfDirection(Direction dir) 
        {
            return ((int)dir + 2) * 60; //subtract 2 from the direction so that the default direction is north
        }

        static void ShuffleList<T>(IList<T> list, Random numberGen = null)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k;
                if (numberGen == null)
                {
                    k = new Random().Next(n + 1);
                }
                else 
                {
                    k = numberGen.Next(n + 1);
                }

                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        internal static int HashCoordinates(int x, int y)
        {
            int h = x * 374761393 + y * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            return h ^ (h >> 16);
        }
    }
}
