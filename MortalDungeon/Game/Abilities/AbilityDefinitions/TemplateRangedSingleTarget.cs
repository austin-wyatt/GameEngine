using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Game.Units.AIFunctions;
using MortalDungeon.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public class TemplateRangedSingleTarget : Ability
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

            SetIcon(Character.T, Spritesheets.CharacterSheet);

            AbilityClass = abilityClass;
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null, List<Unit> validUnits = null)
        {
            base.GetValidTileTargets(tileMap);

            if (position == null)
            {
                position = CastingUnit.Info.TileMapPosition;
            }

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(position, Range)
            {
                TraversableTypes = TileMapConstants.AllTileClassifications,
                Units = units,
                CastingUnit = CastingUnit
            };

            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(param);

            TrimTiles(validTiles, units, validUnits: validUnits);

            TargetAffectedUnits();

            return validTiles;
        }

        public override bool UnitInRange(Unit unit, BaseTile position = null)
        {
            if (position == null)
            {
                position = CastingUnit.Info.TileMapPosition;
            }

            List<Unit> validUnits = new List<Unit>();

            var tiles = GetValidTileTargets(unit.GetTileMap(), new List<Unit> { unit }, position, validUnits: validUnits);

            return validUnits.Exists(u => u.ObjectID == unit.ObjectID);
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedTiles.FindIndex(t => t.TilePoint == unit.Info.TileMapPosition) != -1 && UnitTargetParams.CheckUnit(unit, CastingUnit))
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            SelectedUnit.ApplyDamage(new Unit.DamageParams(GetDamageInstance()) { Ability = this });

            Casted();
            EffectEnded();
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            instance.Damage.Add(DamageType, GetDamage());

            ApplyBuffDamageInstanceModifications(instance);
            return instance;
        }

        public override UnitAIAction GetAction(List<Unit> unitsInCombat)
        {
            var action = new UnitAIAction(CastingUnit, AIAction.MoveCloser);

            if (!CanCast())
                return action;

            //Find all movements that can be moved to and attacked.
            //Calculate the probably of wanting to do that based on the parameters such as movement aversion, bloodthirsty, etc
            //Pick the unit with the best weight


            var enemies = unitsInCombat.FindAll(u => u.AI.Team.GetRelation(CastingUnit.AI.Team) == Relation.Hostile);

            List<PotentialAIAction> potentialActions = new List<PotentialAIAction>();

            foreach (var enemy in enemies)
            {
                if (UnitInRange(enemy))
                {
                    PotentialAIAction pot = new PotentialAIAction();

                    pot.TargetUnit = enemy;
                    pot.Weight += 3; //has action 

                    potentialActions.Add(pot);
                }
                else if (AIFunctions.GetPathToPointInRangeOfAbility(CastingUnit, enemy, this, out var path, out float pathCost))
                {
                    PotentialAIAction pot = new PotentialAIAction();

                    pot.TargetUnit = enemy;
                    pot.Weight += 3; //has action 
                    pot.PathCost = pathCost;

                    pot.Weight += (1 - enemy.Info.Health / enemy.Info.MaxHealth) * CastingUnit.AI.Bloodthirsty;

                    pot.Weight -= pathCost * CastingUnit.AI.MovementAversion;

                    pot.Path = path;

                    potentialActions.Add(pot);
                }
            }

            PotentialAIAction chosenAction = null;

            foreach (PotentialAIAction pot in potentialActions)
            {
                if (chosenAction == null || chosenAction.Weight < pot.Weight)
                {
                    chosenAction = pot;
                }
            }

            if (chosenAction != null)
            {
                action.Weight = chosenAction.Weight;

                action.Weight += (float)new Random().NextDouble() * 2; //fuzz the weight a bit

                action.EffectAction = () =>
                {
                    if (chosenAction.Path != null && chosenAction.Path.Count > 0)
                    {
                        CastingUnit.Info._movementAbility.CurrentTiles = chosenAction.Path;

                        void effectEnded()
                        {
                            SelectedUnit = chosenAction.TargetUnit;
                            EnactEffect();

                            CastingUnit.AI.BeginNextAction();

                            CastingUnit.Info._movementAbility.EffectEndedAction -= effectEnded;
                        }

                        CastingUnit.Info._movementAbility.EffectEndedAction += effectEnded;

                        CastingUnit.Info._movementAbility.EnactEffect();
                    }
                    else
                    {
                        SelectedUnit = chosenAction.TargetUnit;
                        EnactEffect();

                        CastingUnit.AI.BeginNextAction();
                    }
                };
            }

            return action;
        }
    }
}
