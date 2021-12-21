using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public enum BuffType 
    {
        Neutral,
        Debuff,
        Buff
    }
    public class Buff
    {
        public Unit AffectedUnit;

        public BuffModifier OutgoingDamage = new BuffModifier();

        public BuffModifier ShieldBlock = new BuffModifier();

        public BuffModifier EnergyCost = new BuffModifier();

        public BuffModifier ActionEnergyCost = new BuffModifier();

        public BuffModifier SpeedModifier = new BuffModifier();

        public BuffModifier DamageReduction = new BuffModifier();

        public BuffModifier EnergyBoost = new BuffModifier();

        public BuffModifier ActionEnergyBoost = new BuffModifier();

        public BuffModifier SoundModifier = new BuffModifier();

        public Dictionary<DamageType, float> DamageResistances = new Dictionary<DamageType, float>();

        public int MaxDuration = 0;
        public int Duration = 0;
        public bool IndefiniteDuration = false;
        public bool Hidden = false;

        public bool Dispellable = false;
        public bool DispellableStrong = false;

        public string Name = "";
        public BuffType BuffType = BuffType.Neutral;

        /// <summary>
        /// The status condition that having this buff/debuff provides
        /// </summary>
        public StatusCondition StatusCondition;

        public int Grade = 1;

        public int BuffID => _buffID;
        protected int _buffID = _currentBuffID++;
        protected static int _currentBuffID = 0;

        public Icon Icon = new Icon(Icon.DefaultIconSize, Icon.DefaultIcon, Spritesheets.IconSheet);

        public Buff(int duration = -1)
        {
            MaxDuration = duration;
            Duration = duration;
        }
        public Buff(Unit unit, int duration = -1) 
        {
            MaxDuration = duration;
            Duration = duration;
        }

        public virtual void AddBuffToUnit(Unit unit) 
        {
            if (AffectedUnit != null)
            {
                RemoveBuffFromUnit();
            }

            unit.Info.AddBuff(this);
        }

        public virtual void RemoveBuffFromUnit() 
        {
            if (AffectedUnit != null) 
            {
                AffectedUnit.Info.RemoveBuff(this); 
            }
        }

        public virtual Icon GenerateIcon(UIScale scale, bool withBackground = false, Icon.BackgroundType backgroundType = Icon.BackgroundType.NeutralBackground)
        {
            Icon icon = new Icon(Icon, scale, withBackground, backgroundType);
           
            return icon;
        }

        public virtual Icon GenerateIcon(UIScale scale)
        {
            return GenerateIcon(scale, false);
        }

        public virtual Tooltip GenerateTooltip() 
        {
            Tooltip tooltip = new Tooltip();

            return tooltip;
        }

        public virtual void OnTurnStart()
        {
            if (!IndefiniteDuration)
            {
                Duration--;

                if (Duration <= 0)
                {
                    AffectedUnit.Info.RemoveBuff(this);
                }
            }
        }

        public virtual void OnRoundStart()
        {

        }

        public virtual void OnTurnEnd()
        {
            
        }

        public virtual void OnRoundEnd()
        {

        }

        public virtual DamageInstance GetDamageInstance()
        {
            return new DamageInstance();
        }

        public virtual void ModifyDamageInstance(DamageInstance instance, Ability ability) 
        {

        }

        public virtual float ModifyShieldBlockAdditive(Unit unit)
        {
            return 0;
        }

        public class BuffModifier
        {
            public float Additive = 0;
            public float Multiplier = 1;
        }
    }
}
