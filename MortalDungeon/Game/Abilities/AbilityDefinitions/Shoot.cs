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
    public class Shoot : Ability
    {
        public Shoot(Unit castingUnit, int range = 6, int minRange = 2, float damage = 10)
        {
            Type = AbilityTypes.RangedAttack;
            Range = range;
            MinRange = minRange;
            CastingUnit = castingUnit;
            Damage = damage;
            EnergyCost = 7;

            Name = "Shoot";

            Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.BowAndArrow, Spritesheets.IconSheet, true);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;

            List <BaseTile> validTiles = tileMap.GetTargetsInRadius(point, (int)Range, new List<TileClassification>(), units);

           
            TrimTiles(validTiles, units, false, MinRange);

            TargetAffectedUnits();

            return validTiles;
        }

        public override bool UnitInRange(Unit unit, BaseTile position = null)
        {
            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;
            
            bool inRange;
            List<BaseTile> validTiles;
            //if (position == null)
            //{
            //    validTiles = GetValidTileTargets(unit.GetTileMap(), null, position);
            //}
            //else 
            //{
            //    validTiles = unit.GetTileMap().GetTargetsInRadius(point, (int)Range, new List<TileClassification>(), new List<Unit> { unit });
            //    unit.Info.TemporaryPosition = point;
            //}

            validTiles = unit.GetTileMap().GetTargetsInRadius(point, (int)Range, new List<TileClassification>(), new List<Unit> { unit });
            unit.Info.TemporaryPosition = point;

            TileMap map = unit.GetTileMap();


            List<BaseTile> teamVision = Scene.GetTeamVision(unit.AI.Team);

            unit.Info.TemporaryPosition = null;

            BaseTile tile = validTiles.Find(t => t.TilePoint == unit.Info.Point);
            if (tile == null)
            {
                inRange = false;
            }
            else 
            {
                inRange = teamVision.Exists(t => t.TilePoint == tile.TilePoint) && map.GetDistanceBetweenPoints(unit.Info.Point, point) >= MinRange;
            }

            return inRange;
        }

        public override bool OnUnitClicked(Unit unit)
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

        public override void OnCast()
        {
            TileMap.DeselectTiles();

            base.OnCast();
        }

        public override void OnAICast()
        {
            base.OnAICast();
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            SelectedUnit.ApplyDamage(GetDamage(), DamageType);

            Casted();
            EffectEnded();
        }
    }
}
