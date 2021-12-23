using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game
{
    public static class DialogueSerializer
    {
        private static string _dialogueCharSet = "abcdefghiklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUV";
        private static int _fileNameLength = 10;

        public static Dialogue LoadDialogueFromFile(int id)
        {
            string path = "Data/" + _dialogueCharSet.CreateRandom(id, _fileNameLength);

            XmlSerializer serializer = new XmlSerializer(typeof(Dialogue));

            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);

            TextReader reader = new StreamReader(fs);


            Dialogue loadedState = (Dialogue)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            return loadedState;
        }

        public static void WriteDialogueToFile(Dialogue state)
        {
            string path = "Data/" + _dialogueCharSet.CreateRandom(state.ID, _fileNameLength);

            XmlSerializer serializer = new XmlSerializer(typeof(Dialogue));

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }
    }
}
