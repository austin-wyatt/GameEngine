using Empyrean.Engine_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Empyrean.Game.Tiles.Meshes
{
    public enum PaletteLocation
    {
        None = -1,
        Red1,
        //Red2,
        Green1,
        //Green2,
        Blue1,
        //Blue2,
    }

    public class BlendMap
    {
        public DirectBitmap DirectBitmap;
        public Texture Texture;
        public TileChunk ChunkHandle;

        public const int WIDTH = 300;
        public const int HEIGHT = 352;

        public const int X_OVERLAP = 8;
        public const int Y_OVERLAP = 16;

        //public const int WIDTH = 248;
        //public const int HEIGHT = 291;

        //public const int X_OVERLAP = 7;
        //public const int Y_OVERLAP = 14;

        public const int HEIGHT_NO_OVERLAP = HEIGHT - Y_OVERLAP;
        public const int WIDTH_NO_OVERLAP = WIDTH - X_OVERLAP;

        private static ObjectPool<BlendMap> Pool = new ObjectPool<BlendMap>(1000);

        public TileType Background = TileType.Grass;

        public TileType[] Palette = new TileType[]
        {
            TileType.None, //R
            //TileType.None,
            TileType.None, //G
            //TileType.None,
            TileType.None, //B
            //TileType.None,
        };

        public BlendMap() { }

        public BlendMap(TileChunk chunk)
        {
            ChunkHandle = chunk;

            DirectBitmap = new DirectBitmap(WIDTH, HEIGHT);

            Color alpha = Color.FromArgb(255, 0, 0, 0);

            for (int i = 0; i < WIDTH; i++)
            {
                for(int j = 0; j < HEIGHT; j++)
                {
                    DirectBitmap.SetPixel(i, j, alpha);
                }
            }

            if(Thread.CurrentThread.ManagedThreadId == WindowConstants.MainThreadId)
            {
                Texture = Texture.LoadFromDirectBitmap(DirectBitmap, true, generateMipMaps: false);
            }
            else
            {
                Window.QueueToRenderCycle(() =>
                {
                    Texture = Texture.LoadFromDirectBitmap(DirectBitmap, true, generateMipMaps: false);
                });
            }
        }

        ~BlendMap()
        {
            Dispose();
        }

        private void Initialize(TileChunk chunk)
        {
            ChunkHandle = chunk;

            Color alpha = Color.FromArgb(255, 0, 0, 0);

            for (int i = 0; i < WIDTH; i++)
            {
                for (int j = 0; j < HEIGHT; j++)
                {
                    DirectBitmap.SetPixel(i, j, alpha);
                }
            }

            UpdateTexture();
        }

        public static BlendMap GetBlendMap(TileChunk chunk)
        {
            BlendMap map;

            if(Pool.Count == 0)
            {
                map = new BlendMap(chunk);
            }
            else
            {
                map = Pool.GetObject();
                map.Initialize(chunk);
            }

            return map;
        }

        public void UpdateTexture()
        {
            if (Thread.CurrentThread.ManagedThreadId == WindowConstants.MainThreadId)
            {
                Texture.UpdateFromBitmap(DirectBitmap.Bitmap);
            }
            else
            {
                Window.QueueToRenderCycle(() =>
                {
                    Texture.UpdateFromBitmap(DirectBitmap.Bitmap);
                });
            }
        }

        private void Dispose()
        {
            DirectBitmap.Dispose();

            if (Thread.CurrentThread.ManagedThreadId == WindowConstants.MainThreadId)
            {
                Texture.DisposeImmediate();
            }
            else
            {
                Window.QueueToRenderCycle(Texture.DisposeImmediate);
            }
        }

        public static void CleanUp(BlendMap map)
        {
            Pool.FreeObject(ref map);
        }

        public PaletteLocation AddOrGetTypePaletteLocation(TileType type)
        {
            int emptyLocationIndex = -1;

            for(int i = 0; i < Palette.Length; i++)
            {
                if(Palette[i] == type)
                {
                    return (PaletteLocation)i;
                }
                else if(emptyLocationIndex == -1 && Palette[i] == TileType.None)
                {
                    emptyLocationIndex = i;
                }
            }

            if(emptyLocationIndex != -1)
            {
                Palette[emptyLocationIndex] = type;
                return (PaletteLocation)emptyLocationIndex;
            }

            return PaletteLocation.None;
        }

        public void RemoveTypeFromPalette(TileType type)
        {
            for (int i = 0; i < Palette.Length; i++)
            {
                if (Palette[i] == type)
                {
                    Palette[i] = TileType.None;
                    return;
                }
            }
        }
    }
}
