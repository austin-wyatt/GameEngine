using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public class Strike : Ability
    {
        public Strike(Unit castingUnit, int range = 1)
        {
            Type = AbilityTypes.MeleeAttack;
            Range = range;
            CastingUnit = castingUnit;

            Name = "Strike";
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default)
        {
            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(CastingUnit.TileMapPosition, Range, new List<TileClassification> { TileClassification.Ground, TileClassification.AttackableTerrain }, units, CastingUnit);
            TileMap = tileMap;

            TrimTiles(validTiles, units);
            return validTiles;
        }

        public override void EnactEffect()
        {
            base.EnactEffect();
        }
    }
}
