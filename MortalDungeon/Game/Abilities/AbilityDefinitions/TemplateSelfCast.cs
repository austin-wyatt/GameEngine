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

            CanTargetGround = false;
            CanTargetSelf = true;
            UnitTargetParams.IsHostile = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            //Name = "Self Cast";

            SetIcon(IconSheetIcons.QuestionMark, Spritesheets.IconSheet);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null, List<Unit> validUnits = null)
        {
            base.GetValidTileTargets(tileMap);

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
            TileMap.Controller.DeselectTiles();

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
