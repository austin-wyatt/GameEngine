using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace MortalDungeon.Engine_Classes
{
    public class BitmapImageData 
    {
        public float[] ImageData;
        public Vector2i ImageDimensions;

        public BitmapImageData(float[] imgData, Vector2i dimensions) 
        {
            ImageData = imgData;
            ImageDimensions = dimensions;
        }
        public BitmapImageData() { }
    }

    public class Texture
    {
        public readonly int Handle;
        public BitmapImageData ImageData = null;
        public int TextureId = (int)TextureName.Unknown;

        public static Dictionary<TextureUnit, int> UsedTextures = new Dictionary<TextureUnit, int>();

        public Texture() { }
        public static Texture LoadFromFile(string path, bool nearest = true, int name = 0, bool generateMipMaps = true)
        {
            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            using (var image = new Bitmap(path))
            {
                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                
                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);
            }

            if (generateMipMaps)
            {

                if (nearest)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }


                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
            else 
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            Texture tex = new Texture(handle, name);

            return tex;
        }

        public static Texture LoadFromBitmap(Bitmap bitmap, bool nearest = true, int name = (int)TextureName.Unknown, bool generateMipMaps = true)
        {
            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);


            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                bitmap.Width,
                bitmap.Height,
                0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);

            bitmap.UnlockBits(data);

            if (generateMipMaps)
            {
                if (nearest)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            Texture tex = new Texture(handle, name);

            return tex;
        }

        public static Texture LoadFromArray(float[] data, Vector2i imageDimensions, bool nearest = true, int name = (int)TextureName.Unknown)
        {
            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                imageDimensions.X,
                imageDimensions.Y,
                0,
                PixelFormat.Rgba,
                PixelType.Float,
                data);

            if (nearest)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            Texture tex = new Texture(handle, name)
            {
                ImageData = new BitmapImageData(data, imageDimensions)
            };


            return tex;
        }

        //public void UpdateTextureArray(Vector2i minBounds, Vector2i maxBounds, TileMap tileMap) 
        //{
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Restart();

        //    Console.WriteLine("Beginning texture array update.");

        //    GL.ActiveTexture(TextureUnit.Texture0);
        //    GL.BindTexture(TextureTarget.Texture2D, Handle);


        //    GL.BindBuffer(BufferTarget.PixelUnpackBuffer, tileMap.DynamicTextureInfo.PixelBufferObject);

        //    unsafe
        //    {
        //        float* temp = (float*)GL.MapBuffer(BufferTarget.PixelUnpackBuffer, BufferAccess.ReadWrite);

        //        if (temp != null)
        //        {
        //            TileTexturer.UpdateTexture(tileMap, temp);
        //        }
        //        GL.UnmapBuffer(BufferTarget.PixelUnpackBuffer);
        //    }


        //    GL.TexSubImage2D(TextureTarget.Texture2D,
        //        0,
        //        0,
        //        0,
        //        ImageData.ImageDimensions.X,
        //        ImageData.ImageDimensions.Y,
        //        PixelFormat.Rgba,
        //        PixelType.Float, new IntPtr());


        //    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        //    Console.WriteLine(stopwatch.ElapsedMilliseconds);

        //    GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
        //}

        public Texture(int glHandle, int name)
        {
            Handle = glHandle;
            TextureId = name;
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            UsedTextures[unit] = TextureId;
        }


        private void _Dispose()
        {
            GL.DeleteTexture(Handle);
            Window.RenderEnd -= _Dispose;
        }
        public void Dispose()
        {
            Window.RenderEnd -= _Dispose;
            Window.RenderEnd += _Dispose;
        }
    }
}