using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Structures
{
    public enum CliffFace 
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
                CliffFace cliffFace;
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
                    CliffFace lastFace = CliffFace.None;
                    for (int i = 0; i < 6; i++)
                    {
                        CliffFace cliffFace = DirectionToCliffFace((Direction)i);

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
            Structure cliffStructure = new Structure(scene, Spritesheets.StructureSheet, (int)StructureEnum.Cliff_1 + length - 1, Tile.Position + new Vector3(0, 0, 0.008f));

            cliffStructure.SetColor(new Vector4(0.5f, 0.5f, 0.5f, 1) + Tile.Properties.Height * new Vector4(0.05f, 0.05f, 0.05f, 0));
            CliffStructure.Add(cliffStructure);

            cliffStructure.SelectionTile.CleanUp();
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

        public static CliffFace DirectionToCliffFace(Direction direction) 
        {
            return direction switch
            {
                Direction.SouthWest => CliffFace.SouthWest,
                Direction.South => CliffFace.South,
                Direction.SouthEast => CliffFace.SouthEast,
                Direction.NorthEast => CliffFace.NorthEast,
                Direction.North => CliffFace.North,
                _ => CliffFace.NorthWest,
            };
        }

        public static Direction CliffFaceToDirection(CliffFace face)
        {
            return face switch
            {
                CliffFace.SouthWest => Direction.SouthWest,
                CliffFace.South => Direction.South,
                CliffFace.SouthEast => Direction.SouthEast,
                CliffFace.NorthEast => Direction.NorthEast,
                CliffFace.North => Direction.North,
                _ => Direction.NorthWest,
            };
        }

        private CliffFace GetNextFace(CliffFace face) 
        {
            return face switch
            {
                CliffFace.North => CliffFace.NorthWest,
                CliffFace.SouthWest => CliffFace.South,
                CliffFace.South => CliffFace.SouthEast,
                CliffFace.SouthEast => CliffFace.NorthEast,
                CliffFace.NorthEast => CliffFace.North,
                _ => CliffFace.SouthWest,
            };
        }
    }
}
