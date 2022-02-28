using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Definitions.Buffs
{
    public class StunDebuff : Buff
    {
        public StunDebuff() : base()
        {
            Invisible = false;

            Duration = 3;
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
