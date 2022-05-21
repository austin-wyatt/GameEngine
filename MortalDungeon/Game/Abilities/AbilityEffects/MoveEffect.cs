using Empyrean.Game.Movement;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities.AbilityEffects
{
    public class MoveEffect : AbilityEffect
    {
        public Func<MoveContract> GetMoveContract = null;

        public MoveEffect(TargetInformation info) : base(info) { }

        protected override async Task<AbilityEffectResults> DoEffect(Ability ability)
        {
            AbilityEffectResults results = new AbilityEffectResults(ability);

            OnEffectEnacted();

            if (GetMoveContract != null)
            {
                List<Unit> units = TargetInformation.GetTargets(ability);

                foreach (Unit unit in units)
                {
                    MoveContract contract = GetMoveContract.Invoke();
                    if (contract.Viable)
                    {
                        await contract.MoveAnimation.EnactMovement(unit);
                    }
                }
            }

            return results;
        }
    }
}
