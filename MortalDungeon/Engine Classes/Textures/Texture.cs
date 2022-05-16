using Empyrean.Engine_Classes.Rendering;
using Empyrean.Game.Tiles;
using Empyrean.Objects;
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

namespace Empyrean.Engine_Classes
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

    public enum TextureWrapType
    {
        Repeat,
        ClampToEdge,
    }

    public class Texture
    {
        public readonly int Handle;
        public BitmapImageData ImageData = null;
        public int TextureId = (int)TextureName.Unknown;

        public static Dictionary<TextureUnit, int> UsedTextures = new Dictionary<TextureUnit, int>();

        private bool GenerateMipmaps;
        private bool Nearest;

        public Texture() { }
        public static Texture LoadFromFile(string path, bool nearest = true, int name = 0, bool generateMipMaps = true, 
            TextureWrapType wrapType = TextureWrapType.Repeat)
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // Generate handle
            int handle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            // Bind the handle
            //GL.ActiveTexture(TextureUnit.Texture0);
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

            switch (wrapType)
            {
                case TextureWrapType.Repeat:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    break;
                case TextureWrapType.ClampToEdge:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    break;
            }
            

            Texture tex = new Texture(handle, name);

            lock (_loadedTexturesLock)
            {
                LoadedTextures.Add(tex);
            }

            return tex;
        }

        public static Texture LoadFromBitmap(Bitmap bitmap, bool nearest = true, int name = (int)TextureName.Unknown, bool generateMipMaps = true,
            TextureWrapType wrapType = TextureWrapType.Repeat)
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            //GL.ActiveTexture(TextureUnit.Texture0);
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
                if (nearest)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }
                
            }

            switch (wrapType)
            {
                case TextureWrapType.Repeat:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    break;
                case TextureWrapType.ClampToEdge:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    break;
            }

            Texture tex = new Texture(handle, name);

            tex.GenerateMipmaps = generateMipMaps;
            tex.Nearest = nearest;

            lock (_loadedTexturesLock)
            {
                LoadedTextures.Add(tex);
            }

            return tex;
        }

        public static Texture LoadFromDirectBitmap(DirectBitmap bitmap, bool nearest = true, int name = (int)TextureName.Unknown, bool generateMipMaps = true)
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            //GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);

            //var data = bitmap.LockBits(
            //    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            //    ImageLockMode.ReadOnly,
            //    System.Drawing.Imaging.PixelFormat.Format32bppArgb);


            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                bitmap.Width,
                bitmap.Height,
                0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                bitmap.Bits);

            //bitmap.UnlockBits(data);

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
                if (nearest)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }

            }

            float[] color = new float[] { 0, 0, 0, 0 };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, color);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            Texture tex = new Texture(handle, name);

            tex.GenerateMipmaps = generateMipMaps;
            tex.Nearest = nearest;

            lock (_loadedTexturesLock)
            {
                LoadedTextures.Add(tex);
            }

            return tex;
        }

        public static Texture LoadFromArray(float[] data, Vector2i imageDimensions, bool nearest = true, int name = (int)TextureName.Unknown)
        {
            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            //GL.ActiveTexture(TextureUnit.Texture0);
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

            lock (_loadedTexturesLock)
            {
                LoadedTextures.Add(tex);
            }

            return tex;
        }

        public void UpdateFromBitmap(Bitmap bitmap)
        {
            GL.BindTexture(TextureTarget.Texture2D, Handle);

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

            if (GenerateMipmaps)
            {
                if (Nearest)
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
                if (Nearest)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }

            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        public void UpdateFromDirectBitmap(DirectBitmap bitmap)
        {
            GL.BindTexture(TextureTarget.Texture2D, Handle);


            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                bitmap.Width,
                bitmap.Height,
                0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                bitmap.Bits);

            if (GenerateMipmaps)
            {
                if (Nearest)
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
                if (Nearest)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                }

            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        public static HashSet<Texture> LoadedTextures = new HashSet<Texture>();
        public static object _loadedTexturesLock = new object();

        private static int[] HandlesToDispose = new int[500];
        private static int _handlesToDisposeLength = 0;

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

        public static void Use(TextureUnit unit, int handle)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }

        private void _Dispose()
        {
            lock (_loadedTexturesLock)
            {
                LoadedTextures.Remove(this);

                //GL.DeleteTexture(Handle);

                HandlesToDispose[_handlesToDisposeLength] = Handle;

                _handlesToDisposeLength++;
                if (_handlesToDisposeLength >= 500)
                {
                    GL.DeleteTextures(_handlesToDisposeLength, HandlesToDispose);
                    _handlesToDisposeLength = 0;
                }
            }
        }
        public void Dispose()
        {
            //Window.QueueToRenderCycle(_Dispose);
            
            if(Thread.CurrentThread.ManagedThreadId == WindowConstants.MainThreadId)
            {
                DisposeImmediate();
            }
            else
            {
                Window.QueueToRenderCycle(DisposeImmediate);
            }
        }

        public void DisposeImmediate()
        {
            lock (_loadedTexturesLock)
            {
                LoadedTextures.Remove(this);

                GL.DeleteTexture(Handle);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Texture texture &&
                   Handle == texture.Handle;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Handle);
        }
    }
}