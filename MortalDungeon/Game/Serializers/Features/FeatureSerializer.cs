using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public static class FeatureSerializer
    {
        private static string _featureCharSet = "DklHtBmKAuNoLMngqrsSvwxyTEFGdeCJiUVafOPQRhzpIbc";
        private static int _fileNameLength = 10;

        /// <summary>
        /// Features will be handled slightly differently in that their origin point load radius are part of the 
        /// file name. Their loading and unloading will be handled by a FeatureManager class
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Feature LoadFeatureFromFile(string filePath)
        {
            string path = filePath;

            if (!File.Exists(path))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Feature));

            FileStream fs = new FileStream(path, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            Feature loadedState = (Feature)serializer.Deserialize(reader);

            loadedState.CompleteDeserialization();

            reader.Close();
            fs.Close();

            return loadedState;
        }

        public static Feature LoadFeatureFromFile(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _featureCharSet.CreateRandom(id, _fileNameLength) + ".f";

            return LoadFeatureFromFile(path);
        }

        public static void WriteFeatureToFile(Feature feature)
        {
            string path = SerializerParams.DATA_BASE_PATH + _featureCharSet.CreateRandom(feature.Id, _fileNameLength) + ".f";

            feature.PrepareForSerialization();

            XmlSerializer serializer = new XmlSerializer(typeof(Feature));

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, feature);

            writer.Close();
        }

        public static void DeleteFeature(Feature feature)
        {
            string path = SerializerParams.DATA_BASE_PATH + _featureCharSet.CreateRandom(feature.Id, _fileNameLength) + ".f";

            File.Delete(path);
        }

        public static List<Feature> LoadAllFeatures()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.Contains(".f"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<Feature> features = new List<Feature>();

            foreach (string file in filesToLoad)
            {
                var feature = LoadFeatureFromFile(file);

                features.Add(feature);
            }

            return features;
        }

        public static void CreateFeatureListFile()
        {
            List<Feature> features = LoadAllFeatures();

            string featureListPath = SerializerParams.DATA_BASE_PATH + "qqqqqqqqqqqqqqqq.f";

            if (File.Exists(featureListPath))
            {
                File.Delete(featureListPath);
            }

            FeatureList list = new FeatureList();

            foreach (var feature in features)
            {
                list.Features.Add(new FeatureListNode(feature));
            }

            list.Features.Sort((a, b) => a.Id.CompareTo(b.Id));

            XmlSerializer serializer = new XmlSerializer(typeof(FeatureList));

            TextWriter writer = new StreamWriter(featureListPath);

            serializer.Serialize(writer, list);

            writer.Close();
        }

        public static FeatureList LoadFeatureListFile()
        {
            string featureListPath = SerializerParams.DATA_BASE_PATH + "qqqqqqqqqqqqqqqq.f";

            if (!File.Exists(featureListPath))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(FeatureList));

            FileStream fs = new FileStream(featureListPath, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            FeatureList featureList = (FeatureList)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            return featureList;
        }
    }
}
