using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public static class FeatureGroupManager
    {
        public static FeatureGroupList FeatureGroups;

        static FeatureGroupManager()
        {
            FeatureGroups = FeatureSerializer.LoadFeatureGroupFile();
        }

        public static void WriteFeatureGroupFile()
        {
            FeatureSerializer.CreateFeatureGroupFile();
            FeatureGroups.RefillGroupsDict();
        }

        public static void AddGroup(FeatureGroup featureGroup)
        {
            FeatureGroups.Groups.Add(featureGroup);
            FeatureGroups.RefillGroupsDict();
        }

        public static void RemoveGroup(FeatureGroup featureGroup)
        {
            FeatureGroups.Groups.Remove(featureGroup);
            FeatureGroups.RefillGroupsDict();
        }
    }


    [Serializable]
    public class FeatureGroupList : ISerializable
    {
        [XmlIgnore]
        public Dictionary<string, FeatureGroup> GroupsDict = new Dictionary<string, FeatureGroup>();

        public List<FeatureGroup> Groups = new List<FeatureGroup>();

        public FeatureGroupList() { }

        public void RefillGroupsDict()
        {
            GroupsDict.Clear();
            foreach (var group in Groups)
            {
                GroupsDict.TryAdd(group.GroupName, group);
            }
        }

        public void CompleteDeserialization()
        {
            GroupsDict = new Dictionary<string, FeatureGroup>();

            RefillGroupsDict();
        }

        public void PrepareForSerialization()
        {

        }
    }

    [Serializable]
    public class FeatureGroup
    {
        public List<int> FeatureIds = new List<int>();
        public string GroupName = "<default>";


    }
}
