using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public class Skeleton : Unit
    {
        public Skeleton(Vector3 position, CombatScene scene, int tileMapPosition, string name = "Skeleton") : base(scene)
        {
            Name = name;
            TileMapPosition = tileMapPosition;
            Clickable = true;
            Selectable = true;

            BaseObject Skeleton = new BaseObject(SKELETON_ANIMATION.List, ObjectID, "", position, EnvironmentObjects.BASE_TILE.Bounds);
            Skeleton.BaseFrame.CameraPerspective = true;
            Skeleton.BaseFrame.RotateX(25);

            BaseObjects.Add(Skeleton);

            SetPosition(position);

            VisionRadius = 6;

            CurrentShields = 5;

            Buff shieldBlock = new Buff(this);
            shieldBlock.ShieldBlock.Additive = 3;
            shieldBlock.IndefiniteDuration = true;
            shieldBlock.Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.Shield, MortalDungeon.Objects.Spritesheets.IconSheet);

            Slow slowAbility = new Slow(this, 5, 0.1f, 3);
            Abilities.Add(slowAbility.AbilityID, slowAbility);


            //Abilities.Strike melee = new Abilities.Strike(this, 1, 45);
            //Abilities.Add(melee.AbilityID, melee);
        }

        public override void OnKill()
        {
            base.OnKill();

            BaseObjects[0].SetAnimation(AnimationType.Die);
        }
    }
}
