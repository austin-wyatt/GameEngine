using Empyrean.Engine_Classes;
using Empyrean.Game.Abilities;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Units
{
    public enum ResI
    {
        None = -1,
        Stamina,
        MaxStamina,
        Shields,
        MaxShields,

        FireAffinity,
        FireAffinityTemp,
    }

    public enum ResF
    {
        None = -1,
        Health,
        MaxHealth,
        MovementEnergy,
        MaxMovementEnergy,
        ActionEnergy,
        MaxActionEnergy,
        Speed,
        ShieldBlock,
        DamageBlockedByShields
    }

    [Serializable]
    public class ResourceManager : ISerializable
    {
        [XmlIgnore]
        public Dictionary<ResI, int> ResourceI = new Dictionary<ResI, int>();
        [XmlIgnore]
        public Dictionary<ResF, float> ResourceF = new Dictionary<ResF, float>();

        [XmlElement(Namespace = "_resI")]
        public DeserializableDictionary<ResI, int> _resourceI = new DeserializableDictionary<ResI, int>();
        [XmlElement(Namespace = "_resF")]
        public DeserializableDictionary<ResF, float> _resourceF = new DeserializableDictionary<ResF, float>();

        [XmlIgnore]
        public Unit Unit;

        public static readonly Dictionary<ResI, int> DEFAULT_VALUES_I = new Dictionary<ResI, int>()
        {
            {ResI.None, 0},
            {ResI.Stamina, 3},
            {ResI.MaxStamina, 3},
            {ResI.Shields, 0},
            {ResI.MaxShields, 5},
            {ResI.FireAffinity, 0},
        };

        public static Dictionary<ResF, float> DEFAULT_VALUES_F = new Dictionary<ResF, float>()
        {
            {ResF.None, 0},
            {ResF.Health, 40},
            {ResF.MaxHealth, 40},
            {ResF.MovementEnergy, 6},
            {ResF.MaxMovementEnergy, 6},
            {ResF.ActionEnergy, 3},
            {ResF.MaxActionEnergy, 3},
            {ResF.Speed, 5},
            {ResF.ShieldBlock, 5},
        };

        public ResourceManager() { }

        public ResourceManager(ResourceManager manager)
        {
            ResourceI = new Dictionary<ResI, int>(manager.ResourceI);
            ResourceF = new Dictionary<ResF, float>(manager.ResourceF);
        }

        public int GetResource(ResI resource)
        {
            if(ResourceI.TryGetValue(resource, out int result))
            {
                return ApplyModifiers(resource, result);
            }
            else
            {
                return ApplyModifiers(resource, 0);
            }
        }

        public float GetResource(ResF resource)
        {
            if (ResourceF.TryGetValue(resource, out float result))
            {
                return ApplyModifiers(resource, result);
            }
            else
            {
                return ApplyModifiers(resource, 0);
            }
        }

        public void SetResource(ResI resource, int value)
        {
            if (value == 0) ResourceI.Remove(resource);
            else ResourceI.AddOrSet(resource, value);
        }

        public void SetResource(ResF resource, float value)
        {
            if (value == 0) ResourceF.Remove(resource);
            else ResourceF.AddOrSet(resource, value);
        }

        public void AddResource(ResF resource, float value)
        {
            value += GetResource(resource);

            SetResource(resource, value);
        }

        public void AddResource(ResI resource, int value)
        {
            value += GetResource(resource);

            SetResource(resource, value);
        }

        private int ApplyModifiers(ResI resource, int value)
        {
            if (Unit == null) return value;

            switch (resource)
            {
                case ResI.MaxStamina:
                    return (int)((Unit.Info.BuffManager.GetValue(BuffEffect.MaxStaminaAdditive) + value) * 
                        Unit.Info.BuffManager.GetValue(BuffEffect.MaxStaminaMultiplier));
                case ResI.FireAffinity:
                    return value + GetResource(ResI.FireAffinityTemp);
                case ResI.MaxShields:
                    return value + (int)Unit.Info.BuffManager.GetValue(BuffEffect.MaxShieldsAdditive);
                default:
                    return value;
            }
        }

        private float ApplyModifiers(ResF resource, float value)
        {
            if (Unit == null) return value;

            switch (resource)
            {
                case ResF.Speed:
                    return (value + Unit.Info.BuffManager.GetValue(BuffEffect.SpeedAdditive)) *
                        Unit.Info.BuffManager.GetValue(BuffEffect.SpeedMultiplier);
                case ResF.ShieldBlock:
                    return (value + Unit.Info.BuffManager.GetValue(BuffEffect.ShieldBlockAdditive)) *
                        Unit.Info.BuffManager.GetValue(BuffEffect.ShieldBlockMultiplier);
                case ResF.MaxHealth:
                    return (value + Unit.Info.BuffManager.GetValue(BuffEffect.MaxHealthAdditive)) *
                        Unit.Info.BuffManager.GetValue(BuffEffect.MaxHealthMultiplier);
                case ResF.MaxActionEnergy:
                    return (value + Unit.Info.BuffManager.GetValue(BuffEffect.MaxActionEnergyAdditive)) *
                        Unit.Info.BuffManager.GetValue(BuffEffect.MaxActionEnergyMultiplier);
                case ResF.MaxMovementEnergy:
                    return (value + Unit.Info.BuffManager.GetValue(BuffEffect.MaxMovementEnergyAdditive)) *
                        Unit.Info.BuffManager.GetValue(BuffEffect.MaxMovementEnergyMultiplier);
                default:
                    return value;
            }
        }

        public void PrepareForSerialization()
        {
            _resourceI = new DeserializableDictionary<ResI, int>(ResourceI);
            _resourceF = new DeserializableDictionary<ResF, float>(ResourceF);
        }

        public void CompleteDeserialization()
        {
            ResourceF.Clear();
            ResourceI.Clear();

            _resourceF.FillDictionary(ResourceF);
            _resourceI.FillDictionary(ResourceI);
        }
    }
}
