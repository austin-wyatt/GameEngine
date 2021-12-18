using OpenTK.Audio.OpenAL;
using System;

namespace MortalDungeon.Engine_Classes.Audio
{
    internal class Source
    {
        internal int Handle;
        internal float Gain
        {
            get
            {
                AL.GetSource(Handle, ALSourcef.Gain, out var value);
                return value;
            }
            set => AL.Source(Handle, ALSourcef.Gain, value);
        }

        internal float Pitch
        {
            get
            {
                AL.GetSource(Handle, ALSourcef.Pitch, out var value);
                return value;
            }
            set => AL.Source(Handle, ALSourcef.Pitch, value);
        }

        internal bool Loop
        {
            get 
            {
                AL.GetSource(Handle, ALSourceb.Looping, out bool value);
                return value;
            }
            set => AL.Source(Handle, ALSourceb.Looping, value);
        }

        internal float PlaybackPosition
        {
            get 
            {
                AL.GetSource(Handle, ALSourcef.SecOffset, out float value);
                return value;
            }
            set 
            {
                AL.Source(Handle, ALSourcef.SecOffset, value);
            }
        }

        internal float Duration 
        {
            get
            {
                if (ActiveSound != null)
                {
                    AL.GetBuffer(ActiveSound.Buffer.Handle, ALGetBufferi.Size, out int size);
                    AL.GetBuffer(ActiveSound.Buffer.Handle, ALGetBufferi.Channels, out int channels);
                    AL.GetBuffer(ActiveSound.Buffer.Handle, ALGetBufferi.Bits, out int bits);
                    AL.GetBuffer(ActiveSound.Buffer.Handle, ALGetBufferi.Frequency, out int frequency);

                    int lengthInSamples = size * 8 / (channels * bits);

                    return (float)lengthInSamples / frequency;

                }
                else 
                {
                    return 0;
                }
            }
        }

        internal ALSourceState State
        {
            get
            {
                AL.GetSource(Handle, ALGetSourcei.SourceState, out var value);
                return (ALSourceState)value;
            }
        }

        internal bool KeepAlive = false;
        internal float EndTime = float.MaxValue;

        internal Sound ActiveSound = null;

        internal Source()
        {
            Handle = AL.GenSource();
        }

        ~Source()
        {
            AL.DeleteSource(Handle);
        }

        internal void SetBuffer(AudioBuffer buffer)
        {
            AL.Source(Handle, ALSourcei.Buffer, buffer.Handle);
        }

        internal void Pause(bool unpause = false)
        {
            if (unpause && State == ALSourceState.Paused) Play();
            else AL.SourcePause(Handle);
        }

        internal void Play()
        {
            AL.SourcePlay(Handle);
        }

        internal void Stop()
        {
            AL.SourceStop(Handle);
        }



        internal void ApplyDefaultValues()
        {
            Gain = 1;
            Pitch = 1;

            KeepAlive = false;
            EndTime = float.MaxValue;
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
