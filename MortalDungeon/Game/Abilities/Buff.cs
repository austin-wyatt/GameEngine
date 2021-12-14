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
        public Unit Unit;

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
            Unit = unit;
            MaxDuration = duration;
            Duration = duration;

            AddBuffToUnit(unit);
        }

        public virtual void AddBuffToUnit(Unit unit) 
        {
            if (Unit != null) 
            {
                RemoveBuffFromUnit();
            }

            unit.Info.Buffs.Add(this);
            Unit = unit;
        }

        public virtual void RemoveBuffFromUnit() 
        {
            if (Unit != null) 
            {
                Unit.Info.Buffs.Remove(this);
                Unit = null;
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
                    Unit.Info.Buffs.Remove(this);
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

        public class BuffModifier
        {
            public float Additive = 0;
            public float Multiplier = 1;
        }
    }
}
