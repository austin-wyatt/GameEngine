using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Engine_Classes
{
    internal enum AnimationType
    {
        Idle,
        Misc_One,
        Die,
        Transparent,
        Grass,
        Misc
    }
    internal class Animation : ITickable
    {
        internal bool Repeat = true;
        internal bool Reverse = false;
        internal bool Playing = true;

        internal List<RenderableObject> Frames = new List<RenderableObject>();
        internal int Frequency = 6; //in ticks (1/45th of a second)
        internal AnimationType Type = AnimationType.Idle;
        internal RenderableObject CurrentFrame;

        internal int GenericType 
        {
            get 
            {
                return (int)Type;
            }
            set 
            {
                Type = (AnimationType)value;
            }
        }

        internal int Repeats = 0;
        private int _repeatCount = 0;

        internal Action OnFinish = null;
        internal bool Finished = false;

        private uint tick = 0;
        private int _currentFrame;

        internal Animation() { }

        internal Animation(Animation anim) {
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
        internal void AdvanceFrame()
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

        internal void RewindFrame()
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

        internal void Tick()
        {
            if (!Playing)
                return;

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

        internal void Stop()
        {
            OnFinish?.Invoke();
            OnFinish = null;
        }

        internal void Pause() 
        {
            Playing = false;
        }

        internal void Play() 
        {
            Playing = true;
        }

        internal void Reset()
        {
            _currentFrame = 0;
            _repeatCount = 0;
            Finished = false;
            CurrentFrame = Frames[_currentFrame];
            Playing = true;
        }

        private bool RepeatAnimation() 
        {
            return Repeat && (_repeatCount < Repeats || Repeats < 0);
        }
    }
}
