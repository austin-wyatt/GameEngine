using MortalDungeon.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Events
{
    [XmlType(TypeName = "EVB")]
    [Serializable]
    public class EventActionBuilder : ISerializable
    {
        /// <summary>
        /// the event that would trigger this action
        /// </summary>
        [XmlElement("EVt")]
        public string EventTrigger;

        /// <summary>
        /// The flags that must be true in order for the action to be triggered
        /// </summary>
        [XmlElement("EVsf")]
        public List<string> StateFlags = new List<string>();

        /// <summary>
        /// Any parameters that might be necessary when building the action. This can be any serializable class (including another EventActionBuilder).
        /// </summary>
        [XmlElement("EVap")]
        public List<object> ActionParameters = new List<object>();

        [XmlElement("EVan")]
        public string ActionName;

        [XmlElement("EVdn")]
        public string DescriptiveName;

        public EventAction BuildAction()
        {
            var actionType = Type.GetType($"MortalDungeon.Definitions.EventActions.{ActionName}");

            if(actionType != null)
            {
                var action = Activator.CreateInstance(actionType) as EventAction;

                action.StateFlags = new List<string>(StateFlags);
                action.EventTrigger = EventTrigger;
                action.BuildEvent(ActionParameters);

                return action;
            }

            return null;
        }

        public static EventAction BuildAction(string actionName, List<object> actionParameters)
        {
            var actionType = Type.GetType($"MortalDungeon.Definitions.EventActions.{actionName}");

            if (actionType != null)
            {
                var action = Activator.CreateInstance(actionType) as EventAction;

                action.BuildEvent(actionParameters);

                return action;
            }

            return null;
        }

        public void CompleteDeserialization()
        {

        }

        public void PrepareForSerialization()
        {

        }
    }
}
