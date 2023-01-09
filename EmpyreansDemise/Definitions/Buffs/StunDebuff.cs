using Empyrean.Game.Abilities;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Definitions.Buffs
{
    public class StunDebuff : Buff
    {
        public StunDebuff(int duration) : base()
        {
            Invisible = false;

            Duration = duration;
        }

        public StunDebuff(Buff buff) : base(buff) { }

        protected override void AssignAnimationSet()
        {
            base.AssignAnimationSet();

            AnimationSet = AnimationSetManager.GetAnimationSet(69);
        }

        public override void AddEventListeners()
        {
            base.AddEventListeners();

            Unit.Info.StatusManager.AddStatusCondition(StatusCondition.Stunned);
        }

        public override void RemoveEventListeners()
        {
            base.RemoveEventListeners();

            Unit.Info.StatusManager.RemoveStatusCondition(StatusCondition.Stunned);
        }
    }
}
