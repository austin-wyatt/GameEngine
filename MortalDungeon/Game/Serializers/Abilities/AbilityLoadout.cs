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
        public int Id = -1;

        public static AbilityLoadout GenerateLoadoutFromTree(AbilityTreeType type, int abilityCount = 2)
        {
            AbilityLoadout loadout = new AbilityLoadout();

            if (AbilityTrees.FindTree(type, out var tree))
            {
                loadout.Items.Add(new AbilityLoadoutItem(type));
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
                    if (tree.GetNodeFromTreeByID(item.NodeID, out var n))
                    {
                        n.ApplyToUnit(unit, item);
                    }
                    else if (item.NodeName != "")
                    {
                        if (tree.GetNodeFromTreeByName(item.NodeName, out var node))
                        {
                            node.ApplyToUnit(unit, item);
                        }
                    }
                }
            }
        }

        public AbilityLoadout() { }

        public AbilityLoadout(AbilityLoadout loadout)
        {
            foreach(var item in loadout.Items)
            {
                Items.Add(new AbilityLoadoutItem(item));
            }

            Name = loadout.Name;
            Id = loadout.Id;
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

        [XmlElement("Acc")]
        public int CurrentCharges = -1;
        [XmlElement("ALm")]
        public int MaxCharges = -1;

        [XmlElement("ALmod")]
        public int Modifier = 0;



        /// <summary>
        /// Descriptive name because we have to manually map all of these without an automatic process
        /// </summary>
        [XmlElement("ALn")]
        public string Name = "";

        public AbilityLoadoutItem() { }

        public AbilityLoadoutItem(AbilityTreeType type, int nodeID = -1, string name = "")
        {
            AbilityTreeType = type;
            NodeID = nodeID;
            NodeName = name;
        }

        public AbilityLoadoutItem(AbilityTreeNode node)
        {
            AbilityTreeType = node.TreeType;
            NodeID = node.ID;
            NodeName = node.Name;
        }

        public AbilityLoadoutItem(AbilityLoadoutItem item)
        {
            AbilityTreeType = item.AbilityTreeType;
            NodeID = item.NodeID;
            NodeName = item.NodeName;
            CurrentCharges = item.CurrentCharges;
            MaxCharges = item.MaxCharges;
            Modifier = item.Modifier;
        }

        public Ability GetAbilityFromLoadoutItem(Unit unit)
        {
            if (AbilityTrees.FindTree(AbilityTreeType, out var tree))
            {
                //if (BasicAbility > 0)
                //{
                //    return tree.BasicAbility[0].CreateAbility(unit);
                //}
                //else if (NodeName != "")
                if (NodeName != "")
                {
                    if (tree.GetNodeFromTreeByName(NodeName, out var node))
                    {
                        return node.CreateAbility(unit);
                    }
                }
                else if (NodeID != -1)
                {
                    if (tree.GetNodeFromTreeByID(NodeID, out var node))
                    {
                        return node.CreateAbility(unit);
                    }
                }
            }

            return null;
        }
    }
}
