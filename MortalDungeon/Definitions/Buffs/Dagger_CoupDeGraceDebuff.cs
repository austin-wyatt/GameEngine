using Empyrean.Game.Abilities;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Empyrean.Definitions.Buffs
{
    public class Dagger_CoupDeGraceDebuff : Buff
    {
        public Dagger_CoupDeGraceDebuff() : base()
        {
            Invisible = false;

            RemoveOnZeroStacks = true;

            Duration = -1;
            Stacks = 1;
        }

        public Dagger_CoupDeGraceDebuff(Buff buff) : base(buff) { }

        protected override void AssignAnimationSet()
        {
            base.AssignAnimationSet();

            AnimationSet = AnimationSetManager.GetAnimationSet(67);
        }

        public override void AddEventListeners()
        {
            base.AddEventListeners();

            Unit.TurnEnd += CheckStacks;
        }

        public override void RemoveEventListeners()
        {
            base.RemoveEventListeners();

            Unit.TurnEnd -= CheckStacks;
        }

        public override async Task AddStack()
        {
            await base.AddStack();

            if (Stacks >= 10)
            {
                int stacks = Stacks;
                Stacks = 0;
                for (int i = 0; i < stacks; i++)
                {
                    DamageInstance coupDeGraceDamage = new DamageInstance();
                    coupDeGraceDamage.Damage.Add(DamageType.Piercing, 1);
                    coupDeGraceDamage.PiercingPercent = 0;

                    DamageParams damageParams = new DamageParams(coupDeGraceDamage, buff: this);

                    Unit.ApplyDamage(damageParams);

                    Thread.Sleep(50);
                }

                Unit.Info.BuffManager.RemoveBuff(this);
                return;
            }
        }

        private async Task CheckStacks(Unit unit)
        {
            await RemoveStack();
        }
    }
}
