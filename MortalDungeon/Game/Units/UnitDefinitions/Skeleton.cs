using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public class Skeleton : Unit
    {
        public Skeleton(Vector3 position, CombatScene scene, BaseTile tileMapPosition, string name = "Skeleton") : base(scene)
        {
            Name = name;
            SetTileMapPosition(tileMapPosition);
            Clickable = true;
            Selectable = true;

            BaseObject Skeleton = CreateBaseObject();
            Skeleton.SetPosition(position);
            Skeleton.BaseFrame.CameraPerspective = true;
            Skeleton.BaseFrame.RotateX(25);

            AddBaseObject(Skeleton);

            SetPosition(position);

            Info.CurrentShields = 5;

            Buff shieldBlock = new Buff(this);
            shieldBlock.ShieldBlock.Additive = 3;
            shieldBlock.IndefiniteDuration = true;
            shieldBlock.Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.Shield, MortalDungeon.Objects.Spritesheets.IconSheet);

            Slow slowAbility = new Slow(this, 5, 0.1f, 3);
            Info.Abilities.Add(slowAbility);

            Bleed bleedAbility = new Bleed(this, 2, 15, 5);
            Info.Abilities.Add(bleedAbility);

            Shoot shootAbility = new Shoot(this, 15, 4, 5) { EnergyCost = 4 };
            Info.Abilities.Add(shootAbility);

            Hide hideAbility = new Hide(this);
            Info.Abilities.Add(hideAbility);

            AI.RangedDamageDealer disp = new AI.RangedDamageDealer(this)
            {
                Weight = 1,
                Bloodthirsty = 1
            };

            AI.Dispositions.Add(disp);

            Info.Stealth.Skill = 0;

            //Abilities.Strike melee = new Abilities.Strike(this, 1, 45);
            //Abilities.Add(melee.AbilityID, melee);
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
        }

        public override BaseObject CreateBaseObject()
        {
            return new BaseObject(SKELETON_ANIMATION.List, ObjectID, "", new Vector3(), EnvironmentObjects.BASE_TILE.Bounds);
        }
    }
}
