using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public class TileEffect
    {
        public TileEffect() { }

        public class TileEffectEventArgs
        {
            public Unit Unit;
            public BaseTile Tile;
            public TileEffectEventArgs(Unit unit, BaseTile tile)
            {
                Unit = unit;
                Tile = tile;
            }
        }

        public delegate void TileEffectEventHandler(TileEffectEventArgs args);

        public event TileEffectEventHandler SteppedOnEvent;
        public event TileEffectEventHandler TurnStartEvent;
        public event TileEffectEventHandler TurnEndEvent;

        public virtual void OnSteppedOn(Unit unit, BaseTile tile) 
        {
            SteppedOnEvent?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        public virtual void OnTurnStart(Unit unit, BaseTile tile)
        {
            TurnStartEvent?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        public virtual void OnTurnEnd(Unit unit, BaseTile tile)
        {
            TurnEndEvent?.Invoke(new TileEffectEventArgs(unit, tile));
        }
    }
}
