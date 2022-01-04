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
    public static class AbilityLoadoutSerializer
    {
        private static string _abilityCharSet = "DtezAcdIJswxyKLEFbfCHTiuMNaPhqOQkGBUopgmnvlrRSV";
        private static int _fileNameLength = 10;

        public static AbilityLoadout LoadAbilityLoadoutFromFile(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _abilityCharSet.CreateRandom(id, _fileNameLength) + ".A";

            return LoadAbilityLoadoutFromFile(path);
        }

        public static AbilityLoadout LoadAbilityLoadoutFromFile(string filePath)
        {
            string path = filePath;

            if (!File.Exists(path))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(AbilityLoadout));

            FileStream fs = new FileStream(path, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            AbilityLoadout loadedState = (AbilityLoadout)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            return loadedState;
        }

        public static void WriteAbilityLoadoutToFile(AbilityLoadout state)
        {
            string path = SerializerParams.DATA_BASE_PATH + _abilityCharSet.CreateRandom(state.Id, _fileNameLength) + ".A";

            XmlSerializer serializer = new XmlSerializer(typeof(AbilityLoadout));

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, state);

            writer.Close();
        }

        public static void DeleteAbilityLoadout(int id)
        {
            string path = SerializerParams.DATA_BASE_PATH + _abilityCharSet.CreateRandom(id, _fileNameLength) + ".A";

            File.Delete(path);
        }

        public static List<AbilityLoadout> LoadAllAbilityLoadouts()
        {
            string[] files = Directory.GetFiles(SerializerParams.DATA_BASE_PATH);

            List<string> filesToLoad = new List<string>();

            foreach (string file in files)
            {
                if (file.Contains(".A"))
                {
                    filesToLoad.Add(file);
                }
            }

            List<AbilityLoadout> loadouts = new List<AbilityLoadout>();

            foreach (string file in filesToLoad)
            {
                var loadout = LoadAbilityLoadoutFromFile(file);
                loadouts.Add(loadout);
            }

            return loadouts;
        }
    }
}
