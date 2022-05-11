using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities.AbilityEffects
{
    public class ModifyResF : AbilityEffect
    {
        public Func<float> GetResourceValue;

        private ResOperation Operation;
        private ResF Resource;

        public ModifyResF(ResOperation operation, ResF resource, TargetInformation info) : base(info)
        {
            Operation = operation;
            Resource = resource;
        }

        protected override async Task<AbilityEffectResults> DoEffect(Ability ability)
        {
            var units = TargetInformation.GetTargets(ability);

            foreach(var unit in units)
            {
                ModifyRes(unit);
            }

            await AwaitAnimation();

            return new AbilityEffectResults(ability);
        }

        private void ModifyRes(Unit unit)
        {
            if (GetResourceValue == null) return;

            float val = GetResourceValue.Invoke();

            switch (Operation)
            {
                case ResOperation.Set:
                    unit.SetResF(Resource, val);
                    break;
                case ResOperation.Add:
                    unit.SetResF(Resource, val);
                    break;
                case ResOperation.Subtract:
                    unit.SetResF(Resource, -val);
                    break;
                case ResOperation.Divide:
                    unit.SetResF(Resource, unit.GetResF(Resource) / val);
                    break;
                case ResOperation.Multiply:
                    unit.SetResF(Resource, unit.GetResF(Resource) * val);
                    break;
            }
        }
    }
}
