using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public static class UnitInfoBlockSerializer
    {
        private static string _unitInfoCharSet = "DKrUoTikhGvIagmfRBCJspncdwxyPtezAqbEFuOQlLMNHSV";
        private static int _fileNameLength = 10;

        public const int UNIT_BLOCK_SIZE = 500;

        public static UnitInfoBlock LoadUnitBlockInfoFromFile(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _unitInfoCharSet.CreateRandom(id, _fileNameLength) + ".UB";

            return LoadUnitBlockInfoFromFile(path);
        }

        private static object _loadLock = new object();
        public static UnitInfoBlock LoadUnitBlockInfoFromFile(string filePath)
        {
            lock (_loadLock)
            {
                string path = filePath;

                if (!File.Exists(path))
                {
                    return null;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(UnitInfoBlock));

                FileStream fs = new FileStream(path, FileMode.Open);

                TextReader reader = new StreamReader(fs);


                UnitInfoBlock loadedState = (UnitInfoBlock)serializer.Deserialize(reader);

                loadedState.CompleteDeserialization();

                reader.Close();
                fs.Close();

                return loadedState;
            }
        }

        public static void WriteUnitBlockInfoToFile(UnitInfoBlock state)
        {
            string path = SerializerParams.DATA_BASE_PATH + _unitInfoCharSet.CreateRandom(state.BlockId, _fileNameLength) + ".UB";

            XmlSerializer serializer = new XmlSerializer(typeof(UnitInfoBlock));

            TextWriter writer = new StreamWriter(path);

            state.PrepareForSerialization();

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteUnitBlockInfo(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _unitInfoCharSet.CreateRandom(id, _fileNameLength) + ".UB";

            File.Delete(path);
        }

        public static List<UnitInfoBlock> LoadAllUnitBlockInfo()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.Contains(".UB"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<UnitInfoBlock> unitInfo = new List<UnitInfoBlock>();

            foreach (string file in filesToLoad)
            {
                var info = LoadUnitBlockInfoFromFile(file);
                unitInfo.Add(info);
            }

            return unitInfo;
        }
    }
}
