using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Events
{
    public abstract class EventAction
    {
        public string EventTrigger;

        public Conditional Conditional;

        /// <summary>
        /// Convert object parameters into what the action requires
        /// </summary>
        public abstract void BuildEvent(List<dynamic> parameters);

        public abstract void Invoke(params dynamic[] parameters);

    }
}
