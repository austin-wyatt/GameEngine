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
    public class Guy : Unit
    {
        public Guy(Vector3 position, CombatScene scene, int tileMapPosition, string name = "Guy") : base(scene)
        {
            Name = name;
            TileMapPosition = tileMapPosition;
            Clickable = true;
            Selectable = true;

            BaseObject Guy = new BaseObject(BAD_GUY_ANIMATION.List, ObjectID, "BadGuy", position, EnvironmentObjects.BASE_TILE.Bounds);
            Guy.BaseFrame.CameraPerspective = true;
            Guy.BaseFrame.RotateX(25);

            BaseObjects.Add(Guy);

            SetPosition(position);

            VisionRadius = 6;

            Buff shieldBlock = new Buff(this);
            shieldBlock.ShieldBlock.Additive = 10;
            shieldBlock.IndefiniteDuration = true;
            shieldBlock.Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.Shield, MortalDungeon.Objects.Spritesheets.IconSheet);

            shieldBlock.DamageResistances[DamageType.Slashing] = -1;


            Strike melee = new Strike(this, 1, 45);
            Abilities.Add(melee.AbilityID, melee);

            MaxEnergy = 15;
        }

        public override void OnKill()
        {
            base.OnKill();

            BaseObjects[0].SetAnimation(AnimationType.Die);
        }
    }
}
