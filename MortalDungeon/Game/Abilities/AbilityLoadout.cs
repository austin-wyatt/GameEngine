using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    [Serializable]
    public class AbilityLoadout
    {
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
                        tree.BasicAbility.ApplyToUnit(unit);
                    }
                    else if (item.NodeName != "")
                    {
                        if (tree.GetNodeFromTreeByName(item.NodeName, out var node))
                        {
                            node.ApplyToUnit(unit);
                        }
                    }
                    else if (item.NodeID != -1)
                    {
                        if (tree.GetNodeFromTreeByID(item.NodeID, out var node))
                        {
                            node.ApplyToUnit(unit);
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class AbilityLoadoutItem
    {
        public AbilityTreeType AbilityTreeType;
        public int NodeID = -1;
        public string NodeName = "";
        public bool BasicAbility = false;

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
