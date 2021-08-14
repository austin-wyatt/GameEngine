using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public enum CliffFaces 
    {
        None = -1,
        NorthWest = 1,
        SouthWest = 2,
        South = 4,
        SouthEast = 8,
        NorthEast = 16,
        North = 32
    }

    public class Cliff
    {
        public BaseTile Tile;
        public List<Structure> CliffStructure = new List<Structure>();

        public short CliffBitArray;

        public Cliff(CombatScene scene, BaseTile tile, short cliffBitArray)
        {
            Tile = tile;
            CliffBitArray = cliffBitArray;

            int cliffLength = 0;
            Direction direction;

            if (CliffBitArray == 63)
            {
                GenerateCliff(scene, Direction.NorthWest, 6);
            }
            else if(CliffBitArray != 0)
            {
                List<short> bitArrays = new List<short>();
                short currBitArray = 0;
                //loop until you find an empty spot then start a fresh direction loop to create as many bit arrays as necessary. (when an empty spot is found in the direction loop start a new bit array)
                CliffFaces cliffFace;
                Direction startingDirection = Direction.None;

                int firstIndex = 0;
                for (int i = 0; i < 6; i++) 
                {
                    if ((cliffBitArray & (short)DirectionToCliffFace((Direction)i)) == 0)
                    {
                        firstIndex = i;
                        break;
                    }
                }

                bool loop = false;
                for (int i = firstIndex; i < 6; i++) 
                {
                    if ((Direction)i == startingDirection)
                    {
                        break;
                    }

                    cliffFace = DirectionToCliffFace((Direction)i);

                    if ((cliffBitArray & (short)cliffFace) == 0)
                    {
                        if (currBitArray != 0)
                        {
                            bitArrays.Add(currBitArray);
                            currBitArray = 0;
                        }
                    }
                    else 
                    {
                        currBitArray += (short)cliffFace;

                        if (loop == false && startingDirection == Direction.None)
                        {
                            startingDirection = (Direction)i;
                            loop = true;
                        }
                    }

                    if (i == 5 && loop || (startingDirection == Direction.None && i == 5)) 
                    {
                        i = -1;
                        loop = false;
                    }
                }

                if (bitArrays.Count > 1) 
                {
                    Console.WriteLine(bitArrays);
                }

                bitArrays.ForEach(arr =>
                {
                    cliffLength = 0;
                    CliffFaces lastFace = CliffFaces.None;
                    for (int i = 0; i < 6; i++)
                    {
                        CliffFaces cliffFace = DirectionToCliffFace((Direction)i);

                        if ((arr & (short)cliffFace) > 0)
                        {
                            lastFace = cliffFace;

                            cliffLength++;
                        }
                    }

                    while ((arr & (short)GetNextFace(lastFace)) > 0)
                    {
                        lastFace = GetNextFace(lastFace);
                    }

                    direction = CliffFaceToDirection(lastFace);

                    GenerateCliff(scene, direction, cliffLength);
                });
            }


            tile.Cliff = this;
        }

        public void GenerateCliff(CombatScene scene, Direction direction, int length) 
        {
            Structure cliffStructure = new Structure(scene, Spritesheets.StructureSheet, (int)Structures.Cliff_1 + length - 1, Tile.Position + new Vector3(0, 0, 0.005f));
            CliffStructure.Add(cliffStructure);

            cliffStructure.SelectionTile = null;

            Tile.Chunk.Structures.Add(cliffStructure);

            switch (direction)
            {
                case Direction.North:
                    cliffStructure.BaseObject.BaseFrame.RotateZ(5 * 60);
                    break;
                case Direction.NorthEast:
                    cliffStructure.BaseObject.BaseFrame.RotateZ(4 * 60);
                    break;
                case Direction.SouthEast:
                    cliffStructure.BaseObject.BaseFrame.RotateZ(3 * 60);
                    break;
                case Direction.South:
                    cliffStructure.BaseObject.BaseFrame.RotateZ(2 * 60);
                    break;
                case Direction.SouthWest:
                    cliffStructure.BaseObject.BaseFrame.RotateZ(1 * 60);
                    break;
                case Direction.NorthWest:
                default:
                    break;
            }
        }

        public void ClearCliff() 
        {
            CliffStructure.ForEach(structure =>
            {
                Tile.Chunk.Structures.Remove(structure);
            });

            CliffStructure.Clear();
        }

        public static CliffFaces DirectionToCliffFace(Direction direction) 
        {
            switch (direction) 
            {
                case Direction.SouthWest:
                    return CliffFaces.SouthWest;
                case Direction.South:
                    return CliffFaces.South;
                case Direction.SouthEast:
                    return CliffFaces.SouthEast;
                case Direction.NorthEast:
                    return CliffFaces.NorthEast;
                case Direction.North:
                    return CliffFaces.North;
                default:
                    return CliffFaces.NorthWest;
            }
        }

        public static Direction CliffFaceToDirection(CliffFaces face)
        {
            switch (face)
            {
                case CliffFaces.SouthWest:
                    return Direction.SouthWest;
                case CliffFaces.South:
                    return Direction.South;
                case CliffFaces.SouthEast:
                    return Direction.SouthEast;
                case CliffFaces.NorthEast:
                    return Direction.NorthEast;
                case CliffFaces.North:
                    return Direction.North;
                default:
                    return Direction.NorthWest;
            }
        }

        private CliffFaces GetNextFace(CliffFaces face) 
        {
            switch(face)
            {
                case CliffFaces.North:
                    return CliffFaces.NorthWest;
                case CliffFaces.SouthWest:
                    return CliffFaces.South;
                case CliffFaces.South:
                    return CliffFaces.SouthEast;
                case CliffFaces.SouthEast:
                    return CliffFaces.NorthEast;
                case CliffFaces.NorthEast:
                    return CliffFaces.North;
                default:
                    return CliffFaces.SouthWest;
            }
        }
    }
}
