using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public static class FeatureSerializer
    {
        private static string _featureCharSet = "defgqrsJKAHtBCDEFGhiklmnouNOPQRSvwxyzpILMTUVabc";
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

            loadedState._affectedPoints.FillDictionary(loadedState.AffectedPoints);
            loadedState._affectedPoints = null;

            reader.Close();
            fs.Close();

            return loadedState;
        }

        public static void WriteFeatureToFile(Feature state)
        {
            string path = "Data/" + Feature.HashCoordinates(state.Origin.X, state.Origin.Y) + "_" + state.LoadRadius + ".f";

            state._affectedPoints = new DeserializableDictionary<FeaturePoint, int>(state.AffectedPoints);

            XmlSerializer serializer = new XmlSerializer(typeof(Feature));

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteFeature(Feature feature)
        {
            string path = "Data/" + Feature.HashCoordinates(feature.Origin.X, feature.Origin.Y) + "_" + feature.LoadRadius + ".f";

            File.Delete(path);
        }

        public static List<Feature> LoadAllFeatures()
        {
            string[] files = Directory.GetFiles("Data/");

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
    }
}
