using Empyrean.Engine_Classes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Tiles.Meshes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Map
{
    public static class BlendHelper
    {
        public static BlendPoint GetBlendPointFromChunk(TileChunk chunk)
        {
            //TEMP, 0 based 
            //Vector2i tileMapOffsetFromTopLeft = chunk.TileMap.TileMapCoords - TileMapHelpers._topLeftMap.TileMapCoords;
            //Vector2i chunkOffsetFromTopLeft = new Vector2i(tileMapOffsetFromTopLeft.X * TileMap.ChunksPerTileMap.X + chunk.ChunkPosition.X,
            //    tileMapOffsetFromTopLeft.Y * TileMap.ChunksPerTileMap.Y + chunk.ChunkPosition.Y);

            Vector2i chunkOffsetFromTopLeft = new Vector2i(chunk.TileMap.TileMapCoords.X * TileMap.ChunksPerTileMap.X + chunk.ChunkPosition.X,
                 chunk.TileMap.TileMapCoords.Y * TileMap.ChunksPerTileMap.Y + chunk.ChunkPosition.Y);

            int blendCoordX = chunkOffsetFromTopLeft.X * BlendMap.WIDTH_NO_OVERLAP;

            int blendCoordY = chunkOffsetFromTopLeft.Y * BlendMap.HEIGHT_NO_OVERLAP;

            return new BlendPoint(blendCoordX, blendCoordY);
        }

        public static BlendPoint GetBlendPointFromChunk(ref Vector2i chunkCoords)
        {
            int blendCoordX = chunkCoords.X * BlendMap.WIDTH_NO_OVERLAP;

            int blendCoordY = chunkCoords.Y * BlendMap.HEIGHT_NO_OVERLAP;

            return new BlendPoint(blendCoordX, blendCoordY);
        }

        public static TileChunk GetChunkFromBlendPoint(BlendPoint point)
        {
            TileMapPoint mapPoint = new TileMapPoint();

            Vector2i chunkPoint = new Vector2i();

            chunkPoint.X = (int)((float)point.X / BlendMap.WIDTH_NO_OVERLAP);
            chunkPoint.Y = (int)((float)point.Y / BlendMap.HEIGHT_NO_OVERLAP);

            BlendPoint localCoords = point - GetBlendPointFromChunk(ref chunkPoint);
            BlendPoint tempCoords;
            Vector2i tempChunkPoint = new Vector2i();

            //check if the coordinate is valid in the calculated chunk
            if (localCoords.IsValid())
            {
                goto END;
            }

            #region diagonal overlap checks
            //top left overlap
            if (localCoords.X <= BlendMap.X_OVERLAP && localCoords.Y <= BlendMap.Y_OVERLAP)
            {
                tempChunkPoint.X = chunkPoint.X - 1;
                tempChunkPoint.Y = chunkPoint.Y - 1;
                tempCoords = point - GetBlendPointFromChunk(ref tempChunkPoint);
                //check
                if (tempCoords.IsValid())
                {
                    chunkPoint = tempChunkPoint;
                    goto END;
                }
            }

            //bottom left overlap
            if (localCoords.X <= BlendMap.X_OVERLAP && localCoords.Y >= BlendMap.HEIGHT_NO_OVERLAP)
            {
                tempChunkPoint.X = chunkPoint.X - 1;
                tempChunkPoint.Y = chunkPoint.Y + 1;
                tempCoords = point - GetBlendPointFromChunk(ref tempChunkPoint);
                //check
                if (tempCoords.IsValid())
                {
                    chunkPoint = tempChunkPoint;
                    goto END;
                }
            }

            //bottom right overlap
            if (localCoords.X >= BlendMap.WIDTH_NO_OVERLAP && localCoords.Y >= BlendMap.HEIGHT_NO_OVERLAP)
            {
                tempChunkPoint.X = chunkPoint.X + 1;
                tempChunkPoint.Y = chunkPoint.Y + 1;
                tempCoords = point - GetBlendPointFromChunk(ref tempChunkPoint);
                //check
                if (tempCoords.IsValid())
                {
                    chunkPoint = tempChunkPoint;
                    goto END;
                }
            }

            //top right overlap
            if (localCoords.X >= BlendMap.WIDTH_NO_OVERLAP && localCoords.Y <= BlendMap.Y_OVERLAP)
            {
                tempChunkPoint.X = chunkPoint.X + 1;
                tempChunkPoint.Y = chunkPoint.Y - 1;
                tempCoords = point - GetBlendPointFromChunk(ref tempChunkPoint);
                //check
                if (tempCoords.IsValid())
                {
                    chunkPoint = tempChunkPoint;
                    goto END;
                }
            }
            #endregion
            #region orthogonal overlap checks
            //Overlap on left side
            if (localCoords.X <= BlendMap.X_OVERLAP)
            {
                tempChunkPoint.X = chunkPoint.X - 1;
                tempChunkPoint.Y = chunkPoint.Y;
                tempCoords = point - GetBlendPointFromChunk(ref tempChunkPoint);
                //check
                if (tempCoords.IsValid())
                {
                    chunkPoint = tempChunkPoint;
                    goto END;
                }
            }

            //Overlap on right side
            if (localCoords.X >= BlendMap.WIDTH_NO_OVERLAP)
            {
                tempChunkPoint.X = chunkPoint.X + 1;
                tempChunkPoint.Y = chunkPoint.Y;
                tempCoords = point - GetBlendPointFromChunk(ref tempChunkPoint);
                //check
                if (tempCoords.IsValid())
                {
                    chunkPoint = tempChunkPoint;
                    goto END;
                }
            }

            //Overlap on the top
            if (localCoords.Y <= BlendMap.Y_OVERLAP)
            {
                tempChunkPoint.X = chunkPoint.X;
                tempChunkPoint.Y = chunkPoint.Y - 1;
                tempCoords = point - GetBlendPointFromChunk(ref tempChunkPoint);
                //check
                if (tempCoords.IsValid())
                {
                    chunkPoint = tempChunkPoint;
                    goto END;
                }
            }

            //Overlap on the bottom
            if (localCoords.Y >= BlendMap.HEIGHT_NO_OVERLAP)
            {
                tempChunkPoint.X = chunkPoint.X;
                tempChunkPoint.Y = chunkPoint.Y + 1;
                tempCoords = point - GetBlendPointFromChunk(ref tempChunkPoint);
                //check
                if (tempCoords.IsValid())
                {
                    chunkPoint = tempChunkPoint;
                    goto END;
                }
            }
            #endregion

            END:
            mapPoint.X = (int)Math.Round((float)chunkPoint.X / TileMap.ChunksPerTileMap.X, MidpointRounding.ToNegativeInfinity);
            mapPoint.Y = (int)Math.Round((float)chunkPoint.Y / TileMap.ChunksPerTileMap.Y, MidpointRounding.ToNegativeInfinity);

            if (TileMapManager.LoadedMaps.TryGetValue(mapPoint, out var foundMap))
            {
                return foundMap.GetChunkAtPosition(Math.Abs(chunkPoint.X) % TileMap.ChunksPerTileMap.X, Math.Abs(chunkPoint.Y) % TileMap.ChunksPerTileMap.Y);
            }
            return null;
        }

        public static void GetChunksFromBlendPoint(BlendPoint point, in HashSet<TileChunk> tileChunks)
        {
            TileMapPoint mapPoint = new TileMapPoint();

            Vector2i chunkPoint = new Vector2i();

            chunkPoint.X = (int)Math.Floor((float)point.X / BlendMap.WIDTH_NO_OVERLAP);
            chunkPoint.Y = (int)Math.Floor((float)point.Y / BlendMap.HEIGHT_NO_OVERLAP);

            BlendPoint localCoords = point - GetBlendPointFromChunk(ref chunkPoint);
            BlendPoint tempCoords;
            Vector2i tempChunkPoint = new Vector2i();


            TileChunk chunk;

            void addChunk(HashSet<TileChunk> chunks, ref Vector2i point)
            {
                chunk = getChunkFromChunkPoint(ref point);

                if (chunk != null)
                {
                    chunks.Add(getChunkFromChunkPoint(ref point));
                }
            }

            //check if the coordinate is valid in the calculated chunk
            if (localCoords.IsValidBoundsOnly())
            {
                addChunk(tileChunks, ref chunkPoint);
            }

            if (!CheckPointOnOverlapEdge(localCoords)) return;

            bool leftOverlap = localCoords.X <= BlendMap.X_OVERLAP;
            bool topOverlap = localCoords.Y <= BlendMap.Y_OVERLAP;
            bool bottomOverlap = localCoords.Y >= BlendMap.HEIGHT_NO_OVERLAP;
            bool rightOverlap = localCoords.X >= BlendMap.WIDTH_NO_OVERLAP;

            #region diagonal overlap checks
            //top left overlap
            if (leftOverlap && topOverlap)
            {
                tempChunkPoint.X = chunkPoint.X - 1;
                tempChunkPoint.Y = chunkPoint.Y - 1;
                tempCoords = GetBlendPointFromChunk(ref tempChunkPoint);
                tempCoords.X = point.X - tempCoords.X;
                tempCoords.Y = tempCoords.Y - point.Y;
                //check
                if (tempCoords.IsValidBoundsOnly())
                {
                    addChunk(tileChunks, ref tempChunkPoint);
                }
            }

            //bottom left overlap
            if (leftOverlap && bottomOverlap)
            {
                tempChunkPoint.X = chunkPoint.X - 1;
                tempChunkPoint.Y = chunkPoint.Y + 1;
                tempCoords = point - GetBlendPointFromChunk(ref tempChunkPoint);
                //check
                if (tempCoords.IsValidBoundsOnly())
                {
                    addChunk(tileChunks, ref tempChunkPoint);
                }
            }

            //bottom right overlap
            if (rightOverlap && bottomOverlap)
            {
                tempChunkPoint.X = chunkPoint.X + 1;
                tempChunkPoint.Y = chunkPoint.Y + 1;
                tempCoords = GetBlendPointFromChunk(ref tempChunkPoint);
                tempCoords.X = tempCoords.X - point.X;
                tempCoords.Y = point.Y - tempCoords.Y;
                //check
                if (tempCoords.IsValidBoundsOnly())
                {
                    addChunk(tileChunks, ref tempChunkPoint);
                }
            }

            //top right overlap
            if (rightOverlap && topOverlap)
            {
                tempChunkPoint.X = chunkPoint.X + 1;
                tempChunkPoint.Y = chunkPoint.Y - 1;
                tempCoords = GetBlendPointFromChunk(ref tempChunkPoint);
                tempCoords.X = tempCoords.X - point.X;
                tempCoords.Y = tempCoords.Y - point.Y;
                //check
                if (tempCoords.IsValidBoundsOnly())
                {
                    addChunk(tileChunks, ref tempChunkPoint);
                }
            }
            #endregion
            #region orthogonal overlap checks
            //Overlap on left side
            if (leftOverlap)
            {
                tempChunkPoint.X = chunkPoint.X - 1;
                tempChunkPoint.Y = chunkPoint.Y;
                tempCoords = point - GetBlendPointFromChunk(ref tempChunkPoint);
                //check
                if (tempCoords.IsValidBoundsOnly())
                {
                    addChunk(tileChunks, ref tempChunkPoint);
                }
            }

            //Overlap on right side
            if (rightOverlap)
            {
                tempChunkPoint.X = chunkPoint.X + 1;
                tempChunkPoint.Y = chunkPoint.Y;
                tempCoords = GetBlendPointFromChunk(ref tempChunkPoint);
                tempCoords.X = tempCoords.X - point.X;
                tempCoords.Y = point.Y - tempCoords.Y;
                //check
                if (tempCoords.IsValidBoundsOnly())
                {
                    addChunk(tileChunks, ref tempChunkPoint);
                }
            }

            //Overlap on the top
            if (topOverlap)
            {
                tempChunkPoint.X = chunkPoint.X;
                tempChunkPoint.Y = chunkPoint.Y - 1;
                tempCoords = GetBlendPointFromChunk(ref tempChunkPoint);
                tempCoords.X = point.X - tempCoords.X;
                tempCoords.Y = point.Y - tempCoords.Y;
                //check
                if (tempCoords.IsValidBoundsOnly())
                {
                    addChunk(tileChunks, ref tempChunkPoint);
                }
            }

            //Overlap on the bottom
            if (bottomOverlap)
            {
                tempChunkPoint.X = chunkPoint.X;
                tempChunkPoint.Y = chunkPoint.Y + 1;
                tempCoords = GetBlendPointFromChunk(ref tempChunkPoint);
                tempCoords.X = point.X - tempCoords.X;
                tempCoords.Y = tempCoords.Y - point.Y;
                //check
                if (tempCoords.IsValidBoundsOnly())
                {
                    addChunk(tileChunks, ref tempChunkPoint);
                }
            }
            #endregion

            TileChunk getChunkFromChunkPoint(ref Vector2i chunkP)
            {
                mapPoint.X = (int)Math.Round((float)chunkP.X / TileMap.ChunksPerTileMap.X, MidpointRounding.ToNegativeInfinity);
                mapPoint.Y = (int)Math.Round((float)chunkP.Y / TileMap.ChunksPerTileMap.Y, MidpointRounding.ToNegativeInfinity);

                if (TileMapManager.LoadedMaps.TryGetValue(mapPoint, out var foundMap))
                {
                    return foundMap.GetChunkAtPosition(Math.Abs(chunkP.X) % TileMap.ChunksPerTileMap.X, Math.Abs(chunkP.Y) % TileMap.ChunksPerTileMap.Y);
                }
                return null;
            }
        }

        public static bool CheckPointOnOverlapEdge(BlendPoint localCoords)
        {
            return localCoords.X <= BlendMap.X_OVERLAP || localCoords.Y <= BlendMap.Y_OVERLAP
                || localCoords.X >= BlendMap.WIDTH_NO_OVERLAP || localCoords.Y >= BlendMap.HEIGHT_NO_OVERLAP;
        }

        public static BlendPoint GetBlendPointFromFeaturePoint(FeaturePoint point)
        {
            Vector2i chunkPos = point.ToChunkPosition();

            TileMapPoint mapPoint = point.ToTileMapPoint();

            chunkPos.X = mapPoint.X * TileMap.ChunksPerTileMap.X + chunkPos.X;
            chunkPos.Y = mapPoint.Y * TileMap.ChunksPerTileMap.Y + chunkPos.Y;

            int blendCoordX = chunkPos.X * BlendMap.WIDTH_NO_OVERLAP;

            int blendCoordY = chunkPos.Y * BlendMap.HEIGHT_NO_OVERLAP;

            const float BLEND_TILE_HEIGHT = 33.5f;
            const int BLEND_TILE_WIDTH = 39;

            int verticalTileOffset = (int)(point.X % 2 == 0 ? BLEND_TILE_HEIGHT : BLEND_TILE_HEIGHT * 0.5f);

            blendCoordX += (int)(BLEND_TILE_WIDTH * 0.5f);
            blendCoordX += (int)(GMath.NegMod(point.X, TileChunk.DefaultChunkWidth) * BLEND_TILE_WIDTH * 0.75f);

            blendCoordY += verticalTileOffset;
            blendCoordY += (int)Math.Round(GMath.NegMod(point.Y, TileChunk.DefaultChunkHeight) * BLEND_TILE_HEIGHT, MidpointRounding.ToZero);

            return new BlendPoint(blendCoordX, blendCoordY);
        }

        /// <summary>
        /// Take a global blend point and return a new blend point relative to the passed chunk.
        /// </summary>
        public static BlendPoint ConvertGlobalToLocalBlendPoint(BlendPoint globalPoint, TileChunk chunk)
        {
            BlendPoint chunkPoint = GetBlendPointFromChunk(chunk);
            globalPoint.X -= chunkPoint.X;
            globalPoint.Y -= chunkPoint.Y;

            //if (globalPoint.Y == BlendMap.HEIGHT - 1) globalPoint.Y = 0;
            //if (globalPoint.Y == 0) globalPoint.Y = BlendMap.HEIGHT - 1;

            return globalPoint;
        }

        public static BlendPoint ConvertGlobalToLocalBlendPoint(BlendPoint globalPoint, Vector2i chunk)
        {
            BlendPoint chunkPoint = GetBlendPointFromChunk(ref chunk);
            globalPoint.X -= chunkPoint.X;
            globalPoint.Y -= chunkPoint.Y;

            //if (globalPoint.Y == BlendMap.HEIGHT - 1) globalPoint.Y = 0;
            //if (globalPoint.Y == 0) globalPoint.Y = BlendMap.HEIGHT - 1;

            return globalPoint;
        }

        /// <summary>
        /// Take a local blend point and return a new global blend point.
        /// </summary>
        public static BlendPoint ConvertLocalToGlobalBlendPoint(BlendPoint local, TileChunk chunk)
        {
            BlendPoint chunkPoint = GetBlendPointFromChunk(chunk);
            local.X += chunkPoint.X;
            local.Y += chunkPoint.Y;

            return local;
        }

        public static BlendPoint ConvertLocalToGlobalBlendPoint(BlendPoint local, Vector2i chunk)
        {
            BlendPoint chunkPoint = GetBlendPointFromChunk(ref chunk);
            local.X += chunkPoint.X;
            local.Y += chunkPoint.Y;

            return local;
        }


        /// <summary>
        /// TODO, use arrays instead of hash sets for this. This would be done by getting the offset from the top left wall point to 0,0
        /// and then filling the wall array that spans from top left -> bottom right in size with the wall values. <para/>
        /// Output values could then simply be added to the wall array.
        /// </summary>
        public static bool FloodFill(in HashSet<BlendPoint> walls, ref HashSet<BlendPoint> foundPoints, BlendPoint seedPoint)
        {
            Stack<BlendPoint> pointStack = new Stack<BlendPoint>();

            pointStack.Push(seedPoint);
            foundPoints.Add(seedPoint);

            BlendPoint currPoint;

            BlendPoint tempPoint = new BlendPoint();
            BlendPoint newPoint;

            while (pointStack.Count > 0)
            {
                currPoint = pointStack.Pop();

                //TEMP
                if (pointStack.Count > 50000) return false;

                //check east
                tempPoint.X = currPoint.X + 1;
                tempPoint.Y = currPoint.Y;

                if(!(walls.Contains(tempPoint) || foundPoints.Contains(tempPoint)))
                {
                    newPoint = new BlendPoint(tempPoint);
                    pointStack.Push(newPoint);
                    foundPoints.Add(newPoint);
                }

                //check west
                tempPoint.X -= 2;
                if (!(walls.Contains(tempPoint) || foundPoints.Contains(tempPoint)))
                {
                    newPoint = new BlendPoint(tempPoint);
                    pointStack.Push(newPoint);
                    foundPoints.Add(newPoint);
                }

                //check north
                tempPoint.X += 1;
                tempPoint.Y += 1;
                if (!(walls.Contains(tempPoint) || foundPoints.Contains(tempPoint)))
                {
                    newPoint = new BlendPoint(tempPoint);
                    pointStack.Push(newPoint);
                    foundPoints.Add(newPoint);
                }

                //check south
                tempPoint.Y -= 2;
                if (!(walls.Contains(tempPoint) || foundPoints.Contains(tempPoint)))
                {
                    newPoint = new BlendPoint(tempPoint);
                    pointStack.Push(newPoint);
                    foundPoints.Add(newPoint);
                }
            }

            return true;
        }
    }
}
