using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Game.Combat;
using Empyrean.Game.Map;
using Empyrean.Game.Movement.Animations;
using Empyrean.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Movement
{
    public static class MovementHelper
    {

        //Calculate forced movement given angle and force
        public static MoveContract CalculateForcedMovement(Tile source, float direction, float magnitude, NavType navType = NavType.Base)
        {
            MoveContract contract = new MoveContract();
            contract.Intent = MoveIntent.Forced;


            Vector2 destPoint = new Vector2(MathF.Cos(direction) * magnitude, -MathF.Sin(direction) * magnitude);



            Vector3i destCube = CubeMethods.PixelToCube(destPoint);

            Vector3i empty = new Vector3i(0, 0, 0);
            float distance = CubeMethods.GetDistanceBetweenPoints(empty, destCube);

            FeaturePoint sourceFeaturePoint = source.ToFeaturePoint();

            destCube = CubeMethods.OffsetToCube(sourceFeaturePoint) + destCube;

            FeaturePoint destFeaturePoint = CubeMethods.CubeToFeaturePoint(destCube);


            TileMapManager.NavMesh.GetLineToPoint(sourceFeaturePoint, destFeaturePoint, navType, out List<Tile> tileList);

            for(int i = 0; i < tileList.Count - 1; i++)
            {
                MoveNode node = new MoveNode()
                {
                    Source = tileList[i],
                    Destination = tileList[i + 1]
                };

                contract.Moves.Add(node);

                contract.Viable = true;
            }

            contract.MoveAnimation = new StraightLineMove(contract);

            return contract;
        }
    }
}
