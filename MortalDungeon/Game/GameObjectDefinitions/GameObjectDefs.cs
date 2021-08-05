using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.GameObjects
{
    public class Fire : GameObject
    {
        public Fire(Vector3 position)
        {
            Name = "Fire";

            BaseObject FireBaseObject = new BaseObject(FIRE_BASE_ANIMATION.List, 0, "", new Vector3(0, 0, 0.2f), EnvironmentObjects.FIRE_BASE.Bounds);
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

    public class Mountain : GameObject
    {
        public Mountain() { }
        public Mountain(Vector3 position, int id = 0)
        {
            Name = "Moutain";

            BaseObject Mountain = new BaseObject(MOUNTAIN_ANIMATION.List, id, "Mountain", position, EnvironmentObjects.BASE_TILE.Bounds);
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
            Name = "CaveBackground";

            BaseObject Cave = new BaseObject(CAVE_BACKGROUND_ANIMATION.List, id, "CaveBackground", position);
            Cave.BaseFrame.CameraPerspective = true;
            Cave.BaseFrame.ScaleAll(20);

            BaseObjects.Add(Cave);

            SetPosition(position);
        }
    }

    public class MountainTwo : GameObject
    {
        public MountainTwo() { }
        public MountainTwo(Vector3 position, int id = 0)
        {
            Name = "MoutainTwo";

            BaseObject Mountain = new BaseObject(MOUNTAIN_TWO_ANIMATION.List, id, "MoutainTwo", position, EnvironmentObjects.BASE_TILE.Bounds);
            Mountain.BaseFrame.CameraPerspective = true;
            Mountain.BaseFrame.ScaleAll(20);

            BaseObjects.Add(Mountain);

            SetPosition(position);
        }
    }
    
}
