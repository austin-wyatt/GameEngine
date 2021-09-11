namespace MortalDungeon.Engine_Classes.Audio
{
    public class Sound
    {
        public AudioBuffer Buffer = null;

        public Source Source = null;

        public bool Valid => Source != null;

        public string Name { get; private set; }

        public Sound(AudioBuffer buffer)
        {
            Buffer = buffer;

            Name = buffer.Name + $".{buffer.BufferInstance}";
        }

        public void Dispose()
        {
            if (Valid) SoundPlayer.FreeSource(Source);
        }

        /// <summary>
        /// Assigns the sound a source from the source pool and returns a bool indicating whether it was successful.
        /// If the sound has non-default source params they will be set here if a source was successfully allocated.
        /// </summary>
        public bool Prepare()
        {
            if (Valid)
                return true;

            bool successful = SoundPlayer.AssignSourceToSound(this);

            if (successful) SetSourceParams();

            return successful;
        }

        /// <summary>
        /// Here is where default source values such as location or gain should be set.
        /// </summary>
        public virtual void SetSourceParams() { }

        public void Play()
        {
            if (Valid)
                Source.Play();
        }

        public void Pause()
        {
            if (Valid)
                Source.Pause();
        }
    }
}
