using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class AttackEnemy : UnitAIAction
    {
        public AttackEnemy(Unit castingUnit, Ability ability, BaseTile tile = null, Unit unit = null) : base(castingUnit, ability, tile, unit) { }

        public override void EnactEffect()
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
                Ability.EnactEffect();
            }
        }
    }
}
