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
    public static class BuildingSkeletonSerializer
    {
        private static string _buildingCharSet = "vfHtiQmnlsNOPSTFGuIghwxcdRAVBCDEopqrebkyzaMJKLU";
        private static int _fileNameLength = 9;

        public static SerializableBuildingSkeleton LoadBuildingSkeletonFromFile(string filePath)
        {
            string path = filePath;

            if (!File.Exists(path))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(SerializableBuildingSkeleton));

            FileStream fs = new FileStream(path, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            SerializableBuildingSkeleton loadedState = (SerializableBuildingSkeleton)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            return loadedState;
        }

        public static SerializableBuildingSkeleton LoadBuildingSkeletonFromFile(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _buildingCharSet.CreateRandom(id, _fileNameLength) + ".bs";

            return LoadBuildingSkeletonFromFile(path);
        }

        public static void WriteBuildingSkeletonToFile(SerializableBuildingSkeleton state)
        {
            string path = SerializerParams.DATA_BASE_PATH + _buildingCharSet.CreateRandom(state.BuildingID, _fileNameLength) + ".bs";

            XmlSerializer serializer = new XmlSerializer(typeof(SerializableBuildingSkeleton));

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteBuildingSkeleton(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _buildingCharSet.CreateRandom(id, _fileNameLength) + ".bs";

            File.Delete(path);
        }

        public static List<SerializableBuildingSkeleton> LoadAllBuildingSkeletons()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.Contains(".bs"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<SerializableBuildingSkeleton> buildingSkeletons = new List<SerializableBuildingSkeleton>();

            foreach (string file in filesToLoad)
            {
                var skeleton = LoadBuildingSkeletonFromFile(file);
                buildingSkeletons.Add(skeleton);
            }

            return buildingSkeletons;
        }
    }
}
