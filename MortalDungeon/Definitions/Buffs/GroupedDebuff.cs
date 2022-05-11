using Empyrean.Game.Abilities;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Definitions.Buffs
{
    public class GroupedDebuff : Buff
    {
        public GroupedDebuff() : base()
        {
            Invisible = false;

            Duration = -1;
        }

        protected override void AssignAnimationSet()
        {
            base.AssignAnimationSet();

            AnimationSet = AnimationSetManager.GetAnimationSet(67);
        }

        public override async Task RemoveStack()
        {
            await base.RemoveStack();

            SetBuffEffect(BuffEffect.MovementEnergyCostMultiplier, 1.2f + 0.1f * Stacks);
            SetBuffEffect(BuffEffect.SpeedMultiplier, 0.8f - 0.1f * Stacks);

            if (Stacks == 0)
            {
                Unit.Info.RemoveBuff(this);
            }
        }

        public override void AddEventListeners()
        {
            base.AddEventListeners();
        }

        public override void RemoveEventListeners()
        {
            base.RemoveEventListeners();
        }
    }
}
