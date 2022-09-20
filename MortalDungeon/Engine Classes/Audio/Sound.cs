using OpenTK.Mathematics;
using System;
using System.Threading;

namespace Empyrean.Engine_Classes.Audio
{
    public class Sound
    {
        public AudioBuffer Buffer = null;

        public Source Source = null;

        public bool Valid => Source != null && Buffer.Loaded;

        public string Name { get; private set; }

        private Vector3 Position = Vector3.PositiveInfinity;

        public float Gain = 1;
        public float Pitch = 1;
        public bool Loop = false;
        public float EndTime = -1;

        public object _operationLock = new object();

        public Sound(AudioBuffer buffer)
        {
            Buffer = buffer;

            buffer.AttachedSounds.Add(this);

            Name = buffer.Name + $".{buffer.BufferInstance}";
        }

        /// <summary>
        /// Remove the allocated source from the sound.
        /// </summary>
        public void DeallocateSource()
        {
            if (Valid) SoundPlayer.FreeSource(Source);
        }

        /// <summary>
        /// This should be called when the sound is being unloaded.
        /// </summary>
        public void Dispose() 
        {
            DeallocateSource();
            Buffer.AttachedSounds.Remove(this);
        }

        /// <summary>
        /// Assigns the sound a source from the source pool and returns a bool indicating whether it was successful.
        /// If the sound has non-default source params they will be set here if a source was successfully allocated.
        /// </summary>
        public bool Prepare(Action onFinish = null)
        {
            if (Valid)
                return true;

            if (!Buffer.Loaded)
            {
                Buffer.Load(() =>
                {
                    if (SoundPlayer.DISPLAY_DEBUG_MESSAGES)
                    {
                        Console.WriteLine("Buffer loaded: " + Buffer.Name);
                    }

                    ProcureSource();

                    onFinish?.Invoke();
                });
            }
            else 
            {
                bool successful = ProcureSource();

                if (successful)
                {
                    onFinish?.Invoke();
                }
                
                return successful;
            }

            return false;
        }

        private bool ProcureSource() 
        {
            if(SoundPlayer.ActiveSources.Count == SoundPlayer.MAX_SOURCES)
                return false;

            bool successful = SoundPlayer.AssignSourceToSound(this);

            if (successful) SetSourceParams();

            return successful;
        }

        /// <summary>
        /// Here is where default source values such as location or gain should be set.
        /// </summary>
        public void SetSourceParams() 
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

            if(Position != Vector3.PositiveInfinity)
            {
                Source.SetPosition(Position.X, Position.Y, Position.Z);
            }
        }

        public void Play()
        {
            Monitor.Enter(_operationLock);

            if (Valid) 
            {
                Source.Play();
            }
            else 
            {
                Prepare(Play);
            }

            Monitor.Exit(_operationLock);
        }

        public void Pause()
        {
            Monitor.Enter(_operationLock);

            if (Valid) Source.Pause();
            else 
            {
                Prepare(Pause);
            }

            Monitor.Exit(_operationLock);
        }

        public void Stop()
        {
            Monitor.Enter(_operationLock);

            if (Valid) Source.Stop();
            else
            {
                Prepare(Stop);
            }

            Monitor.Exit(_operationLock);
        }

        public void SetPosition(float x, float y, float z)
        {
            Position.X = x;
            Position.Y = y;
            Position.Z = z;
        }
    }
}
