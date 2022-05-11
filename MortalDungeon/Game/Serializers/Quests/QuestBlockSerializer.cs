using Empyrean.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    internal static class QuestBlockSerializer
    {
        private static string _questCharSet = "qrstuHIJKLMNOPQRbcdefghiklSTUVavwxyzABCDEFGmnop";
        private static int _fileNameLength = 10;

        public const int BLOCK_SIZE = 500;

        public static DataBlock<Quest> LoadQuestBlockFromFile(string filePath)
        {
            string path = filePath;

            if (!File.Exists(path))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(DataBlock<Quest>));

            FileStream fs = new FileStream(path, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            DataBlock<Quest> loadedState = (DataBlock<Quest>)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            loadedState.CompleteDeserialization();

            return loadedState;
        }

        public static DataBlock<Quest> LoadQuestBlockFromFile(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _questCharSet.CreateRandom(id, _fileNameLength) + ".qB";

            return LoadQuestBlockFromFile(path);
        }

        public static void WriteQuestBlockToFile(DataBlock<Quest> state)
        {
            string path = SerializerParams.DATA_BASE_PATH + _questCharSet.CreateRandom(state.BlockId, _fileNameLength) + ".qB";

            XmlSerializer serializer = new XmlSerializer(typeof(DataBlock<Quest>));

            TextWriter writer = new StreamWriter(path);

            state.PrepareForSerialization();

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteQuestBlock(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _questCharSet.CreateRandom(id, _fileNameLength) + ".qB";

            File.Delete(path);
        }

        public static List<DataBlock<Quest>> LoadAllQuestBlocks()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.EndsWith(".qB"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<DataBlock<Quest>> quests = new List<DataBlock<Quest>>();

            foreach (string file in filesToLoad)
            {
                var quest = LoadQuestBlockFromFile(file);
                quests.Add(quest);
            }

            return quests;
        }
    }
}
