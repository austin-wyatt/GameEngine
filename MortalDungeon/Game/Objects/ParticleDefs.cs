using MortalDungeon.Engine_Classes;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;

namespace MortalDungeon.Game.Objects
{
    public class ParticleGenTest : ParticleGenerator
    {
        public ParticleGenTest(Vector3 position) 
        {
            ParticleCount = 1000;
            Position = position;

            var rand = new Random();

            SpritesheetObject particleObj = new SpritesheetObject(0, Spritesheets.TestSheet, 3);
            ObjectDefinition particleObjDef = particleObj.CreateObjectDefinition();

            for(int i = 0; i < ParticleCount; i++) 
            {
                Particle fillParticle = new Particle();
                fillParticle.Position = Position;
                fillParticle.Velocity = new Vector3(((float)rand.NextDouble() * 2 - 1) * 10, ((float)rand.NextDouble() * 2 - 1) * 10, 0);
                fillParticle.Color = new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1);
                fillParticle.Display = new RenderableObject(particleObjDef, default, ObjectRenderType.Texture, Shaders.PARTICLE_SHADER);

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
            if(CurrentParticle.Life == 0) 
            {
                CurrentParticle.Life = 1000;
                CurrentParticle.SetPosition(Position);
            }

            base.GenerateParticle();
        }
    }
}
