using OpenTK.Mathematics;
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
                ID = -1,
                Name = "Bony Bash",
                CreateAbility = (unit) => new BonyBash(unit),
            });

            var mendBones = new AbilityTreeNode()
            {
                ID = 0,
                Name = "Mend Bones",
                CreateAbility = (unit) => new MendBones(unit),
                RelativePosition = new Vector2(0.31635115f, 0.78735596f)
            };

            skeletonTree.EntryPoint = mendBones;

            var ancientArmor = new AbilityTreeNode()
            {
                ID = 1,
                Name = "Ancient Armor",
                CreateAbility = (unit) => new AncientArmor(unit),
                RelativePosition = new Vector2(0.39181894f, 0.67528665f)
            };

            ancientArmor.AddConnection(mendBones);

            var strongBones = new AbilityTreeNode()
            {
                ID = 2,
                Name = "Strong Bones",
                CreateAbility = (unit) => new StrongBones(unit),
                RelativePosition = new Vector2(0.4564043f, 0.7710824f)
            };

            ancientArmor.AddConnection(strongBones);

            AbilityTrees.AddTree(skeletonTree);
        }
    }
}
