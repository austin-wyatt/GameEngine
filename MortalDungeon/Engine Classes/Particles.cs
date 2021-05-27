using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class ParticleGenerator
    {
        public List<Particle> Particles = new List<Particle>();
        public Vector3 Position = default;
        public int ParticleCount = 0;
        public bool Playing = false;

        protected int _currentParticle = 0; //the index of the current particle
        public Particle CurrentParticle 
        {
            get 
            {
                return Particles[_currentParticle];
            }
        }

        protected int _tickCount = 0;
        public ParticleGenerator() { }

        //logic for when/where to create a particle is calculated here
        public virtual void Tick()
        {
            if(Playing)
            {
                _tickCount++;
            }
        }

        //generates a particle
        public virtual void GenerateParticle()
        {
            _currentParticle++;
            if(_currentParticle == ParticleCount)
            {
                _currentParticle = 0;
            }
        }

        public virtual void DecayParticles()
        {
            Particles.ForEach(particle =>
            {
                particle.Tick();
            });
        }
    }
    public class Particle
    {
        public Vector3 Position = default;
        public Vector3 Velocity = default;
        public Vector4 Color = default;
        public int Life = 0; //duration of the particle in ticks

        public RenderableObject Display;

        public Particle() { }

        public void Tick() 
        {
            if(Life != 0)
            {
                Life--;
                Translate();
            }
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
            Display.SetTranslation(ConvertGlobalToLocalCoordinates(position));
        }
        public void Translate(Vector3 velocity)
        {
            Position += velocity;
            Display.Translate(ConvertGlobalToLocalCoordinates(velocity));
        }
        public void Translate()
        {
            Position += Velocity;
            Display.SetTranslation(ConvertGlobalToLocalCoordinates(Position));
        }

        private Vector3 ConvertGlobalToLocalCoordinates(Vector3 position) 
        {
            Vector3 newPosition = new Vector3(position);
            newPosition.X = (position.X / WindowConstants.ScreenUnits.X) * 2 - 1;
            newPosition.Y = ((position.Y / WindowConstants.ScreenUnits.Y) * 2 - 1) * -1;

            return newPosition;
        }
    }
}
