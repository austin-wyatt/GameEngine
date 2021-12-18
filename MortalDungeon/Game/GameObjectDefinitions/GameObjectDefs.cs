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
    internal class Fire : GameObject
    {
        internal Fire(Vector3 position)
        {
            Name = "Fire";

            BaseObject FireBaseObject = new BaseObject(FIRE_BASE_ANIMATION.List, 0, "", new Vector3(0, 0, 0.2f), EnvironmentObjects.FIRE_BASE.Bounds);
            FireBaseObject.BaseFrame.CameraPerspective = true;
            FireBaseObject.BaseFrame.RotateX(25);
            AddBaseObject(FireBaseObject);

            var fireGenerator = new Particles.FireGen(new Vector3(0, 0, 0.12f), 0.01f);
            fireGenerator.PositionalOffset = new Vector3(0, 100, -0.01f);
            fireGenerator.Playing = true;
            ParticleGenerators.Add(fireGenerator);

            SetPosition(position);

            fireGenerator.PrimeParticles();
        }
    }

    internal class Mountain : GameObject
    {
        internal Mountain() { }
        internal Mountain(Vector3 position, int id = 0)
        {
            Name = "Moutain";

            BaseObject Mountain = new BaseObject(MOUNTAIN_ANIMATION.List, id, "Mountain", position, EnvironmentObjects.BASE_TILE.Bounds);
            Mountain.BaseFrame.CameraPerspective = true;
            Mountain.BaseFrame.ScaleAll(20);

            AddBaseObject(Mountain);

            SetPosition(position);
        }
    }

    internal class CaveBackground : GameObject
    {
        internal CaveBackground() { }
        internal CaveBackground(Vector2i clientSize, Vector3 position, int id = 0)
        {
            Name = "CaveBackground";

            BaseObject Cave = new BaseObject(CAVE_BACKGROUND_ANIMATION.List, id, "CaveBackground", position);
            Cave.BaseFrame.CameraPerspective = true;
            Cave.BaseFrame.ScaleAll(20);

            AddBaseObject(Cave);

            SetPosition(position);
        }
    }

    internal class MountainTwo : GameObject
    {
        internal MountainTwo() { }
        internal MountainTwo(Vector3 position, int id = 0)
        {
            Name = "MoutainTwo";

            BaseObject Mountain = new BaseObject(MOUNTAIN_TWO_ANIMATION.List, id, "MoutainTwo", position, EnvironmentObjects.BASE_TILE.Bounds);
            Mountain.BaseFrame.CameraPerspective = true;
            Mountain.BaseFrame.ScaleAll(20);

            AddBaseObject(Mountain);

            SetPosition(position);
        }
    }
    
}
