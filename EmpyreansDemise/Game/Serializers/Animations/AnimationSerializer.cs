using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    public static class AnimationSerializer
    {
        public static Dictionary<string, AnimationSet> AllAnimationSets = new Dictionary<string, AnimationSet>();

        public static AnimationSet LoadAnimationFromFileWithName(string name)
        {
            string path = SerializerParams.DATA_BASE_PATH + name + ".a";

            return LoadAnimationFromFile(path);
        }

        public static AnimationSet LoadAnimationFromFile(string filePath)
        {
            string path = filePath;

            if (!File.Exists(path))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(AnimationSet));

            FileStream fs = new FileStream(path, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            AnimationSet loadedState = (AnimationSet)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            return loadedState;
        }

        public static void WriteAnimationToFile(AnimationSet state)
        {
            string path = SerializerParams.DATA_BASE_PATH + state.Name + ".a";

            XmlSerializer serializer = new XmlSerializer(typeof(AnimationSet));

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteAnimation(string name)
        {
            string path = SerializerParams.DATA_BASE_PATH + name + ".a";

            File.Delete(path);
        }

        public static List<AnimationSet> LoadAllAnimations()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.Contains(".a"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<AnimationSet> animations = new List<AnimationSet>();

            foreach (string file in filesToLoad)
            {
                var animation = LoadAnimationFromFile(file);
                animations.Add(animation);
            }

            return animations;
        }

        public static void Initialize()
        {
            var animations = LoadAllAnimations();

            foreach (var animation in animations)
            {
                AllAnimationSets.TryAdd(animation.Name, animation);
            }
        }
    }
}
