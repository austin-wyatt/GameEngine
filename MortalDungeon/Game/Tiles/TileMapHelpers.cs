using MortalDungeon.Game.Map;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MortalDungeon.Game.Tiles
{
    public static class TileMapHelpers
    {
        public static Vector2i PointToClusterPosition(TilePoint point)
        {
            Vector2i globalPoint = FeatureEquation.PointToMapCoords(point);

            Vector2i zeroPoint = GetTopLeftTilePosition();

            return globalPoint - zeroPoint;
        }

        public static TileMap _topLeftMap = null;
        public static bool IsValidTile(FeaturePoint point)
        {
            Vector2i topLeftCoord = GetTopLeftTilePosition();

            Vector2i botRightCoord = topLeftCoord + new Vector2i(_topLeftMap.Width * TileMapManager.LOAD_DIAMETER, _topLeftMap.Height * TileMapManager.LOAD_DIAMETER);

            return point.X >= topLeftCoord.X && point.X <= botRightCoord.X && point.Y >= topLeftCoord.Y && point.Y <= botRightCoord.Y;
        }

        public static bool IsValidTile(int xIndex, int yIndex, TileMap map)
        {
            int mapX = (int)Math.Floor((float)(map.TileMapCoords.X * TileMapManager.TILE_MAP_DIMENSIONS.X + xIndex) / TileMapManager.TILE_MAP_DIMENSIONS.X);
            int mapY = (int)Math.Floor((float)(map.TileMapCoords.Y * TileMapManager.TILE_MAP_DIMENSIONS.Y + xIndex) / TileMapManager.TILE_MAP_DIMENSIONS.Y);

            TileMapPoint calculatedPoint = new TileMapPoint(mapX, mapY);

            return TileMapManager.LoadedMaps.ContainsKey(calculatedPoint);
        }

        public static Vector2i GetTopLeftTilePosition()
        {
            if (_topLeftMap != null)
            {
                return FeatureEquation.PointToMapCoords(_topLeftMap.Tiles[0].TilePoint);
            }

            return new Vector2i(0, 0);
        }

        public static TileMap GetTopLeftMap()
        {
            if (_topLeftMap != null)
            {
                return _topLeftMap;
            }

            return null;
        }

        public static TileMapPoint GlobalPositionToMapPoint(Vector3 position)
        {
            if (TileMapManager.LoadedMaps.Count == 0)
                return null;

            Vector3 camPos = WindowConstants.ConvertLocalToScreenSpaceCoordinates(position.Xy);

            var map = TileMapManager.LoadedMaps.Values.First();

            Vector3 dim = map.GetTileMapDimensions();

            Vector3 mapPos = map.Position;

            Vector3 offsetPos = camPos - mapPos;

            TileMapPoint point = new TileMapPoint((int)Math.Floor(offsetPos.X / dim.X) + map.TileMapCoords.X, (int)Math.Floor(offsetPos.Y / dim.Y) + map.TileMapCoords.Y);

            return point;
        }

        public static BaseTile GetTile(int xIndex, int yIndex, TileMap map)
        {
            int currX;
            int currY;

            int mapX = (int)Math.Floor((float)(map.TileMapCoords.X * TileMapManager.TILE_MAP_DIMENSIONS.X + xIndex) / TileMapManager.TILE_MAP_DIMENSIONS.X);
            int mapY = (int)Math.Floor((float)(map.TileMapCoords.Y * TileMapManager.TILE_MAP_DIMENSIONS.Y + yIndex) / TileMapManager.TILE_MAP_DIMENSIONS.Y);

            TileMapPoint calculatedPoint = new TileMapPoint(mapX, mapY);

            if (TileMapManager.LoadedMaps.TryGetValue(calculatedPoint, out var foundMap))
            {
                if (xIndex < 0)
                {
                    currX = TileMapManager.TILE_MAP_DIMENSIONS.X - Math.Abs(xIndex) % TileMapManager.TILE_MAP_DIMENSIONS.X;
                }
                else
                {
                    currX = Math.Abs(xIndex % TileMapManager.TILE_MAP_DIMENSIONS.X);
                }

                if (yIndex < 0)
                {
                    currY = TileMapManager.TILE_MAP_DIMENSIONS.Y - Math.Abs(yIndex) % TileMapManager.TILE_MAP_DIMENSIONS.Y;
                }
                else
                {
                    currY = Math.Abs(yIndex) % TileMapManager.TILE_MAP_DIMENSIONS.Y;
                }

                return foundMap.GetLocalTile(currX, currY);
            }


            return null;
        }

        public static BaseTile GetTile(int xIndex, int yIndex)
        {
            return GetTile(xIndex, yIndex, _topLeftMap);
        }

        public static BaseTile GetTile(FeaturePoint point)
        {
            Vector2i topLeftCoord = GetTopLeftTilePosition();

            return GetTile(point.X - topLeftCoord.X, point.Y - topLeftCoord.Y);
        }

        public static BaseTile GetTile(TilePoint point)
        {
            return GetTile(point.X, point.Y, point.ParentTileMap);
        }

        private static List<TilePoint> _visitedTiles = new List<TilePoint>();
        private static object _visitedTilesLock = new object();

        public static void AddVisitedTile(TilePoint point)
        {
            lock(_visitedTilesLock)
            {
                _visitedTiles.Add(point);
            }
        }

        public static void RemoveVisitedTile(TilePoint point)
        {
            lock (_visitedTilesLock)
            {
                _visitedTiles.Remove(point);
            }
        }

        public static void ClearAllVisitedTiles()
        {
            lock (_visitedTilesLock)
            {
                for(int i = 0; i < _visitedTiles.Count; i++)
                {
                    _visitedTiles[i].Visited = false;
                }

                _visitedTiles.Clear();
            }
        }

        public static List<(float Distance, TileChunk Chunk)> GetChunksByDistance(Vector3 mouseRayNear, Vector3 mouseRayFar)
        {
            lock (TileMapManager._visibleMapLock)
            {
                float percentageAlongLine = (0 - mouseRayNear.Z) / (mouseRayFar.Z - mouseRayNear.Z);

                Vector3 pointAtZ = mouseRayNear + (mouseRayFar - mouseRayNear) * percentageAlongLine;

                pointAtZ = WindowConstants.ConvertLocalToScreenSpaceCoordinates(pointAtZ.Xy);

                List<TileMap> mapsByDistance = new List<TileMap>();

                foreach (var map in TileMapManager.VisibleMaps)
                {
                    if (!map.Render)
                        continue;

                    var dimensions = map.GetTileMapDimensions();

                    var distance = Vector3.Distance(pointAtZ, map.Position);

                    if (distance < dimensions.LengthFast)
                    {
                        mapsByDistance.Add(map);
                    }
                }

                List<(float Distance, TileChunk Chunk)> chunksByDistance = new List<(float, TileChunk)>();

                TileChunk chunk;
                for (int i = 0; i < mapsByDistance.Count; i++)
                {
                    for (int j = 0; j < mapsByDistance[i].TileChunks.Count; j++)
                    {
                        chunk = mapsByDistance[i].TileChunks[j];

                        var distance = Vector3.Distance(pointAtZ, chunk.Center);

                        if (distance < chunk.Radius)
                        {
                            chunksByDistance.Add((distance, chunk));
                        }
                    }
                }

                chunksByDistance.Sort((a, b) => a.Distance.CompareTo(b.Distance));

                return chunksByDistance;
            }
        }
    }
}
