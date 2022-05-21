using Empyrean.Game.Abilities;
using Empyrean.Game.Map;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Combat
{
    public class UnitMorsels
    {
        const int MAX_TURN_MORSELS = 10;

        public Unit Unit;

        public List<InformationMorsel> TurnMorsels = new List<InformationMorsel>();

        private InformationMorsel _actionMorsel;
        public InformationMorsel ActionMorsel 
        { 
            get
            {
                if(_actionMorsel == null)
                {
                    CreateActionMorsel();
                }

                return _actionMorsel;
            }
            private set => _actionMorsel = value;
        }

        public UnitMorsels(Unit unit)
        {
            Unit = unit;
            //add consideration for active teams 

            //turn morsels also need to update when the unit takes damage and etc
        }

        public void CreateTurnMorsel()
        {
            TurnMorsels.Add(new InformationMorsel(Unit));
            if(TurnMorsels.Count > MAX_TURN_MORSELS)
            {
                TurnMorsels.RemoveAt(0);
            }
        }

        public void CreateActionMorsel()
        {
            ActionMorsel = new InformationMorsel(Unit);
        }



        public override bool Equals(object obj)
        {
            return obj is UnitMorsels morsels &&
                   EqualityComparer<Unit>.Default.Equals(Unit, morsels.Unit);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Unit);
        }
    }

    public class InformationMorsel
    {
        public FeaturePoint Position;

        public List<BuffMorsel> BuffMorsels = new List<BuffMorsel>();
        public float Health = 0;
        public float Shields = 0;
        public Unit Unit;

        public bool Alive = true;
        public UnitTeam Team;
        public UnitTeam BaseTeam;

        public InformationMorsel(Unit unit)
        {
            Unit = unit;

            if (unit.Info.TileMapPosition != null)
            {
                Position = unit.Info.TileMapPosition.ToFeaturePoint();
            }
            else
            {
                Position = new FeaturePoint(int.MinValue, int.MinValue);
            }

            foreach(var buff in unit.Info.BuffManager.Buffs)
            {
                BuffMorsels.Add(new BuffMorsel(buff));
            }

            Health = unit.GetResF(ResF.Health);
            Shields = unit.GetResI(ResI.Shields);
            Alive = !unit.Info.Dead;
            Team = unit.AI.GetTeam();
            BaseTeam = unit.AI.Team;
        }
    }

    public class BuffMorsel
    {
        public string Identifier;
        public int Stacks;
        public int Duration;

        public BuffMorsel(Buff buff)
        {
            Identifier = buff.Identifier;
            Stacks = buff.Stacks;
            Duration = buff.Duration;
        }
    }
}
