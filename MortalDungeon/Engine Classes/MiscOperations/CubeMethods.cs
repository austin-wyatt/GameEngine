using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.MiscOperations
{
    public static class CubeMethods
    {
        public static Dictionary<Direction, Vector3i> CubeDirections = new Dictionary<Direction, Vector3i>
        {
            { Direction.SouthWest, new Vector3i(-1, 0, 1) },
            { Direction.South, new Vector3i(0, -1, 1) },
            { Direction.SouthEast, new Vector3i(1, -1, 0) },
            { Direction.NorthEast, new Vector3i(1, 0, -1) },
            { Direction.North, new Vector3i(0, 1, -1) },
            { Direction.NorthWest, new Vector3i(-1, 1, 0) },
            { Direction.None, new Vector3i(0, 0, 0) },
        };
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        public static Vector3 CubeLerp(Vector3 start, Vector3 end, float t)
        {
            return new Vector3(Lerp(start.X, end.X, t), Lerp(start.Y, end.Y, t), Lerp(start.Z, end.Z, t));
        }
        public static Vector3i CubeRound(Vector3 cube)
        {
            float rx = (float)Math.Round(cube.X);
            float ry = (float)Math.Round(cube.Y);
            float rz = (float)Math.Round(cube.Z);

            float x_diff = Math.Abs(rx - cube.X);
            float y_diff = Math.Abs(ry - cube.Y);
            float z_diff = Math.Abs(rz - cube.Z);

            if (x_diff > y_diff && x_diff > z_diff)
            {
                rx = -ry - rz;
            }
            else if (y_diff > (z_diff - 0.0001f))
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new Vector3i((int)rx, (int)ry, (int)rz);
        }
        public static Vector3i CubeNeighbor(Vector3i cube, Direction direction)
        {
            return cube + CubeDirections[direction];
        }

        public static Vector3i OffsetToCube(FeaturePoint offset)
        {
            Vector3i cubeCoord = new Vector3i
            {
                X = offset.X,
                Z = offset.Y - (offset.X + (offset.X & 1)) / 2
            };
            cubeCoord.Y = -cubeCoord.X - cubeCoord.Z;

            return cubeCoord;
        }
        public static int GetDistanceBetweenPoints(Vector3i a, Vector3i b)
        {
            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
        }

        public static Vector2i CubeToOffset(Vector3i cube)
        {
            Vector2i offsetCoord = new Vector2i
            {
                X = cube.X,
                Y = cube.Z + (cube.X + (cube.X & 1)) / 2
            };

            return offsetCoord;
        }

        public static Vector3i OffsetToCube(Vector2i offset)
        {
            Vector3i cubeCoord = new Vector3i
            {
                X = offset.X,
                Z = offset.Y - (offset.X + (offset.X & 1)) / 2
            };
            cubeCoord.Y = -cubeCoord.X - cubeCoord.Z;

            return cubeCoord;
        }

        public static Vector3i OffsetToCube(TilePoint offset)
        {
            return OffsetToCube(new Vector2i(offset.X, offset.Y));
        }

        public static Vector3i RotateCube(Vector3i cube, int rotations) 
        {
            Vector3i rotatedCube = cube;

            for (int i = 0; i < rotations; i++) 
            {
                Vector3i temp = new Vector3i(rotatedCube.X, rotatedCube.Y, rotatedCube.Z);

                rotatedCube.X = -temp.Y;
                rotatedCube.Y = -temp.Z;
                rotatedCube.Z = -temp.X;
            }

            return rotatedCube;
        }
    }
}
