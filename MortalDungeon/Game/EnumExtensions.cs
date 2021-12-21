using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
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
                case TileType.Dead_Grass:
                    return "Dessicated Grass";
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
                case StructureEnum.Grave_1:
                case StructureEnum.Grave_2:
                case StructureEnum.Grave_3:
                    return "Grave";
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
                case StructureEnum.Wall_Iron_1:
                    return "Iron Fence";
                case StructureEnum.Wall_Iron_Door:
                    return "Iron Gate";
                default:
                    return type.ToString();
            }
        }

        public static SimplifiedTileType SimplifiedType(this TileType type) 
        {
            switch (type)
            {
                case TileType.Water:
                case TileType.AltWater:
                    return SimplifiedTileType.Water;
                case TileType.Grass:
                case TileType.Grass_2:
                case TileType.Dead_Grass:
                case TileType.AltGrass:
                case TileType.Dirt:
                    return SimplifiedTileType.Grass;
                case TileType.Stone_1:
                case TileType.Stone_2:
                case TileType.Stone_3:
                case TileType.Gravel:
                    return SimplifiedTileType.Stone;
                case TileType.WoodPlank:
                    return SimplifiedTileType.Wood;
                default:
                    return SimplifiedTileType.Unknown;
            }
        }

        public static bool BoolValue(this UnitCheckEnum val)
        {
            if (val == UnitCheckEnum.True || val == UnitCheckEnum.SoftTrue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether the check is for a "soft" true or false. Softness of a value indicates values 
        /// where only one of them is required whereas a normal true or false would cause the check to be 
        /// false if they did not match.
        /// </summary>
        public static bool IsSoft(this UnitCheckEnum val)
        {
            if (val == UnitCheckEnum.SoftFalse || val == UnitCheckEnum.SoftTrue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static long Hash(this UnitTeam team1, UnitTeam team2) 
        {
            List<int> teams = new List<int> { (int)team1, (int)team2 };
            teams.Sort();

            return ((long)teams[0] << 32) + teams[1];
        }

        public static void SetRelation(this UnitTeam team1, UnitTeam team2, Relation relation) 
        {
            UnitAI.SetTeamRelation(team1, team2, relation);
        }

        public static Relation GetRelation(this UnitTeam team1, UnitTeam team2)
        {
            if (team1 == team2)
                return Relation.Friendly;

            return UnitAI.GetTeamRelation(team1, team2);
        }

        public static string Name(this UnitTeam team) 
        {
            switch (team) 
            {
                case UnitTeam.Unknown:
                    return "Unknown";
                case UnitTeam.PlayerUnits:
                    return "Player Units";
                case UnitTeam.BadGuys:
                    return "Bad Guys";
                case UnitTeam.Skeletons:
                    return "Skeletons";
                default:
                    return "<Unit Team>";
            }
        }

        public static string Name(this ControlType controlType)
        {
            switch (controlType)
            {
                case ControlType.Basic_AI:
                    return "Basic AI";
                case ControlType.Controlled:
                    return "Player Controlled";
                default:
                    return "<Control Type>";
            }
        }
    }
}
