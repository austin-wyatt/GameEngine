using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Save;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public static class TextTableSerializer
    {
        private static string _textTableCharSet = "BCDEFGmnoqrMIJKLiklSTUPQRNOgVavwxyzhstuHbcdefAp";
        private static int _fileNameLength = 10;

        public static TextTable LoadTextTableFromFile(string filePath)
        {
            string path = filePath;

            XmlSerializer serializer = new XmlSerializer(typeof(TextTable));

            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);

            TextReader reader = new StreamReader(fs);


            TextTable loadedState = (TextTable)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            loadedState.CompleteDeserialization();

            return loadedState;
        }

        public static void WriteTextTableToFile(TextTable state)
        {
            state.PrepareForSerialization();

            string path = SerializerParams.DATA_BASE_PATH + _textTableCharSet.CreateRandom(state.TableID, _fileNameLength) + ".T";

            XmlSerializer serializer = new XmlSerializer(typeof(TextTable));

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteTextTable(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _textTableCharSet.CreateRandom(id, _fileNameLength) + ".T";

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
