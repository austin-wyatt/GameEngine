using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.Rendering
{
    internal class FrameBufferObject
    {
        internal int FrameBuffer;
        internal int RenderTexture;
        internal int DepthBuffer;

        internal IntPtr _texturePointer;

        internal Vector2i FBODimensions;

        internal Shader Shader;
        internal FrameBufferObject(Vector2i dimensions = default, Shader shader = null) 
        {
            if (dimensions.X == 0)
            {
                FBODimensions = new Vector2i(WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y);
            }
            else 
            {
                FBODimensions = new Vector2i(dimensions.X, dimensions.Y);
            }

            CreateFrameBuffer();


            FramebufferStatus err = GL.CheckNamedFramebufferStatus(FrameBuffer, FramebufferTarget.Framebuffer);
            if (err != FramebufferStatus.FramebufferComplete)
            {
                Console.WriteLine(err);
            }

            if (shader == null)
            {
                Shader = Shaders.SIMPLE_SHADER; //no transformations
            }
            else 
            {
                Shader = shader;
            }
        }

        /// <summary>
        /// Scales the texture and depth buffer associated with the frame buffer to the new width and height
        /// </summary>
        /// <param name="newSize"></param>
        internal void ResizeFBO(Vector2i newSize) 
        {
            FBODimensions.X = (int)(FBODimensions.X * (float)newSize.X / FBODimensions.X);
            FBODimensions.Y = (int)(FBODimensions.Y * (float)newSize.Y / FBODimensions.Y);

            BindFrameBuffer();

            ResizeTexture();
            CreateDepthBuffer();

            UnbindFrameBuffer();
        }

        internal void BindFrameBuffer() 
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer);
        }

        internal void UnbindFrameBuffer() 
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>
        /// Color buffer should be cleared before subsequent uses of the frame buffer
        /// </summary>
        /// <param name="active"></param>
        internal void ClearColorBuffer(bool active = false, Vector4 color = default) 
        {
            GL.ClearColor(color.X, color.Y, color.Z, color.W);

            if (!active)
            {
                BindFrameBuffer();
                //GL.ClearBuffer(ClearBuffer.Color, 0, new float[1]);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                UnbindFrameBuffer();
            }
            else 
            {
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[1]);
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }

            GL.ClearColor(Renderer.ClearColor.X, Renderer.ClearColor.Y, Renderer.ClearColor.Z, Renderer.ClearColor.W);
        }

        private void CreateTexture() 
        {
            if (RenderTexture != 0)
            {
                GL.DeleteTexture(RenderTexture);
            }

            RenderTexture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, RenderTexture);

            _texturePointer = new IntPtr();
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, FBODimensions.X, FBODimensions.Y, 0, PixelFormat.Rgba, PixelType.Float, _texturePointer);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, RenderTexture, 0);
        }

        private void ResizeTexture() 
        {
            _texturePointer = new IntPtr();
            GL.BindTexture(TextureTarget.Texture2D, RenderTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, FBODimensions.X, FBODimensions.Y, 0, PixelFormat.Rgba, PixelType.Float, _texturePointer);
        }


        //TODO, this might need to be looked at later when post processing techniques begin to be incorporated
        internal void CreateDepthBuffer() 
        {
            //if (DepthBuffer != 0)
            //{
            //    GL.DeleteRenderbuffer(DepthBuffer);
            //}

            //DepthBuffer = GL.GenRenderbuffer();

            //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthBuffer);
            //GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, FBODimensions.X, FBODimensions.Y);
            //GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.RenderbufferExt, DepthBuffer);
        }

        internal void CreateFrameBuffer() 
        {
            if (FrameBuffer != 0) 
            {
                GL.DeleteFramebuffer(FrameBuffer);
            }
            FrameBuffer = GL.GenFramebuffer();

            BindFrameBuffer();

            CreateTexture();
            CreateDepthBuffer();

            UnbindFrameBuffer();
        }

        internal void ClearBuffers() 
        {
            BindFrameBuffer();
            GL.Clear(ClearBufferMask.DepthBufferBit);
            UnbindFrameBuffer();
        }

        /// <summary>
        /// Deletes the depth buffer, texture, and the render buffer from the GPU
        /// </summary>
        internal void Dispose() 
        {
            if (DepthBuffer != 0)
            {
                GL.DeleteRenderbuffer(DepthBuffer);
            }
            if (RenderTexture != 0) 
            {
                GL.DeleteTexture(RenderTexture);
            }
            
            GL.DeleteFramebuffer(FrameBuffer);
        }
    }
}
