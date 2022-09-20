using Empyrean.Engine_Classes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities
{
    public enum AbilityEffectResult
    {
        HealthRemoved,
        ShieldsRemoved,
        DamageResisted,
        DamageBlockedByShields,
        UnitKilled,
        PotentialDamageBeforeModifications,
        AmountOfTilesMoved
    }

    public class AbilityEffectResults
    {
        public Dictionary<AbilityEffectResult, float> ResultValues = new Dictionary<AbilityEffectResult, float>();
        public Ability Ability;

        public AbilityEffectResults(Ability ability)
        {
            Ability = ability;
        }

        public void ApplyUnitDamageReturnValues(Units.AppliedDamageReturnValues vals)
        {
            ResultValues.AddOrSet(AbilityEffectResult.HealthRemoved, vals.ActualDamageDealt);
            ResultValues.AddOrSet(AbilityEffectResult.ShieldsRemoved, vals.AttackBrokeShield ? 1 : 0);
            ResultValues.AddOrSet(AbilityEffectResult.DamageResisted, vals.DamageResisted);
            ResultValues.AddOrSet(AbilityEffectResult.DamageBlockedByShields, vals.DamageBlockedByShields);
            ResultValues.AddOrSet(AbilityEffectResult.UnitKilled, vals.KilledEnemy ? 1 : 0);
            ResultValues.AddOrSet(AbilityEffectResult.PotentialDamageBeforeModifications, vals.PotentialDamageBeforeModifications);
        }

        public void AddUnitDamageReturnValues(Units.AppliedDamageReturnValues vals)
        {
            if (ResultValues.Count == 0)
            {
                ApplyUnitDamageReturnValues(vals);
                return;
            }

            ResultValues[AbilityEffectResult.HealthRemoved] += vals.ActualDamageDealt;
            ResultValues[AbilityEffectResult.ShieldsRemoved] += vals.AttackBrokeShield ? 1 : 0;
            ResultValues[AbilityEffectResult.DamageResisted] += vals.DamageResisted;
            ResultValues[AbilityEffectResult.DamageBlockedByShields] += vals.DamageBlockedByShields;
            ResultValues[AbilityEffectResult.UnitKilled] += vals.KilledEnemy ? 1 : 0;
            ResultValues[AbilityEffectResult.PotentialDamageBeforeModifications] += vals.PotentialDamageBeforeModifications;
        }
    }

    public class CombinedAbilityEffectResults
    {
        public Dictionary<AbilityEffectResult, float> ResultValues = new Dictionary<AbilityEffectResult, float>();

        public Ability Ability;

        public CombinedAbilityEffectResults(Ability ability)
        {
            Ability = ability;
        }

        private object _resultsLock = new object();

        public void AddAbilityEffectResults(AbilityEffectResults results)
        {
            lock (_resultsLock)
            {
                foreach (var kvp in results.ResultValues)
                {
                    if (ResultValues.ContainsKey(kvp.Key))
                    {
                        ResultValues[kvp.Key] += kvp.Value;
                    }
                    else
                    {
                        ResultValues.AddOrSet(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        public void AddCombinedAbilityEffectResults(CombinedAbilityEffectResults results)
        {
            lock (_resultsLock)
            {
                foreach (var kvp in results.ResultValues)
                {
                    if (ResultValues.ContainsKey(kvp.Key))
                    {
                        ResultValues[kvp.Key] += kvp.Value;
                    }
                    else
                    {
                        ResultValues.AddOrSet(kvp.Key, kvp.Value);
                    }
                }
            }
        }
    }

    public enum AbilityUnitTarget
    {
        CastingUnit,
        SelectedUnit,
        SelectedUnits
    }

    public class AbilityEffect
    {
        protected List<ChainCondition> ChainConditions = new List<ChainCondition>();
        protected List<AbilityEffect> AdjacentEffects = new List<AbilityEffect>();

        public TargetInformation TargetInformation;

        public AbilityAnimation Animation = null;

        public event Action EffectEnacted;

        public AbilityEffect(TargetInformation info)
        {
            TargetInformation = info;
        }

        /// <summary>
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="combinedResults">The combined results for every effect in a chain of effects</param>
        /// <returns></returns>
        public async virtual Task EnactEffect(Ability ability, CombinedAbilityEffectResults combinedResults)
        {
            AbilityEffectResults results = await DoEffect(ability);

            combinedResults.AddAbilityEffectResults(results);

            for (int i = 0; i < AdjacentEffects.Count; i++)
            {
                await AdjacentEffects[i].EnactEffect(ability, combinedResults);
            }

            for (int i = 0; i < ChainConditions.Count; i++)
            {
                await ChainConditions[i].ContinueEffect(results, combinedResults);
            }
        }

        protected virtual async Task<AbilityEffectResults> DoEffect(Ability ability)
        {
            await AwaitAnimation();

            OnEffectEnacted();

            return new AbilityEffectResults(ability);
        }

        protected void OnEffectEnacted()
        {
            EffectEnacted?.Invoke();
        }

        protected async Task AwaitAnimation()
        {
            if (Animation != null)
            {
                await Animation.PlayAnimation();
            }
        }

        public void AddChainCondition(ChainCondition chainCondition)
        {
            ChainConditions.Add(chainCondition);
        }

        /// <summary>
        /// An adjacent effect is an ability effect that occurs after the main effect
        /// without any check occuring
        /// </summary>
        public void AddAdjacentEffect(AbilityEffect effect)
        {
            AdjacentEffects.Add(effect);
        }
    }



    
}
