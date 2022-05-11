using Empyrean.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    public static class DialogueBlockSerializer
    {
        private static string _dialogueCharSet = "abcdefghiklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUV";
        private static int _fileNameLength = 10;

        public const int BLOCK_SIZE = 500;
        public static DataBlock<Dialogue> LoadDialogueBlockFromFile(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _dialogueCharSet.CreateRandom(id, _fileNameLength) + ".dB";

            return LoadDialogueBlockFromFile(path);
        }

        private static object _loadLock = new object();
        public static DataBlock<Dialogue> LoadDialogueBlockFromFile(string filePath)
        {
            lock (_loadLock)
            {
                string path = filePath;

                if (!File.Exists(path))
                {
                    return null;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(DataBlock<Dialogue>));

                FileStream fs = new FileStream(path, FileMode.Open);

                TextReader reader = new StreamReader(fs);


                DataBlock<Dialogue> loadedState = (DataBlock<Dialogue>)serializer.Deserialize(reader);

                loadedState.CompleteDeserialization();

                reader.Close();
                fs.Close();

                return loadedState;
            }
        }

        public static void WriteDialogueBlockToFile(DataBlock<Dialogue> state)
        {
            string path = SerializerParams.DATA_BASE_PATH + _dialogueCharSet.CreateRandom(state.BlockId, _fileNameLength) + ".dB";

            XmlSerializer serializer = new XmlSerializer(typeof(DataBlock<Dialogue>));

            state.PrepareForSerialization();

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteDialogueBlock(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _dialogueCharSet.CreateRandom(id, _fileNameLength) + ".dB";

            File.Delete(path);
        }

        public static List<DataBlock<Dialogue>> LoadAllDialogueBlocks()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.EndsWith(".dB"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<DataBlock<Dialogue>> dialogues = new List<DataBlock<Dialogue>>();

            foreach (string file in filesToLoad)
            {
                var dialogue = LoadDialogueBlockFromFile(file);
                dialogues.Add(dialogue);
            }

            return dialogues;
        }
    }
}
