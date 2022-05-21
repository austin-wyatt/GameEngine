using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities.AbilityEffects
{
    public class ApplyDamage : AbilityEffect
    {
        public Func<DamageInstance> CreateDamageInstance = null;

        public ApplyDamage(TargetInformation info) : base(info)
        {

        }

        protected override async Task<AbilityEffectResults> DoEffect(Ability ability)
        {
            OnEffectEnacted();

            AbilityEffectResults results = new AbilityEffectResults(ability);

            List<Unit> units = TargetInformation.GetTargets(ability);

            foreach(Unit unit in units)
            {
                DamageInstance damageInstance = CreateDamageInstance?.Invoke();

                if (damageInstance == null)
                {
                    return results;
                }

                DamageParams damageParams = new DamageParams(damageInstance, ability);

                var damageResults = ability.SelectionInfo.SelectedUnit.ApplyDamage(damageParams);

                results.AddUnitDamageReturnValues(damageResults);
            }

            await AwaitAnimation();

            return results;
        }
    }
}
