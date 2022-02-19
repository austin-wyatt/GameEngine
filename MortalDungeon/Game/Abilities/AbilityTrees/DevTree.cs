using OpenTK.Mathematics;
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

            //devTree.BasicAbility.Add(new AbilityTreeNode()
            //{
            //    ID = -1,
            //    Name = "Strike",
            //    AbilityType = typeof(Strike),
            //    RelativePosition = new Vector2(1, 1)
            //});

            int id = 0;

            var smiteDev = new AbilityTreeNode()
            {
                ID = id++,
                Name = "Smite_dev",
                AbilityType = typeof(Smite_dev),
                RelativePosition = new Vector2(0.31635115f, 0.78735596f),
                TreeType = AbilityTreeType.Dev
            };

            var rangedAoe = new AbilityTreeNode()
            {
                ID = id++,
                Name = "RangedAOE",
                AbilityType = typeof(TemplateRangedAOE),
                RelativePosition = new Vector2(0, 0),
                TreeType = AbilityTreeType.Dev
            };
            smiteDev.AddConnection(rangedAoe);

            devTree.EntryPoint = smiteDev;


            AbilityTrees.AddTree(devTree);
        }
    }
}
