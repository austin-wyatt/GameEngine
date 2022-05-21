using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities.AbilityEffects
{
    public enum ResOperation
    {
        Set,
        Add,
        Subtract,
        Multiply,
        Divide,
    }

    public class ModifyResI : AbilityEffect
    {
        private Func<int> GetResourceValue;

        private ResOperation Operation;
        private ResI Resource;

        public ModifyResI(ResOperation operation, ResI resource, TargetInformation info, Func<int> getResourceValue) : base(info)
        {
            Operation = operation;
            Resource = resource;

            GetResourceValue = getResourceValue;
        }

        protected override async Task<AbilityEffectResults> DoEffect(Ability ability)
        {
            OnEffectEnacted();

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

            int val = GetResourceValue.Invoke();

            switch (Operation)
            {
                case ResOperation.Set:
                    unit.SetResI(Resource, val);
                    break;
                case ResOperation.Add:
                    unit.AddResI(Resource, val);
                    break;
                case ResOperation.Subtract:
                    unit.AddResI(Resource, -val);
                    break;
                case ResOperation.Divide:
                    unit.SetResI(Resource, unit.GetResI(Resource) / val);
                    break;
                case ResOperation.Multiply:
                    unit.SetResI(Resource, unit.GetResI(Resource) * val);
                    break;
            }
        }
    }
}
