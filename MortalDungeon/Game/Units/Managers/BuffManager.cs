using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Units
{

    [Serializable]
    public class BuffManager : ISerializable
    {
        [XmlIgnore]
        public Unit Unit;

        [XmlIgnore]
        public List<Buff> Buffs = new List<Buff>();
        public List<Buff> _buffs = new List<Buff>();

        [XmlIgnore]
        private Dictionary<int, float> _collatedBuffValues = new Dictionary<int, float>();


        public const float ADDITIVE_BASE_VALUE = 0;
        public const float MULTIPLIER_BASE_VALUE = 1;
        public const string BUFF_NAMESPACE = "MortalDungeon.Definitions.Buffs.";

        public BuffManager() { }

        public float GetValue(BuffEffect effect)
        {
            if(_collatedBuffValues.TryGetValue((int)effect, out float value))
            {
                return value;
            }
            else
            {
                if (effect > BuffEffect.ADDITIVE_START && effect < BuffEffect.ADDITIVE_END)
                {
                    return ADDITIVE_BASE_VALUE;
                }
                else if (effect > BuffEffect.MULTIPLIER_START && effect < BuffEffect.MULTIPLIER_END)
                {
                    return MULTIPLIER_BASE_VALUE;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void GetDamageResistances(DamageType type, out float additive, out float multiplier)
        {
            additive = GetValue((BuffEffect)((int)type + (int)BuffEffect.DAMAGE_RESISTANCE_ADDITIVE));
            multiplier = GetValue((BuffEffect)((int)type + (int)BuffEffect.DAMAGE_RESISTANCE_MULTIPLIER));
        }

        public void GetDamageBonuses(DamageType type, out float additive, out float multiplier)
        {
            additive = GetValue((BuffEffect)((int)type + (int)BuffEffect.DAMAGE_TYPE_ADDITIVE));
            multiplier = GetValue((BuffEffect)((int)type + (int)BuffEffect.DAMAGE_TYPE_MULTIPLIER));
        }

        public void CollateBuffValues()
        {
            _collatedBuffValues = new Dictionary<int, float>();

            foreach(var buff in Buffs)
            {
                foreach(var effectKVP in buff.BuffEffects)
                {
                    float effectValue = float.MinValue;

                    BuffEffect effect = (BuffEffect)effectKVP.Key;

                    if(_collatedBuffValues.TryGetValue(effectKVP.Key, out var val))
                    {
                        effectValue = val;
                    }

                    if(effect > BuffEffect.ADDITIVE_START && effect < BuffEffect.ADDITIVE_END)
                    {
                        if(effectValue == float.MinValue)
                        {
                            effectValue = ADDITIVE_BASE_VALUE; //additive effect values need to start at 0
                        }

                        effectValue += effectKVP.Value;
                    }
                    else if (effect > BuffEffect.MULTIPLIER_START && effect < BuffEffect.MULTIPLIER_END)
                    {
                        if (effectValue == float.MinValue)
                        {
                            effectValue = MULTIPLIER_BASE_VALUE; //multiplicative effect values need to start at 1
                        }

                        effectValue *= effectKVP.Value;
                    }

                    _collatedBuffValues.AddOrSet(effectKVP.Key, effectValue);
                }
            }
        }

        [XmlIgnore]
        public object _buffLock = new object();
        public void AddBuff(Buff buff)
        {
            lock (_buffLock)
            {
                Buffs.Add(buff);
            }

            buff.OnAddedToUnit(Unit);

            CollateBuffValues();

            if(Unit.Scene.Footer.CurrentUnit == Unit)
            {
                Unit.Scene.Footer.RefreshFooterInfo();
            }
        }

        public void RemoveBuff(Buff buff)
        {
            lock (_buffLock)
            {
                Buffs.Remove(buff);
            }

            buff.OnRemovedFromUnit(Unit);

            CollateBuffValues();

            if (Unit.Scene.Footer.CurrentUnit == Unit)
            {
                Unit.Scene.Footer.RefreshFooterInfo();
            }
        }

        public void CompleteDeserialization()
        {
            Buffs.Clear();

            List<Buff> newBuffs = new List<Buff>();

            foreach (Buff buff in _buffs)
            {
                buff.CompleteDeserialization();

                Type type = Type.GetType(BUFF_NAMESPACE + buff._typeName);

                var newBuff = Activator.CreateInstance(type, new object[] { buff }) as Buff;

                newBuff.OnRecreated(Unit);

                newBuffs.Add(newBuff);
            }

            Buffs = newBuffs;

            CollateBuffValues();
        }

        public void PrepareForSerialization()
        {
            _buffs.Clear();

            foreach (Buff buff in Buffs)
            {
                buff.PrepareForSerialization();
            }

            for(int i = 0; i < Buffs.Count; i++)
            {
                _buffs.Add(new Buff(Buffs[i]));
            }
        }
    }
}
