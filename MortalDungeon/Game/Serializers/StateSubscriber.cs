using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Serializers
{
    [Serializable]
    public class StateSubscriber
    {
        public StateIDValuePair TriggerValue;
        public List<StateIDValuePair> SubscribedValues = new List<StateIDValuePair>();
        public string Script = "";
        public bool Permanent = false;
    }
}
