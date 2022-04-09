using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Game.Units.AIFunctions;
using MortalDungeon.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public partial class TemplateRangedSingleTarget : Ability
    {
        public TemplateRangedSingleTarget(Unit castingUnit, AbilityClass abilityClass = AbilityClass.Unknown, int range = 1, float damage = 0)
        {
            Type = AbilityTypes.MeleeAttack;
            DamageType = DamageType.Slashing;
            Range = range;
            CastingUnit = castingUnit;
            Damage = damage;
            ActionCost = 1;

            CastingMethod |= CastingMethod.BruteForce | CastingMethod.Weapon;

            Grade = 1;

            MaxCharges = 0;
            Charges = 0;
            ChargeRechargeCost = 0;

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.T },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });

            AbilityClass = abilityClass;

            UnitTargetParams = new UnitSearchParams()
            {
                Dead = UnitCheckEnum.False,
                IsFriendly = UnitCheckEnum.SoftTrue,
                IsHostile = UnitCheckEnum.SoftTrue,
                IsNeutral = UnitCheckEnum.SoftTrue,
                Self = UnitCheckEnum.False,
                InVision = UnitCheckEnum.True,
            };
        }

        public override void GetValidTileTargets(TileMap tileMap, out List<Tile> affectedTiles, out List<Unit> affectedUnits,
            List<Unit> units = default, Tile position = null)
        {
            if (position == null)
            {
                position = CastingUnit.Info.TileMapPosition;
            }

            var unitsInRadius = UnitPositionManager.GetUnitsInRadius((int)Range, position.ToFeaturePoint());

            affectedTiles = new List<Tile>();
            affectedUnits = new List<Unit>();

            foreach (var unit in unitsInRadius)
            {
                bool valid = true;

                if(!UnitTargetParams.CheckUnit(unit, CastingUnit))
                {
                    continue;
                }

                if (RequiresLineToTarget)
                {
                    var lineOfTiles = TileMap.GetLineOfTiles(position, unit.Info.TileMapPosition);

                    for(int i = 0; i < lineOfTiles.Count; i++)
                    {
                        if (lineOfTiles[i].BlocksType(BlockingType.Abilities))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid)
                {
                    affectedUnits.Add(unit);
                    affectedTiles.Add(unit.Info.TileMapPosition);
                }
            }
        }

        public override bool GetPositionValid(TilePoint sourcePos, TilePoint destinationPos)
        {
            int distanceBetweenPoints = TileMap.GetDistanceBetweenPoints(sourcePos, destinationPos);

            if (distanceBetweenPoints > Range || distanceBetweenPoints < MinRange)
            {
                return false;
            }

            if (RequiresLineToTarget)
            {
                var lineOfTiles = TileMap.GetLineOfTiles(sourcePos, destinationPos);

                for (int i = 0; i < lineOfTiles.Count; i++)
                {
                    if (lineOfTiles[i].BlocksType(BlockingType.Abilities))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedUnits.FindIndex(u => u == unit) != -1 && UnitTargetParams.CheckUnit(unit, CastingUnit))
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        public override void EnactEffect()
        {
            BeginEffect();

            SelectedUnit.ApplyDamage(new DamageParams(GetDamageInstance()) { Ability = this });

            Casted();
            EffectEnded();
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            instance.Damage.Add(DamageType, GetDamage());

            return instance;
        }

        
    }
}
