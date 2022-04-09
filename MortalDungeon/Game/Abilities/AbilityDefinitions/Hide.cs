﻿using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes;

namespace MortalDungeon.Game.Abilities
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

            CanTargetGround = false;
            UnitTargetParams.Self = UnitCheckEnum.True;

            UnitTargetParams.IsHostile = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            BreakStealth = false;

            //Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.MasqueradeMask, Spritesheets.IconSheet, true, Icon.BackgroundType.NeutralBackground);
            //BrokenMaskIcon = new Icon(Icon.DefaultIconSize, IconSheetIcons.BrokenMask, Spritesheets.IconSheet, true, Icon.BackgroundType.NeutralBackground);
        }

        //public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null, List<Unit> validUnits = null)
        //{
        //    base.GetValidTileTargets(tileMap);

        //    List<BaseTile> validTiles = new List<BaseTile> { CastingUnit.Info.TileMapPosition };

        //    AffectedUnits.Add(CastingUnit);

        //    TargetAffectedUnits();

        //    return validTiles;
        //}

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedTiles.FindIndex(t => t.TilePoint == unit.Info.TileMapPosition) != -1)
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
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
