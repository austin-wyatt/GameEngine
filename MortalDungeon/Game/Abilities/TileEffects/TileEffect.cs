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


        public virtual void AddedToTile(TilePoint point)
        {
            //do stuff, assign events, add visuals, etc
        }

        public virtual void RemovedFromTile(TilePoint point)
        {
            //clean up any objects here
        }


        public delegate void TileEffectEventHandler(TileEffectEventArgs args);
        public delegate void TileEffectRoundHandler(TilePoint point);

        public event TileEffectEventHandler SteppedOn;
        public event TileEffectEventHandler SteppedOff;
        public event TileEffectEventHandler TurnStart;
        public event TileEffectEventHandler TurnEnd;
        public event TileEffectRoundHandler RoundEnd;
        public event TileEffectRoundHandler RoundStart;

        public virtual void OnSteppedOn(Unit unit, BaseTile tile) 
        {
            SteppedOn?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        public virtual void OnSteppedOff(Unit unit, BaseTile tile)
        {
            SteppedOff?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        public virtual void OnTurnStart(Unit unit, BaseTile tile)
        {
            TurnStart?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        public virtual void OnTurnEnd(Unit unit, BaseTile tile)
        {
            TurnEnd?.Invoke(new TileEffectEventArgs(unit, tile));
        }

        public virtual void OnRoundStart(TilePoint point)
        {
            RoundStart?.Invoke(point);
        }

        public virtual void OnRoundEnd(TilePoint point)
        {
            RoundEnd?.Invoke(point);
        }

        
    }
}
