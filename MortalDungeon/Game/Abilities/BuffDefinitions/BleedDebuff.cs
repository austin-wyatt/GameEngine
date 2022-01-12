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
            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.BleedingDagger, Spritesheets.IconSheet);
            Name = "Bleed";
            BuffType = BuffType.Debuff;

            Damage = damage;
        }

        public override Icon GenerateIcon(UIScale scale)
        {
            Icon icon = GenerateIcon(scale, true, Icon.BackgroundType.DebuffBackground);

            return icon;
        }

        public override void OnTurnStart()
        {
            AffectedUnit.ApplyDamage(new Unit.DamageParams(GetDamageInstance()) { Buff = this });

            base.OnTurnStart();
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            instance.Damage.Add(DamageType.Bleed, Damage);

            return instance;
        }

        public override Tooltip GenerateTooltip()
        {
            Tooltip tooltip = new Tooltip();

            TextComponent header = new TextComponent();
            header.SetTextScale(0.1f);
            header.SetColor(_Colors.UITextBlack);
            header.SetText(Name);

            TextComponent description = new TextComponent();
            description.SetTextScale(0.05f);
            description.SetColor(_Colors.UITextBlack);
            description.SetText("At the beginning of the unit's turn \nit will suffer " + Damage.ToString("n1").Replace(".0", "") + " bleed damage." +
                "\n\n" + Duration + " turns remaining.");

            tooltip.AddChild(header);
            tooltip.AddChild(description);

            UIDimensions letterScale = header._textField.Letters[0].GetDimensions();

            header.SetPositionFromAnchor(tooltip.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10 + letterScale.Y / 2, 0), UIAnchorPosition.TopLeft);
            description.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            tooltip.Margins = new UIDimensions(0, 60);

            tooltip.FitContents();
            tooltip.BaseComponent.SetPosition(tooltip.Position + tooltip.Margins);

            return tooltip;
        }
    }
}
