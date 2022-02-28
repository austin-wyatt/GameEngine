using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [Serializable]
    public class FeatureList : ISerializable
    {
        public List<FeatureListNode> Features = new List<FeatureListNode>();

        //add feature group designations
        //add feature group window and feature group edit screen

        public FeatureList() { }

        public void CompleteDeserialization()
        {

        }

        public void PrepareForSerialization()
        {

        }
    }

    [Serializable]
    public class FeatureListNode
    {
        public int Id = 0;

        public string DescriptiveName = "";

        public int LoadRadius;

        public int Layer;

        public FeaturePoint Origin;

        public int MapSize = 0;

        public int AnimationSetId = 0;

        public string GroupName = "";

        public int NameTextEntry = 0;

        public FeatureListNode() { }
        public FeatureListNode(Feature feature)
        {
            Id = feature.Id;
            DescriptiveName = feature.DescriptiveName;
            Origin = feature.Origin;
            Layer = feature.Layer;
            LoadRadius = feature.LoadRadius;

            MapSize = feature.MapSize;
            AnimationSetId = feature.AnimationSetId;

            NameTextEntry = feature.NameTextEntry;
        }
    }
}
