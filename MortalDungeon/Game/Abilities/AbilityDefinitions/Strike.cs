using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Objects;

namespace Empyrean.Game.Abilities
{
    public class Strike : TemplateRangedSingleTarget
    {
        public Strike(Unit castingUnit, int range = 1, float damage = 10) : base(castingUnit)
        {
            Type = AbilityTypes.MeleeAttack;
            DamageType = DamageType.Slashing;
            Range = range;
            CastingUnit = castingUnit;

            CastRequirements.AddResourceCost(ResF.ActionEnergy, 2, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

            CastingMethod |= CastingMethod.Weapon | CastingMethod.PhysicalDexterity | CastingMethod.BruteForce;

            //Name = "Strike";

            //Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.CrossedSwords, Spritesheets.IconSheet, true);
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();


            return instance;
        }
    }
}
