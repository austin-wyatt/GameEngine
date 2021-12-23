using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public static class SkeletonTree
    {
        public static void Initialize()
        {
            AbilityTree skeletonTree = new AbilityTree() { TreeType = AbilityTreeType.Skeleton };

            skeletonTree.BasicAbility.Add(new AbilityTreeNode() 
            {
                ID = 0,
                Name = "Bony Bash",
                CreateAbility = (unit) => new BonyBash(unit),
            });

            var mendBones = new AbilityTreeNode()
            {
                ID = 0,
                Name = "Mend Bones",
                CreateAbility = (unit) => new MendBones(unit),
            };

            skeletonTree.EntryPoint = mendBones;

            var ancientArmor = new AbilityTreeNode()
            {
                ID = 1,
                Name = "Ancient Armor",
                CreateAbility = (unit) => new AncientArmor(unit),
            };

            ancientArmor.AddChild(mendBones);

            var strongBones = new AbilityTreeNode()
            {
                ID = 2,
                Name = "Strong Bones",
                CreateAbility = (unit) => new StrongBones(unit),
            };

            ancientArmor.AddChild(strongBones);

            AbilityTrees.AddTree(skeletonTree);
        }
    }
}
