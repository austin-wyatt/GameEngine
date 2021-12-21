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
    public class TimedAnimation : ITickable
    {
        public RenderableObject BaseFrame;
        public List<TimedKeyframe> Keyframes = new List<TimedKeyframe>();

        public Vector3 BaseTranslation = new Vector3();
        public Vector4 BaseColor = new Vector4();

        public bool Playing = false;
        public bool Repeat = false;

        public bool Finished = false; //this reflects not playing and hasn't been reset yet

        public Action OnFinish = null;

        public int CurrentKeyframe = 0;

        public long StartTime = 0;

        public int AnimationID => _animationID;
        protected int _animationID = currentAnimationID++;
        protected static int currentAnimationID = 0;

        public int DEBUG_ID = 0;

        public TimedAnimation(RenderableObject baseFrame)
        {
            BaseFrame = baseFrame;

            SetDefaultValues();

            timer.Start();
        }

        public TimedAnimation() { }


        public static PropertyAnimation CreateSingleFrameAnimation(RenderableObject baseFrame, Action action, int delay)
        {
            PropertyAnimation temp = new PropertyAnimation(baseFrame);
            temp.Play();

            Keyframe frame = new Keyframe(delay, action);

            temp.Keyframes.Add(frame);

            return temp;
        }

        private int count = 0;
        private Stopwatch timer = new Stopwatch();
        public void Tick()
        {
            if (Playing && Keyframes.Count >= 0)
            {
                long currentTime = WindowConstants.GlobalTimer.ElapsedMilliseconds;

                if (CurrentKeyframe >= Keyframes.Count)
                {
                    Playing = Repeat;
                    Finished = !Repeat;
                    Restart();
                    OnFinish?.Invoke();

                    return;
                }

                if (currentTime >= StartTime + Keyframes[CurrentKeyframe].ActivationTime)
                {
                    Keyframes[CurrentKeyframe].Action?.Invoke();

                    CurrentKeyframe++;
                }

                if (DEBUG_ID == 1)
                {

                }
            }
        }

        /// <summary>
        /// Replay the animation
        /// </summary>
        public void Restart()
        {
            CurrentKeyframe = 0;
            StartTime = WindowConstants.GlobalTimer.ElapsedMilliseconds;

            Tick();
        }

        public void Play()
        {
            Playing = true;
            StartTime = WindowConstants.GlobalTimer.ElapsedMilliseconds;

            SetDefaultValues();
        }

        public void Stop()
        {
            Playing = false;
            Finished = true;
        }

        public void Finish()
        {
            Stop();
            OnFinish?.Invoke();
        }

        /// <summary>
        /// Set the BaseFrame back to it's initial values and stop the animation
        /// </summary>
        public void Reset()
        {
            if (BaseFrame != null)
            {
                BaseFrame.SetTranslation(BaseTranslation);
                BaseFrame.SetBaseColor(BaseColor);
            }

            Playing = false;
            Finished = false;
            Restart();
        }

        public void SetDefaultValues()
        {
            if (BaseFrame != null)
            {
                BaseTranslation = BaseFrame.Translation.ExtractTranslation();
                BaseColor = new Vector4(BaseFrame.BaseColor);
            }
        }

        public void SetDefaultColor()
        {
            if (BaseFrame != null)
            {
                BaseColor = new Vector4(BaseFrame.BaseColor);
            }
        }

        public void SetDefaultTranslation()
        {
            if (BaseFrame != null)
            {
                BaseTranslation = BaseFrame.Translation.ExtractTranslation();
            }
        }
    }

    public class TimedKeyframe
    {
        /// <summary>
        /// in milliseconds
        /// </summary>
        public long ActivationTime = 0;
        public Action Action = null;

        public TimedKeyframe(long activationTime)
        {
            ActivationTime = activationTime;
        }
        public TimedKeyframe(long activationTime, Action action)
        {
            ActivationTime = activationTime;
            Action = action;
        }

        public TimedKeyframe() { }
    }
}
