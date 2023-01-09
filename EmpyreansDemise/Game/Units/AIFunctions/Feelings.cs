using Empyrean.Game.Combat;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Units.AIFunctions
{
    public enum FeelingType
    {
        /// <summary>
        /// Increases the weight of actions targeted towards units with lower health
        /// </summary>
        Bloodthirst,
        Fear,
        Caution,
        FearThreshold, //If a unit's fear (after buff value modifications, etc) is above this value then adverse effects can occur
        /// <summary>
        /// Passivity directly counteracts the effects of bloodlust against a target. <para/>
        /// A value of 0 will not reduce bloodlust value at all while a value of 1 will completely remove the effects of bloodlust.
        /// </summary>
        Passivity, 
        /// <summary>
        /// Increases the weight of actions targeted towards units with lower shields
        /// </summary>
        Opportunism,

    }

    [XmlType(TypeName = "FEL")]
    [Serializable]
    public class Feelings : ISerializable
    {
        [XmlIgnore]
        public Dictionary<FeelingType, float> BaseFeelingValues = new Dictionary<FeelingType, float>();

        [XmlElement("fv")]
        public DeserializableDictionary<FeelingType, float> _baseFeelingValues = new DeserializableDictionary<FeelingType, float>();

        [XmlIgnore]
        public Unit Unit;

        public Feelings(Unit unit)
        {
            Unit = unit;
        }

        private static Dictionary<FeelingType, float> DefaultFeelingValues = new Dictionary<FeelingType, float>
        {
            { FeelingType.Bloodthirst, 0.2f },
            { FeelingType.Fear, 0.3f },
            { FeelingType.Caution, 0.2f }, //Caution > (1 - TileEffect.Danger) then the AI will not want to walk over that tile (unless immune)
            { FeelingType.FearThreshold, 1 },
            { FeelingType.Passivity, 0 },
            { FeelingType.Opportunism, 0.2f }
        };

        public float GetFeelingValue(FeelingType feeling, InformationMorsel target = null)
        {
            float baseVal;

            if(BaseFeelingValues.TryGetValue(feeling, out float val))
            {
                baseVal = val;
            }
            else
            {
                baseVal = DefaultFeelingValues[feeling];
            }

            //get base val, modify based on unit species
            //then in a switch statement do special modifications per FeelingType
            //(such as fearful being a product of the unit's current HP)

            switch (feeling)
            {
                case FeelingType.Fear:
                    baseVal *= 1 + Unit.GetResF(ResF.Health) / Unit.GetResF(ResF.MaxHealth);
                    break;
                case FeelingType.Bloodthirst:
                    if(target != null)
                    {
                        baseVal *= (1 + (1 - target.Health / target.Unit.GetResF(ResF.MaxHealth))) * 
                            (1 - target.Unit.AI.Feelings.GetFeelingValue(FeelingType.Passivity));
                    }
                    break;
                case FeelingType.Opportunism:
                    if (target != null)
                    {
                        baseVal *= 1 + (-target.Shields / 20);
                    }
                    break;
            }

            float buffAdditive = Unit.Info.BuffManager.GetValue(Abilities.BuffEffect.AI_FEELINGS_ADDITIVE + (int)feeling);
            float buffMultiplicative = Unit.Info.BuffManager.GetValue(Abilities.BuffEffect.AI_FEELINGS_MULTIPLICATIVE + (int)feeling);

            baseVal = (baseVal + buffAdditive) * buffMultiplicative;

            return baseVal;
        }

        public void CompleteDeserialization()
        {
            _baseFeelingValues.FillDictionary(BaseFeelingValues);
        }

        public void PrepareForSerialization()
        {
            _baseFeelingValues = new DeserializableDictionary<FeelingType, float>(BaseFeelingValues);
        }
    }
}
