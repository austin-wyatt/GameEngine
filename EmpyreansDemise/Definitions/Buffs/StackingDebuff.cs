using Empyrean.Game.Abilities;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Definitions.Buffs
{
    [Flags]
    public enum StackBehavior
    {
        DecreaseAtTurnStart = 1 << 0,
        RefreshOnStackAdded = 1 << 1,
        TrackStackDurationSeparately = 1 << 2,
    }

    public class StackingValue
    {
        public BuffEffect BuffEffect;
        public float BaseValue;
        public float AdditiveAmountPerStack;
        public float MultiplicativeAmountPerStack;
    }

    public class StackingDebuff : Buff
    {
        public List<int> StackDurations = new List<int>();
        public StackBehavior Behavior = StackBehavior.DecreaseAtTurnStart;
        public int StackDuration;
        public int AnimationSetId = 70;

        public List<StackingValue> StackingValues = new List<StackingValue>();

        public StackingDebuff() : base()
        {
            Invisible = false;
        }

        public StackingDebuff(StackingDebuff buff) : base(buff) 
        {
            StackDurations = new List<int>(buff.StackDurations);
            Behavior = buff.Behavior;
            StackDuration = buff.StackDuration;
            AnimationSetId = buff.AnimationSetId;

            StackingValues = buff.StackingValues;

            AssignAnimationSet();
        }

        protected override void AssignAnimationSet()
        {
            base.AssignAnimationSet();

            AnimationSet = AnimationSetManager.GetAnimationSet(AnimationSetId);
        }

        public override async Task AddStack()
        {
            await base.AddStack();

            if (Behavior.HasFlag(StackBehavior.TrackStackDurationSeparately))
            {
                StackDurations.Add(StackDuration);
            }

            if (Behavior.HasFlag(StackBehavior.RefreshOnStackAdded))
            {
                Duration = BaseDuration;
            }

            SetBuffEffects();
        }

        public override async Task RemoveStack()
        {
            if (Stacks >= 0)
            {
                await base.RemoveStack();

                if (Behavior.HasFlag(StackBehavior.TrackStackDurationSeparately))
                {
                    StackDurations.RemoveAt(0);
                }

                SetBuffEffects();
            }
        }

        public override void AddEventListeners()
        {
            base.AddEventListeners();

            Unit.TurnStart += ResolveTurnStart;

            if(!Initialized)
            {
                Stacks = 0;

                AddStack().Wait();
            }
        }

        public override void RemoveEventListeners()
        {
            base.RemoveEventListeners();

            Unit.TurnStart -= ResolveTurnStart;
        }

        private async Task ResolveTurnStart(Unit unit)
        {
            if (Behavior.HasFlag(StackBehavior.DecreaseAtTurnStart))
            {
                await RemoveStack();
            }

            if (Behavior.HasFlag(StackBehavior.TrackStackDurationSeparately))
            {
                int stacksToRemove = 0;

                for(int i = 0; i < StackDurations.Count; i++)
                {
                    StackDurations[i]--;

                    if(StackDurations[i] <= 0) 
                        stacksToRemove++;
                }

                for(int i = 0; i < stacksToRemove; i++)
                {
                    await RemoveStack();
                }
            }
        }

        private void SetBuffEffects()
        {
            foreach(var stackValue in StackingValues)
            {
                SetBuffEffect(stackValue.BuffEffect, stackValue.BaseValue 
                    + stackValue.AdditiveAmountPerStack * Stacks 
                    + (float)Math.Pow(stackValue.MultiplicativeAmountPerStack, Stacks));
            }
        }
    }
}
