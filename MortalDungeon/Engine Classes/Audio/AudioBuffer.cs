using OpenTK.Audio.OpenAL;
using System;

namespace MortalDungeon.Engine_Classes.Audio
{
    public class AudioBuffer
    {
        public int Handle;

        public string Name { get; private set; }

        private int _bufferInstanceCount = 0;
        public int BufferInstance
        {
            get => _bufferInstanceCount++;
        }

        public AudioBuffer(string name = "default")
        {
            Handle = AL.GenBuffer();

            Name = name;
        }

        ~AudioBuffer()
        {
            AL.DeleteBuffer(Handle);
        }


        public override bool Equals(object obj)
        {
            return obj is AudioBuffer buffer &&
                   Handle == buffer.Handle;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Handle);
        }
    }
}
