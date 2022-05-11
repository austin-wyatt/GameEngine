using OpenTK.Audio.OpenAL;
using System;

namespace Empyrean.Engine_Classes.Audio
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

        public float Pitch
        {
            get
            {
                AL.GetSource(Handle, ALSourcef.Pitch, out var value);
                return value;
            }
            set => AL.Source(Handle, ALSourcef.Pitch, value);
        }

        public bool Loop
        {
            get 
            {
                AL.GetSource(Handle, ALSourceb.Looping, out bool value);
                return value;
            }
            set => AL.Source(Handle, ALSourceb.Looping, value);
        }

        public float PlaybackPosition
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

        public float Duration 
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

        

        public ALSourceState State
        {
            get
            {
                AL.GetSource(Handle, ALGetSourcei.SourceState, out var value);
                return (ALSourceState)value;
            }
        }

        public bool KeepAlive = false;
        public float EndTime = float.MaxValue;

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
            Gain = 1;
            Pitch = 1;

            KeepAlive = false;
            EndTime = float.MaxValue;
        }

        public void SetPosition(float x, float y, float z)
        {
            AL.Source(Handle, ALSource3f.Position, x / 1000, y / 1000, z / 100);
        }

        public void SetVelocity(float x, float y, float z)
        {
            AL.Source(Handle, ALSource3f.Velocity, x, y, z);
        }

        public void SetDirection(float x, float y, float z)
        {
            AL.Source(Handle, ALSource3f.Direction, x, y, z);
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
