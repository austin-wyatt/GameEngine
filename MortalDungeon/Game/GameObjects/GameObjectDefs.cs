using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.GameObjects
{
    public class Fire : GameObject 
    {
        public Fire(Vector2i clientSize, Vector3 position) 
        {
            ClientSize = clientSize;
            Name = "Fire";

            BaseObject FireBaseObject = new BaseObject(ClientSize, FIRE_BASE_ANIMATION.List, 0, "", new Vector3(0, 0, 0.2f), EnvironmentObjects.FIRE_BASE.Bounds);
            FireBaseObject.BaseFrame.CameraPerspective = true;
            FireBaseObject.BaseFrame.RotateX(25);
            BaseObjects.Add(FireBaseObject);

            FireGen fireGenerator = new FireGen(new Vector3(0, 0, 0.12f), 0.01f);
            fireGenerator.PositionalOffset = new Vector3(0, 100, -0.01f);
            fireGenerator.Playing = true;
            ParticleGenerators.Add(fireGenerator);

            SetPosition(position);

            fireGenerator.PrimeParticles();
        }
    }

    public class BaseTile : GameObject
    {
        public BaseTile() { }
        public BaseTile(Vector2i clientSize, Vector3 position, int id)
        {
            ClientSize = clientSize;
            Name = "BaseTile";

            BaseObject BaseTile = new BaseObject(ClientSize, BASE_TILE_ANIMATION.List, id, "Base Tile " + id, default, EnvironmentObjects.BASE_TILE.Bounds);
            BaseTile.BaseFrame.CameraPerspective = true;
            BaseTile.BaseFrame.Color = WindowConstants.FullColor - new Vector4(0.5f, 0.5f, 0.5f, 0);

            BaseObjects.Add(BaseTile);

            SetPosition(position);
        }
    }

    public class Guy : GameObject
    {
        public Guy() { }
        public Guy(Vector2i clientSize, Vector3 position, int id = 0)
        {
            ClientSize = clientSize;
            Name = "Guy";

            BaseObject Guy = new BaseObject(ClientSize, BAD_GUY_ANIMATION.List, id, "BadGuy", position, EnvironmentObjects.BASE_TILE.Bounds);
            Guy.BaseFrame.CameraPerspective = true;
            Guy.BaseFrame.Color = WindowConstants.FullColor - new Vector4(1f, 0f, 0f, 0);
            //Guy.BaseFrame.ScaleAll(1.5f);
            //Guy.PositionalOffset += Vector3.UnitY * -Guy.Dimensions.Y * Guy.BaseFrame.Scale.M11; 
            Guy.BaseFrame.RotateX(25);

            BaseObjects.Add(Guy);

            SetPosition(position);
        }
    }
}
