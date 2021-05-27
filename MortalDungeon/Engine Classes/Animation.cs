using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public enum AnimationType
    {
        Idle,
        Misc_One
    }
    public class Animation
    {
        public bool Repeat = true;
        public bool Reverse = false;

        public int CurrentFrame;
        public List<RenderableObject> Frames = new List<RenderableObject>();
        public int Frequency = 6; //in ticks (1/30th of a second)
        public AnimationType Type = AnimationType.Idle;

        public int Repeats = 0;
        private int _repeatCount = 0;

        public Action OnFinish = null;
        public bool Finished = false;

        private uint tick = 0;
        public Animation() { }

        public static bool operator ==(Animation operand, Animation operand2)
        {
            return operand.Type == operand2.Type && operand.CurrentFrame == operand2.CurrentFrame;
        }
        public static bool operator !=(Animation operand, Animation operand2)
        {
            return !(operand == operand2);
        }
        public Animation(Animation anim) {
            Repeat = anim.Repeat;
            Reverse = anim.Reverse;

            CurrentFrame = anim.CurrentFrame;
            Frequency = anim.Frequency;
            Type = anim.Type;

            Repeats = anim.Repeats;

            OnFinish = anim.OnFinish;

            for(int i = 0; i < anim.Frames.Count; i++)
            {
                Frames.Add(new RenderableObject(anim.Frames[i]));
            }
        }
        public void AdvanceFrame()
        {
            if (Frames.Count > 1 && !Finished)
            {
                if (CurrentFrame == Frames.Count - 1)
                {
                    if (RepeatAnimation())
                    {
                        CurrentFrame = 0;
                        _repeatCount++;
                    }
                    else 
                    {
                        OnFinish?.Invoke();
                        Finished = true;
                    }
                }
                else
                {
                    CurrentFrame++;
                }
            }
        }

        public void RewindFrame()
        {
            if (Frames.Count > 1 && !Finished)
            {
                if (CurrentFrame <= 0)
                {
                    if (RepeatAnimation())
                    {
                        CurrentFrame = Frames.Count - 1;
                        _repeatCount++;
                    }
                    else
                    {
                        OnFinish?.Invoke();
                        Finished = true;
                    }
                }
                else
                {
                    CurrentFrame--;
                }
            }
        }

        public void Tick()
        {
            tick++;
            if(tick % Frequency == 0)
            {
                if(Reverse)
                {
                    RewindFrame();
                }
                else 
                {
                    AdvanceFrame();
                }
            }
        }

        public void Stop()
        {
            OnFinish?.Invoke();
        }

        public void Reset()
        {
            CurrentFrame = 0;
            _repeatCount = 0;
            Finished = false;
        }

        public RenderableObject GetCurrentFrame()
        {
            return Frames[CurrentFrame];
        }

        private bool RepeatAnimation() 
        {
            return Repeat && (_repeatCount < Repeats || Repeats < 0);
        }
    }

    //Handles transformations that might need to be repeated/used frequently
    public class TransformationAnimation 
    {
         

    }
}
