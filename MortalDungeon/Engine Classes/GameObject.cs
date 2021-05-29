using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class GameObject
    {
        public string Name = "";
        public Vector3 Position = new Vector3();
        public List<BaseObject> BaseObjects = new List<BaseObject>();
        public List<ParticleGenerator> ParticleGenerators = new List<ParticleGenerator>();
        public Vector3 PositionalOffset = new Vector3();
        public Vector2i ClientSize = new Vector2i();

        //public Stats Stats; //contains game parameters for the object
        public GameObject() { }

        public void SetPosition(Vector3 position) 
        {
            BaseObjects.ForEach(obj =>
            {
                obj.SetPosition(position + PositionalOffset);
            });

            ParticleGenerators.ForEach(particleGen =>
            {
                particleGen.SetPosition(position + PositionalOffset);
            });

            Position = position;
        }
    }
}
