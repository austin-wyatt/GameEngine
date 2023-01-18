using Empyrean.Definitions.Buffs;
using Empyrean.Engine_Classes;
using Empyrean.Game.Abilities;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Definitions.TileEffects
{
    public class WeakSpiderWeb : TileEffect
    {
        public WeakSpiderWeb()
        {
            Duration = 3;

            Identifier = "spider_web";
            Danger = 0.1f;
            Immunities = new List<UnitCondition>
            {
                UnitCondition.WebImmuneStrong, 
                UnitCondition.WebImmuneWeak, 
                UnitCondition.WebImmuneMed 
            };
        }

        public WeakSpiderWeb(TileEffect effect) : base(effect) { }

        IndividualMesh _spiderWebVisual = null;

        public override void CreateVisuals()
        {
            base.CreateVisuals();

            var tile = TileMapHelpers.GetTile(Location);

            if (tile != null)
            {
                _spiderWebVisual = new IndividualMesh();
                _spiderWebVisual.FillFromMeshTile(tile.MeshTileHandle);

                _spiderWebVisual.Texture = new SimpleTexture(SpritesheetManager.GetSpritesheet(50009));
                _spiderWebVisual.LoadTexture();

                Vector3 pos = WindowConstants.ConvertScreenSpaceToLocalCoordinates(tile._position);
                pos.Z += 0.003f;
                _spiderWebVisual.SetTranslation(pos);

                TileMapManager.Scene.IndividualMeshes.Add(_spiderWebVisual);
            }
        }

        public override void RemoveVisuals()
        {
            base.RemoveVisuals();

            if (_spiderWebVisual != null)
            {
                TileMapManager.Scene.IndividualMeshes.Remove(_spiderWebVisual);
            }
        }

        public override void OnRoundEnd(TilePoint point)
        {
            base.OnRoundEnd(point);

            Duration--;

            if(Duration == 0)
            {
                TileEffectManager.RemoveTileEffectOnRoundEnd(this, point);
            }
        }

        public override void OnSteppedOn(Unit unit, Tile tile)
        {
            base.OnSteppedOn(unit, tile);

            HandleSlowDebuff(unit);
        }

        public override void OnTurnStart(Unit unit, Tile tile)
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
                    buff = new WebSlowDebuff() 
                    { 
                        Identifier = "spider_web",
                        OwnerId = OwnerId 
                    };

                    unit.Info.AddBuff(buff);
                }
            }
        }
    }
}
