using Empyrean.Game.Items;
using Empyrean.Game.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Save
{
    [Serializable]
    public class GlobalInfo : ISerializable
    {
        public POIGroup PointsOfInterest = new POIGroup();
        public InventoryGroup Inventories = new InventoryGroup();

        public static GlobalInfo DefaultInfo = new GlobalInfo();
        public static GlobalInfo PlayerInfo = new GlobalInfo();

        public static POIEntry GetPOI(int infoId)
        {
            POIEntry poi;

            if(PlayerInfo.PointsOfInterest.POIInfo.TryGetValue(infoId, out poi))
            {
                
            }
            else if(DefaultInfo.PointsOfInterest.POIInfo.TryGetValue(infoId, out poi))
            {

            }

            return poi;
        }

        public static InventoryEntry GetInventory(int infoId)
        {
            InventoryEntry inventory;

            if (PlayerInfo.Inventories.InventoryInfo.TryGetValue(infoId, out inventory))
            {

            }
            else if (DefaultInfo.Inventories.InventoryInfo.TryGetValue(infoId, out inventory))
            {
                
            }

            return inventory;
        }

        public static void OverwritePOI(POIEntry entry)
        {
            if (entry.IsDefault)
            {
                throw new Exception("Attempted to save a default POI to the player save");
            }
            else
            {
                PlayerInfo.PointsOfInterest.POIInfo.TryAdd(entry.Id, entry);
            }
        }

        public static void OverwriteInventory(InventoryEntry entry)
        {
            if (entry.IsDefault)
            {
                throw new Exception("Attempted to save a default Inventory to the player save");
            }
            else
            {
                PlayerInfo.Inventories.InventoryInfo.TryAdd(entry.Id, entry);
            }
        }

        /// <summary>
        /// This function should be called before attempting to write info to an entry.
        /// This ensures that default data will not be overwritten.
        /// </summary>
        /// <param name="entry"></param>
        public static void WillModify(ref POIEntry entry)
        {
            if (entry.IsDefault)
            {
                entry = new POIEntry(entry)
                {
                    IsDefault = false
                };

                OverwritePOI(entry);
            }
        }

        /// <summary>
        /// This function should be called before attempting to write info to an entry.
        /// This ensures that default data will not be overwritten.
        /// </summary>
        /// <param name="entry"></param>
        public static void WillModify(ref InventoryEntry entry)
        {
            if (entry.IsDefault)
            {
                entry = new InventoryEntry(entry)
                {
                    IsDefault = false
                };

                OverwriteInventory(entry);
            }
        }

        public static void LoadDefaultGlobalInfo()
        {
            string path = SerializerParams.DATA_BASE_PATH + "g_inf";

            if (!File.Exists(path))
            {
#if DEBUG
                WriteDefaultGlobalInfoFile();
#else
                throw new Exception("Global Info file not found.");
#endif
            }

            XmlSerializer serializer = new XmlSerializer(typeof(GlobalInfo));

            FileStream fs = new FileStream(path, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            GlobalInfo loadedState = (GlobalInfo)serializer.Deserialize(reader);

            loadedState.CompleteDeserialization();

            reader.Close();
            fs.Close();

            //Set all default global info here
            DefaultInfo = loadedState;
            POIGroup.DefaultInfo = loadedState.PointsOfInterest.POIInfo;
            InventoryGroup.DefaultInfo = loadedState.Inventories.InventoryInfo;

            //Fill the static campsite ids list so that 
            POIGroup.CampsiteIDs.Clear();
            foreach(var item in POIGroup.DefaultInfo)
            {
                if(item.Value.Type == POIType.Campsite)
                {
                    POIGroup.CampsiteIDs.Add(item.Value.Id);
                }
            }
        }

        public static void WriteDefaultGlobalInfoFile()
        {
            string path = SerializerParams.DATA_BASE_PATH + "g_inf";

            GlobalInfo globalInfo = new GlobalInfo()
            {
                PointsOfInterest = new POIGroup() { POIInfo = POIGroup.DefaultInfo },
                Inventories = new InventoryGroup() { InventoryInfo = InventoryGroup.DefaultInfo },
            };

            XmlSerializer serializer = new XmlSerializer(typeof(GlobalInfo));

            globalInfo.PrepareForSerialization();

            TextWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, globalInfo);

            writer.Close();
        }

        public void CompleteDeserialization()
        {
            PointsOfInterest.CompleteDeserialization();
            Inventories.CompleteDeserialization();
        }

        public void PrepareForSerialization()
        {
            PointsOfInterest.PrepareForSerialization();
            Inventories.PrepareForSerialization();  
        }

    }
}
