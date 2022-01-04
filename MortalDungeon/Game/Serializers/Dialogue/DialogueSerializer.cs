using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public static class DialogueSerializer
    {
        private static string _dialogueCharSet = "abcdefghiklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUV";
        private static int _fileNameLength = 10;

        public static Dialogue LoadDialogueFromFile(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _dialogueCharSet.CreateRandom(id, _fileNameLength) + ".d";

            return LoadDialogueFromFile(path);
        }

        public static Dialogue LoadDialogueFromFile(string filePath)
        {
            string path = filePath;

            if (!File.Exists(path))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Dialogue));

            FileStream fs = new FileStream(path, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            Dialogue loadedState = (Dialogue)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            return loadedState;
        }

        public static void WriteDialogueToFile(Dialogue state)
        {
            string path = SerializerParams.DATA_BASE_PATH + _dialogueCharSet.CreateRandom(state.ID, _fileNameLength) + ".d";

            XmlSerializer serializer = new XmlSerializer(typeof(Dialogue));

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteDialogue(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _dialogueCharSet.CreateRandom(id, _fileNameLength) + ".d";

            File.Delete(path);
        }

        public static List<Dialogue> LoadAllDialogues()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.Contains(".d"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<Dialogue> dialogues = new List<Dialogue>();

            foreach (string file in filesToLoad)
            {
                var dialogue = LoadDialogueFromFile(file);
                dialogues.Add(dialogue);
            }

            return dialogues;
        }
    }
}
