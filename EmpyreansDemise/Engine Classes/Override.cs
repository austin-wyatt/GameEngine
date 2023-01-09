using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    [Serializable]
    public class Override<T>
    {
        public T Value;
        public int Priority;

        public Override() { }
        public Override(T value, int priority = 0)
        {
            Value = value;
            Priority = priority;
        }
    }

    [Serializable]
    public class OverrideContainer<T>
    {
        public List<Override<T>> Overrides = new List<Override<T>>();

        public bool _hasOverride = false;
        public T _overriddenValue;

        public OverrideContainer() { }

        public void AddOverride(T value, int priority = 0)
        {
            AddOverride(new Override<T>(value, priority));
        }

        public void AddOverride(Override<T> item)
        {
            Overrides.Add(item);
            CalculateOverridenValue();
        }

        public void RemoveOverride(T value)
        {
            for(int i = 0; i < Overrides.Count; i++)
            {
                if(Overrides[i].Value.Equals(value))
                {
                    Overrides.RemoveAt(i);
                    CalculateOverridenValue();
                    return;
                }
            }
        }

        public void RemoveOverride(Override<T> item)
        {
            Overrides.Remove(item);
            CalculateOverridenValue();
        }

        private void CalculateOverridenValue()
        {
            if (Overrides.Count == 0)
            {
                _hasOverride = false;
                return;
            }

            Override<T> currOverride = Overrides[0];
            _hasOverride = true;
            for (int i = 1; i < Overrides.Count; i++)
            {
                if(Overrides[i].Priority >= currOverride.Priority)
                {
                    currOverride = Overrides[i];
                }
            }
            _overriddenValue = currOverride.Value;
        }

        public bool TryGetValue(out T overriddenValue)
        {
            overriddenValue = _overriddenValue;
            return _hasOverride;
        }
    }
}
