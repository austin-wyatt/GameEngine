using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public enum AbilityTreeType
    {
        None,
        Skeleton
    }
    public static class AbilityTrees
    {
        public static List<AbilityTree> Trees = new List<AbilityTree>();

        static AbilityTrees()
        {
            SkeletonTree.Initialize();
        }

        public static bool FindTree(AbilityTreeType type, out AbilityTree tree)
        {
            tree = Trees.Find(t => t.TreeType == type);

            return tree != null;
        }

        public static void AddTree(AbilityTree tree) 
        {
            List<AbilityTreeNode> nodeQueue = new List<AbilityTreeNode>();

            HashSet<int> visited = new HashSet<int>();

            nodeQueue.Add(tree.EntryPoint);

            while (nodeQueue.Count > 0)
            {
                foreach (AbilityTreeNode child in nodeQueue[0].Children)
                {
                    if (!visited.Contains(child.ID))
                    {
                        nodeQueue.Add(child);
                        visited.Add(child.ID);
                    }
                }
                foreach (AbilityTreeNode parent in nodeQueue[0].Parents)
                {
                    if (!visited.Contains(parent.ID))
                    {
                        nodeQueue.Add(parent);
                        visited.Add(parent.ID);
                    }
                }

                nodeQueue.RemoveAt(0);
            }

            tree.NodeCount = visited.Count;

            Trees.Add(tree);
        }
    }

    public class AbilityTree
    {
        public AbilityTreeNode EntryPoint;
        public AbilityTreeType TreeType;

        public List<AbilityTreeNode> BasicAbility = new List<AbilityTreeNode>(); //guaranteed to be orphaned

        public int NodeCount = 0;

        public bool GetNodeFromTreeByID(int id, out AbilityTreeNode node)
        {
            List<AbilityTreeNode> nodeQueue = new List<AbilityTreeNode>();

            HashSet<int> visited = new HashSet<int>();

            nodeQueue.Add(EntryPoint);

            while(nodeQueue.Count > 0)
            {
                if(nodeQueue[0].ID == id) 
                {
                    node = nodeQueue[0];
                    return true;
                }
                else
                {
                    foreach(AbilityTreeNode child in nodeQueue[0].Children)
                    {
                        if (!visited.Contains(child.ID))
                        {
                            nodeQueue.Add(child);
                            visited.Add(child.ID);
                        }
                    }
                    foreach (AbilityTreeNode parent in nodeQueue[0].Parents)
                    {
                        if (!visited.Contains(parent.ID))
                        {
                            nodeQueue.Add(parent);
                            visited.Add(parent.ID);
                        }
                    }

                    nodeQueue.RemoveAt(0);
                }
            }

            node = null;
            return false;
        }

        public bool GetNodeFromTreeByName(string name, out AbilityTreeNode node)
        {
            List<AbilityTreeNode> nodeQueue = new List<AbilityTreeNode>();

            HashSet<int> visited = new HashSet<int>();

            nodeQueue.Add(EntryPoint);

            while (nodeQueue.Count > 0)
            {
                if (nodeQueue[0].Name == name)
                {
                    node = nodeQueue[0];
                    return true;
                }
                else
                {
                    foreach (AbilityTreeNode child in nodeQueue[0].Children)
                    {
                        if (!visited.Contains(child.ID))
                        {
                            nodeQueue.Add(child);
                            visited.Add(child.ID);
                        }
                    }
                    foreach (AbilityTreeNode parent in nodeQueue[0].Parents)
                    {
                        if (!visited.Contains(parent.ID))
                        {
                            nodeQueue.Add(parent);
                            visited.Add(parent.ID);
                        }
                    }

                    nodeQueue.RemoveAt(0);
                }
            }

            node = null;
            return false;
        }
    }

    public class AbilityTreeNode 
    {
        public List<AbilityTreeNode> Parents = new List<AbilityTreeNode>();
        public List<AbilityTreeNode> Children = new List<AbilityTreeNode>();

        //ID should roughly correlate to strength of the ability (although that should be added as an actual field)
        public int ID = 0;

        public string Name;
        public Func<Unit, Ability> CreateAbility;

        public void AddChild(AbilityTreeNode child) 
        {
            Children.Add(child);
            child.Parents.Add(this);
        }

        public void AddParent(AbilityTreeNode parent)
        {
            Parents.Add(parent);
            parent.Children.Add(this);
        }

        public void ApplyToUnit(Unit unit, AbilityLoadoutItem item)
        {
            var ability = CreateAbility(unit);

            if(item.CurrentCharges != -1)
            {
                ability.Charges = item.CurrentCharges;
            }

            ability.Name = Name;

            ability.AbilityTreeType = item.AbilityTreeType;
            ability.NodeID = item.NodeID;
            ability.BasicAbility = item.BasicAbility;

            unit.Info.Abilities.Add(ability);
        }
    }
}
