using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.UI;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public enum UnitTeam 
    {
        Ally,
        Enemy,
        Neutral
    }

    public class Unit : GameObject //main unit class. Tracks position on tilemap
    {
        public int TileMapPosition = -1; //can be anything from -1 to infinity. If the value is below 0 then it is not being positioned on the tilemap
        public List<Ability> Abilities = new List<Ability>();

        public int MaxEnergy = 10;
        public int CurrentEnergy = 10;
        public int Health = 100;

        public UnitTeam Team = UnitTeam.Ally;

        public bool BlocksSpace = true;
        public bool PhasedMovement = false;
        public bool BlocksVision = false;

        public int VisionRadius = 6;

        public Direction Facing = Direction.North;

        public TileMap CurrentTileMap;


        public Unit() { }
        public Unit(Vector3 position, int tileMapPosition = 0, int id = 0, string name = "Unit")
        {
            Position = position;
            Name = name;
        }

        public Ability GetFirstAbilityOfType(AbilityTypes type)
        {
            for (int i = 0; i < Abilities.Count; i++)
            {
                if (Abilities[i].Type == type)
                    return Abilities[i];
            }

            return new Ability();
        }

        public List<Ability> GetAbilitiesOfType(AbilityTypes type)
        {
            List<Ability> abilities = new List<Ability>();
            for (int i = 0; i < Abilities.Count; i++)
            {
                if (Abilities[i].Type == type)
                    abilities.Add(Abilities[i]);
            }

            return abilities;
        }
    }
}
