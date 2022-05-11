using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Objects;
using OpenTK.Mathematics;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes;

namespace Empyrean.Game.Abilities
{
    public class Hide : Ability
    {
        private Icon BrokenMaskIcon;
        public Hide(Unit castingUnit)
        {
            Type = AbilityTypes.BuffDefensive;
            Range = 1;
            CastingUnit = castingUnit;

            //Name = "Hide";

            SelectionInfo.CanSelectTiles = false;
            SelectionInfo.UnitTargetParams.Self = UnitCheckEnum.True;

            SelectionInfo.UnitTargetParams.IsHostile = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            BreakStealth = false;

            //Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.MasqueradeMask, Spritesheets.IconSheet, true, Icon.BackgroundType.NeutralBackground);
            //BrokenMaskIcon = new Icon(Icon.DefaultIconSize, IconSheetIcons.BrokenMask, Spritesheets.IconSheet, true, Icon.BackgroundType.NeutralBackground);
        }

        public override void EnactEffect()
        {
            BeginEffect();

            //CastingUnit.Info.Stealth.SetHiding(true);
            //StealthBuff stealthBuff = new StealthBuff(CastingUnit, -1);

            //CastingUnit.Info.AddBuff(stealthBuff);

            //_Color stealthColor = new _Color(1, 1, 1, 0.5f);

            //void hidingBroken() 
            //{
            //    CastingUnit.Info.Stealth.HidingBrokenActions.Remove(hidingBroken);
            //    CastingUnit.Info.RemoveBuff(stealthBuff);
            //    Scene.Footer.RefreshFooterInfo();

            //    CastingUnit.BaseObject.BaseFrame.RemoveAppliedColor(stealthColor);

            //    CreateIconHoverEffect(BrokenMaskIcon);
            //}

            //CastingUnit.Info.Stealth.HidingBrokenActions.Add(hidingBroken);
            //CastingUnit.BaseObject.BaseFrame.AddAppliedColor(stealthColor);

            //Scene.Footer.RefreshFooterInfo();

            //if (CastingUnit.Info.Stealth.EnemyHasVision()) 
            //{
            //    hidingBroken();
            //    Context.SetFlag(AbilityContext.SkipIconAnimation, true);
            //    Context.SetFlag(AbilityContext.SkipEnergyCost, true);
            //}

            Casted();
            EffectEnded();
        }
    }
}
