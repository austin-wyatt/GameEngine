using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public enum TickDurationTarget 
    {
        OnRoundStart,
        OnUnitTurnStart,
        OnUnitTurnEnd
    }

    public class TemporaryVision
    {
        public List<BaseTile> TilesToReveal = new List<BaseTile>();
        public int Duration = 0;

        public TickDurationTarget TickTarget = TickDurationTarget.OnRoundStart;

        public Unit TargetUnit = null;

        public UnitTeam Team = UnitTeam.PlayerUnits;

        public void ClearTiles() 
        {
            TilesToReveal.Clear();
        }

        public void TickDuration() 
        {
            Duration--;
        }
    }
}
