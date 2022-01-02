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
        public static List<Func<SerialiableBuildingSkeleton, Building>> CreateBuildings = new List<Func<SerialiableBuildingSkeleton, Building>>();
        static Buildings()
        {
            //tent
            CreateBuildings.Add((skeleton) =>
            {
                Tent tent = new Tent();

                List<Vector3i> cubeCoords = new List<Vector3i>();

                foreach(var point in skeleton.TilePattern)
                {
                    cubeCoords.Add(CubeMethods.OffsetToCube(point));
                }

                tent.TilePattern = cubeCoords;

                tent.RotateTilePattern(skeleton.Rotations);

                return tent;
            });
        }
    }
}
