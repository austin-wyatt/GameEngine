using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Serializers;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Structures
{
    public static class Buildings
    {
        public static List<Func<SerialiableBuildingSkeleton, Building>> CreateBuilding = new List<Func<SerialiableBuildingSkeleton, Building>>();
        static Buildings()
        {
            //tent
            CreateBuilding.Add((skeleton) =>
            {
                Tent tent = new Tent();

                tent.TilePattern = skeleton.TilePattern;

                tent.RotateTilePattern(skeleton.Rotations);

                return tent;
            });
        }
    }
}
