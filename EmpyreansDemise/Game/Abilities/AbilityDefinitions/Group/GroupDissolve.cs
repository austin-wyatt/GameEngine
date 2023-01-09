using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Player;
using Empyrean.Game.Tiles;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities.AbilityDefinitions
{
    public class GroupDissolve : Ability
    {
        UnitGroup SelectedGroup;

        public GroupDissolve(UnitGroup group)
        {
            CastingUnit = group.Units[0];

            SelectedGroup = group;

            RefreshFooterOnFinish = false;

            SelectionInfo = new SelectionInfo(this);

            SelectionInfo.CanSelectTiles = false;

            MaxCharges = -1;

            AnimationSet = new Serializers.AnimationSet();

            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)UIControls.Stop },
                Spritesheet = (int)TextureName.UIControlsSpritesheet
            });

            Name = new Serializers.TextInfo(19, 3);
            Description = new Serializers.TextInfo(20, 3);
        }

        public override void EnactEffect()
        {
            BeginEffect();

            SelectedGroup.DissolveGroup();

            Casted();
            EffectEnded();

            Scene.Footer.UpdateFooterInfo(SelectedGroup.Leader, footerMode: UI.FooterMode.SingleUnit);
        }

        public override void OnSelect(CombatScene scene, TileMap currentMap)
        {
            if (SelectedGroup.Units.Count > 0)
            {
                EnactEffect();
            }
            else
            {
                Scene.DeselectAbility();
            }
        }

        public override Tooltip GenerateTooltip()
        {
            string body = Description.ToString();

            Tooltip tooltip = UIHelpers.GenerateTooltipWithHeader(Name.ToString(), body);

            return tooltip;
        }
    }
}
