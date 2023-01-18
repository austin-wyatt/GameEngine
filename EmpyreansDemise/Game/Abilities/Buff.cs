using Empyrean.Definitions.Buffs;
using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Entities;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Empyrean.Game.Abilities
{
    public enum BuffEffect
    {
        #region Additive effects
        ADDITIVE_START = 0,
        //--------------------
        //Additive effects go here

        SpeedAdditive,
        ShieldBlockAdditive,
        PhysicalDamageAdditive,
        EnergyAdditive,
        ActionEnergyAdditive,
        EnergyCostAdditive,
        ActionEnergyCostAdditive,
        GeneralDamageAdditive,
        MovementEnergyAdditive,
        MaxStaminaAdditive,
        MaxShieldsAdditive,
        MaxHealthAdditive,
        HealthCostAdditive,
        StaminaCostAdditive,
        FireAffinityCostAdditive,
        MaxMovementEnergyAdditive,
        MaxActionEnergyAdditive,

        #region ai feelings additive
        AI_FEELINGS_ADDITIVE = 4250,
        //[4250-4499] reserved for AI feeling additive values
        #endregion

        #region damage types additive
        DAMAGE_TYPE_ADDITIVE = 4500,
        //[4500-4749] reserved for damage type additive values
        #endregion

        #region damage resistance additive
        DAMAGE_RESISTANCE_ADDITIVE = 4750,
        //[4750-4999] reserved for damage type multipliers
        #endregion

        //End additive effects
        //--------------------
        ADDITIVE_END = 5000,
        #endregion
        #region Multiplier effects
        MULTIPLIER_START = 5000,
        //--------------------
        //Multipler effects go here

        SpeedMultiplier,
        ShieldBlockMultiplier,
        PhysicalDamageMultiplier,
        EnergyMultiplier,
        ActionEnergyMultiplier,
        EnergyCostMultiplier,
        ActionEnergyCostMultiplier,
        GeneralDamageMultiplier,
        MovementEnergyCostMultiplier,
        MaxStaminaMultiplier,
        MaxHealthMultiplier,
        HealthCostMultiplier,
        StaminaCostMultiplier,
        FireAffinityCostMultiplier,
        MaxMovementEnergyMultiplier,
        MaxActionEnergyMultiplier,

        #region ai feelings multiplicative
        AI_FEELINGS_MULTIPLICATIVE = 9250,
        //[9250-9499] reserved for AI feeling additive values
        #endregion

        #region damage types multiplicative
        DAMAGE_TYPE_MULTIPLIER = 9500,
        //[9500-9749] reserved for damage type multipliers
        #endregion

        #region damage resistance multiplicative
        DAMAGE_RESISTANCE_MULTIPLIER = 9750,
        //[9750-9999] reserved for damage type multipliers
        #endregion

        //End Multiplier effects
        //--------------------
        MULTIPLIER_END = 10000,

        #region Boolean values
        //General boolean values (0 or 1)
        //--------------------
        BOOL_START = 20000,
        Equipment_DisableArmor,
        Equipment_DisableWeapon1,
        Equipment_DisableWeapon2,
        Equipment_DisableConsumable1,
        Equipment_DisableConsumable2,
        Equipment_DisableConsumable3,
        Equipment_DisableConsumable4,
        Equipment_DisableGloves,
        Equipment_DisableBoots,
        Equipment_DisableJewlery1,
        Equipment_DisableJewlery2,
        Equipment_DisableTrinket,

        #endregion
        #endregion
    }

    [XmlInclude(typeof(Dagger_CoupDeGraceDebuff))]
    [XmlInclude(typeof(GenericEffectBuff))]
    [XmlInclude(typeof(GroupedDebuff))]
    [XmlInclude(typeof(StackingDebuff))]
    [XmlInclude(typeof(StunDebuff))]
    [XmlInclude(typeof(WebSlowDebuff))]
    [XmlInclude(typeof(StrongBonesBuff))]

    [Serializable]
    public class Buff : ISerializable
    {
        [XmlIgnore]
        public Unit Unit;

        public PermanentId OwnerId;

        [XmlIgnore]
        public AnimationSet AnimationSet = null;

        [XmlIgnore]
        public Dictionary<int, float> BuffEffects = new Dictionary<int, float>();

        public DeserializableDictionary<int, float> _buffEffects = new DeserializableDictionary<int, float>();

        public string _typeName = "";

        public TextEntry Name = TextEntry.EMPTY_ENTRY;
        public TextEntry Description = TextEntry.EMPTY_ENTRY;

        public bool Initialized = false;


        /// <summary>
        /// An arbitrary string that can be set by the source of the buff. <para />
        /// With this a buff can be be made unique across multiple instances of a source
        /// by first checking that the identifier is not already present on a unit.
        /// </summary>
        public string Identifier = "";

        /// <summary>
        /// The remaining duration of the buff. -1 indicates the duration is indefinite.
        /// </summary>
        public int Duration = -1;

        /// <summary>
        /// The initial duration of the buff. Used for cases where the duration of a buff can be refreshed
        /// </summary>
        public int BaseDuration = -1;

        /// <summary>
        /// The amount of stacks on the buff. MinValue indicates that the buff has not been initialized
        /// </summary>
        public int Stacks = int.MinValue;
        /// <summary>
        /// Determines whether the buff should be displayed in the UI.
        /// </summary>
        public bool Invisible = true;

        public bool RemoveOnZeroStacks = false;

        public Buff() 
        {
            AssignAnimationSet();
        }

        public Buff(Buff buff)
        {
            BuffEffects = new Dictionary<int, float>(buff.BuffEffects);
            _buffEffects = new DeserializableDictionary<int, float>(buff._buffEffects);

            Invisible = buff.Invisible;
            Duration = buff.Duration;
            BaseDuration = buff.BaseDuration;

            Stacks = buff.Stacks;
            _typeName = buff._typeName;

            Identifier = buff.Identifier;
            RemoveOnZeroStacks = buff.RemoveOnZeroStacks;

            Initialized = buff.Initialized;
            OwnerId = buff.OwnerId;

            AssignAnimationSet();
        }

        #region Buff effects
        public void SetBuffEffect(BuffEffect effect, float value)
        {
            if (effect < BuffEffect.ADDITIVE_END && effect > BuffEffect.ADDITIVE_START)
            {
                if(value == BuffManager.ADDITIVE_BASE_VALUE)
                {
                    BuffEffects.Remove((int)effect);
                }
                else
                {
                    BuffEffects.AddOrSet((int)effect, value);
                }
            }
            else if(effect < BuffEffect.MULTIPLIER_END && effect > BuffEffect.MULTIPLIER_START)
            {
                if (value == BuffManager.MULTIPLIER_BASE_VALUE)
                {
                    BuffEffects.Remove((int)effect);
                }
                else
                {
                    BuffEffects.AddOrSet((int)effect, value);
                }
            }
            else
            {
                BuffEffects.AddOrSet((int)effect, value);
            }

            Unit?.Info.BuffManager.CollateBuffValues();
        }

        public void RemoveBuffEffect(BuffEffect effect)
        {
            BuffEffects.Remove((int)effect);

            Unit?.Info.BuffManager.CollateBuffValues();
        }

        public void SetDamageAdditive(DamageType type, float value)
        {
            SetBuffEffect((BuffEffect)((int)type + (int)BuffEffect.DAMAGE_TYPE_ADDITIVE), value);
        }
        public void SetDamageMultiplier(DamageType type, float value)
        {
            SetBuffEffect((BuffEffect)((int)type + (int)BuffEffect.DAMAGE_TYPE_MULTIPLIER), value);
        }

        public void SetDamageResistanceAdditive(DamageType type, float value)
        {
            SetBuffEffect((BuffEffect)((int)type + (int)BuffEffect.DAMAGE_RESISTANCE_ADDITIVE), value);
        }

        public void SetDamageResistanceMultiplier(DamageType type, float value)
        {
            SetBuffEffect((BuffEffect)((int)type + (int)BuffEffect.DAMAGE_RESISTANCE_MULTIPLIER), value);
        }

        #endregion

        public virtual async Task OnAddedToUnit(Unit unit) 
        {
            Unit = unit;

            AddEventListeners();
            Initialize();
        }
        public virtual async Task OnRemovedFromUnit(Unit unit) 
        {
            RemoveEventListeners();
        }

        protected void Initialize()
        {
            Initialized = true;
        }

        protected virtual void AssignAnimationSet() { }

        public Icon GetIcon()
        {
            if(AnimationSet != null)
            {
                return new Icon(Icon.DefaultIconSize, AnimationSet.BuildAnimationsFromSet());
            }

            return null;
        }

        public virtual void OnRecreated(Unit unit)
        {
            Unit = unit;

            AddEventListeners();
        }

        public virtual void AddEventListeners() 
        {
            Unit.TurnEnd += CheckDuration;
        }
        public virtual void RemoveEventListeners() 
        {
            Unit.TurnEnd -= CheckDuration;
        }

        public virtual async Task AddStack()
        {
            Stacks++;

            if (!Invisible && Unit?.Scene.Footer.CurrentUnit == Unit && Unit != null)
            {
                Unit.Scene.Footer.RefreshFooterInfo();
            }

            Unit?.OnStateChanged();
        }

        public virtual async Task RemoveStack()
        {
            Stacks--;

            if(!Invisible && Unit?.Scene.Footer.CurrentUnit == Unit)
            {
                Unit?.Scene.Footer.RefreshFooterInfo();
            }

            if (RemoveOnZeroStacks && Stacks == 0)
            {
                Unit?.Info.BuffManager.RemoveBuff(this);
            }

            Unit?.OnStateChanged();
        }

        protected virtual async Task CheckDuration(Unit unit)
        {
            if (Duration > -1)
            {
                Duration--;

                if (Duration <= 0)
                {
                    Unit?.Info.BuffManager.RemoveBuff(this);
                }
            }
        }

        public bool CompareIdentifier(Buff buffToCheck)
        {
            return Identifier != "" && (Identifier == buffToCheck.Identifier);
        }


        public void PrepareForSerialization()
        {
            _buffEffects = new DeserializableDictionary<int, float>(BuffEffects);

            _typeName = GetType().Name;
        }

        public void CompleteDeserialization()
        {
            BuffEffects.Clear();
            _buffEffects.FillDictionary(BuffEffects);
        }
    }
}
