using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public static class DevTree
    {
        public static void Initialize()
        {
            AbilityTree devTree = new AbilityTree() { TreeType = AbilityTreeType.Dev };

            devTree.BasicAbility.Add(new AbilityTreeNode()
            {
                ID = 0,
                Name = "Strike",
                CreateAbility = (unit) => new Strike(unit),
            });

            var mendBones = new AbilityTreeNode()
            {
                ID = 0,
                Name = "Smite_dev",
                CreateAbility = (unit) => new Smite_dev(unit),
            };

            devTree.EntryPoint = mendBones;


            AbilityTrees.AddTree(devTree);
        }
    }
}
