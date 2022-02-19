using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace MortalDungeon.Game.Serializers
{
    public static class UnitCreationInfoSerializer
    {
        private static string _unitInfoCharSet = "DKrUoTikhGvIagmfRBCJspncdwxyPtezAqbEFuOQlLMNHSV";
        private static int _fileNameLength = 10;

        public static UnitCreationInfo LoadUnitCreationInfoFromFile(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _unitInfoCharSet.CreateRandom(id, _fileNameLength) + ".U";

            return LoadUnitCreationInfoFromFile(path);
        }

        public static UnitCreationInfo LoadUnitCreationInfoFromFile(string filePath)
        {
            string path = filePath;

            if (!File.Exists(path))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(UnitCreationInfo));

            FileStream fs = new FileStream(path, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            UnitCreationInfo loadedState = (UnitCreationInfo)serializer.Deserialize(reader);

            loadedState.CompleteDeserialization();

            reader.Close();
            fs.Close();

            return loadedState;
        }

        public static void WriteUnitCreationInfoToFile(UnitCreationInfo state)
        {
            string path = SerializerParams.DATA_BASE_PATH + _unitInfoCharSet.CreateRandom(state.Id, _fileNameLength) + ".U";

            XmlSerializer serializer = new XmlSerializer(typeof(UnitCreationInfo));

            TextWriter writer = new StreamWriter(path);

            state.PrepareForSerialization();

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteUnitCreationInfo(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _unitInfoCharSet.CreateRandom(id, _fileNameLength) + ".U";

            File.Delete(path);
        }

        public static List<UnitCreationInfo> LoadAllUnitCreationInfo()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.EndsWith(".U"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<UnitCreationInfo> unitInfo = new List<UnitCreationInfo>();

            foreach (string file in filesToLoad)
            {
                var info = LoadUnitCreationInfoFromFile(file);
                unitInfo.Add(info);
            }

            return unitInfo;
        }
    }
}
