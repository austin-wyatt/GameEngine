using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [XmlType(TypeName = "AL")]
    [Serializable]
    public class AbilityLoadout
    {
        [XmlElement("Ait")]
        public List<AbilityLoadoutItem> Items = new List<AbilityLoadoutItem>();

        [XmlElement("An")]
        public string Name = "";

        [XmlElement("AId")]
        public int Id = 0;

        public static AbilityLoadout GenerateLoadoutFromTree(AbilityTreeType type, int abilityCount = 2)
        {
            AbilityLoadout loadout = new AbilityLoadout();

            if (AbilityTrees.FindTree(type, out var tree))
            {
                loadout.Items.Add(new AbilityLoadoutItem(type, isBasic: true));
                abilityCount--;

                List<int> nodeIds = new List<int>();

                for (int i = 0; i < tree.NodeCount; i++)
                {
                    nodeIds.Add(i);
                }

                for (int i = 0; i < abilityCount; i++)
                {
                    int id = nodeIds.GetRandom();

                    loadout.Items.Add(new AbilityLoadoutItem(type, nodeID: id));

                    nodeIds.Remove(id);
                }
            }


            return loadout;
        }
        public void ApplyLoadoutToUnit(Unit unit)
        {
            foreach (var item in Items)
            {
                if (AbilityTrees.FindTree(item.AbilityTreeType, out var tree))
                {
                    if (item.BasicAbility > 0)
                    {
                        tree.BasicAbility[0].ApplyToUnit(unit, item);
                    }
                    else if (item.NodeName != "")
                    {
                        if (tree.GetNodeFromTreeByName(item.NodeName, out var node))
                        {
                            node.ApplyToUnit(unit, item);
                        }
                    }
                    else if (item.NodeID != -1)
                    {
                        if (tree.GetNodeFromTreeByID(item.NodeID, out var node))
                        {
                            node.ApplyToUnit(unit, item);
                        }
                    }
                }
            }
        }

        public AbilityLoadout() { }
    }

    [XmlType(TypeName = "ALI")]
    [Serializable]
    public class AbilityLoadoutItem
    {
        [XmlElement("Aty")]
        public AbilityTreeType AbilityTreeType;
        [XmlElement("Aid")]
        public int NodeID = -1;
        [XmlElement("Ana")]
        public string NodeName = "";
        [XmlElement("Ab")]
        public int BasicAbility = 0;

        [XmlElement("Acc")]
        public int CurrentCharges = -1;
        [XmlElement("ALm")]
        public int MaxCharges = -1;

        /// <summary>
        /// Descriptive name because we have to manually map all of these without an automatic process
        /// </summary>
        [XmlElement("ALn")]
        public string Name = "";

        public AbilityLoadoutItem() { }

        public AbilityLoadoutItem(AbilityTreeType type, int nodeID = -1, string name = "", bool isBasic = false)
        {
            AbilityTreeType = type;
            NodeID = nodeID;
            NodeName = name;
            BasicAbility = isBasic ? 1 : 0;
        }
    }
}
