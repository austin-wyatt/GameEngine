using OpenTK.Audio.OpenAL;
using System;

namespace MortalDungeon.Engine_Classes.Audio
{
    public class Source
    {
        public int Handle;
        public float Gain
        {
            get
            {
                AL.GetSource(Handle, ALSourcef.Gain, out var value);
                return value;
            }
            set => AL.Source(Handle, ALSourcef.Gain, value);
        }

        public ALSourceState State
        {
            get
            {
                AL.GetSource(Handle, ALGetSourcei.SourceState, out var value);
                return (ALSourceState)value;
            }
        }

        public bool KeepAlive = false;

        public Sound ActiveSound = null;

        public Source()
        {
            Handle = AL.GenSource();
        }

        ~Source()
        {
            AL.DeleteSource(Handle);
        }

        public void SetBuffer(AudioBuffer buffer)
        {
            AL.Source(Handle, ALSourcei.Buffer, buffer.Handle);
        }

        public void Pause(bool unpause = false)
        {
            if (unpause && State == ALSourceState.Paused) Play();
            else AL.SourcePause(Handle);
        }

        public void Play()
        {
            AL.SourcePlay(Handle);
        }

        public void Stop()
        {
            AL.SourceStop(Handle);
        }


        public void ApplyDefaultValues()
        {
            AL.Source(Handle, ALSourcef.Gain, 1);
        }

        public override bool Equals(object obj)
        {
            return obj is Source source &&
                   Handle == source.Handle;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Handle);
        }
    }
}
