using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Empyrean.Game.Tiles.Meshes
{
    public static class MeshTileBlender
    {
        /// <summary>
        /// The heights of each tile in column major order.
        /// </summary>
        public static float[] TileHeightMap = new float[TileMapManager.TILE_MAP_DIMENSIONS.X * TileMapManager.TILE_MAP_DIMENSIONS.Y 
            * TileMapManager.LOAD_DIAMETER * TileMapManager.LOAD_DIAMETER];

        private static readonly int COLUMN_SIZE = TileMapManager.TILE_MAP_DIMENSIONS.Y * TileMapManager.LOAD_DIAMETER;


        /// <summary>
        /// Fills the static TileHeightMap array with the heights of every tile of each active map.
        /// </summary>
        public static void FillTileHeightMap()
        {
            for (int i = 0; i < TileMapManager.ActiveMaps.Count; i++)
            {
                Vector2i mapPointOffset = TileMapManager.ActiveMaps[i].TileMapCoords - TileMapHelpers._topLeftMap.TileMapCoords;
                int initialOffset = mapPointOffset.X * COLUMN_SIZE * TileMapManager.TILE_MAP_DIMENSIONS.X + mapPointOffset.Y * TileMapManager.TILE_MAP_DIMENSIONS.Y;

                for (int j = 0; j < TileMapManager.ActiveMaps[i].Tiles.Count; j++)
                {
                    int tileIndex = initialOffset + (j / TileMapManager.TILE_MAP_DIMENSIONS.Y) * COLUMN_SIZE + j % TileMapManager.TILE_MAP_DIMENSIONS.Y;

                    TileHeightMap[tileIndex] = TileMapManager.ActiveMaps[i].Tiles[j].Properties.Height;
                }
            }
        }


        private static HashSet<TileChunk> ChunksToUpdate = new HashSet<TileChunk>();
        private static HashSet<Tile> NormalTilesToUpdate = new HashSet<Tile>();
        private static object _blendPassLock = new object();

        /// <summary>
        /// Called when maps are loaded. This will find and blend all tiles that need it in the whole 
        /// loaded set of tilemaps. <para/>
        /// Further blending can be done via minor blending passes that are more localized.
        /// </summary>
        public static void MajorBlendPass()
        {
            Monitor.Enter(_blendPassLock);

            ChunksToUpdate.Clear();
            NormalTilesToUpdate.Clear();

            Vector2i tileCoords = new Vector2i();

            Tile current;

            Tile north;
            Tile northEast;
            Tile southEast;

            bool blendNorth;
            bool blendNorthEast;
            bool blendSouthEast;

            bool tileExistsNorth;
            bool tileExistsNorthEast;
            bool tileExistsSouthEast;

            float heightNorth;
            float heightNorthEast;
            float heightSouthEast;

            float currHeight;

            float blendedValue;

            int vertexCount;

            for (int i = 0; i < TileHeightMap.Length; i++)
            {
                //check which tiles will need to be blended
                currHeight = TileHeightMap[i];

                heightNorth = GetHeightInDirection(Direction.North, i);
                tileExistsNorth = heightNorth != float.MinValue;
                blendNorth = heightNorth != currHeight && tileExistsNorth;

                heightNorthEast = GetHeightInDirection(Direction.NorthEast, i);
                tileExistsNorthEast = heightNorthEast != float.MinValue;
                blendNorthEast = heightNorthEast != currHeight && tileExistsNorthEast;

                heightSouthEast = GetHeightInDirection(Direction.SouthEast, i);
                tileExistsSouthEast = heightSouthEast != float.MinValue;
                blendSouthEast = heightSouthEast != currHeight && tileExistsSouthEast;

                GetTileIndex(i, ref tileCoords);

                if (blendNorth || blendNorthEast || blendSouthEast)
                {
                    current = GetTileInDirection(Direction.None, ref tileCoords);
                    ChunksToUpdate.Add(current.Chunk);
                    NormalTilesToUpdate.Add(current);
                }
                else
                {
                    continue;
                }

                //only get the tiles if they exist AND if we actually need them
                north = (tileExistsNorth && (blendNorth || blendNorthEast)) ? GetTileInDirection(Direction.North, ref tileCoords) : null;
                northEast = (tileExistsNorthEast && (blendNorth || blendNorthEast || blendSouthEast)) ? GetTileInDirection(Direction.NorthEast, ref tileCoords) : null;
                southEast = (tileExistsSouthEast && (blendNorthEast || blendSouthEast)) ? GetTileInDirection(Direction.SouthEast, ref tileCoords) : null;

                //equivalent vertices are hardcoded since this blending process needs to be quick.

                if (blendNorth)
                {
                    //blend 1
                    //blendedValue = (currHeight + heightNorth) / 2;
                    blendedValue = BlendVertices(currHeight, heightNorth);

                    current.MeshTileHandle.Weights[1] = blendedValue;
                    current.MeshTileHandle.ApplyWeight(1);

                    north.MeshTileHandle.Weights[7] = blendedValue;
                    north.MeshTileHandle.ApplyWeight(7);

                    BlendTextures(current.MeshTileHandle, 1, north.MeshTileHandle, 7);

                    ChunksToUpdate.Add(north.Chunk);
                }

                if (blendNorthEast || blendNorth)
                {
                    //blend 2
                    vertexCount = 1 + (tileExistsNorthEast ? 1 : 0) + (tileExistsNorth ? 1 : 0);

                    //blendedValue = (currHeight +
                    //    (heightNorth != float.MinValue ? heightNorth : 0) +
                    //    (heightNorthEast != float.MinValue ? heightNorthEast : 0)) / vertexCount;

                    blendedValue = BlendVertices(currHeight, heightNorth, heightNorthEast);

                    current.MeshTileHandle.Weights[2] = blendedValue;
                    current.MeshTileHandle.ApplyWeight(2);


                    if (tileExistsNorth)
                    {
                        north.MeshTileHandle.Weights[6] = blendedValue;
                        north.MeshTileHandle.ApplyWeight(6);

                        NormalTilesToUpdate.Add(north);

                        BlendTextures(current.MeshTileHandle, 2, north.MeshTileHandle, 6);
                    }

                    if (tileExistsNorthEast)
                    {
                        northEast.MeshTileHandle.Weights[10] = blendedValue;
                        northEast.MeshTileHandle.ApplyWeight(10);

                        NormalTilesToUpdate.Add(northEast);

                        BlendTextures(current.MeshTileHandle, 2, northEast.MeshTileHandle, 10);
                    }
                }

                if (blendNorthEast)
                {
                    //blend 3
                    //blendedValue = (currHeight + heightNorthEast) / 2;
                    blendedValue = BlendVertices(currHeight, heightNorthEast);

                    current.MeshTileHandle.Weights[3] = blendedValue;
                    current.MeshTileHandle.ApplyWeight(3);

                    northEast.MeshTileHandle.Weights[9] = blendedValue;
                    northEast.MeshTileHandle.ApplyWeight(9);

                    ChunksToUpdate.Add(northEast.Chunk);

                    BlendTextures(current.MeshTileHandle, 3, northEast.MeshTileHandle, 9);
                }

                if (blendNorthEast || blendSouthEast)
                {
                    //blend 4
                    vertexCount = 1 + (tileExistsNorthEast ? 1 : 0) + (tileExistsSouthEast ? 1 : 0);

                    //blendedValue = (currHeight +
                    //    (heightSouthEast != float.MinValue ? heightSouthEast : 0) +
                    //    (heightNorthEast != float.MinValue ? heightNorthEast : 0)) / vertexCount;

                    blendedValue = BlendVertices(currHeight, heightSouthEast, heightNorthEast);



                    current.MeshTileHandle.Weights[4] = blendedValue;
                    current.MeshTileHandle.ApplyWeight(4);

                    if (tileExistsSouthEast)
                    {
                        southEast.MeshTileHandle.Weights[0] = blendedValue;
                        southEast.MeshTileHandle.ApplyWeight(0);

                        NormalTilesToUpdate.Add(southEast);

                        BlendTextures(current.MeshTileHandle, 4, southEast.MeshTileHandle, 0);
                    }

                    if (tileExistsNorthEast)
                    {
                        northEast.MeshTileHandle.Weights[8] = blendedValue;
                        northEast.MeshTileHandle.ApplyWeight(8);

                        NormalTilesToUpdate.Add(northEast);

                        BlendTextures(current.MeshTileHandle, 4, northEast.MeshTileHandle, 8);
                    }
                }

                if (blendSouthEast)
                {
                    //blend 5
                    //blendedValue = (currHeight + heightSouthEast) / 2;
                    blendedValue = BlendVertices(currHeight, heightSouthEast);

                    current.MeshTileHandle.Weights[5] = blendedValue;
                    current.MeshTileHandle.ApplyWeight(5);

                    southEast.MeshTileHandle.Weights[11] = blendedValue;
                    southEast.MeshTileHandle.ApplyWeight(11);

                    ChunksToUpdate.Add(southEast.Chunk);

                    BlendTextures(current.MeshTileHandle, 5, southEast.MeshTileHandle, 11);
                }

            }

            foreach(var chunk in ChunksToUpdate)
            {
                chunk.Update(TileUpdateType.Vertex);
            }

            Monitor.Exit(_blendPassLock);

            Task.Run(() =>
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                Monitor.Enter(_blendPassLock);
                foreach (var tile in NormalTilesToUpdate)
                {
                    tile.MeshTileHandle.CalculateNormal();
                };

                Console.WriteLine($"Normals calculated in {timer.ElapsedMilliseconds}ms");
                Monitor.Exit(_blendPassLock);
            });
        }

        private static Tile GetTileInDirection(Direction direction, ref Vector2i centerCoords)
        {
            switch (direction)
            {
                case Direction.None:
                    return TileMapHelpers.GetTile(centerCoords.X, centerCoords.Y);
                case Direction.North:
                    return TileMapHelpers.GetTile(centerCoords.X, centerCoords.Y - 1);
                case Direction.South:
                    return TileMapHelpers.GetTile(centerCoords.X, centerCoords.Y + 1);
                case Direction.NorthEast:
                    return TileMapHelpers.GetTile(centerCoords.X + 1, centerCoords.Y - centerCoords.X % 2);
                case Direction.NorthWest:
                    return TileMapHelpers.GetTile(centerCoords.X - 1, centerCoords.Y - centerCoords.X % 2);
                case Direction.SouthEast:
                    return TileMapHelpers.GetTile(centerCoords.X + 1, centerCoords.Y + (centerCoords.X + 1) % 2);
                case Direction.SouthWest:
                    return TileMapHelpers.GetTile(centerCoords.X - 1, centerCoords.Y + (centerCoords.X + 1) % 2);
            }

            return null;
        }

        /// <summary>
        /// Returns the height of the tile in the specified direction from the passed tile index.
        /// If no tile exists in that direction, float.MinValue is returned.
        /// </summary>
        private static float GetHeightInDirection(Direction direction, int tileIndex)
        {
            int index = tileIndex;

            bool evenColumn = index / COLUMN_SIZE % 2 == 0;

            switch (direction)
            {
                case Direction.North:
                    if (index % COLUMN_SIZE == 0)
                    {
                        return float.MinValue;
                    }

                    index -= 1;
                    break;
                case Direction.South:
                    if (index % COLUMN_SIZE == (COLUMN_SIZE - 1))
                    {
                        return float.MinValue;
                    }

                    index += 1;
                    break;
                case Direction.NorthEast:
                    index += COLUMN_SIZE;

                    if (index >= TileHeightMap.Length) return float.MinValue;

                    //if the column is odd then we need to add 1 to get the northeast index
                    if (!evenColumn)
                    {
                        //check if this index is on the top row of active maps
                        if (index % COLUMN_SIZE == 0)
                        {
                            return float.MinValue;
                        }
                        else
                        {
                            index -= 1;
                        }
                    }
                    break;
                case Direction.NorthWest:
                    index -= COLUMN_SIZE;

                    if (index < 0) return float.MinValue;

                    //if the column is odd then we need to add 1 to get the northwest index
                    if (!evenColumn)
                    {
                        //check if this index is on the top row of active maps
                        if(index % COLUMN_SIZE == 0)
                        {
                            return float.MinValue;
                        }
                        else
                        {
                            index -= 1;
                        }
                    }
                    break;
                case Direction.SouthEast:
                    index += COLUMN_SIZE;

                    if (index >= TileHeightMap.Length) return float.MinValue;

                    //if the column is even then we need to add 1 to get the southeast index
                    if (evenColumn)
                    {
                        //check if this index is on the bottom row of active maps
                        if (index % COLUMN_SIZE == (COLUMN_SIZE - 1))
                        {
                            return float.MinValue;
                        }
                        else
                        {
                            index += 1;
                        }
                    }
                    break;
                case Direction.SouthWest:
                    index -= COLUMN_SIZE;

                    if (index < 0) return float.MinValue;

                    //if the column is even then we need to add 1 to get the southeast index
                    if (evenColumn)
                    {
                        //check if this index is on the bottom row of active maps
                        if (index % COLUMN_SIZE == (COLUMN_SIZE - 1))
                        {
                            return float.MinValue;
                        }
                        else
                        {
                            index += 1;
                        }
                    }
                    break;
            }

            return TileHeightMap[index];
        }


        /// <summary>
        /// Gets the tile's X and Y index from the index in the TileHeightMap
        /// </summary>
        private static void GetTileIndex(int index, ref Vector2i vec)
        {
            vec.X = index / COLUMN_SIZE;
            vec.Y = index % COLUMN_SIZE;
        }

        private static float BlendVertices(params float[] heights)
        {
            float max = float.MinValue;
            float min = float.MaxValue;

            for(int i =0; i < heights.Length; i++)
            {
                max = heights[i] > max ? heights[i] : max;
                min = heights[i] < min ? heights[i] : min;
            }

            float mixPercent = 0.5f;

            float blend = MathHelper.Lerp(min, max, mixPercent);

            return blend;
        }

        private static void BlendTextures(MeshTile a, int aVertex, MeshTile b, int bVertex)
        {
            //if (a.Weights[^1] > b.Weights[^1])
            //{
            //    b.CopyTextureInfoForVertex(bVertex, a, aVertex);
            //}
            //else
            //{
            //    a.CopyTextureInfoForVertex(aVertex, b, bVertex);
            //}
        }
    }
}
