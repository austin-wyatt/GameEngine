using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.GameObjects;
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
        public Guy(Vector3 position, int tileMapPosition, int id = 0, string name = "Guy")
        {
            Name = name;
            TileMapPosition = tileMapPosition;

            BaseObject Guy = new BaseObject(BAD_GUY_ANIMATION.List, id, "BadGuy", position, EnvironmentObjects.BASE_TILE.Bounds);
            Guy.BaseFrame.CameraPerspective = true;
            //Guy.BaseFrame.Color = WindowConstants.FullColor - new Vector4(1f, 0f, 0f, 0);
            //Guy.BaseFrame.ScaleAll(1.5f);
            //Guy.PositionalOffset += Vector3.UnitY * -Guy.Dimensions.Y * Guy.BaseFrame.Scale.M11; 
            Guy.BaseFrame.RotateX(25);

            BaseObjects.Add(Guy);

            SetPosition(position);


            Abilities.Move movement = new Abilities.Move(this, 6);
            Abilities.Add(movement);

            Abilities.BasicMelee melee = new Abilities.BasicMelee(this);
            Abilities.Add(melee);
        }
    }
}
