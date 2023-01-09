using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Rendering
{
    /// <summary>
    /// A framebuffer set up for deferred shading
    /// </summary>
    public class GBuffer
    {
        public readonly int FramebufferHandle;

        /// <summary>
        /// Contains the positions at each fragment
        /// </summary>
        public readonly int PositionTextureHandle;
        /// <summary>
        /// Contains the normals at each fragment
        /// </summary>
        public readonly int NormalTextureHandle;
        /// <summary>
        /// Contains the colors at each fragment
        /// </summary>
        public readonly int ColorTextureHandle;

        public readonly int DepthTextureHandle;

        public GBuffer()
        {
            FramebufferHandle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferHandle);

            ColorTextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorTextureHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y, 0, PixelFormat.Rgba, PixelType.Float, new IntPtr());
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorTextureHandle, 0);


            PositionTextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, PositionTextureHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, 
                WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y, 0, PixelFormat.Rgba, PixelType.Float, new IntPtr());
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, PositionTextureHandle, 0);


            NormalTextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, NormalTextureHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f,
                WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y, 0, PixelFormat.Rgba, PixelType.Float, new IntPtr());
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, NormalTextureHandle, 0);
            

            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });

            DepthTextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, DepthTextureHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8,
                WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, new IntPtr());
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, DepthTextureHandle, 0);

            GL.ClearStencil(0x00);
            GL.ClearDepth(1);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FramebufferHandle);
        }

        public void BindForWriting()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FramebufferHandle);
        }

        public void BindForReading()
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, FramebufferHandle);
        }

        public void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Clear() 
        {
            GL.ClearColor(0, 0, 0, 0);
            GL.ClearDepth(1);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);
        }
    }
}
