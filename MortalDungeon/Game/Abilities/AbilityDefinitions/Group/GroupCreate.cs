using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Player;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities.AbilityDefinitions
{
    public class GroupCreate : Ability
    {
        public List<Unit> UnitsToGroup = new List<Unit>();

        public GroupCreate(List<Unit> units)
        {
            CastingUnit = units[0];

            UnitsToGroup = units;

            RefreshFooterOnFinish = false;

            SelectionInfo.CanSelectTiles = false;
            SelectionInfo.UnitTargetParams.Self = UnitCheckEnum.True;
            SelectionInfo.UnitTargetParams.IsControlled = UnitCheckEnum.True;

            MaxCharges = -1;

            AnimationSet = new Serializers.AnimationSet();

            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)UIControls.StatusOnline },
                Spritesheet = (int)TextureName.UIControlsSpritesheet
            });

            Name = new Serializers.TextInfo(18, 3);
            Description = new Serializers.TextInfo(17, 3);

            //if (PlayerParty.Grouped)
            //{
            //    AnimationSet.Animations.Add(new Serializers.Animation()
            //    {
            //        FrameIndices = { (int)UIControls.StatusOnline },
            //        Spritesheet = (int)TextureName.UIControlsSpritesheet
            //    });
            //}
            //else
            //{
            //    if (PlayerParty.CanGroupUnits())
            //    {
            //        AnimationSet.Animations.Add(new Serializers.Animation()
            //        {
            //            FrameIndices = { (int)UIControls.StatusAway },
            //            Spritesheet = (int)TextureName.UIControlsSpritesheet
            //        });
            //    }
            //    else
            //    {
            //        AnimationSet.Animations.Add(new Serializers.Animation()
            //        {
            //            FrameIndices = { (int)UIControls.StatusOffline },
            //            Spritesheet = (int)TextureName.UIControlsSpritesheet
            //        });
            //    }
            //}
        }

        public override void EnactEffect()
        {
            BeginEffect();

            foreach(Unit unit in UnitsToGroup)
            {
                if(unit.Info.Group != null)
                {
                    unit.Info.Group.DissolveGroup();
                }
            }

            UnitGroup unitGroup = new UnitGroup(UnitsToGroup);
            unitGroup.GroupAbilities.Add(new GroupMove(unitGroup));

            Casted();
            EffectEnded();

            Scene.Footer.UpdateFooterInfo(unitGroup.Leader, footerMode: UI.FooterMode.Group);
        }

        public override void OnSelect(CombatScene scene, TileMap currentMap)
        {
            //base.OnSelect(scene, currentMap);

            if (UnitsToGroup.Count > 0)
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
