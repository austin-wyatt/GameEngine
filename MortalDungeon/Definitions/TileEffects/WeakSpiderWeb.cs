using MortalDungeon.Definitions.Buffs;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Definitions.TileEffects
{
    public class WeakSpiderWeb : TileEffect
    {
        public WeakSpiderWeb()
        {
            Duration = 3;

            Identifier = "spider_web";
        }

        public WeakSpiderWeb(TileEffect effect) : base(effect) { }

        GameObject _spiderWebVisual = null;

        public override void CreateVisuals()
        {
            base.CreateVisuals();

            var tile = TileMapHelpers.GetTile(Location);

            if (tile != null)
            {
                _spiderWebVisual = new GameObject(SpritesheetManager.GetSpritesheet(50006), 23);

                _spiderWebVisual.SetPosition(tile.Position + new Vector3(0, 0, 0.003f));

                TileMapManager.Scene._genericObjects.Add(_spiderWebVisual);
            }
        }

        public override void RemoveVisuals()
        {
            base.RemoveVisuals();

            if (_spiderWebVisual != null)
            {
                TileMapManager.Scene._genericObjects.Remove(_spiderWebVisual);
            }
        }

        public override void OnRoundEnd(TilePoint point)
        {
            base.OnRoundEnd(point);

            Duration--;

            if(Duration == 0)
            {
                TileEffectManager.RemoveTileEffect(this, point);
            }
        }

        public override void OnSteppedOn(Unit unit, BaseTile tile)
        {
            base.OnSteppedOn(unit, tile);

            HandleSlowDebuff(unit);
        }

        public override void OnTurnStart(Unit unit, BaseTile tile)
        {
            base.OnTurnStart(unit, tile);

            HandleSlowDebuff(unit);
        }

        private void HandleSlowDebuff(Unit unit)
        {
            if (!(unit.Info.StatusManager.CheckCondition(UnitCondition.WebImmuneWeak) ||
                unit.Info.StatusManager.CheckCondition(UnitCondition.WebImmuneMed) ||
                unit.Info.StatusManager.CheckCondition(UnitCondition.WebImmuneStrong)))
            {
                var buff = unit.Info.BuffManager.Buffs.Find(b => b.Identifier == "spider_web");

                if (buff != null)
                {
                    buff.AddStack();
                }
                else
                {
                    buff = new WebSlowDebuff() { Identifier = "spider_web" };
                    unit.Info.AddBuff(buff);
                }
            }
        }
    }
}
