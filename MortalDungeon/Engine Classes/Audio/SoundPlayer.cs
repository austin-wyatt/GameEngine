using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenTK.Audio.OpenAL;
using System.IO;
using NVorbis;
using System.Threading.Tasks;

namespace MortalDungeon.Engine_Classes.Audio
{
    public static class SoundPlayer
    {
        private const int MAX_SOURCES = 32;

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
        public static List<Source> ActiveSources = new List<Source>(MAX_SOURCES);
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

        private static void StartSourceWatchdog() 
        {
            Task sourceWatchdog = new Task(() =>
            {
                List<Source> sourcesToRemove = new List<Source>();
                while (true)
                {
                    for(int i = 0; i < ActiveSources.Count; i++)
                    {
                        Source source = ActiveSources[i];
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

                        if (ActiveSources[i].EndTime < ActiveSources[i].PlaybackPosition) 
                        {
                            ActiveSources[i].PlaybackPosition = ActiveSources[i].Duration;
                        }
                    }

                    for (int i = 0; i < sourcesToRemove.Count; i++)
                    {
                        ActiveSources.Remove(sourcesToRemove[i]);
                    }

                    sourcesToRemove.Clear();

                    Thread.Sleep(250);
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

                    AL.BufferData(buffer.Handle, ALFormat.Stereo16, ref data[0], data.Length * sizeof(short), sampleRate);

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
                //if (!ActiveSources.TryGetValue(Sources[i], out var _))
                if (!ActiveSources.Exists(s => s == Sources[i]))
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
                

            ActiveSources.Add(foundSource);
            foundSource.SetBuffer(sound.Buffer);

            foundSource.ActiveSound = sound;
            sound.Source = foundSource;

            return true;
        }
    }

   
    //public void PlayWave() 
    //{
    //    int[] empty = new int[1];

    //    ALDevice device = ALC.OpenDevice("");
    //    ALContext context = ALC.CreateContext(device, ref empty[0]);

    //    ALC.MakeContextCurrent(context);

    //    Stream fileStream = File.OpenRead("Resources/Sound/test.wav");

    //    int buffer = AL.GenBuffer();
    //    int source = AL.GenSource();

    //    //just try to get a WAV working using the OpenTK example

    //    BinaryReader reader = new BinaryReader(fileStream);

    //    string signature = new string(reader.ReadChars(4));
    //    if (signature != "RIFF")
    //        throw new Exception("Not a wave file");

    //    int riff_chunk_size = reader.ReadInt32();

    //    string format = new string(reader.ReadChars(4));
    //    if (format != "WAVE")
    //        throw new Exception("Not a wave file");

    //    string format_signature = new string(reader.ReadChars(4));
    //    if (format_signature != "fmt ")
    //        throw new Exception("Not supported");

    //    int format_chunk_size = reader.ReadInt32();
    //    int audio_format = reader.ReadInt16();
    //    int channels = reader.ReadInt16();
    //    int sample_rate = reader.ReadInt32();
    //    int byte_rate = reader.ReadInt32();
    //    int block_align = reader.ReadInt16();
    //    int bits = reader.ReadInt16();

    //    string data_signature = new string(reader.ReadChars(4));
    //    if (data_signature != "data")
    //        throw new Exception("Not supported");

    //    int data_chunk_size = reader.ReadInt32();

    //    byte[] data = reader.ReadBytes((int)reader.BaseStream.Length);

    //    AL.BufferData(buffer, ALFormat.Stereo16, ref data[0], data.Length, sample_rate);
    //    AL.Source(source, ALSourcei.Buffer, buffer);
    //    AL.Source(source, ALSourcef.Gain, 0.1f);

    //    AL.SourcePlay(source);
    //}
}
