using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using Empyrean.Game.Tiles.Meshes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace Empyrean.Definitions.BlendControls
{

    [Serializable]
    public class ImageBlendControl : BlendControl
    {
        public string ImagePath;

        /// <summary>
        /// The ratio of blend control image pixels to chunk blend map pixels. <para/>
        /// Ex. a ScaleFactor of 5 would mean that every blend control image pixel would apply to 5 chunk blend map pixels
        /// </summary>
        public int ScaleFactor = 1;

        public override void ApplyControl()
        {
            Bitmap map = new Bitmap(ImagePath);
            BitmapData bitmapData = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            BlendPoint originPoint = BlendHelper.GetBlendPointFromFeaturePoint(Origin);
            BlendPoint point = new BlendPoint();
            BlendPoint localPoint = new BlendPoint();

            int width = map.Width / 2;
            int height = map.Height / 2;

            TileChunk chunk = BlendHelper.GetChunkFromBlendPoint(originPoint);

            HashSet<TileChunk> usedChunks = new HashSet<TileChunk>();

            if (chunk != null)
            {
                usedChunks.Add(chunk);
            }

            unsafe
            {

                int r;
                int g;
                int b;
                int a;

                int finalR;
                int finalG;
                int finalB;

                PaletteLocation loc;
                float offset;

                float divideFactor;
                const float RECIPROCAL_255 = 1 / 255f;

                byte* bitmapPoint = (byte*)bitmapData.Scan0.ToPointer();

                int mapWidth = map.Width;
                int mapHeight = map.Height;

                bool valid;

                bool getValidChunk()
                {
                    chunk = BlendHelper.GetChunkFromBlendPoint(point);

                    if (chunk != null)
                    {
                        usedChunks.Add(chunk);
                        localPoint = BlendHelper.ConvertGlobalToLocalBlendPoint(point, chunk);

                        return true;
                    }

                    return false;
                }

                HashSet<TileChunk> chunksList = new HashSet<TileChunk>();

                for (int i = 0; i < mapHeight; i++)
                {
                    for (int j = 0; j < mapWidth; j++)
                    {
                        for (int k = 0; k < ScaleFactor; k++)
                        {
                            for (int l = 0; l < ScaleFactor; l++)
                            {
                                b = *(bitmapPoint + i * mapWidth * 4 + j * 4);
                                g = *(bitmapPoint + i * mapWidth * 4 + j * 4 + 1);
                                r = *(bitmapPoint + i * mapWidth * 4 + j * 4 + 2);
                                a = *(bitmapPoint + i * mapWidth * 4 + j * 4 + 3);

                                if (a == 0) continue;

                                //this is how much background should be displayed
                                a = 255 - a; 

                                //r = Red == TileType.None ? 0 : r;
                                //g = Green == TileType.None ? 0 : g;
                                //b = Blue == TileType.None ? 0 : b;

                                divideFactor = 1 / (RECIPROCAL_255 * (b + g + r + a));

                                b = (int)(b * divideFactor);
                                g = (int)(g * divideFactor);
                                r = (int)(r * divideFactor);
                                a = (int)(a * divideFactor);

                                point.X = originPoint.X + j * ScaleFactor - width * ScaleFactor + k;
                                point.Y = originPoint.Y + i * ScaleFactor - height * ScaleFactor + l;

                                //point.X = originPoint.X + j;
                                //point.Y = originPoint.Y + i;

                                if (chunk != null)
                                {
                                    localPoint = BlendHelper.ConvertGlobalToLocalBlendPoint(point, chunk);
                                }
                                else
                                {
                                    if (!getValidChunk()) continue;
                                }

                                valid = localPoint.IsValidBoundsOnly();

                                if (!valid)
                                {
                                    if (!getValidChunk()) continue;
                                }


                                BlendHelper.GetChunksFromBlendPoint(point, in chunksList);

                                foreach (var foundChunk in chunksList)
                                {
                                    usedChunks.Add(foundChunk);

                                    BlendPoint newPoint = BlendHelper.ConvertGlobalToLocalBlendPoint(point, foundChunk);

                                    if (newPoint.IsValidBoundsOnly())
                                    {
                                        Color oldColor = foundChunk.BlendMap.DirectBitmap.GetPixel(newPoint.X, newPoint.Y);

                                        finalR = 0;
                                        finalG = 0;
                                        finalB = 0;

                                        #region Color mapping from chunk
                                        if (Red != TileType.None)
                                        {
                                            loc = foundChunk.BlendMap.AddOrGetTypePaletteLocation(Red);
                                            SetColorByLocation(ref finalR, ref finalG, ref finalB, r, loc);
                                        }

                                        if (Green != TileType.None)
                                        {
                                            loc = foundChunk.BlendMap.AddOrGetTypePaletteLocation(Green);
                                            SetColorByLocation(ref finalR, ref finalG, ref finalB, g, loc);
                                        }

                                        if (Blue != TileType.None)
                                        {
                                            loc = foundChunk.BlendMap.AddOrGetTypePaletteLocation(Blue);
                                            SetColorByLocation(ref finalR, ref finalG, ref finalB, b, loc);
                                        }
                                        #endregion


                                        Color col = Color.FromArgb(a, finalR, finalG, finalB);

                                        foundChunk.BlendMap.DirectBitmap.SetPixel(newPoint.X, newPoint.Y, col);
                                    }
                                }

                                chunksList.Clear();
                            }
                        }
                    }
                }
            }
            map.UnlockBits(bitmapData);
            map.Dispose();

            foreach (var usedChunk in usedChunks)
            {
                usedChunk.BlendMap.UpdateTexture();
            }
        }
    }
}
