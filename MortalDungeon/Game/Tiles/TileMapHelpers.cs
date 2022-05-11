using Empyrean.Engine_Classes;
using Empyrean.Game.Map;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Empyrean.Game.Tiles
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
        public static TileMap _bottomRightMap = null;
        public static bool IsValidTile(FeaturePoint point)
        {
            return TileMapManager.LoadedMaps.ContainsKey(point.ToTileMapPoint());
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

        public static FeaturePoint GetTopLeftFeaturePoint()
        {
            return _topLeftMap?.Tiles[0].ToFeaturePoint() ?? FeaturePoint.MinPoint;
        }

        public static FeaturePoint GetBottomRightFeaturePoint()
        {
            return _bottomRightMap?.Tiles[^1].ToFeaturePoint() ?? FeaturePoint.MinPoint;
        }

        public static TileMapPoint GlobalPositionToMapPoint(Vector3 position)
        {
            if (TileMapManager.LoadedMaps.Count == 0)
                return TileMapPoint.Empty;

            Vector3 camPos = WindowConstants.ConvertLocalToScreenSpaceCoordinates(position.Xy);

            var map = TileMapManager.ActiveMaps[0];

            Vector3 dim = map.GetTileMapDimensions();

            Vector3 mapPos = map.Position;

            Vector3 offsetPos = camPos - mapPos;

            TileMapPoint point = new TileMapPoint((int)Math.Floor(offsetPos.X / dim.X) + map.TileMapCoords.X, (int)Math.Floor(offsetPos.Y / dim.Y) + map.TileMapCoords.Y);

            return point;
        }

        public static Tile GetTile(int xIndex, int yIndex, TileMap map)
        {
            int currX;
            int currY;

            int mapX = (int)Math.Floor((float)(map.TileMapCoords.X * TileMapManager.TILE_MAP_DIMENSIONS.X + xIndex) / TileMapManager.TILE_MAP_DIMENSIONS.X);
            int mapY = (int)Math.Floor((float)(map.TileMapCoords.Y * TileMapManager.TILE_MAP_DIMENSIONS.Y + yIndex) / TileMapManager.TILE_MAP_DIMENSIONS.Y);

            TileMapPoint calculatedPoint = new TileMapPoint(mapX, mapY);

            if(calculatedPoint == map.TileMapCoords)
            {
                return map.GetLocalTile(xIndex, yIndex);
            }

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

        public static Tile GetTile(int xIndex, int yIndex)
        {
            return GetTile(xIndex, yIndex, _topLeftMap);
        }

        public static Tile GetTile(FeaturePoint point)
        {
            TileMapPoint mapPoint = TileMapPoint.Pool.GetObject();
            point.ToTileMapPoint(ref mapPoint);
            if(TileMapManager.LoadedMaps.TryGetValue(mapPoint, out var tileMap))
            {
                int currX;
                int currY;

                if (point.X < 0)
                {
                    currX = TileMapManager.TILE_MAP_DIMENSIONS.X - Math.Abs(point.X) % TileMapManager.TILE_MAP_DIMENSIONS.X;
                    currX = currX == TileMapManager.TILE_MAP_DIMENSIONS.X ? 0 : currX;
                }
                else
                {
                    currX = point.X % TileMapManager.TILE_MAP_DIMENSIONS.X;
                }

                if (point.Y < 0)
                {
                    currY = TileMapManager.TILE_MAP_DIMENSIONS.Y - Math.Abs(point.Y) % TileMapManager.TILE_MAP_DIMENSIONS.Y;
                    currY = currY == TileMapManager.TILE_MAP_DIMENSIONS.Y ? 0 : currY;
                }
                else
                {
                    currY = point.Y % TileMapManager.TILE_MAP_DIMENSIONS.Y;
                }

                TileMapPoint.Pool.FreeObject(ref mapPoint);

                return tileMap.GetLocalTile(currX, currY);
            }

            TileMapPoint.Pool.FreeObject(ref mapPoint);

            return null;
        }

        public static Tile GetTile(TilePoint point)
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

        private static ObjectPool<List<TileMap>> _tileMapListPool = new ObjectPool<List<TileMap>>();
        private static ObjectPool<List<DistanceAndChunk>> _distanceChunkPool = new ObjectPool<List<DistanceAndChunk>>();
        /// <summary>
        /// Gathers a list of chunks near the mouse ray. <para/>
        /// Be sure to call FreeDistanceAndChunkList when finished consuming the returned list.
        /// </summary>
        public static List<DistanceAndChunk> GetChunksByDistance(Vector3 mouseRayNear, Vector3 mouseRayFar)
        {
            lock (TileMapManager._visibleMapLock)
            {
                float percentageAlongLine = (0 - mouseRayNear.Z) / (mouseRayFar.Z - mouseRayNear.Z);

                Vector3 pointAtZ = mouseRayNear + (mouseRayFar - mouseRayNear) * percentageAlongLine;

                pointAtZ = WindowConstants.ConvertLocalToScreenSpaceCoordinates(pointAtZ.Xy);

                List<TileMap> mapsByDistance = _tileMapListPool.GetObject();

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

                List<DistanceAndChunk> chunksByDistance = _distanceChunkPool.GetObject();

                TileChunk chunk;
                for (int i = 0; i < mapsByDistance.Count; i++)
                {
                    for (int j = 0; j < mapsByDistance[i].TileChunks.Count; j++)
                    {
                        chunk = mapsByDistance[i].TileChunks[j];

                        var distance = Vector3.Distance(pointAtZ, chunk.Center);

                        if (distance < chunk.Radius)
                        {
                            chunksByDistance.Add(new DistanceAndChunk(distance, chunk));
                        }
                    }
                }

                mapsByDistance.Clear();
                _tileMapListPool.FreeObject(ref mapsByDistance);

                chunksByDistance.Sort((a, b) => a.Distance.CompareTo(b.Distance));

                return chunksByDistance;
            }
        }

        public static void FreeDistanceAndChunkList(List<DistanceAndChunk> list)
        {
            list.Clear();
            _distanceChunkPool.FreeObject(ref list);
        }

        public struct DistanceAndChunk
        {
            public float Distance;
            public TileChunk Chunk;

            public DistanceAndChunk(float distance, TileChunk chunk)
            {
                Distance = distance; 
                Chunk = chunk; 
            }
        }

        public static bool TestCameraTileMapPosition(TileMapPoint currPos, Vector3 cameraPosition)
        {
            if (TileMapManager.LoadedMaps.Count == 0)
                return false;

            Vector3 camPos = WindowConstants.ConvertLocalToScreenSpaceCoordinates(ref cameraPosition);

            var map = TileMapManager.ActiveMaps[0];

            Vector3 dim = map.GetTileMapDimensions();

            float offsetX = camPos.X - map._position.X;
            float offsetY = camPos.Y - map._position.Y;

            return currPos.X != (int)Math.Floor(offsetX / dim.X) + map.TileMapCoords.X 
                || currPos.Y != (int)Math.Floor(offsetY / dim.Y) + map.TileMapCoords.Y;
        }
    }
}
