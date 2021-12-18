using System;

namespace MortalDungeon.Engine_Classes.Audio
{
    internal class Sound
    {
        internal AudioBuffer Buffer = null;

        internal Source Source = null;

        internal bool Valid => Source != null && Buffer.Loaded;

        internal string Name { get; private set; }


        internal float Gain = 1;
        internal float Pitch = 1;
        internal bool Loop = false;
        internal float EndTime = -1;

        internal Sound(AudioBuffer buffer)
        {
            Buffer = buffer;

            buffer.AttachedSounds.Add(this);

            Name = buffer.Name + $".{buffer.BufferInstance}";
        }

        /// <summary>
        /// Remove the allocated source from the sound.
        /// </summary>
        internal void DeallocateSource()
        {
            if (Valid) SoundPlayer.FreeSource(Source);
        }

        /// <summary>
        /// This should be called when the sound is being unloaded.
        /// </summary>
        internal void Dispose() 
        {
            DeallocateSource();
            Buffer.AttachedSounds.Remove(this);
        }

        /// <summary>
        /// Assigns the sound a source from the source pool and returns a bool indicating whether it was successful.
        /// If the sound has non-default source params they will be set here if a source was successfully allocated.
        /// </summary>
        internal bool Prepare(Action onFinish = null)
        {
            if (Valid)
                return true;

            if (!Buffer.Loaded)
            {
                Buffer.Load(() =>
                {
                    ProcureSource();
                    onFinish?.Invoke();
                });
            }
            else 
            {
                bool successful = ProcureSource();

                onFinish?.Invoke();
                return successful;
            }

            return false;
        }

        private bool ProcureSource() 
        {
            bool successful = SoundPlayer.AssignSourceToSound(this);

            if (successful) SetSourceParams();

            return successful;
        }

        /// <summary>
        /// Here is where default source values such as location or gain should be set.
        /// </summary>
        internal void SetSourceParams() 
        {
            Source.Gain = Gain;
            Source.Pitch = Pitch;

            if (EndTime != -1)
            {
                Source.EndTime = EndTime;
            }

            if (Loop) 
            {
                Source.Loop = Loop;
            }
        }

        internal void Play()
        {
            if (Valid) Source.Play();
            else 
            {
                Prepare(Play);
            }
        }

        internal void Pause()
        {
            if (Valid) Source.Pause();
            else 
            {
                Prepare(Pause);
            }
        }

        internal void Stop()
        {
            if (Valid) Source.Stop();
            else
            {
                Prepare(Stop);
            }
        }
    }
}
