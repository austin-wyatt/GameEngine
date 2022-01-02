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

        public static Quest LoadQuestFromFile(int id)
        {
            string path = "Data/" + _questCharSet.CreateRandom(id, _fileNameLength) + ".q";

            XmlSerializer serializer = new XmlSerializer(typeof(Quest));

            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);

            TextReader reader = new StreamReader(fs);


            Quest loadedState = (Quest)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            return loadedState;
        }

        public static void WriteQuestToFile(Quest state)
        {
            string path = "Data/" + _questCharSet.CreateRandom(state.ID, _fileNameLength) + ".q";

            XmlSerializer serializer = new XmlSerializer(typeof(Quest));

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }
    }
}
