using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MortalDungeon.Game.Abilities
{
    public class StunDebuff : Buff
    {
        public StunDebuff(Unit affected, int duration) : base(affected, duration)
        {
            Name = "Stun";
            BuffType = BuffType.Debuff;

            StatusCondition = StatusCondition.Stunned;

            Icon = new Icon(Icon.DefaultIconSize, Character.And, Spritesheets.CharacterSheet);
        }

        public override void RemoveBuffFromUnit()
        {
            base.RemoveBuffFromUnit();
        }

        public override Icon GenerateIcon(UIScale scale)
        {
            Icon icon = GenerateIcon(scale, true, Icon.BackgroundType.DebuffBackground);

            return icon;
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
            description.SetText($"Unit is stunned for {Duration} turn{(Duration != 1 ? "s" : "")}");

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

        public override void OnTurnStart()
        {
            base.OnTurnStart();
        }
    }
}
