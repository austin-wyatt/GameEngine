using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public enum AnimationType
    {
        Idle,
        Misc_One,
        Die
    }
    public class Animation
    {
        public bool Repeat = true;
        public bool Reverse = false;

        public List<RenderableObject> Frames = new List<RenderableObject>();
        public int Frequency = 6; //in ticks (1/30th of a second)
        public AnimationType Type = AnimationType.Idle;
        public RenderableObject CurrentFrame;

        public int Repeats = 0;
        private int _repeatCount = 0;

        public Action OnFinish = null;
        public bool Finished = false;

        private uint tick = 0;
        private int _currentFrame;

        public Animation() { }

        public static bool operator ==(Animation operand, Animation operand2)
        {
            return operand.Type == operand2.Type && operand._currentFrame == operand2._currentFrame;
        }
        public static bool operator !=(Animation operand, Animation operand2)
        {
            return !(operand == operand2);
        }
        public Animation(Animation anim) {
            Repeat = anim.Repeat;
            Reverse = anim.Reverse;

            _currentFrame = anim._currentFrame;
            Frequency = anim.Frequency;
            Type = anim.Type;

            Repeats = anim.Repeats;

            OnFinish = anim.OnFinish;

            for(int i = 0; i < anim.Frames.Count; i++)
            {
                Frames.Add(new RenderableObject(anim.Frames[i]));
            }

            CurrentFrame = Frames[_currentFrame];
        }
        public void AdvanceFrame()
        {
            if (Frames.Count > 1 && !Finished)
            {
                if (_currentFrame >= Frames.Count - 1)
                {
                    if (RepeatAnimation())
                    {
                        _currentFrame = 0;
                        _repeatCount++;
                        CurrentFrame = Frames[_currentFrame];
                    }
                    else 
                    {
                        Finished = true;
                        OnFinish?.Invoke();
                        OnFinish = null;
                    }
                }
                else
                {
                    _currentFrame++;
                    CurrentFrame = Frames[_currentFrame];
                }
            }
        }

        public void RewindFrame()
        {
            if (Frames.Count > 1 && !Finished)
            {
                if (_currentFrame <= 0)
                {
                    if (RepeatAnimation())
                    {
                        _currentFrame = Frames.Count - 1;
                        _repeatCount++;
                        CurrentFrame = Frames[_currentFrame];
                    }
                    else
                    {
                        Finished = true;
                        OnFinish?.Invoke();
                        OnFinish = null;
                    }
                }
                else
                {
                    _currentFrame--;
                    CurrentFrame = Frames[_currentFrame];
                }
            }
        }

        public void Tick()
        {
            tick++;
            if (Frequency != 0) //Setting Frequency to 0 is another method to cancel playback of an animation (as opposed to only supplying 1 frame)
            {
                if (tick % Frequency == 0)
                {
                    if (Reverse)
                    {
                        RewindFrame();
                    }
                    else
                    {
                        AdvanceFrame();
                    }
                }
            }
        }

        public void Stop()
        {
            OnFinish?.Invoke();
            OnFinish = null;
        }

        public void Reset()
        {
            _currentFrame = 0;
            _repeatCount = 0;
            Finished = false;
            CurrentFrame = Frames[_currentFrame];
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
