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
    public class GenericSelectGround : Ability
    {
        public Action<Tile> OnGroundSelected = null;

        public GenericSelectGround(Unit castingUnit, int range = 3)
        {
            Type = AbilityTypes.Empty;
            DamageType = DamageType.NonDamaging;
            Range = range;
            CastingUnit = castingUnit;

            SelectionInfo.CanSelectTiles = true;
            SelectionInfo.UnitTargetParams.IsHostile = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            //Name = "Generic Select Ground";

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
        //    TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(CastingUnit.Info.TileMapPosition, Range)
        //    {
        //        Units = units,
        //        CastingUnit = CastingUnit
        //    };

        //    List<Tile> validTiles = tileMap.FindValidTilesInRadius(param);

        //    if (CastingUnit.AI.ControlType == ControlType.Controlled && CanTargetGround)
        //    {
        //        validTiles.ForEach(tile =>
        //        {
        //            tile.TilePoint.ParentTileMap.Controller.SelectTile(tile);
        //        });
        //    }

        //    affectedTiles = validTiles;
        //    affectedUnits = new List<Unit>();
        //}

        //public override void OnTileClicked(TileMap map, Tile tile)
        //{
        //    if (AffectedTiles.Exists(t => t == tile))
        //    {
        //        SelectedTile = tile;
        //        EnactEffect();
        //        Scene._selectedAbility = null;

        //        map.Controller.DeselectTiles();
        //    }
        //}


        //public override void OnCast()
        //{
        //    ClearSelectedTiles();

        //    base.OnCast();
        //}

        public override void OnAICast()
        {
            base.OnAICast();
        }

        //public override void EnactEffect()
        //{
        //    BeginEffect();

        //    Casted();
        //    EffectEnded();

        //    OnGroundSelected?.Invoke(SelectedTile);
        //}

        public override void OnRightClick()
        {
            base.OnRightClick();
        }

        //public override void OnAbilityDeselect()
        //{
        //    ClearSelectedTiles();

        //    base.OnAbilityDeselect();

        //    SelectedTile = null;
        //}

        //public void ClearSelectedTiles()
        //{
        //    lock (AffectedTiles)
        //        AffectedTiles.ForEach(tile =>
        //        {
        //            tile.TilePoint.ParentTileMap.Controller.DeselectTiles();
        //        });
        //}
    }
}
