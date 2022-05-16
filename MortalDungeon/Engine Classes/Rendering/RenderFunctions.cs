using Empyrean.Objects;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Empyrean.Engine_Classes.Rendering
{
    public enum FadeDirection
    {
        In,
        Out
    }
    public struct FadeParameters
    {
        public float Alpha;
        public float StepSize;
        public float TimeDelay;
        public Stopwatch Timer;

        public FadeDirection FadeDirection;

        public delegate void FadeEventHandler();
        public event FadeEventHandler FadeComplete;

        private bool _fadeCompleted;

        public FadeParameters(FadeDirection fadeDirection)
        {
            Alpha = 1.0f;
            StepSize = 0.05f;
            TimeDelay = 100f; //ms

            Timer = new Stopwatch();

            FadeDirection = fadeDirection;

            FadeComplete = null;

            _fadeCompleted = false;
        }


        public void StartFade(FadeDirection direction)
        {
            RenderingQueue.RenderStateManager.SetFlag(RenderingStates.Fade, true);
            Timer.Restart();

            FadeDirection = direction;

            if (FadeDirection == FadeDirection.Out)
            {
                Alpha = 1;
            }
            else
            {
                Alpha = 0;
            }

            _fadeCompleted = false;
        }

        public void OnCompleteFade()
        {
            if (_fadeCompleted)
                return;

            FadeComplete?.Invoke();

            _fadeCompleted = true;
        }

        public void EndFade()
        {
            RenderingQueue.RenderStateManager.SetFlag(RenderingStates.Fade, false);
        }
    }

    public static class RenderFunctions
    {
        public static FadeParameters FadeParameters = new FadeParameters(FadeDirection.Out);
        public static void Fade()
        {
            if(FadeParameters.Timer.ElapsedMilliseconds > FadeParameters.TimeDelay)
            {
                FadeParameters.Timer.Restart();

                switch(FadeParameters.FadeDirection)
                {
                    case FadeDirection.In:
                        if (FadeParameters.Alpha > 1)
                        {
                            FadeParameters.OnCompleteFade();
                        }

                        FadeParameters.Alpha += FadeParameters.StepSize;
                        break;
                    case FadeDirection.Out:
                        if (FadeParameters.Alpha < 0)
                        {
                            FadeParameters.OnCompleteFade();
                        }

                        FadeParameters.Alpha -= FadeParameters.StepSize;
                        break;
                }
            }


            Renderer.DrawToFrameBuffer(Renderer.StageOneFBO); //Framebuffer should only be used when we want to do post processing
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Renderer.ClearColor.X, Renderer.ClearColor.Y, Renderer.ClearColor.Z, Renderer.ClearColor.W);

            GL.Enable(EnableCap.FramebufferSrgb);

            DrawGame();

            GL.Clear(ClearBufferMask.DepthBufferBit);

            Renderer.RenderFrameBuffer(Renderer.StageOneFBO);
            Renderer.StageOneFBO.UnbindFrameBuffer();

            
            GL.Clear(ClearBufferMask.DepthBufferBit);
            //RenderingQueue.RenderQueuedLetters();

            GL.Disable(EnableCap.FramebufferSrgb);

            //RenderingQueue.RenderQueuedUI();
            RenderingQueue.RenderInstancedUIData();

            RenderingQueue.RenderLowPriorityQueue();
        }

        public static void DrawGame()
        {
            Renderer.GBuffer.BindForWriting();
            Renderer.GBuffer.BindForReading();
            Renderer.GBuffer.Clear();


            //GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);

            //RenderingQueue.RenderTileQuadQueue();
            RenderingQueue.RenderInstancedTileData();
            //DrawFog();
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            
            DrawUnits();

            //GL.Disable(EnableCap.Blend);

            Renderer.RenderGBuffer(Renderer.GBuffer);


            Renderer.GBuffer.BindForReading();
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.Disable(EnableCap.FramebufferSrgb);
            //copy depth and stencil values from GBuffer
            GL.BlitFramebuffer(0, 0, WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y,
                0, 0, WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y,
                ClearBufferMask.StencilBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            GL.Enable(EnableCap.FramebufferSrgb);

            Renderer.GBuffer.Unbind();

            //GL.Enable(EnableCap.Blend);

            RenderingQueue.RenderQueuedObjects();

            RenderingQueue.RenderTileQueue();

            RenderingQueue.RenderIndividualMeshes();

            RenderingQueue.RenderQueuedParticles();

            DrawFog();
            //RenderingQueue.ClearFogQuad();

            //Span<int> x = stackalloc int[10];
        }


        public static void DrawUnits() 
        {
            //GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Keep);

            //GL.Enable(EnableCap.StencilTest);
            //GL.StencilFunc(StencilFunction.Equal, 7, 3);

            //RenderingQueue.RenderQueuedUnits();

            //GL.Disable(EnableCap.StencilTest);
            RenderingQueue.RenderQueuedUnits();
            //GL.Enable(EnableCap.StencilTest);

            //GL.Disable(EnableCap.DepthTest);

            //GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
            //GL.StencilFunc(StencilFunction.Equal, 4, 4);

            //RenderingQueue.RenderFogQuad();

            //GL.Enable(EnableCap.DepthTest);
            RenderingQueue.ClearUnitQueue();

            //GL.Disable(EnableCap.StencilTest);
        }

        public static void DrawFog()
        {
            //Renderer.GBuffer.BindForReading();
            //GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.Enable(EnableCap.StencilTest);
            //GL.StencilMask(3);
            GL.StencilFunc(StencilFunction.Notequal, 1, 1);

            GL.Disable(EnableCap.DepthTest);

            RenderingQueue.RenderFogQuad();

            RenderingQueue.ClearFogQuad();
            GL.Disable(EnableCap.StencilTest);


            GL.Enable(EnableCap.DepthTest);

            //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        }

        public static void DrawSkybox()
        {
            GL.Viewport(WindowConstants.GameViewport.X, WindowConstants.GameViewport.Y, WindowConstants.GameViewport.Width, WindowConstants.GameViewport.Height);
            Renderer.ViewportRectangle = WindowConstants.GameViewport;

            GL.DepthFunc(DepthFunction.Lequal);

            //Renderer.CheckError();

            Shaders.SKYBOX_SHADER.Use();
            Renderer.RenderSkybox(Window.SkyBox);

            GL.DepthFunc(DepthFunction.Less);
        }
    }

}
