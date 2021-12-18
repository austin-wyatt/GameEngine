using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class UseAbilityOnUnit : UnitAIAction
    {
        internal UseAbilityOnUnit(Unit castingUnit, AIAction actionType, Ability ability, BaseTile tile = null, Unit unit = null) : base(castingUnit, actionType, ability, tile, unit) { }

        internal override void EnactEffect()
        {
            Ability.EffectEndedAction = () =>
            {
                CastingUnit.AI.BeginNextAction();
                Ability.EffectEndedAction = null;
            };

            if (TargetedTile != null)
            {
                Ability.OnTileClicked(Tile.TileMap, Tile);
            }
            else if (TargetedUnit != null) 
            {
                Ability.SelectedUnit = TargetedUnit;

                if (!Ability.UnitInRange(TargetedUnit)) 
                {
                    Console.WriteLine("How did this happen?");
                }

                Ability.EnactEffect();
            }
        }
    }
}
