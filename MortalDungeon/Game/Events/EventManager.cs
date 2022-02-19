using MortalDungeon.Engine_Classes.Scenes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Events
{
    public interface IEventTarget
    {
        public Dictionary<string, List<EventAction>> EventActions { get; set; }
        public Dictionary<string, dynamic> EventObjects { get; set; }
    }

    public static class EventManager
    {
        public static CombatScene Scene;

        public static void FireEvent(string eventType, IEventTarget target, params dynamic[] parameters)
        {
            if(target.EventActions.TryGetValue(eventType, out var actions))
            {
                foreach(var action in actions)
                {
                    action.Invoke(target, parameters);
                }
            }
        }
    }
}
