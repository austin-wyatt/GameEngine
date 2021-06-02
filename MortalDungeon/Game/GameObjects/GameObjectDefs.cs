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
        public bool Render = true;

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

    public class Mountain : GameObject
    {
        public Mountain() { }
        public Mountain(Vector2i clientSize, Vector3 position, int id = 0)
        {
            ClientSize = clientSize;
            Name = "Moutain";

            BaseObject Mountain = new BaseObject(ClientSize, MOUNTAIN_ANIMATION.List, id, "Mountain", position, EnvironmentObjects.BASE_TILE.Bounds);
            Mountain.BaseFrame.CameraPerspective = true;
            Mountain.BaseFrame.ScaleAll(20);

            BaseObjects.Add(Mountain);

            SetPosition(position);
        }
    }

    public class CaveBackground : GameObject
    {
        public CaveBackground() { }
        public CaveBackground(Vector2i clientSize, Vector3 position, int id = 0)
        {
            ClientSize = clientSize;
            Name = "CaveBackground";

            BaseObject Cave = new BaseObject(ClientSize, CAVE_BACKGROUND_ANIMATION.List, id, "CaveBackground", position);
            Cave.BaseFrame.CameraPerspective = true;
            Cave.BaseFrame.ScaleAll(20);

            BaseObjects.Add(Cave);

            SetPosition(position);
        }
    }

    public class MountainTwo : GameObject
    {
        public MountainTwo() { }
        public MountainTwo(Vector2i clientSize, Vector3 position, int id = 0)
        {
            ClientSize = clientSize;
            Name = "MoutainTwo";

            BaseObject Mountain = new BaseObject(ClientSize, MOUNTAIN_TWO_ANIMATION.List, id, "MoutainTwo", position, EnvironmentObjects.BASE_TILE.Bounds);
            Mountain.BaseFrame.CameraPerspective = true;
            Mountain.BaseFrame.ScaleAll(20);

            BaseObjects.Add(Mountain);

            SetPosition(position);
        }
    }
}
