using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MortalDungeon.Engine_Classes.Rendering
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


            RenderingQueue.RenderTileQueue();

            RenderingQueue.RenderTileQuadQueue();

            RenderingQueue.RenderQueuedParticles();

            RenderingQueue.RenderQueuedObjects();
            RenderingQueue.RenderQueuedUnits();
            RenderingQueue.RenderInstancedStructureData();

            RenderingQueue.RenderLowPriorityQueue();

            GL.Clear(ClearBufferMask.DepthBufferBit);

            Renderer.RenderFrameBuffer(Renderer.StageOneFBO);
            Renderer.StageOneFBO.UnbindFrameBuffer();


            GL.Clear(ClearBufferMask.DepthBufferBit);
            RenderingQueue.RenderQueuedLetters();

            GL.Disable(EnableCap.FramebufferSrgb);

            RenderingQueue.RenderQueuedUI();
        }
    }

}
