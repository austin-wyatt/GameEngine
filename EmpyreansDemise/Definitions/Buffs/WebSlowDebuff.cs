using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Definitions.Buffs
{
    public class WebSlowDebuff : Buff
    {
        public WebSlowDebuff() : base()
        {
            Invisible = false;

            Duration = -1;
            Stacks = 0;
            AddStack().Wait();
        }

        public WebSlowDebuff(Buff buff) : base(buff) { }

        protected override void AssignAnimationSet()
        {
            base.AssignAnimationSet();

            AnimationSet = AnimationSetManager.GetAnimationSet(70);
        }

        public override async Task AddStack()
        {
            await base.AddStack();

            SetBuffEffect(BuffEffect.MovementEnergyCostMultiplier, 1.2f + 0.1f * Stacks);
            SetBuffEffect(BuffEffect.SpeedMultiplier, 0.8f - 0.1f * Stacks);
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

            Unit.TurnEnd += CheckTile;
        }

        public override void RemoveEventListeners()
        {
            base.RemoveEventListeners();

            Unit.TurnEnd -= CheckTile;
        }

        private async Task CheckTile(Unit unit)
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
                await RemoveStack();
            }
        }
    }
}
