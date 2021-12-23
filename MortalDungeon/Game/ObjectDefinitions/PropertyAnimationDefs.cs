using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Objects
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

        public class DayNightCycle : TimedAnimation 
        {
            private static Color NightColor = new Color(0.1f, 0.1f, 0.2f, 1f);
            private static Color MorningColor = new Color(0.5f, 0.5f, 0.43f, 1f);
            private static Color MiddayColor = new Color(0.52f, 0.52f, 0.52f, 1f);
            private static Color EveningColor = new Color(0.5f, 0.47f, 0.42f, 1f);


            private const int STATIC_PERIOD = 24;
            private const int TRANSITION_PERIOD = 64;

            private const int NightEnd = 0;
            public const int MorningStart = NightEnd + TRANSITION_PERIOD;
            private const int MorningEnd = MorningStart + STATIC_PERIOD;
            public const int MiddayStart = MorningEnd + TRANSITION_PERIOD;
            private const int MiddayEnd = MiddayStart + STATIC_PERIOD * 5;
            public const int EveningStart = MiddayEnd + TRANSITION_PERIOD;
            private const int EveningEnd = EveningStart + STATIC_PERIOD;
            public const int NightStart = EveningEnd + TRANSITION_PERIOD;

            private const int DAY_PERIOD = NightStart + STATIC_PERIOD * 3; //496

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

                Color startColor = new Color(NightColor);

                for (int i = 0; i < DAY_PERIOD; i++) 
                {
                    TimedKeyframe frame = new TimedKeyframe(i * timeDelay);
                    Color colorDif = new Color(0, 0, 0, 0);

                    if (i >= NightEnd && i < MorningStart) 
                    {
                        colorDif = (MorningColor - NightColor) / 64;
                    }
                    if (i >= MorningEnd && i < MiddayStart)
                    {
                        colorDif = (MiddayColor - MorningColor) / 64;
                    }
                    if (i >= MiddayEnd && i < EveningStart)
                    {
                        colorDif = (EveningColor - MiddayColor) / 64;
                    }
                    if (i >= EveningEnd && i <= NightStart)
                    {
                        colorDif = (NightColor - EveningColor) / 64;
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
    }
}
