using MortalDungeon.Game.Abilities.AbilityClasses.Spider;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public static class SpiderTree
    {
        public static void Initialize()
        {
            AbilityTreeType type = AbilityTreeType.Spider;

            AbilityTree abilityTree = new AbilityTree() { TreeType = type };

            //banditTree.BasicAbility.Add(new AbilityTreeNode()
            //{
            //    ID = -1,
            //    Name = "Bony Bash",
            //    AbilityType = typeof(BonyBash),
            //    RelativePosition = new Vector2(0, 1),
            //    TreeType = AbilityTreeType.Skeleton
            //});

            var createWeakWeb = new AbilityTreeNode()
            {
                ID = 0,
                Name = "Create Weak Web",
                AbilityType = typeof(CreateWeakWeb),
                RelativePosition = new Vector2(0.31635115f, 0.78735596f),
                TreeType = type
            };

            abilityTree.EntryPoint = createWeakWeb;

            var webImmunity = new AbilityTreeNode()
            {
                ID = 1,
                Name = "Web Immunity",
                AbilityType = typeof(WebImmunity),
                RelativePosition = new Vector2(0.39181894f, 0.67528665f),
                TreeType = type
            };
            createWeakWeb.AddConnection(webImmunity);


            AbilityTrees.AddTree(abilityTree);
        }
    }
}
