using Empyrean.Game.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Empyrean.Definitions.Buffs
{
    public class GenericEffectBuff : Buff
    {
        public GenericEffectBuff()
        {
            Invisible = true;
        }
        public GenericEffectBuff(Buff buff) : base(buff) { }

        public override void AddEventListeners()
        {
            base.AddEventListeners();

            Dictionary<int, float> tempEffectsDict = new Dictionary<int, float>(BuffEffects);

            foreach(var kvp in tempEffectsDict)
            {
                SetBuffEffect((BuffEffect)kvp.Key, kvp.Value);
            }
        }

        public override void RemoveEventListeners()
        {
            base.RemoveEventListeners();

            List<int> buffEffects = BuffEffects.Keys.ToList();

            foreach (var effect in buffEffects)
            {
                RemoveBuffEffect((BuffEffect)effect);
            }
        }

        
    }
}
