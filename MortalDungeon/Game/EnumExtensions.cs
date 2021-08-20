using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game
{
    public static class EnumExtensions
    {
        public static string Name(this TileType type) 
        {
            switch (type) 
            {
                case TileType.Water:
                    return "Water";
                case TileType.Grass:
                case TileType.Grass_2:
                    return "Grass";
                case TileType.Stone_1:
                case TileType.Stone_2:
                case TileType.Stone_3:
                    return "Stone";
                case TileType.Gravel:
                    return "Gravel";
                case TileType.WoodPlank:
                    return "Wood";
                case TileType.Dirt:
                    return "Dirt";
                default:
                    return type.ToString();
            }
        }

        public static string Name(this StructureEnum type)
        {
            switch (type)
            {
                case StructureEnum.Tree_1:
                case StructureEnum.Tree_2:
                    return "Tree";
                case StructureEnum.Rock_1:
                case StructureEnum.Rock_2:
                case StructureEnum.Rock_3:
                    return "Rock";
                case StructureEnum.Wall_Wood_1:
                case StructureEnum.Wall_Wood_Corner:
                    return "Wooden Wall";
                case StructureEnum.Wall_1:
                case StructureEnum.Wall_Corner:
                    return "Stone Wall";
                case StructureEnum.Wall_Wood_Door:
                    return "Wooden Door";
                case StructureEnum.Wall_Door:
                    return "Stone Door";
                default:
                    return type.ToString();
            }
        }
    }
}
