using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities
{
    /// <summary>
    /// Contains 
    /// </summary>
    public class EffectManager
    {
        public Ability Ability;

        public List<AbilityEffect> Effects = new List<AbilityEffect>();
        public List<ChainCondition> ChainConditions = new List<ChainCondition>();

        public EffectManager(Ability ability)
        {
            Ability = ability;
        }

        public virtual async Task EnactEffect()
        {
            if(Ability == null)
            {
                return;
            }
            else
            {
                Ability.BeginEffect();

                CombinedAbilityEffectResults combinedResults = new CombinedAbilityEffectResults(Ability);

                for (int i = 0; i < Effects.Count; i++)
                {
                    await Effects[i].EnactEffect(Ability, combinedResults);
                }

                for (int i = 0; i < ChainConditions.Count; i++)
                {
                    await ChainConditions[i].ContinueEffect(new AbilityEffectResults(Ability), combinedResults);
                }
                Casted();
                EffectEnded();
            }
        }

        public void ClearEffects()
        {
            //remove all current effects
        }


        private void Casted()
        {
            Ability.Casted();
        }

        private void EffectEnded()
        {
            Ability.EffectEnded();
        }
    }
}
