using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    internal class ParticleGenerator : ITickable
    {
        internal List<Particle> Particles = new List<Particle>();
        internal RenderableObject ParticleDisplay;
        internal Vector3 Position = default;
        internal Vector3 PositionalOffset = default;
        internal int ParticleCount = 0;
        internal bool Playing = false;
        protected bool Priming = false;

        internal bool RefreshParticles = true;

        internal bool Repeat = true;

        internal Action OnFinish = null;

        protected int _currentParticle = 0; //the index of the current particle

        protected int _tickCount = 0;
        internal ParticleGenerator() { }

        //logic for when/where to create a particle is calculated here
        internal virtual void Tick()
        {
            if(Playing || Priming)
            {
                _tickCount++;
            }
        }

        //generates a particle
        internal virtual void GenerateParticle()
        {
            _currentParticle++;
            if(_currentParticle >= ParticleCount && RefreshParticles)
            {
                _currentParticle = 0;
            }

            if (_currentParticle == 0 && !Repeat)
            {
                RefreshParticles = false;
            }
        }

        internal virtual void DecayParticles()
        {
            bool hasLivingParticle = false;

            Particles.ForEach(particle =>
            {
                if (particle.Life > 0) 
                {
                    UpdateParticle(particle);

                    hasLivingParticle = true;
                }

                particle.Tick();
            });

            if (!hasLivingParticle && !Repeat) 
            {
                Playing = false;
                OnFinish?.Invoke();
            }
        }

        internal virtual void UpdateParticle(Particle particle) 
        {

        }

        internal void SetPosition(Vector3 position) 
        {
            Position = position + PositionalOffset;
        }

        internal virtual void PrimeParticles()
        {
            Priming = true;
            for (int i = 0; i < ParticleCount; i++)
            {
                Tick();
            }
            Priming = false;
        }
    }
    internal class Particle
    {
        internal Vector3 Position = default;
        internal Vector3 Velocity = default;
        internal Vector4 Color = default;
        internal int Life = 0; //duration of the particle in ticks

        internal Matrix4 Translation = Matrix4.Identity;
        internal Matrix4 Rotation = Matrix4.Identity;
        internal Matrix4 Scale = Matrix4.Identity;

        internal Matrix4 Transformations = Matrix4.Identity;

        internal Vector3 RotationInfo = default;

        internal float SpritesheetPosition;
        internal Vector2 SideLengths = new Vector2();

        internal bool Cull = false;


        internal Particle() { }

        internal void Tick() 
        {
            if(Life != 0)
            {
                Life--;
                Translate();
            }
        }

        private static Vector3 _positionHelper = default;
        internal void SetPosition(Vector3 position)
        {
            Position = position;

            _positionHelper = WindowConstants.ConvertGlobalToLocalCoordinates(position);
            
            Translation.M41 = _positionHelper.X;
            Translation.M42 = _positionHelper.Y;
            Translation.M43 = _positionHelper.Z;

            CalculateTransformationMatrix();
        }
        internal void Translate(Vector3 velocity)
        {
            Position += velocity;

            SetPosition(Position);
        }
        internal void Translate()
        {
            Position += Velocity;

            SetPosition(Position);
        }

        internal void ScaleAll(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale.X *= f;
            currentScale.Y *= f;
            currentScale.Z *= f;

            Scale = Matrix4.CreateScale(currentScale);

            CalculateTransformationMatrix();
        }

        internal void ScaleAddition(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale.X += f;
            currentScale.Y += f;
            currentScale.Z += f;

            Scale = Matrix4.CreateScale(currentScale);

            CalculateTransformationMatrix();
        }

        internal void RotateX(float degrees)//extremely expensive, research at some point maybe
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(degrees));
            RotationInfo.X += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }
        internal void RotateY(float degrees)//extremely expensive, research at some point maybe
        {
            Matrix4 rotationMatrix = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(degrees));
            RotationInfo.Y += degrees;

            Rotation *= rotationMatrix;

            CalculateTransformationMatrix();
        }
        internal void RotateZ(float degrees) //extremely expensive, research at some point maybe
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
