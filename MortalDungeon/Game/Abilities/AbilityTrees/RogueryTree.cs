using Empyrean.Game.Abilities.AbilityClasses.Bandit;
using Empyrean.Game.Abilities.AbilityClasses.Roguery;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities
{
    public static class RogueryTree
    {
        public static void Initialize()
        {
            AbilityTreeType type = AbilityTreeType.Roguery;

            AbilityTree banditTree = new AbilityTree() { TreeType = type };

            //banditTree.BasicAbility.Add(new AbilityTreeNode()
            //{
            //    ID = -1,
            //    Name = "Bony Bash",
            //    AbilityType = typeof(BonyBash),
            //    RelativePosition = new Vector2(0, 1),
            //    TreeType = AbilityTreeType.Skeleton
            //});

            var suckerPunch = new AbilityTreeNode()
            {
                ID = 0,
                Name = "Sucker Punch",
                AbilityType = typeof(SuckerPunch),
                RelativePosition = new Vector2(0.31635115f, 0.78735596f),
                TreeType = type
            };

            banditTree.EntryPoint = suckerPunch;

            var throwDirt = new AbilityTreeNode()
            {
                ID = 1,
                Name = "Throw Dirt",
                AbilityType = typeof(ThrowDirt),
                RelativePosition = new Vector2(0.41635115f, 0.88735596f),
                TreeType = type
            };

            suckerPunch.AddConnection(throwDirt);


            var shove = new AbilityTreeNode()
            {
                ID = 2,
                Name = "Shove",
                AbilityType = typeof(Roguery_Shove),
                RelativePosition = new Vector2(0.51635115f, 0.88735596f),
                TreeType = type
            };

            suckerPunch.AddConnection(shove);

            AbilityTrees.AddTree(banditTree);
        }
    }
}
