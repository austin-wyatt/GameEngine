using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    /// <summary>
    /// Works similarly to the Animation class but changes properties such as transformations and color instead of the sprite.
    /// </summary>
    public class PropertyAnimation
    {
        public RenderableObject BaseFrame;
        public List<Keyframe> Keyframes = new List<Keyframe>();
        public int AnimationID = 0; //semi-unique identifier to distinguish between active animations in a list

        public Vector3 BaseTranslation = new Vector3();
        public Vector4 BaseColor = new Vector4();

        public bool Playing = false;
        public bool Repeat = false;

        public Action OnFinish = null;

        public int CurrentKeyframe = 0;
        private int tick = 0;

        public PropertyAnimation(RenderableObject baseFrame, int id = 0)
        {
            BaseFrame = baseFrame;
            AnimationID = id;

            SetDefaultValues();

            timer.Start();
        }

        public PropertyAnimation() { }

        private int count = 0;
        private Stopwatch timer = new Stopwatch();
        public void Tick()
        {
            if (Playing && Keyframes.Count >= 0)
            {
                if (CurrentKeyframe >= Keyframes.Count)
                {
                    Playing = Repeat;
                    Restart();
                    OnFinish?.Invoke();

                    return;
                }

                if (Keyframes[CurrentKeyframe].ActivationTick <= tick)
                {
                    Keyframes[CurrentKeyframe].Action?.Invoke(BaseFrame);

                    CurrentKeyframe++;
                }

                tick++;
            }

            if (WindowConstants.ShowTicksPerSecond)
            {
                if (timer.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine("Ticks per second: " + count);
                    count = 0;
                    timer.Restart();
                }
                count++;
            }
        }

        /// <summary>
        /// Replay the animation
        /// </summary>
        public void Restart()
        {
            CurrentKeyframe = 0;
            tick = 0;
            Tick();
        }

        public void Play()
        {
            Playing = true;
            SetDefaultValues();
        }

        public void Stop()
        {
            Playing = false;
        }

        /// <summary>
        /// Set the BaseFrame back to it's initial values and stop the animation
        /// </summary>
        public void Reset()
        {
            BaseFrame.SetTranslation(BaseTranslation);
            BaseFrame.SetColor(BaseColor);

            Playing = false;
            Restart();
        }

        public void SetDefaultValues()
        {
            BaseTranslation = BaseFrame.Translation.ExtractTranslation();
            BaseColor = new Vector4(BaseFrame.Color);
        }

        public void SetDefaultColor() 
        {
            BaseColor = new Vector4(BaseFrame.Color);
        }

        public void SetDefaultTranslation() 
        {
            BaseTranslation = BaseFrame.Translation.ExtractTranslation();
        }
    }

    public class Keyframe
    {
        public int ActivationTick = 0; //the tick to activate on. 
        public Action<RenderableObject> Action = null;

        public Keyframe(int tick)
        {
            ActivationTick = tick;
        }
        public Keyframe(int tick, Action<RenderableObject> action)
        {
            ActivationTick = tick;
            Action = action;
        }

        public Keyframe() { }
    }
}
