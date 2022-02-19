using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public static class QuestSerializer
    {
        private static string _questCharSet = "qrstuHIJKLMNOPQRbcdefghiklSTUVavwxyzABCDEFGmnop";
        private static int _fileNameLength = 10;

        public static Quest LoadQuestFromFile(string filePath)
        {
            string path = filePath;

            if (!File.Exists(path))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Quest));

            FileStream fs = new FileStream(path, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            Quest loadedState = (Quest)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            loadedState.CompleteDeserialization();

            return loadedState;
        }

        public static Quest LoadQuestFromFile(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _questCharSet.CreateRandom(id, _fileNameLength) + ".q";

            return LoadQuestFromFile(path);
        }

        public static void WriteQuestToFile(Quest state)
        {
            string path = SerializerParams.DATA_BASE_PATH + _questCharSet.CreateRandom(state.ID, _fileNameLength) + ".q";

            XmlSerializer serializer = new XmlSerializer(typeof(Quest));

            TextWriter writer = new StreamWriter(path);

            state.PrepareForSerialization();

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteQuest(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _questCharSet.CreateRandom(id, _fileNameLength) + ".q";

            File.Delete(path);
        }

        public static List<Quest> LoadAllQuests()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.EndsWith(".q"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<Quest> quests = new List<Quest>();

            foreach (string file in filesToLoad)
            {
                var quest = LoadQuestFromFile(file);
                quests.Add(quest);
            }

            return quests;
        }
    }
}
