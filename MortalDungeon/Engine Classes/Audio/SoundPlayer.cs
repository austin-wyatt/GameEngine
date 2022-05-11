using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenTK.Audio.OpenAL;
using System.IO;
using NVorbis;
using System.Threading.Tasks;

namespace Empyrean.Engine_Classes.Audio
{
    public static class SoundPlayer
    {
        public const int MAX_SOURCES = 32;

        public static float Volume
        {
            get 
            {
                AL.GetListener(ALListenerf.Gain, out float val);
                return val;
            }
            set 
            {
                AL.Listener(ALListenerf.Gain, value);
            }
        }

        private static Source[] Sources = new Source[MAX_SOURCES];
        public static HashSet<Source> ActiveSources = new HashSet<Source>(MAX_SOURCES);
        public static List<AudioBuffer> LoadedBuffers = new List<AudioBuffer>();


        public static bool DISPLAY_DEBUG_MESSAGES = false;

        public static void Initialize() 
        {
            int[] empty = new int[1];

            ALDevice device = ALC.OpenDevice("");
            ALContext context = ALC.CreateContext(device, ref empty[0]);

            ALC.MakeContextCurrent(context);

            Volume = 2f;

            for (int i = 0; i < Sources.Length; i++) 
            {
                Sources[i] = new Source();
            }


            StartSourceWatchdog();
            StartBufferWatchdog();
        }

        public static object _activeSourcesLock = new object();

        private static void StartSourceWatchdog() 
        {
            Task sourceWatchdog = new Task(() =>
            {
                List<Source> sourcesToRemove = new List<Source>();
                while (true)
                {
                    lock (_activeSourcesLock)
                    {
                        for (int i = 0; i < sourcesToRemove.Count; i++)
                        {
                            ActiveSources.Remove(sourcesToRemove[i]);
                        }

                        sourcesToRemove.Clear();

                        foreach (var source in ActiveSources)
                        {
                            if (source.State == ALSourceState.Stopped && !source.KeepAlive)
                            {
                                sourcesToRemove.Add(source);

                                source.ApplyDefaultValues();

                                if (DISPLAY_DEBUG_MESSAGES)
                                    Console.WriteLine($"Freed source {source.Handle}");

                                if (source.ActiveSound != null)
                                {
                                    source.ActiveSound.Dispose();
                                }
                            }

                            if (source.EndTime < source.PlaybackPosition)
                            {
                                source.PlaybackPosition = source.Duration;
                            }
                        }
                    }

                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);

            sourceWatchdog.Start();
        }

        private static void StartBufferWatchdog() 
        {
            List<AudioBuffer> buffersToUnload = new List<AudioBuffer>();
            Task bufferWatchdog = new Task(() =>
            {
                while (true)
                {
                    foreach (var buffer in LoadedBuffers)
                    {
                        if (buffer.AttachedSounds.Count == 0)
                        {
                            buffersToUnload.Add(buffer);
                        }
                    }

                    foreach (var buffer in buffersToUnload)
                    {
                        if (DISPLAY_DEBUG_MESSAGES)
                            Console.WriteLine($"Unloaded buffer {buffer.Name}");

                        buffer.Unload();
                    }

                    buffersToUnload.Clear();

                    Thread.Sleep(1000 * 30); //check every 30 seconds
                }
            }, TaskCreationOptions.LongRunning);

            bufferWatchdog.Start();
        }

        public static void FreeAllSources() 
        {
            foreach (var source in Sources) 
            {
                FreeSource(source);
            }
        }

        public static void LoadOggToBuffer(string filename, AudioBuffer buffer, Action onFinish = null) 
        {
            Task.Run(() =>
            {
                using (var vorbis = new VorbisReader(filename))
                {
                    short[] empty = new short[1];

                    AL.BufferData(buffer.Handle, ALFormat.Stereo16, ref empty[0], empty.Length * sizeof(short), 1);

                    var channels = vorbis.Channels;
                    var sampleRate = vorbis.SampleRate;
                    var bitrate = vorbis.NominalBitrate;

                    var readBuffer = new float[vorbis.TotalSamples * channels];

                    var position = TimeSpan.Zero;

                    int samples = vorbis.ReadSamples(readBuffer, 0, readBuffer.Length);

                    short[] data = new short[readBuffer.Length];

                    for (int i = 0; i < readBuffer.Length; i++)
                    {
                        int item = (int)(readBuffer[i] * 32768);

                        if (item > 32767) item = 32767;
                        if (item < -32767) item = -32767;

                        data[i] = (short)item;
                    }

                    if(channels == 2)
                    {
                        AL.BufferData(buffer.Handle, ALFormat.Stereo16, ref data[0], data.Length * sizeof(short), sampleRate);
                    }
                    else if(channels == 1)
                    {
                        AL.BufferData(buffer.Handle, ALFormat.Mono16, ref data[0], data.Length * sizeof(short), sampleRate);
                    }

                    data = null;
                    readBuffer = null;
                    onFinish?.Invoke();
                }
            });
        }

        public static void PlaySound(Sound sound) 
        {
            if (sound.Source == null && !AssignSourceToSound(sound))
                return;

            sound.Source.Play();

            if (DISPLAY_DEBUG_MESSAGES)
                Console.WriteLine($"Playing {sound.Name} on source {sound.Source.Handle}");
        }

        public static void FreeSource(Source source) 
        {
            source.Stop();
            source.KeepAlive = false;

            if (source.ActiveSound != null)
            {
                source.ActiveSound.Source = null;
                source.ActiveSound = null;
            }
        }

        private static Source GetAvailableSource()
        {
            for (int i = 0; i < Sources.Length; i++)
            {
                if (!ActiveSources.Contains(Sources[i]))
                {
                    return Sources[i];
                }
            }

            return null;
        }

        public static bool AssignSourceToSound(Sound sound) 
        {
            Source foundSource = GetAvailableSource();

            if (foundSource == null) 
            {
                if (DISPLAY_DEBUG_MESSAGES)
                    Console.WriteLine("Source limit reached");
                return false;
            }

            if (DISPLAY_DEBUG_MESSAGES)
            {
                Console.WriteLine("Procured source " + ActiveSources.Count);
            }

            lock (_activeSourcesLock)
            {
                ActiveSources.Add(foundSource);
            }
            
            foundSource.SetBuffer(sound.Buffer);

            foundSource.ActiveSound = sound;
            sound.Source = foundSource;

            return true;
        }

        public static void SetListenerPosition(float x, float y, float z)
        {
            AL.Listener(ALListener3f.Position, x / 1000, y / 1000, z / 100);
        }
    }
}
