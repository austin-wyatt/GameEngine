using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Particles;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units.AI;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public class Skeleton : Unit
    {
        public Skeleton() { }
        public Skeleton(CombatScene scene) : base(scene)
        {
            Clickable = true;
            Selectable = true;

            _createStatusBar = true;
        }

        public Skeleton(CombatScene scene, BaseTile tileMapPosition, string name = "Skeleton") : base(scene)
        {
            Name = name;
            SetTileMapPosition(tileMapPosition);
            Clickable = true;
            Selectable = true;

            ProfileType = UnitProfileType.Skeleton;

            _createStatusBar = true;
        }

        public override void InitializeUnitInfo()
        {
            base.InitializeUnitInfo();

            VisionGenerator.Radius = 12;

            Info.Stealth.Skill = 0;

            RangedDamageDealer disp = new RangedDamageDealer(this)
            {
                Weight = 1,
                Bloodthirsty = 1
            };
            AI.Dispositions.Add(disp);

            MeleeDamageDealer meleeDisp = new MeleeDamageDealer(this) { Weight = 1 };
            AI.Dispositions.Add(meleeDisp);

            Utility utilityDisp = new Utility(this) { Weight = 1 };
            AI.Dispositions.Add(utilityDisp);

            Healer healDisp = new Healer(this) { Weight = 1 };
            AI.Dispositions.Add(healDisp);

            AbilityLoadout = AbilityLoadout.GenerateLoadoutFromTree(AbilityTreeType.Skeleton, 3);
        }

        public override void InitializeVisualComponent()
        {
            base.InitializeVisualComponent();

            BaseObject Skeleton = CreateBaseObject();
            Skeleton.BaseFrame.CameraPerspective = true;
            Skeleton.BaseFrame.RotateX(25);

            AddBaseObject(Skeleton);
        }

        public override void EntityLoad(FeaturePoint position)
        {
            base.EntityLoad(position);

            TileOffset = new Vector3(0, -Info.TileMapPosition.GetDimensions().Y / 2, 0.2f);

            SetPosition(Info.TileMapPosition.Position + TileOffset);

            SelectionTile.UnitOffset.Y += Info.TileMapPosition.GetDimensions().Y / 2;
            SelectionTile.SetPosition(Position);

            //Move movement = new Move(this);
            //Info.Abilities.Add(movement);

            //Info._movementAbility = movement;


            //Buff shieldBlock = new Buff(this);
            //shieldBlock.ShieldBlock.Additive = 3;
            //shieldBlock.IndefiniteDuration = true;
            //shieldBlock.Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.Shield, MortalDungeon.Objects.Spritesheets.IconSheet);

            //BonyBash bash = new BonyBash(this);
            //Info.Abilities.Add(bash);

            //Bleed bleedAbility = new Bleed(this, 2, 15, 5);
            //Info.Abilities.Add(bleedAbility);
            //bleedAbility.AbilityClass = AbilityClass.Skeleton;

            //AncientArmor armor = new AncientArmor(this);
            //Info.Abilities.Add(armor);

            //Shoot shootAbility = new Shoot(this, 15, 4, 3);
            //shootAbility.MaxCharges = 100;
            //shootAbility.Charges = 100;
            //Info.Abilities.Add(shootAbility);
            //shootAbility.AbilityClass = AbilityClass.Skeleton;

            //StrongBones strongBonesAbility = new StrongBones(this);
            //Info.Abilities.Add(strongBonesAbility);

            //MendBones mendBonesAbility = new MendBones(this);
            //Info.Abilities.Add(mendBonesAbility);

            //Hide hideAbility = new Hide(this);
            //Info.Abilities.Add(hideAbility);
        }

        public override void OnKill()
        {
            base.OnKill();

            BaseObjects[0].SetAnimation(AnimationType.Die);
        }

        public override void OnHurt()
        {
            Sound sound = new Sound(Sounds.UnitHurt) { Gain = 1f, Pitch = GlobalRandom.NextFloat(1.2f, 1.4f) };
            sound.Play();

            var bloodExplosion = new Explosion(Position, new Vector4(0.8f, 0.8f, 0.776f, 1), Explosion.ExplosionParams.Default);
            bloodExplosion.OnFinish = () =>
            {
                Scene._particleGenerators.Remove(bloodExplosion);
            };

            Scene._particleGenerators.Add(bloodExplosion);
        }

        public override BaseObject CreateBaseObject()
        {
            return new BaseObject(SKELETON_ANIMATION.List, ObjectID, "", new Vector3(), EnvironmentObjects.BASE_TILE.Bounds);
        }
    }
}
