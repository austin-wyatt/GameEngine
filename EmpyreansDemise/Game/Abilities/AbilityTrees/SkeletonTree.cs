using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities
{
    public static class SkeletonTree
    {
        public static void Initialize()
        {
            AbilityTree skeletonTree = new AbilityTree() { TreeType = AbilityTreeType.Skeleton };

            skeletonTree.BasicAbility.Add(new AbilityTreeNode() 
            {
                ID = -1,
                Name = "Bony Bash",
                AbilityType = typeof(BonyBash),
                RelativePosition = new Vector2(0, 1),
                TreeType = AbilityTreeType.Skeleton
            });

            var mendBones = new AbilityTreeNode()
            {
                ID = 0,
                Name = "Mend Bones",
                AbilityType = typeof(MendBones),
                RelativePosition = new Vector2(0.31635115f, 0.78735596f),
                TreeType = AbilityTreeType.Skeleton
            };

            skeletonTree.EntryPoint = mendBones;

            var ancientArmor = new AbilityTreeNode()
            {
                ID = 1,
                Name = "Ancient Armor",
                AbilityType = typeof(AncientArmor),
                RelativePosition = new Vector2(0.39181894f, 0.67528665f),
                TreeType = AbilityTreeType.Skeleton
            };

            ancientArmor.AddConnection(mendBones);

            var strongBones = new AbilityTreeNode()
            {
                ID = 2,
                Name = "Strong Bones",
                AbilityType = typeof(StrongBones),
                RelativePosition = new Vector2(0.4564043f, 0.7710824f),
                TreeType = AbilityTreeType.Skeleton
            };

            ancientArmor.AddConnection(strongBones);

            AbilityTrees.AddTree(skeletonTree);
        }
    }
}
