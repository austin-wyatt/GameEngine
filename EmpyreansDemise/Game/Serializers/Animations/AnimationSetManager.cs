using Empyrean.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    public static class AnimationSetManager
    {
        public static DataBlock<AnimationSet> LoadedAnimationSets = new DataBlock<AnimationSet>();
        private static bool _loaded = false;
        public static AnimationSet GetAnimationSet(int id)
        {
            if (!_loaded)
            {
                _loaded = LoadAnimationSets();
            }

            LoadedAnimationSets.LoadedItems.TryGetValue(id, out var animationSet);

            return animationSet;
        }

        private static object _loadLock = new object();
        public static bool LoadAnimationSets()
        {
            lock (_loadLock)
            {
                string path = SerializerParams.DATA_BASE_PATH + "ANIMATIONS";

                if (!File.Exists(path))
                {
                    return false;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(DataBlock<AnimationSet>));

                FileStream fs = new FileStream(path, FileMode.Open);

                TextReader reader = new StreamReader(fs);


                DataBlock<AnimationSet> loadedState = (DataBlock<AnimationSet>)serializer.Deserialize(reader);

                reader.Close();
                fs.Close();

                loadedState.CompleteDeserialization();

                LoadedAnimationSets = loadedState;
                return true;
            }
        }

        public static void SaveAnimationSet(AnimationSet animationSet)
        {
            if (!_loaded)
            {
                LoadAnimationSets();
            }

            LoadedAnimationSets.LoadedItems.AddOrSet(animationSet.Id, animationSet);

            SaveAnimationSets();
        }

        public static void SaveAnimationSets()
        {
            string path = SerializerParams.DATA_BASE_PATH + "ANIMATIONS";

            XmlSerializer serializer = new XmlSerializer(typeof(DataBlock<AnimationSet>));

            TextWriter writer = new StreamWriter(path);

            LoadedAnimationSets.PrepareForSerialization();

            serializer.Serialize(writer, LoadedAnimationSets);

            writer.Close();
        }

        public static void DeleteAnimationSet(int id)
        {
            LoadedAnimationSets.LoadedItems.Remove(id);
        }
    }
}
