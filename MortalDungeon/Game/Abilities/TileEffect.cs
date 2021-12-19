using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    internal class TileEffect
    {
        internal class TileEffectEventArgs
        {
            internal Unit Unit;
            internal BaseTile Tile;
            internal TileEffectEventArgs(Unit unit, BaseTile tile)
            {
                Unit = unit;
                Tile = tile;
            }
        }

        internal delegate void TileEffectEventHandler(TileEffectEventArgs args);

        public event TileEffectEventHandler SteppedOnEvent;
        public event TileEffectEventHandler TurnStartEvent;
        public event TileEffectEventHandler TurnEndEvent;

        internal virtual void OnSteppedOn(Unit unit, BaseTile tile) 
        {
            SteppedOnEvent?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        internal virtual void OnTurnStart(Unit unit, BaseTile tile)
        {
            TurnStartEvent?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        internal virtual void OnTurnEnd(Unit unit, BaseTile tile)
        {
            TurnEndEvent?.Invoke(new TileEffectEventArgs(unit, tile));
        }
    }
}
