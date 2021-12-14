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
    public class TemplateSelfCast : Ability
    {
        public TemplateSelfCast(Unit castingUnit)
        {
            CastingUnit = castingUnit;
            EnergyCost = 5;

            CanTargetGround = false;
            CanTargetSelf = true;
            UnitTargetParams.IsHostile = Disposition.CheckEnum.False;
            UnitTargetParams.IsFriendly = Disposition.CheckEnum.False;
            UnitTargetParams.IsNeutral = Disposition.CheckEnum.False;

            Name = "Self Cast";

            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.QuestionMark, Spritesheets.IconSheet, true);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            List<BaseTile> validTiles = new List<BaseTile> { CastingUnit.Info.TileMapPosition };

            AffectedUnits.Add(CastingUnit);

            TargetAffectedUnits();

            return validTiles;
        }

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


        public override void OnCast()
        {
            TileMap.DeselectTiles();

            base.OnCast();
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            Console.WriteLine("Effect");


            Casted();
            EffectEnded();
        }
    }
}
