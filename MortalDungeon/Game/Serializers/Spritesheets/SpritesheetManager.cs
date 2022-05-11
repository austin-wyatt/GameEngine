using Empyrean.Engine_Classes;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    public static class SpritesheetManager
    {
        private const int SPRITESHEET_ID_CUTOFF = 50000;

        public static DataBlock<Spritesheet> LoadedSpritesheets = new DataBlock<Spritesheet>();
        private static bool _loaded = false;
        public static Spritesheet GetSpritesheet(int id)
        {
            if (!_loaded)
            {
                _loaded = LoadSpritesheets();
            }


            if(id < SPRITESHEET_ID_CUTOFF)
            {
                Spritesheets.AllSpritesheets.TryGetValue(id, out var spritesheet1);
                return spritesheet1;
            }

            LoadedSpritesheets.LoadedItems.TryGetValue(id, out var spritesheet);

            return spritesheet;
        }

        public static bool LoadSpritesheets()
        {
            string path = SerializerParams.DATA_BASE_PATH + "SPRITESHEETS";

            if (!File.Exists(path))
            {
                return false;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(DataBlock<Spritesheet>));

            FileStream fs = new FileStream(path, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            DataBlock<Spritesheet> loadedState = (DataBlock<Spritesheet>)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            loadedState.CompleteDeserialization();

            LoadedSpritesheets = loadedState;
            return true;
        }

        public static void SaveSpritesheet(Spritesheet spritesheet)
        {
            if (!_loaded)
            {
                LoadSpritesheets();
            }

            LoadedSpritesheets.LoadedItems.AddOrSet(spritesheet.TextureId, spritesheet);

            SaveSpritesheets();
        }

        public static void SaveSpritesheets() 
        {
            string path = SerializerParams.DATA_BASE_PATH + "SPRITESHEETS";

            XmlSerializer serializer = new XmlSerializer(typeof(DataBlock<Spritesheet>));

            TextWriter writer = new StreamWriter(path);

            LoadedSpritesheets.PrepareForSerialization();

            serializer.Serialize(writer, LoadedSpritesheets);

            writer.Close();
        }

        public static void DeleteSpritesheet(int id)
        {
            LoadedSpritesheets.LoadedItems.Remove(id);
        }
    }
}
