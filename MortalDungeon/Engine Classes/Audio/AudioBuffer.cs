using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;

namespace MortalDungeon.Engine_Classes.Audio
{
    internal class AudioBuffer
    {
        internal int Handle;

        internal bool Loaded = false;

        internal string Name { get; private set; }

        private int _bufferInstanceCount = 0;
        internal int BufferInstance
        {
            get => _bufferInstanceCount++;
        }

        private string Filename = "";


        internal List<Sound> AttachedSounds = new List<Sound>();  

        internal AudioBuffer(string name = "default", string filename = "")
        {
            Handle = AL.GenBuffer();

            Name = name;

            Filename = filename;
        }

        ~AudioBuffer()
        {
            ForceDetach();

            AL.DeleteBuffer(Handle);
        }

        internal void Load(Action onFinish = null) 
        {
            if (Filename != "") 
            {
                SoundPlayer.LoadOggToBuffer(Filename, this, () => 
                {
                    Loaded = true;

                    SoundPlayer.LoadedBuffers.Add(this);

                    onFinish?.Invoke();
                });
            }
        }

        internal void Unload() 
        {
            ForceDetach();

            SoundPlayer.LoadedBuffers.Remove(this);
            Loaded = false;

            AL.DeleteBuffer(Handle);

            Handle = AL.GenBuffer();
        }

        private void ForceDetach() 
        {
            foreach (var sound in AttachedSounds) 
            {
                sound.DeallocateSource();
            }
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
