using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    internal enum TickDurationTarget 
    {
        OnRoundStart,
        OnUnitTurnStart,
        OnUnitTurnEnd
    }

    internal class TemporaryVision
    {
        internal List<BaseTile> TilesToReveal = new List<BaseTile>();
        internal int Duration = 0;

        internal TickDurationTarget TickTarget = TickDurationTarget.OnRoundStart;

        internal Unit TargetUnit = null;

        internal UnitTeam Team = UnitTeam.PlayerUnits;

        internal void ClearTiles() 
        {
            TilesToReveal.Clear();
        }

        internal void TickDuration() 
        {
            Duration--;
        }
    }
}
