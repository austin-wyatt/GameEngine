using Empyrean.Game.Entities;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities.AbilityEffects
{
    public class ApplyBuff : AbilityEffect
    {
        private Buff PrototypeBuff;

        public bool StackIfPresent = false;

        /// <summary>
        /// Only allow 1 buff with this identifer on the target at a time
        /// </summary>
        public bool BuffMustBeUnique = false; 

        public ApplyBuff(Buff debuff, TargetInformation info) : base(info)
        {
            PrototypeBuff = debuff;
        }

        protected override async Task<AbilityEffectResults> DoEffect(Ability ability)
        {
            OnEffectEnacted();

            AbilityEffectResults results = new AbilityEffectResults(ability);

            List<Unit> units = TargetInformation.GetTargets(ability);

            foreach(var unit in units)
            {
                if (StackIfPresent)
                {
                    Buff foundBuff = unit.Info.BuffManager.Buffs.Find(b => b.CompareIdentifier(PrototypeBuff));

                    if (foundBuff != null)
                    {
                        foundBuff.AddStack();
                        continue;
                    }
                }

                if (BuffMustBeUnique)
                {
                    Buff foundBuff = unit.Info.BuffManager.Buffs.Find(b => b.CompareIdentifier(PrototypeBuff));
                    if (foundBuff != null)
                    {
                        continue;
                    }
                }

                Buff buff = (Buff)Activator.CreateInstance(PrototypeBuff.GetType(), PrototypeBuff);
                buff.Initialized = false;
                buff.AnimationSet = PrototypeBuff.AnimationSet;

                unit.Info.AddBuff(buff);
            }

            await AwaitAnimation();

            return results;
        }
    }
}
