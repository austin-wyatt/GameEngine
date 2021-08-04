using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public class SlowDebuff : Buff
    {
        public SlowDebuff(Unit affected, int duration, float slowMultiplier) : base(affected, duration)
        {
            Speed.Multiplier = slowMultiplier;

            Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.SpiderWeb, Spritesheets.IconSheet);
        }

        public override Icon GenerateIcon(UIScale scale)
        {
            Icon icon = GenerateIcon(scale, true, Icon.BackgroundType.DebuffBackground);

            return icon;
        }
    }
}
