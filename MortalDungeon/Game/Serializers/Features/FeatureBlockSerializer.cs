using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public static class FeatureBlockSerializer
    {
        private static string _featureCharSet = "DklHtBmKAuNoLMngqrsSvwxyTEFGdeCJiUVafOPQRhzpIbc";
        private static int _fileNameLength = 10;

        public const int BLOCK_SIZE = 500;
        public static DataBlock<Feature> LoadFeatureBlockFromFile(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _featureCharSet.CreateRandom(id, _fileNameLength) + ".fB";

            return LoadFeatureBlockFromFile(path);
        }

        private static object _loadLock = new object();
        public static DataBlock<Feature> LoadFeatureBlockFromFile(string filePath)
        {
            lock (_loadLock)
            {
                string path = filePath;

                if (!File.Exists(path))
                {
                    return null;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(DataBlock<Feature>));

                FileStream fs = new FileStream(path, FileMode.Open);

                TextReader reader = new StreamReader(fs);


                DataBlock<Feature> loadedState = (DataBlock<Feature>)serializer.Deserialize(reader);

                loadedState.CompleteDeserialization();

                reader.Close();
                fs.Close();

                return loadedState;
            }
        }

        public static void WriteFeatureBlockToFile(DataBlock<Feature> state)
        {
            string path = SerializerParams.DATA_BASE_PATH + _featureCharSet.CreateRandom(state.BlockId, _fileNameLength) + ".fB";

            XmlSerializer serializer = new XmlSerializer(typeof(DataBlock<Feature>));

            state.PrepareForSerialization();

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteFeatureBlock(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _featureCharSet.CreateRandom(id, _fileNameLength) + ".fB";

            File.Delete(path);
        }

        public static List<DataBlock<Feature>> LoadAllFeatureBlocks()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.EndsWith(".fB"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<DataBlock<Feature>> features = new List<DataBlock<Feature>>();

            foreach (string file in filesToLoad)
            {
                var feature = LoadFeatureBlockFromFile(file);
                features.Add(feature);
            }

            return features;
        }
    }
}
