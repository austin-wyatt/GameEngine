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
            UnitTargetParams.Self = UnitCheckEnum.True;
            UnitTargetParams.IsHostile = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            //Name = "Self Cast";

            //SetIcon(IconSheetIcons.QuestionMark, Spritesheets.IconSheet);

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)IconSheetIcons.QuestionMark },
                Spritesheet = (int)TextureName.IconSpritesheet
            });
        }

        public override void GetValidTileTargets(TileMap tileMap, out List<Tile> affectedTiles, out List<Unit> affectedUnits,
            List<Unit> units = default, Tile position = null)
        {
            affectedTiles = new List<Tile> { CastingUnit.Info.TileMapPosition };

            affectedUnits = new List<Unit> { CastingUnit };
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
            BeginEffect();

            Console.WriteLine("Effect");


            Casted();
            EffectEnded();
        }
    }
}
