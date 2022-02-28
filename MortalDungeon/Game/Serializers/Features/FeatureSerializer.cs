using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public static class FeatureSerializer
    {
        public static void CreateFeatureListFile()
        {
            FeatureBlockManager.LoadAllFeatureBlocks();

            List<Feature> features = FeatureBlockManager.GetAllLoadedFeatures();

            string featureListPath = SerializerParams.DATA_BASE_PATH +"qqqqqqqqqqqqqqqq.f";

            if (File.Exists(featureListPath))
            {
                File.Delete(featureListPath);
            }

            FeatureList list = new FeatureList();

            foreach (var feature in features)
            {
                list.Features.Add(new FeatureListNode(feature));
            }

            list.Features.Sort((a, b) => a.Id.CompareTo(b.Id));

            list.PrepareForSerialization();

            XmlSerializer serializer = new XmlSerializer(typeof(FeatureList));

            TextWriter writer = new StreamWriter(featureListPath);

            serializer.Serialize(writer, list);

            writer.Close();
        }

        public static FeatureList LoadFeatureListFile()
        {
            string featureListPath = SerializerParams.DATA_BASE_PATH + "qqqqqqqqqqqqqqqq.f";

            if (!File.Exists(featureListPath))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(FeatureList));

            FileStream fs = new FileStream(featureListPath, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            FeatureList featureList = (FeatureList)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            featureList.CompleteDeserialization();

            return featureList;
        }

        public static void CreateFeatureGroupFile()
        {
            FeatureGroupManager.FeatureGroups.PrepareForSerialization();

            string featureGroupPath = SerializerParams.DATA_BASE_PATH + "gggggggggggggggg.f";

            if (File.Exists(featureGroupPath))
            {
                File.Delete(featureGroupPath);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(FeatureGroupList));

            TextWriter writer = new StreamWriter(featureGroupPath);

            serializer.Serialize(writer, FeatureGroupManager.FeatureGroups);

            writer.Close();
        }

        public static FeatureGroupList LoadFeatureGroupFile()
        {
            string featureGroupPath = SerializerParams.DATA_BASE_PATH + "gggggggggggggggg.f";

            if (!File.Exists(featureGroupPath))
            {
                return new FeatureGroupList();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(FeatureGroupList));

            FileStream fs = new FileStream(featureGroupPath, FileMode.Open);

            TextReader reader = new StreamReader(fs);


            FeatureGroupList featureList = (FeatureGroupList)serializer.Deserialize(reader);

            reader.Close();
            fs.Close();

            featureList.CompleteDeserialization();

            return featureList;
        }
    }
}
