using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.ObjectDefinitions;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Objects
{
    namespace PropertyAnimations
    {
        public class BounceAnimation : PropertyAnimation
        {
            public BounceAnimation(RenderableObject baseFrame, int bounceFrameDelay = 1)
            {
                BaseFrame = baseFrame;
                BaseTranslation = baseFrame.Translation.ExtractTranslation();
                BaseColor = new Vector4(baseFrame.BaseColor);

                Repeat = true;
                Playing = false;



                for (int i = 0; i < 26; i++) 
                {
                    Keyframe temp = new Keyframe(i * bounceFrameDelay);

                    if (i < 13)
                    {
                        temp.Action = () => BaseFrame.TranslateY(0.0005f);
                    }
                    else  
                    {
                        temp.Action = () => BaseFrame.TranslateY(-0.0005f);
                    }

                    Keyframes.Add(temp);
                }
            }
        }

        public class LiftAnimation : PropertyAnimation
        {
            public LiftAnimation(RenderableObject baseFrame)
            {
                BaseFrame = baseFrame;
                BaseTranslation = baseFrame.Translation.ExtractTranslation();
                BaseColor = new Vector4(baseFrame.BaseColor);

                Repeat = false;
                Playing = false;

                Keyframe temp = new Keyframe(0);
                temp.Action = () => BaseFrame.TranslateY(0.02f);

                Keyframes.Add(temp);
            }
        }

        public class HurtAnimation : PropertyAnimation
        {
            public HurtAnimation(RenderableObject baseFrame, float damageTaken)
            {
                Repeat = false;
                Playing = true;

                damageTaken = Math.Clamp(damageTaken, 1, 100);

                damageTaken = damageTaken / 100;

                float rotateAmount = damageTaken * 10;

                for (int i = 0; i < 5; i++)
                {
                    int capturedIndex = i;

                    Keyframe frame = new Keyframe(i * 2, () =>
                    {
                        if(capturedIndex == 0)
                        {
                            baseFrame.RotateZ(-rotateAmount);
                        }
                        else if(capturedIndex % 2 == 0)
                        {
                            baseFrame.RotateZ(-rotateAmount * 2);
                        }
                        else
                        {
                            baseFrame.RotateZ(rotateAmount * 2);
                        }
                        
                    });

                    Keyframes.Add(frame);
                }

                OnFinish += () =>
                {
                    baseFrame.RotateZ(rotateAmount);
                    baseFrame.CalculateTransformationMatrix();
                };
            }
        }

        public class DayNightCycle : TimedAnimation 
        {
            private static _Color NightColor = new _Color(0.1f, 0.1f, 0.2f, 1f);
            private static _Color MorningColor = new _Color(0.5f, 0.5f, 0.43f, 1f);
            private static _Color MiddayColor = new _Color(0.52f, 0.52f, 0.52f, 1f);
            private static _Color EveningColor = new _Color(0.5f, 0.47f, 0.42f, 1f);


            private const int STATIC_PERIOD = 24 * 2;
            private const int TRANSITION_PERIOD = 64 * 2;

            private const int NightEnd = 0;
            public const int MorningStart = NightEnd + TRANSITION_PERIOD;
            private const int MorningEnd = MorningStart + STATIC_PERIOD;
            public const int MiddayStart = MorningEnd + TRANSITION_PERIOD;
            private const int MiddayEnd = MiddayStart + STATIC_PERIOD * 5;
            public const int EveningStart = MiddayEnd + TRANSITION_PERIOD;
            private const int EveningEnd = EveningStart + STATIC_PERIOD;
            public const int NightStart = EveningEnd + TRANSITION_PERIOD;

            public const int DAY_PERIOD = NightStart + STATIC_PERIOD * 3; //496

            public const int HOUR = STATIC_PERIOD;

            public CombatScene Scene;

            public DayNightCycle(int timeDelay, int startTime, CombatScene scene) 
            {
                Repeat = true;
                Playing = true;

                Scene = scene;

                startTime %= DAY_PERIOD;

                Scene.Time = startTime;
                CurrentKeyframe = startTime;

                StartTime = WindowConstants.GlobalTimer.ElapsedMilliseconds - startTime * timeDelay;

                _Color startColor = new _Color(NightColor);

                for (int i = 0; i < DAY_PERIOD; i++) 
                {
                    TimedKeyframe frame = new TimedKeyframe(i * timeDelay);
                    _Color colorDif = new _Color(0, 0, 0, 0);

                    if (i >= NightEnd && i < MorningStart) 
                    {
                        colorDif = (MorningColor - NightColor) / TRANSITION_PERIOD;
                    }
                    if (i >= MorningEnd && i < MiddayStart)
                    {
                        colorDif = (MiddayColor - MorningColor) / TRANSITION_PERIOD;
                    }
                    if (i >= MiddayEnd && i < EveningStart)
                    {
                        colorDif = (EveningColor - MiddayColor) / TRANSITION_PERIOD;
                    }
                    if (i >= EveningEnd && i <= NightStart)
                    {
                        colorDif = (NightColor - EveningColor) / TRANSITION_PERIOD;
                    }

                    if (i < startTime) 
                    {
                        startColor.Add(colorDif);
                    }

                    int time = i;
                    frame.Action = () =>
                    {
                        CombatScene.EnvironmentColor.Add(colorDif);
                        Scene.UpdateTime(time);

                        if(time == 0)
                        {
                            Scene.SetDay(CombatScene.Days + 1);
                        }
                    };

                    Keyframes.Add(frame);
                }

                CombatScene.EnvironmentColor.R = startColor.R;
                CombatScene.EnvironmentColor.G = startColor.G;
                CombatScene.EnvironmentColor.B = startColor.B;
                CombatScene.EnvironmentColor.A = startColor.A;
            }

            public void PrintTime() 
            {
                Console.Write($"{Scene.Time}");

                string identifer = "";
                if (Scene.Time == NightStart) 
                {
                    identifer = " Night start";
                }
                switch (Scene.Time) 
                {
                    case NightStart:
                        identifer = " Night start";
                        break;
                    case MorningStart:
                        identifer = " Morning start";
                        break;
                    case MiddayStart:
                        identifer = " Midday start";
                        break;
                    case EveningStart:
                        identifer = " Evening start";
                        break;
                    case NightEnd:
                        identifer = " Night end";
                        break;
                    case MorningEnd:
                        identifer = " Morning end";
                        break;
                    case MiddayEnd:
                        identifer = " Midday end";
                        break;
                    case EveningEnd:
                        identifer = " Evening end";
                        break;
                }

                Console.Write(identifer + "\n");
            }

            public bool IsNight() 
            {
                return Scene.Time < MorningStart || Scene.Time > EveningEnd;
            }
        }


        public class TrackingParticleAnimation : PropertyAnimation
        {
            public TrackingParticleAnimation(GameObject gameObject, Vector3 initialPosition, Vector3 destination, TrackingSimulation sim = null)
            {
                gameObject.SetPosition(initialPosition);

                Repeat = false;
                Playing = true;

                Random rng = new Random();

                TrackingSimulation simulation;

                if (sim == null)
                {
                    simulation = new TrackingSimulation(initialPosition, destination)
                    {
                        InitialDirection = (float)-rng.NextDouble() * (MathHelper.PiOver2 + 0.2f),
                        Acceleration = (float)rng.NextDouble() + 9f,
                        RotationPerTick = 0.05f,
                        MaximumVelocity = 35f
                    };
                }
                else
                {
                    simulation = sim;
                }
                
                simulation.Simulate();


                for (int i = 0; i < simulation.Points.Count; i++)
                {
                    int capturedIndex = i;

                    Keyframe frame = new Keyframe(capturedIndex, () =>
                    {
                        gameObject.SetPosition(simulation.Points[capturedIndex].X, simulation.Points[capturedIndex].Y, simulation.Points[capturedIndex].Z);
                    });

                    Keyframes.Add(frame);
                }
            }
        }
    }
}
