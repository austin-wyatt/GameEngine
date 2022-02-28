using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Definitions.Buffs
{
    public class WebSlowDebuff : Buff
    {
        public WebSlowDebuff() : base()
        {
            Invisible = false;

            Duration = -1;
            Stacks = 0;
            AddStack();
        }
        public WebSlowDebuff(Buff buff) : base(buff) { }

        protected override void AssignAnimationSet()
        {
            base.AssignAnimationSet();

            AnimationSet = AnimationSetManager.GetAnimationSet(70);
        }

        public override void AddStack()
        {
            base.AddStack();

            SetBuffEffect(BuffEffect.MovementEnergyMultiplier, 1.2f + 0.1f * Stacks);
            SetBuffEffect(BuffEffect.SpeedMultiplier, 0.8f - 0.1f * Stacks);
        }

        public override void RemoveStack()
        {
            base.RemoveStack();

            SetBuffEffect(BuffEffect.MovementEnergyMultiplier, 1.2f + 0.1f * Stacks);
            SetBuffEffect(BuffEffect.SpeedMultiplier, 0.8f - 0.1f * Stacks);

            if (Stacks == 0)
            {
                Unit.Info.RemoveBuff(this);
            }
        }

        public override void AddEventListeners()
        {
            base.AddEventListeners();

            Unit.TurnEnd += CheckTile;
        }

        public override void RemoveEventListeners()
        {
            base.RemoveEventListeners();

            Unit.TurnEnd -= CheckTile;
        }

        private void CheckTile(Unit unit)
        {
            var effects = TileEffectManager.GetTileEffectsOnTilePoint(unit.Info.TileMapPosition);

            bool onSpiderWeb = false;

            foreach(var effect in effects)
            {
                if(effect.Identifier == Identifier)
                {
                    onSpiderWeb = true;
                    break;
                }
            }

            if (!onSpiderWeb)
            {
                RemoveStack();
            }
        }
    }
}
