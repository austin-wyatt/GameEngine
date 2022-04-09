using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class ParticleGenerator : ITickable
    {
        public List<Particle> Particles = new List<Particle>();
        public RenderableObject ParticleDisplay;
        public Vector3 Position = default;
        public Vector3 PositionalOffset = default;
        public int ParticleCount = 0;
        public bool Playing = false;
        protected bool Priming = false;

        public bool RefreshParticles = true;

        public bool Repeat = true;

        public Action OnFinish = null;

        protected int _currentParticle = 0; //the index of the current particle

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
            if(_currentParticle >= ParticleCount && RefreshParticles)
            {
                _currentParticle = 0;
            }

            if (_currentParticle == 0 && !Repeat)
            {
                RefreshParticles = false;
            }
        }

        public virtual void DecayParticles()
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

        public virtual void UpdateParticle(Particle particle) 
        {

        }

        public void SetPosition(Vector3 position) 
        {
            Position = position + PositionalOffset;
        }

        public virtual void PrimeParticles()
        {
            Priming = true;
            for (int i = 0; i < ParticleCount; i++)
            {
                Tick();
            }
            Priming = false;
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

        public bool Cull = false;


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
