using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;

namespace MortalDungeon.Engine_Classes.Audio
{
    public class AudioBuffer
    {
        public int Handle;

        public bool Loaded = false;

        public string Name { get; private set; }

        private int _bufferInstanceCount = 0;
        public int BufferInstance
        {
            get => _bufferInstanceCount++;
        }

        private string Filename = "";


        public List<Sound> AttachedSounds = new List<Sound>();  

        public AudioBuffer(string name = "default", string filename = "")
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

        public void Load(Action onFinish = null) 
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

        public void Unload() 
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
