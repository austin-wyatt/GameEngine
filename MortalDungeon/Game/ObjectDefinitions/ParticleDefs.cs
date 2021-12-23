using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;

namespace MortalDungeon.Game.Particles
{
    public class ParticleGenTest : ParticleGenerator
    {
        public ParticleGenTest(Vector3 position) 
        {
            ParticleCount = 1000;
            Position = position;

            var rand = new ConsistentRandom();

            SpritesheetObject particleObj = new SpritesheetObject(0, Spritesheets.TestSheet, 3);
            ObjectDefinition particleObjDef = particleObj.CreateObjectDefinition();
            ParticleDisplay = new RenderableObject(particleObjDef, default, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);
            ParticleDisplay.CameraPerspective = true;

            for (int i = 0; i < ParticleCount; i++) 
            {
                Particle fillParticle = new Particle();
                fillParticle.Position = Position;
                fillParticle.Velocity = new Vector3(((float)rand.NextDouble() * 2 - 1) * 10, ((float)rand.NextDouble() * 2 - 1) * 7, 0);
                fillParticle.Color = new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 0.5f);

                fillParticle.SpritesheetPosition = ParticleDisplay.SpritesheetPosition;
                fillParticle.SideLengths = ParticleDisplay.SideLengths;

                

                Particles.Add(fillParticle);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (Playing) 
            {
                if (_tickCount % 1 == 0) //define tick frequency wherever you want
                {
                    DecayParticles();
                    for (int i = 0; i < 10; i++)
                        GenerateParticle();
                }
            }
        }

        public override void GenerateParticle()
        {
            if(Particles[_currentParticle].Life == 0) 
            {
                Particles[_currentParticle].Life = 1000;
                Particles[_currentParticle].SetPosition(Position);
            }

            base.GenerateParticle();
        }
    }

    public class FireGen : ParticleGenerator
    {
        private Random rand = new ConsistentRandom();
        public int DefaultLife = 80;

        public float fireSpeedX = 5;
        public float fireSpeedY = 10;

        public float offsetX = 0.5f;
        public float offsetY = 0f;

        public FireGen(Vector3 position, float rotation)
        {
            ParticleCount = 2000;
            Position = position;

            SpritesheetObject particleObj = new SpritesheetObject((int)Tiles.TileType.Default, Spritesheets.TileSheet);
            ObjectDefinition particleObjDef = particleObj.CreateObjectDefinition();
            ParticleDisplay = new RenderableObject(particleObjDef, default, ObjectRenderType.Texture, Shaders.PARTICLE_SHADER);
            ParticleDisplay.CameraPerspective = true;

            Renderer.LoadTextureFromRenderableObject(ParticleDisplay);

            for (int i = 0; i < ParticleCount; i++)
            {
                Particle fillParticle = new Particle();
                fillParticle.Position = Position;
                fillParticle.Velocity = new Vector3(((float)rand.NextDouble() - offsetX) * fireSpeedX, (float)(rand.NextDouble() - offsetY) * -1 * fireSpeedY, ((float)rand.NextDouble()) / 10000);
                //fillParticle.Velocity = new Vector3(0.1f, 0, 0);
                fillParticle.Color = genFlameColor(DefaultLife);
                fillParticle.ScaleAll(0.05f);
                //fillParticle.ScaleAll(1f);
                fillParticle.SpritesheetPosition = ParticleDisplay.SpritesheetPosition;
                fillParticle.SideLengths = ParticleDisplay.SideLengths;

                fillParticle.Velocity.Z = Math.Abs(fillParticle.Velocity.Y) / fireSpeedY * rotation;

                Particles.Add(fillParticle);
            }

            PrimeParticles();
            _fractionOfLife = (float)DefaultLife / 10; //genFlameColor Helper
            _invDefaultLife = 1 / (float)DefaultLife;
        }


        public override void PrimeParticles() 
        {
            Priming = true;
            for(int i = 0; i < DefaultLife; i++)
            {
                Tick();
            }
            Priming = false;
        }

        private readonly Vector4 _baseFlameColor = new Vector4(1, 1, 0.07f, 1f);
        private float _fractionOfLife;
        private float _invDefaultLife;
        private Vector4 genFlameColor(int life) 
        {
            Vector4 color = _baseFlameColor;

            if (life > (_fractionOfLife))
            {
                color.Y *= life * _invDefaultLife;
                color.W *= life * (_invDefaultLife + 0.5f);
            }
            else
            {
                color.Y *= life * _invDefaultLife;
                color.X *= life * _invDefaultLife;
            }

            return color;
        }

        public override void Tick()
        {
            base.Tick();
            if (Playing || Priming)
            {
                if (_tickCount % 1 == 0) //define tick frequency wherever you want (should be = Count / (Life * particles generated per tick))
                {
                    DecayParticles();
                    for (int i = 0; i < 6; i++)
                        GenerateParticle();
                }
            }
        }

        public override void GenerateParticle()
        {
            if (Particles[_currentParticle].Life == 0)
            {
                Particles[_currentParticle].Life = DefaultLife;
                Particles[_currentParticle].SetPosition(Position);
            }

            base.GenerateParticle();
        }


        public override void DecayParticles()
        {
            float xSpeed = 1 / fireSpeedX;
            float ySpeed = 1 / fireSpeedY;

            foreach (var particle in Particles) 
            {
                particle.Tick();
                particle.Color = genFlameColor(particle.Life);

                if ((Math.Abs(particle.Velocity.X) * xSpeed) > 0.35f && (Math.Abs(particle.Velocity.Y) * ySpeed - offsetY) > 0.65f) //"stronger" fire particles die faster
                {
                    particle.Color = new Vector4(0, 0, 100, 1);
                    particle.Life--;
                }
            }
        }
    }

    public class Explosion : ParticleGenerator
    {
        public struct ExplosionParams 
        {
            public Vector3 Acceleration;
            public Vector3 MultiplicativeAcceleration;
            public Vector4 ColorDelta;
            public Vector3 BaseVelocity;
            public int ParticleCount;
            public int Life;

            public float ParticleSize;

            public ExplosionParams(ExplosionParams @params) 
            {
                Acceleration = @params.Acceleration;
                ColorDelta = @params.ColorDelta;
                BaseVelocity = @params.BaseVelocity;
                ParticleCount = @params.ParticleCount;
                Life = @params.Life;
                MultiplicativeAcceleration = @params.MultiplicativeAcceleration;
                ParticleSize = @params.ParticleSize;
            }

            public static ExplosionParams Default = new ExplosionParams()
            {
                Acceleration = new Vector3(0, 1, -0.005f),
                MultiplicativeAcceleration = new Vector3(1, 1, 1),
                BaseVelocity = new Vector3(15, 15, 0.03f),
                ParticleCount = 30,
                Life = 15,
                ParticleSize = 0.05f
            };
        }

        private Random rand = new ConsistentRandom();
        public int DefaultLife = 15;

        ExplosionParams Params;

        public Explosion(Vector3 position, Vector4 color, ExplosionParams explosionParams)
        {
            ParticleCount = explosionParams.ParticleCount;
            DefaultLife = explosionParams.Life;
            Position = position;

            Params = explosionParams;

            SpritesheetObject particleObj = new SpritesheetObject((int)Tiles.TileType.Default, Spritesheets.TileSheet);
            ObjectDefinition particleObjDef = particleObj.CreateObjectDefinition();
            ParticleDisplay = new RenderableObject(particleObjDef, default, ObjectRenderType.Texture, Shaders.PARTICLE_SHADER);
            ParticleDisplay.CameraPerspective = true;

            Repeat = false;
            Playing = true;

            Renderer.LoadTextureFromRenderableObject(ParticleDisplay);

            for (int i = 0; i < ParticleCount; i++)
            {
                Particle fillParticle = new Particle();
                fillParticle.Position = Position;

                fillParticle.Velocity = new Vector3(GlobalRandom.NextFloat(-1, 1) * Params.BaseVelocity.X, 
                    GlobalRandom.NextFloat(-1f, 1) * Params.BaseVelocity.Y, (float)Math.Abs(rand.NextDouble() * Params.BaseVelocity.Z));
                fillParticle.Color = color;
                fillParticle.ScaleAll(Params.ParticleSize);
                fillParticle.SpritesheetPosition = ParticleDisplay.SpritesheetPosition;
                fillParticle.SideLengths = ParticleDisplay.SideLengths;

                //fillParticle.Velocity.Z = Math.Abs(fillParticle.Velocity.Y) / fireSpeedY * rotation;

                Particles.Add(fillParticle);
            }
        }


        public override void Tick()
        {
            base.Tick();
            if (Playing || Priming)
            {
                //if (_tickCount % 1 == 0)
                //{
                    for (int i = 0; i < 6; i++)
                        GenerateParticle();

                    DecayParticles();
                //}
            }
        }

        public override void UpdateParticle(Particle particle)
        {
            particle.Velocity += Params.Acceleration;
            particle.Color += Params.ColorDelta;

            particle.Velocity *= Params.MultiplicativeAcceleration;
        }

        public override void GenerateParticle()
        {
            if (RefreshParticles && Particles[_currentParticle].Life == 0)
            {
                Particles[_currentParticle].Life = Params.Life;
                Particles[_currentParticle].SetPosition(Position);
            }

            base.GenerateParticle();
        }
    }
}
