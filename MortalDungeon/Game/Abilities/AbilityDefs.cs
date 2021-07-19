using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public class Move : Ability
    {
        public Move(Unit castingUnit, int range = 6) 
        {
            Type = AbilityTypes.Move;
            Range = range;
            CastingUnit = castingUnit;
            CanTargetEnemy = false;
            CanTargetAlly = false;
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default)
        {
            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(CastingUnit.TileMapPosition, Range, new List<TileClassification> { TileClassification.Ground }, units, CastingUnit, Type);
            TileMap = tileMap;

            TrimTiles(validTiles, units);
            return validTiles;
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            CastingUnit.GradualMove(TileMap[SelectedTile.TileIndex].Position + Vector3.UnitZ * CastingUnit.Position.Z);
            CastingUnit.TileMapPosition = SelectedTile.TileIndex;
        }
    }

    public class BasicMelee : Ability
    {
        public BasicMelee(Unit castingUnit, int range = 1)
        {
            Type = AbilityTypes.MeleeAttack;
            Range = range;
            CastingUnit = castingUnit;
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
