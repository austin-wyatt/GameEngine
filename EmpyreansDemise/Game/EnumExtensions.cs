using Empyrean.Engine_Classes;
using Empyrean.Game.Items;
using Empyrean.Game.Structures;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game
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

        public static EquipmentSlot EquipmentSlot(this ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Weapon:
                    return Items.EquipmentSlot.Weapon_1 | Items.EquipmentSlot.Weapon_2;
                case ItemType.Armor:
                    return Items.EquipmentSlot.Armor;
                case ItemType.Gloves:
                    return Items.EquipmentSlot.Gloves;
                case ItemType.Boots:
                    return Items.EquipmentSlot.Boots;
                case ItemType.Jewelry:
                    return Items.EquipmentSlot.Jewelry_1 | Items.EquipmentSlot.Jewelry_2;
                case ItemType.Trinket:
                    return Items.EquipmentSlot.Trinket;
                case ItemType.Consumable:
                    return Items.EquipmentSlot.Consumable_1 | Items.EquipmentSlot.Consumable_2 | 
                        Items.EquipmentSlot.Consumable_3 | Items.EquipmentSlot.Consumable_4;
                default:
                    return Items.EquipmentSlot.None;
            }
        }

        public static string Name(this ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Weapon:
                    return "Weapon";
                case ItemType.Armor:
                    return "Armor";
                case ItemType.Gloves:
                    return "Gloves";
                case ItemType.Jewelry:
                    return "Jewelry";
                case ItemType.Trinket:
                    return "Trinket";
                case ItemType.Consumable:
                    return "Consumable";
                case ItemType.CraftingComponent:
                    return "Crafting material";
                case ItemType.BasicItem:
                    return "Item";
                default:
                    return "Item";
            }
        }

        public static string Name(this AnimationType animationType)
        {
            AnimationType[] types = (AnimationType[])Enum.GetValues(typeof(AnimationType));

            for(int i = types.Length - 1; i >= 0; i--)
            {
                if(types[i] <= animationType)
                {
                    return types[i].ToString() + "_" + (animationType - types[i]);
                }
            }

            return animationType.ToString();
        }
    }
}
