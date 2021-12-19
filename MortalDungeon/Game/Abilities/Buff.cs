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
    internal enum BuffType 
    {
        Neutral,
        Debuff,
        Buff
    }
    internal class Buff
    {
        internal Unit AffectedUnit;

        internal BuffModifier OutgoingDamage = new BuffModifier();

        internal BuffModifier ShieldBlock = new BuffModifier();

        internal BuffModifier EnergyCost = new BuffModifier();

        internal BuffModifier ActionEnergyCost = new BuffModifier();

        internal BuffModifier SpeedModifier = new BuffModifier();

        internal BuffModifier DamageReduction = new BuffModifier();

        internal BuffModifier EnergyBoost = new BuffModifier();

        internal BuffModifier ActionEnergyBoost = new BuffModifier();

        internal BuffModifier SoundModifier = new BuffModifier();

        internal Dictionary<DamageType, float> DamageResistances = new Dictionary<DamageType, float>();

        internal int MaxDuration = 0;
        internal int Duration = 0;
        internal bool IndefiniteDuration = false;
        internal bool Hidden = false;

        internal bool Dispellable = false;
        internal bool DispellableStrong = false;

        internal string Name = "";
        internal BuffType BuffType = BuffType.Neutral;

        /// <summary>
        /// The status condition that having this buff/debuff provides
        /// </summary>
        internal StatusCondition StatusCondition;

        internal int Grade = 1;

        internal int BuffID => _buffID;
        protected int _buffID = _currentBuffID++;
        protected static int _currentBuffID = 0;

        internal Icon Icon = new Icon(Icon.DefaultIconSize, Icon.DefaultIcon, Spritesheets.IconSheet);

        internal Buff(int duration = -1)
        {
            MaxDuration = duration;
            Duration = duration;
        }
        internal Buff(Unit unit, int duration = -1) 
        {
            MaxDuration = duration;
            Duration = duration;
        }

        internal virtual void AddBuffToUnit(Unit unit) 
        {
            if (AffectedUnit != null)
            {
                RemoveBuffFromUnit();
            }

            unit.Info.AddBuff(this);
        }

        internal virtual void RemoveBuffFromUnit() 
        {
            if (AffectedUnit != null) 
            {
                AffectedUnit.Info.RemoveBuff(this); 
            }
        }

        internal virtual Icon GenerateIcon(UIScale scale, bool withBackground = false, Icon.BackgroundType backgroundType = Icon.BackgroundType.NeutralBackground)
        {
            Icon icon = new Icon(Icon, scale, withBackground, backgroundType);
           
            return icon;
        }

        internal virtual Icon GenerateIcon(UIScale scale)
        {
            return GenerateIcon(scale, false);
        }

        internal virtual Tooltip GenerateTooltip() 
        {
            Tooltip tooltip = new Tooltip();

            return tooltip;
        }

        internal virtual void OnTurnStart()
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

        internal virtual void OnRoundStart()
        {

        }

        internal virtual void OnTurnEnd()
        {
            
        }

        internal virtual void OnRoundEnd()
        {

        }

        internal virtual DamageInstance GetDamageInstance()
        {
            return new DamageInstance();
        }

        internal virtual void ModifyDamageInstance(DamageInstance instance, Ability ability) 
        {

        }

        internal virtual float ModifyShieldBlockAdditive(Unit unit)
        {
            return 0;
        }

        internal class BuffModifier
        {
            internal float Additive = 0;
            internal float Multiplier = 1;
        }
    }
}
