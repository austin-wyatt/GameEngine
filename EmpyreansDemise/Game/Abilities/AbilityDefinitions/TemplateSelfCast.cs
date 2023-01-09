using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Objects;

namespace Empyrean.Game.Abilities
{
    public class TemplateSelfCast : Ability
    {
        public TemplateSelfCast(Unit castingUnit)
        {
            CastingUnit = castingUnit;

            SelectionInfo.CanSelectTiles = false;
            SelectionInfo.UnitTargetParams.Self = UnitCheckEnum.True;
            SelectionInfo.UnitTargetParams.IsHostile = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            //Name = "Self Cast";

            //SetIcon(IconSheetIcons.QuestionMark, Spritesheets.IconSheet);

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)IconSheetIcons.QuestionMark },
                Spritesheet = (int)TextureName.IconSpritesheet
            });
        }

        //public override void GetValidTileTargets(TileMap tileMap, out List<Tile> affectedTiles, out List<Unit> affectedUnits,
        //    List<Unit> units = default, Tile position = null)
        //{
        //    affectedTiles = new List<Tile> { CastingUnit.Info.TileMapPosition };

        //    affectedUnits = new List<Unit> { CastingUnit };
        //}

        //public override bool OnUnitClicked(Unit unit)
        //{
        //    if (!base.OnUnitClicked(unit))
        //        return false;

        //    if (AffectedTiles.FindIndex(t => t.TilePoint == unit.Info.TileMapPosition) != -1)
        //    {
        //        SelectedUnit = unit;
        //        EnactEffect();
        //    }

        //    return true;
        //}


        public override void OnCast()
        {
            TileMap.Controller.DeselectTiles();

            base.OnCast();
        }

        public override void EnactEffect()
        {
            BeginEffect();

            Console.WriteLine("Effect");


            Casted();
            EffectEnded();
        }
    }
}
