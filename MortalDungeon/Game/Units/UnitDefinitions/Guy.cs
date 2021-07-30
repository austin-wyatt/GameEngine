using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public class Guy : Unit
    {
        public Guy() { }
        public Guy(Vector3 position, int tileMapPosition, int id = 0, string name = "Guy") : base()
        {
            Name = name;
            TileMapPosition = tileMapPosition;

            BaseObject Guy = new BaseObject(BAD_GUY_ANIMATION.List, id, "BadGuy", position, EnvironmentObjects.BASE_TILE.Bounds);
            Guy.BaseFrame.CameraPerspective = true;
            Guy.BaseFrame.RotateX(25);

            BaseObjects.Add(Guy);

            SetPosition(position);

            VisionRadius = 6;

            Abilities.Strike melee = new Abilities.Strike(this, 1, 45);
            Abilities.Add(melee.AbilityID, melee);
        }

        public override void OnKill()
        {
            base.OnKill();

            BaseObjects[0].SetAnimation(AnimationType.Die);
        }
    }
}
