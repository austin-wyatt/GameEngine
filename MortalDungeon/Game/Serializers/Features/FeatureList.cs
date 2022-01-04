using MortalDungeon.Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [Serializable]
    public class FeatureList
    {
        public List<FeatureListNode> Features = new List<FeatureListNode>();

        public FeatureList() { }
    }


    [Serializable]
    public class FeatureListNode
    {
        public int Id = 0;

        public string DescriptiveName = "";

        public int LoadRadius;

        public int Layer;

        public FeaturePoint Origin;

        public FeatureListNode() { }
        public FeatureListNode(Feature feature)
        {
            Id = feature.Id;
            DescriptiveName = feature.DescriptiveName;
            Origin = feature.Origin;
            Layer = feature.Layer;
            LoadRadius = feature.LoadRadius;
        }
    }
}
