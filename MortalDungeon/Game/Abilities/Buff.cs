using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public class Buff
    {
        public Unit Unit;

        public BuffModifier OutgoingDamage = new BuffModifier();

        public BuffModifier ShieldBlock = new BuffModifier();

        public BuffModifier EnergyCost = new BuffModifier();

        public BuffModifier Speed = new BuffModifier();

        public BuffModifier DamageReduction = new BuffModifier();

        public Dictionary<DamageType, float> DamageResistances = new Dictionary<DamageType, float>();

        public int MaxDuration = 0;
        public int Duration = 0;
        public bool IndefiniteDuration = false;
        public bool Hidden = false;

        public string Name = "";

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

            unit.Buffs.Add(this);
            Unit = unit;
        }

        public virtual void RemoveBuffFromUnit() 
        {
            if (Unit != null) 
            {
                Unit.Buffs.Remove(this);
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

        public virtual void OnTurnStart()
        {
            if (!IndefiniteDuration)
            {
                Duration--;

                if (Duration <= 0)
                {
                    Unit.Buffs.Remove(this);
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

        public class BuffModifier
        {
            public float Additive = 0;
            public float Multiplier = 1;
        }
    }
}
