using MortalDungeon.Engine_Classes.Scenes;
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
    internal class MendBones : Ability
    {
        internal MendBones(Unit castingUnit, int range = 3, float heal = 10)
        {
            Type = AbilityTypes.Heal;
            DamageType = DamageType.Healing;
            Range = range;
            CastingUnit = castingUnit;
            Damage = heal;

            Grade = 1;

            ActionCost = 1;
            ChargeRechargeCost = 50;

            MaxCharges = 3;
            Charges = 3;

            CanTargetSelf = true;
            CanTargetGround = false;

            UnitTargetParams.Dead = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsHostile = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsNeutral = UnitCheckEnum.SoftTrue;
            UnitTargetParams.Self = UnitCheckEnum.SoftTrue;

            AbilityClass = AbilityClass.Skeleton;

            Name = "Mend Bones";

            _description = "Patch up your bony friends.";

            Icon = new Icon(Icon.DefaultIconSize, Character.M, Spritesheets.CharacterSheet, true);
        }

        internal override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            base.GetValidTileTargets(tileMap);

            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;

            //List <BaseTile> validTiles = tileMap.GetTargetsInRadius(point, (int)Range, new List<TileClassification>(), units);
            List<Unit> validUnits = VisionMap.GetUnitsInRadius(CastingUnit, units, (int)Range, Scene);

            TrimUnits(validUnits, false, MinRange);

            TargetAffectedUnits();

            return new List<BaseTile>();
        }

        internal override bool UnitInRange(Unit unit, BaseTile position = null)
        {
            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;

            if (CanTargetSelf && unit == CastingUnit)
                return true;

            bool inRange = false;

            var tempVision = new VisionMap.TemporaryVisionParams()
            {
                Unit = CastingUnit,
                TemporaryPosition = point
            };

            List<Vector2i> teamVision = VisionMap.GetTeamVision(CastingUnit.AI.Team, Scene, new List<VisionMap.TemporaryVisionParams>() { tempVision });

            Vector2i clusterPos = Scene._tileMapController.PointToClusterPosition(unit.Info.TileMapPosition);

            if (TileMap.GetDistanceBetweenPoints(point, unit.Info.TileMapPosition) <= MinRange)
            {
                return false;
            }

            if ((teamVision.Exists(p => p == clusterPos) || VisionMap.InVision(clusterPos.X, clusterPos.Y, CastingUnit.AI.Team))
                && VisionMap.TargetInVision(point, unit.Info.TileMapPosition, (int)Range, Scene))
            {
                return true;
            }

            return inRange;
        }

        internal override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedUnits.Contains(unit))
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        internal override void EnactEffect()
        {
            base.EnactEffect();

            Sound sound = new Sound(Sounds.Select) { Gain = 0.75f, Pitch = GlobalRandom.NextFloat(0.95f, 1.05f) };
            sound.Play();

            Casted();


            //arrow.SetScale(1 / WindowConstants.AspectRatio, 1, 1);


            DamageInstance healing = GetDamageInstance();

            SelectedUnit.ApplyDamage(new Unit.DamageParams(healing) { Ability = this });

            EffectEnded();
        }

        internal override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            int skeletonTypeAbilities = 0;

            if(SelectedUnit != null) 
            {
                SelectedUnit.Info.Abilities.ForEach(ability =>
                {
                    if (ability.AbilityClass == AbilityClass.Skeleton)
                    {
                        skeletonTypeAbilities++;
                    }
                });
            }

            float healAmount = skeletonTypeAbilities * Damage;

            instance.Damage.Add(DamageType, healAmount);

            ApplyBuffDamageInstanceModifications(instance);
            return instance;
        }
    }
}
