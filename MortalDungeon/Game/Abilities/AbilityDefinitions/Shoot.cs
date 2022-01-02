using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using MortalDungeon.Game.Map;
using System.Diagnostics;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes;

namespace MortalDungeon.Game.Abilities
{
    public class Shoot : Ability
    {
        public Shoot(Unit castingUnit, int range = 6, int minRange = 2, float damage = 10)
        {
            Type = AbilityTypes.RangedAttack;
            DamageType = DamageType.Piercing;
            Range = range;
            MinRange = minRange;
            CastingUnit = castingUnit;
            Damage = damage;

            ActionCost = 2;

            CastingMethod |= CastingMethod.Weapon | CastingMethod.PhysicalDexterity;

            Name = "Shoot";

            _description = "Fire an arrow at a target within range. \nA direct line to the target must be present.";

            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.BowAndArrow, Spritesheets.IconSheet, true);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null, List<Unit> validUnits = null)
        {
            base.GetValidTileTargets(tileMap);

            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;

            //List <BaseTile> validTiles = tileMap.GetTargetsInRadius(point, (int)Range, new List<TileClassification>(), units);
            List<Unit> unitsInCastRadius = VisionMap.GetUnitsInRadius(CastingUnit, units, (int)Range, Scene);
            
            TrimUnits(unitsInCastRadius, false, MinRange);

            TargetAffectedUnits();

            return new List<BaseTile>();
        }

        public override bool UnitInRange(Unit unit, BaseTile position = null)
        {
            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;
            
            bool inRange = false;

            var tempVision = new VisionMap.TemporaryVisionParams()
            {
                Unit = CastingUnit,
                TemporaryPosition = point
            };

            List<Vector2i> teamVision = VisionMap.GetTeamVision(CastingUnit.AI.Team, Scene, new List<VisionMap.TemporaryVisionParams>() { tempVision });

            Vector2i clusterPos = Scene._tileMapController.PointToClusterPosition(unit.Info.TileMapPosition);

            if (TileMap.GetDistanceBetweenPoints(point, unit.Info.TileMapPosition) <= MinRange) 
            {
                return false;
            }

            if ((teamVision.Exists(p => p == clusterPos) || VisionMap.InVision(clusterPos.X, clusterPos.Y, CastingUnit.AI.Team))
                && VisionMap.TargetInVision(point, unit.Info.TileMapPosition, (int)Range, Scene)) 
            {
                return true;
            }

            return inRange;
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (unit.AI.Team != CastingUnit.AI.Team && AffectedUnits.FindIndex(u => u.ObjectID == unit.ObjectID) != -1)
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

        public override void OnAICast()
        {
            base.OnAICast();
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            Sound sound = new Sound(Sounds.Shoot) { Gain = 0.75f, Pitch = GlobalRandom.NextFloat(0.95f, 1.05f) };
            sound.Play();

            Casted();

            GameObject arrow = new GameObject(Spritesheets.ObjectSheet, 0);

            //arrow.SetScale(1 / WindowConstants.AspectRatio, 1, 1);



            Vector3 a = CastingUnit.Position;
            Vector3 b = SelectedUnit.Position;

            float angle = (float)MathHelper.RadiansToDegrees(Math.Atan2(a.Y - b.Y, a.X - b.X) - Math.PI / 2);

            angle *= -1;

            arrow.BaseObject.BaseFrame.RotateZ(angle);

            GameObject.LoadTexture(arrow);

            arrow.SetPosition(new Vector3(0, 0, -1000));

            Scene._genericObjects.Add(arrow);


            float dist = Vector3.Distance(SelectedUnit.Position, CastingUnit.Position) / 200;

            //int samples = 20;
            int samples = (int) dist;

            Vector3 delta = (SelectedUnit.Position - CastingUnit.Position) / samples;



            TimedAnimation shootAnimation = new TimedAnimation();

            shootAnimation.BaseFrame = arrow.BaseObject.BaseFrame;

            for (int i = 0; i < samples; i++) 
            {
                int temp = i;

                TimedKeyframe frame = new TimedKeyframe(temp)
                {
                    Action = () =>
                    {
                        arrow.SetPosition(CastingUnit.Position + delta * temp + new Vector3(0, 0, 0.2f));
                    }
                };

                shootAnimation.Keyframes.Add(frame);
            }

            shootAnimation.OnFinish = () =>
            {
                Scene.PostTickEvent += removeArrow;
            };

            void removeArrow(SceneEventArgs args) 
            {
                Scene.TimedTickableObjects.Remove(shootAnimation);
                Scene._genericObjects.Remove(arrow);

                DamageInstance damage = GetDamageInstance();

                SelectedUnit.ApplyDamage(new Unit.DamageParams(damage) { Ability = this });

                EffectEnded();
                Scene.PostTickEvent -= removeArrow;
            }


            shootAnimation.Play();
            Scene.TimedTickableObjects.Add(shootAnimation);
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            instance.Damage.Add(DamageType, Damage);

            ApplyBuffDamageInstanceModifications(instance);
            return instance;
        }
    }
}
