using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;

namespace MortalDungeon.Game.Abilities
{
    internal class Strike : Ability
    {
        internal Strike(Unit castingUnit, int range = 1, float damage = 10)
        {
            Type = AbilityTypes.MeleeAttack;
            DamageType = DamageType.Slashing;
            Range = range;
            CastingUnit = castingUnit;
            Damage = damage;
            ActionCost = 2;

            Name = "Strike";

            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.CrossedSwords, Spritesheets.IconSheet, true);
        }

        internal override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            base.GetValidTileTargets(tileMap);

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(CastingUnit.Info.TileMapPosition, Range)
            {
                TraversableTypes = TileMapConstants.AllTileClassifications,
                Units = units,
                CastingUnit = CastingUnit
            };

            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(param);

            TrimTiles(validTiles, units);

            TargetAffectedUnits();

            return validTiles;
        }

        internal override bool UnitInRange(Unit unit, BaseTile position = null)
        {
            GetValidTileTargets(unit.GetTileMap(), new List<Unit> { unit });

            return AffectedUnits.Exists(u => u.ObjectID == unit.ObjectID);
        }

        internal override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;
            
            if (unit.AI.Team != CastingUnit.AI.Team && AffectedTiles.FindIndex(t => t.TilePoint == unit.Info.TileMapPosition) != -1) 
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        internal override void OnCast()
        {
            TileMap.DeselectTiles();

            base.OnCast();
        }

        internal override void OnAICast()
        {
            base.OnAICast();
        }

        internal override void EnactEffect()
        {
            base.EnactEffect();

            SelectedUnit.ApplyDamage(new Unit.DamageParams(GetDamageInstance()) { Ability = this });

            Casted();
            EffectEnded();
        }

        internal override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            instance.Damage.Add(DamageType, GetDamage());

            ApplyBuffDamageInstanceModifications(instance);
            return instance;
        }
    }
}
