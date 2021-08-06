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
    public class BleedDebuff : Buff
    {
        public float Damage;
        public BleedDebuff(Unit affected, int duration, float damage) : base(affected, duration)
        {
            Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.BleedingDagger, Spritesheets.IconSheet);
            Name = "Bleed";

            Damage = damage;
        }

        public override Icon GenerateIcon(UIScale scale)
        {
            Icon icon = GenerateIcon(scale, true, Icon.BackgroundType.DebuffBackground);

            return icon;
        }

        public override void OnTurnStart()
        {
            Unit.ApplyDamage(Damage, DamageType.Bleed);

            base.OnTurnStart();
        }

        public override Tooltip GenerateTooltip()
        {
            Tooltip tooltip = new Tooltip();

            TextComponent header = new TextComponent();
            header.SetTextScale(0.1f);
            header.SetColor(Colors.UITextBlack);
            header.SetText(Name);

            TextComponent description = new TextComponent();
            description.SetTextScale(0.05f);
            description.SetColor(Colors.UITextBlack);
            description.SetText("At the beginning of the unit's turn \nit will suffer " + Damage.ToString("n1").Replace(".0", "") + " bleed damage." +
                "\n\n" + Duration + " turns remaining.");

            tooltip.AddChild(header);
            tooltip.AddChild(description);

            header.SetPositionFromAnchor(tooltip.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            description.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            tooltip.Margins = new UIDimensions(0, 60);

            tooltip.FitContents();
            tooltip.BaseComponent.SetPosition(tooltip.Position + tooltip.Margins);

            return tooltip;
        }
    }
}
