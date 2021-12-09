using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Lighting;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Objects.PropertyAnimations;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public class Guy : Unit
    {
        public Guy(CombatScene scene) : base(scene) 
        {
            Clickable = true;
            Selectable = true;

            _createStatusBar = true;
        }
        public Guy(CombatScene scene, BaseTile tileMapPosition, string name = "Guy") : base(scene)
        {
            Name = name;
            SetTileMapPosition(tileMapPosition);
            Clickable = true;
            Selectable = true;

            _createStatusBar = true;

            //AI.MeleeDamageDealer disp = new AI.MeleeDamageDealer(this)
            //{
            //    Weight = 1,
            //    Bloodthirsty = 1
            //};

            //AI.Dispositions.Add(disp);


            //LightGenerator = new LightGenerator() { Radius = 10, Brightness = 0.1f, LightColor = new Vector3(0.75f, 0.6f, 0.2f) };
            //Scene.LightGenerators.Add(LightGenerator);

            //_lightGenChangeFunc = (_, __) =>
            //{
            //    if (DayNightCycle.IsNight())
            //    {
            //        LightGenerator.On = true;
            //    }
            //    else
            //    {
            //        LightGenerator.On = false;
            //    }
            //};

            //CombatScene.EnvironmentColor.OnChangeEvent += _lightGenChangeFunc;
        }

        //private EventHandler _lightGenChangeFunc;
        //private LightGenerator LightGenerator;

        public override void InitializeUnitInfo()
        {
            base.InitializeUnitInfo();

            VisionGenerator.Radius = 12;

            Info.MaxEnergy = 15;
        }

        public override void InitializeVisualComponent()
        {
            base.InitializeVisualComponent();

            BaseObjects.Clear();

            BaseObject Guy = CreateBaseObject();
            Guy.BaseFrame.CameraPerspective = true;
            Guy.BaseFrame.RotateX(25);

            AddBaseObject(Guy);
        }

        public override void EntityLoad(FeaturePoint position)
        {
            base.EntityLoad(position);

            SetPosition(Info.TileMapPosition.Position + new Vector3(0, -Info.TileMapPosition.GetDimensions().Y / 2, 0.2f));


            SelectionTile.UnitOffset.Y += Info.TileMapPosition.GetDimensions().Y / 2;
            SelectionTile.SetPosition(Position);


            Move movement = new Move(this);
            movement.EnergyCost = 0.3f;
            Info.Abilities.Add(movement);

            Info._movementAbility = movement;


            Buff shieldBlock = new Buff(this);
            shieldBlock.ShieldBlock.Additive = 10;
            shieldBlock.IndefiniteDuration = true;
            shieldBlock.Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.Shield, MortalDungeon.Objects.Spritesheets.IconSheet);

            shieldBlock.DamageResistances[DamageType.Slashing] = 0;


            Strike melee = new Strike(this, 1, 45)
            {
                EnergyCost = 7
            };
            Info.Abilities.Add(melee);

            Shoot shootAbility = new Shoot(this, 15, 4, 5) { EnergyCost = 4 };
            Info.Abilities.Add(shootAbility);

            shootAbility.AddCombo(new Shoot(this, 15, 4, 10) { EnergyCost = 5 }, null);
            shootAbility.Next.AddCombo(new Shoot(this, 15, 4, 15) { EnergyCost = 6 }, shootAbility);


            SpawnSkeleton spawnSkeleton = new SpawnSkeleton(this);
            Info.Abilities.Add(spawnSkeleton);
            spawnSkeleton.ReturnToFirst();
        }

        public override BaseObject CreateBaseObject()
        {
            BaseObject obj = new BaseObject(BAD_GUY_ANIMATION.List, ObjectID, "BadGuy", new Vector3(), EnvironmentObjects.BASE_TILE.Bounds);

            if (BaseObject != null) 
            {
                obj.BaseFrame.SetBaseColor(BaseObject.BaseFrame.BaseColor);
            }

            return obj;
        }

        public override void SetTileMapPosition(BaseTile baseTile)
        {
            base.SetTileMapPosition(baseTile);

            //if (LightGenerator != null) 
            //{
            //    LightGenerator.Position = Map.FeatureEquation.PointToMapCoords(Info.Point);
            //    Scene.QueueLightUpdate();
            //}
        }

        public override void CleanUp()
        {
            base.CleanUp();

            //CombatScene.EnvironmentColor.OnChangeEvent -= _lightGenChangeFunc;
            //Scene.LightGenerators.RemoveImmediate(LightGenerator);
        }
    }
}
