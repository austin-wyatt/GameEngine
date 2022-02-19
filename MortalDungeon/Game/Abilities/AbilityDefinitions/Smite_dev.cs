﻿using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using MortalDungeon.Game.Map;
using System.Diagnostics;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes;

namespace MortalDungeon.Game.Abilities
{
    public class Smite_dev : Ability
    {
        public Smite_dev() { }
        public Smite_dev(Unit castingUnit)
        {
            Type = AbilityTypes.RangedAttack;
            DamageType = DamageType.Piercing;
            Range = 15;
            MinRange = 0;
            CastingUnit = castingUnit;
            Damage = 10;

            ActionCost = 0;

            CastingMethod |= CastingMethod.Weapon | CastingMethod.PhysicalDexterity;

            UnitTargetParams.Dead = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsHostile = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsNeutral = UnitCheckEnum.SoftTrue;
            UnitTargetParams.Self = UnitCheckEnum.SoftTrue;


            Name = new Serializers.TextInfo(11, 3);
            Description = new Serializers.TextInfo(12, 3);


            SetIcon(IconSheetIcons.Circle, Spritesheets.IconSheet);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null, List<Unit> validUnits = null)
        {
            base.GetValidTileTargets(tileMap);

            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;

            //List <BaseTile> validTiles = tileMap.GetTargetsInRadius(point, (int)Range, new List<TileClassification>(), units);
            List<Unit> unitsInRadius = VisionHelpers.GetUnitsInRadius(CastingUnit, units, (int)Range, Scene);

            TrimUnits(unitsInRadius, false, MinRange);

            TargetAffectedUnits();

            return new List<BaseTile>();
        }

        public override bool UnitInRange(Unit unit, BaseTile position = null)
        {
            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;

            bool inRange = false;

            if (TileMap.GetDistanceBetweenPoints(point, unit.Info.TileMapPosition) <= MinRange)
            {
                return false;
            }

            var tempVision = new TemporaryVisionParams()
            {
                Unit = CastingUnit,
                TemporaryPosition = point
            };

            if (VisionHelpers.PointInVision(unit.Info.TileMapPosition, CastingUnit.AI.Team, new List<TemporaryVisionParams>{ tempVision }))
            {
                return true;
            }

            return inRange;
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedUnits.FindIndex(u => u.ObjectID == unit.ObjectID) != -1)
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        public override void OnCast()
        {
            TileMap.Controller.DeselectTiles();

            base.OnCast();
        }

        public override void OnAICast()
        {
            base.OnAICast();
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            var dam = new Dictionary<DamageType, float>();
            dam.Add(DamageType.HealthRemoval, 1000);

            SelectedUnit.ApplyDamage(new Unit.DamageParams(new DamageInstance() 
            { 
                Damage = dam
            }));

            Casted();
            EffectEnded();
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            instance.Damage.Add(DamageType, Damage);

            ApplyBuffDamageInstanceModifications(instance);
            return instance;
        }
    }
}
