using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Abilities
{
    [XmlType(TypeName = "AL")]
    [Serializable]
    public class AbilityLoadout
    {
        [XmlElement("Ait")]
        public List<AbilityLoadoutItem> Items = new List<AbilityLoadoutItem>();

        public AbilityLoadout() { }

        public static AbilityLoadout GenerateLoadoutFromTree(AbilityTreeType type, int abilityCount = 2)
        {
            AbilityLoadout loadout = new AbilityLoadout();

            if(AbilityTrees.FindTree(type, out var tree))
            {
                loadout.Items.Add(new AbilityLoadoutItem(type, isBasic: true));
                abilityCount--;

                List<int> nodeIds = new List<int>();

                for(int i = 0; i < tree.NodeCount; i++)
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
                    if (item.BasicAbility)
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
        public bool BasicAbility = false;
        [XmlElement("Acc")]
        public int CurrentCharges = -1;

        public AbilityLoadoutItem() { }

        public AbilityLoadoutItem(AbilityTreeType type, int nodeID = -1, string name = "", bool isBasic = false)
        {
            AbilityTreeType = type;
            NodeID = nodeID;
            NodeName = name;
            BasicAbility = isBasic;
        }
    }
}
