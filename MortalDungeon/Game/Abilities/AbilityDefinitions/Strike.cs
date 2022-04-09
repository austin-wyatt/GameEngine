using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;

namespace MortalDungeon.Game.Abilities
{
    public class Strike : TemplateRangedSingleTarget
    {
        public Strike(Unit castingUnit, int range = 1, float damage = 10) : base(castingUnit)
        {
            Type = AbilityTypes.MeleeAttack;
            DamageType = DamageType.Slashing;
            Range = range;
            CastingUnit = castingUnit;
            Damage = damage;
            ActionCost = 2;

            CastingMethod |= CastingMethod.Weapon | CastingMethod.PhysicalDexterity | CastingMethod.BruteForce;

            //Name = "Strike";

            //Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.CrossedSwords, Spritesheets.IconSheet, true);
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            instance.Damage.Add(DamageType, GetDamage());

            return instance;
        }
    }
}
