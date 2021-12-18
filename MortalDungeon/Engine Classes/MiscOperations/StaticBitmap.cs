using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MortalDungeon.Engine_Classes.MiscOperations
{
    internal class StaticBitmap : IDisposable
    {
        internal Bitmap Bitmap { get; private set; }
        internal int[] Bits { get; private set; }
        internal bool Disposed { get; private set; }
        internal int Height { get; private set; }
        internal int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        internal StaticBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new int[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
        }

        internal void SetPixel(int x, int y, System.Drawing.Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            Bits[index] = col;
        }

        internal System.Drawing.Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            System.Drawing.Color result = System.Drawing.Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
