using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class MoveToTile : UnitAIAction
    {
        public MoveToTile(Unit castingUnit, Ability ability = null, BaseTile tile = null, Unit unit = null) : base(castingUnit, ability, tile, unit) { }

        public override void EnactEffect()
        {
            CastingUnit.Info._movementAbility.Units = Scene._units;

            List<BaseTile> path = null;

            TileMap.PathToPointParameters param = new TileMap.PathToPointParameters(TilePosition, TargetedTile.TilePoint, CastingUnit.Info.Energy / Ability.GetEnergyCost())
            {
                Units = Scene._units,
                TraversableTypes = new List<TileClassification>() { TileClassification.Ground },
                CastingUnit = CastingUnit,
                CheckTileLower = true,
                AbilityType = AbilityTypes.Move
            };

            path = Map.GetPathToPoint(param);

            if (path == null || path.Count == 0)
            {
                CastingUnit.AI.BeginNextAction();
                return;
            }


            CastingUnit.Info._movementAbility.CurrentTiles = path;
            CastingUnit.Info._movementAbility.EnactEffect();

            CastingUnit.Info._movementAbility.EffectEndedAction = () =>
            {
                CastingUnit.Info._movementAbility.EffectEndedAction = null;
                CastingUnit.AI.BeginNextAction();
            };
        }
    }
}
