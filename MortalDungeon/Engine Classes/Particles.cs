using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class ParticleGenerator
    {
        public List<Particle> Particles = new List<Particle>();
        public RenderableObject ParticleDisplay;
        public Vector3 Position = default;
        public Vector3 PositionalOffset = default;
        public int ParticleCount = 0;
        public bool Playing = false;
        protected bool Priming = false;


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
            if(Playing || Priming)
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

        public void SetPosition(Vector3 position) 
        {
            Position = position + PositionalOffset;
        }
    }
    public class Particle
    {
        public Vector3 Position = default;
        public Vector3 Velocity = default;
        public Vector4 Color = default;
        public int Life = 0; //duration of the particle in ticks

        public Matrix4 Translation = Matrix4.Identity;
        public Matrix4 Rotation = Matrix4.Identity;
        public Matrix4 Scale = Matrix4.Identity;

        public Matrix4 Transformations = Matrix4.Identity;

        public Vector3 RotationInfo = default;

        public float SpritesheetPosition;
        public Vector2 SideLengths = new Vector2();

        public Particle() { }

        public void Tick() 
        {
            if(Life != 0)
            {
                Life--;
                Translate();
            }
        }

        private static Vector3 _positionHelper = default;
        public void SetPosition(Vector3 position)
        {
            Position = position;

            _positionHelper = WindowConstants.ConvertGlobalToLocalCoordinates(position);
            
            Translation.M41 = _positionHelper.X;
            Translation.M42 = _positionHelper.Y;
            Translation.M43 = _positionHelper.Z;

            CalculateTransformationMatrix();
        }
        public void Translate(Vector3 velocity)
        {
            Position += velocity;

            SetPosition(Position);
        }
        public void Translate()
        {
            Position += Velocity;
            //Display.SetTranslation(WindowConstants.ConvertGlobalToLocalCoordinates(Position));
            SetPosition(Position);
        }

        public void ScaleAll(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale.X *= f;
            currentScale.Y *= f;
            currentScale.Z *= f;

            Scale = Matrix4.CreateScale(currentScale);

            CalculateTransformationMatrix();
        }

        public void ScaleAddition(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale.X += f;
            currentScale.Y += f;
            currentScale.Z += f;

            Scale = Matrix4.CreateScale(currentScale);

            CalculateTransformationMatrix();
        }

        public void RotateX(float degrees)//extremely expensive, research at some point maybe
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(degrees));
            RotationInfo.X += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }
        public void RotateY(float degrees)//extremely expensive, research at some point maybe
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(degrees));
            RotationInfo.Y += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }
        public void RotateZ(float degrees) //extremely expensive, research at some point maybe
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(degrees));
            RotationInfo.Z += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }

        private void CalculateTransformationMatrix()
        {
            Transformations = Rotation * Scale * Translation;
        }
    }
}
