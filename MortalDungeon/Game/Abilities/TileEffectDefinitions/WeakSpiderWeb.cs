using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities.TileEffectDefinitions
{
    public class WeakSpiderWeb : TileEffect
    {
        public WeakSpiderWeb()
        {

        }

        GameObject _spiderWebVisual = null;
        public override void AddedToTile(TilePoint point)
        {
            base.AddedToTile(point);

            var tile = TileMapHelpers.GetTile(point);
            
            if(tile != null)
            {
                _spiderWebVisual = new GameObject(SpritesheetManager.GetSpritesheet(50006), 23);

                _spiderWebVisual.SetPosition(tile.Position + new Vector3(0, 0, 0.003f));

                TileMapManager.Scene._genericObjects.Add(_spiderWebVisual);
            }
        }

        public override void RemovedFromTile(TilePoint point)
        {
            base.RemovedFromTile(point);

            if(_spiderWebVisual != null)
            {
                TileMapManager.Scene._genericObjects.Remove(_spiderWebVisual);
            }
        }

        private int _roundCounter = 0;
        public override void OnRoundEnd(TilePoint point)
        {
            base.OnRoundEnd(point);

            _roundCounter++;

            if(_roundCounter == 3)
            {
                TileEffectManager.RemoveTileEffect(this, point);
            }
        }

        public override void OnSteppedOn(Unit unit, BaseTile tile)
        {
            base.OnSteppedOn(unit, tile);

            if(!(unit.Info.UnitConditions.Contains(UnitConditions.WebImmuneWeak) ||
                unit.Info.UnitConditions.Contains(UnitConditions.WebImmuneMed) ||
                unit.Info.UnitConditions.Contains(UnitConditions.WebImmuneStrong)))
            {
                //add a slow debuff
            }
        }

        public override void OnTurnEnd(Unit unit, BaseTile tile)
        {
            base.OnTurnEnd(unit, tile);

            if (!(unit.Info.UnitConditions.Contains(UnitConditions.WebImmuneWeak) ||
                unit.Info.UnitConditions.Contains(UnitConditions.WebImmuneMed) ||
                unit.Info.UnitConditions.Contains(UnitConditions.WebImmuneStrong)))
            {
                //add a stacking debuff that traps the unit after several turns
            }
        }
    }
}
