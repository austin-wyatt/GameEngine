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
                        temp.Action = (baseFrame) => baseFrame.TranslateY(0.0005f);
                    }
                    else  
                    {
                        temp.Action = (baseFrame) => baseFrame.TranslateY(-0.0005f);
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
                temp.Action = (baseFrame) => baseFrame.TranslateY(0.02f);

                Keyframes.Add(temp);
            }
        }

        public class DayNightCycle : PropertyAnimation 
        {
            private static Color NightColor = new Color(0.25f, 0.25f, 0.5f, 0.5f);
            private static Color MorningColor = new Color(0.65f, 0.65f, 0.25f, 0.2f);
            private static Color MiddayColor = new Color(1, 1, 1, 0f);
            private static Color EveningColor = new Color(0.82f, 0.53f, 0.3f, 0.3f);


            private const int STATIC_PERIOD = 24;
            private const int TRANSITION_PERIOD = 64;

            private const int NightEnd = 0;
            private const int MorningStart = NightEnd + TRANSITION_PERIOD;
            private const int MorningEnd = MorningStart + STATIC_PERIOD;
            private const int MiddayStart = MorningEnd + TRANSITION_PERIOD;
            private const int MiddayEnd = MiddayStart + STATIC_PERIOD * 5;
            private const int EveningStart = MiddayEnd + TRANSITION_PERIOD;
            private const int EveningEnd = EveningStart + STATIC_PERIOD;
            private const int NightStart = EveningEnd + TRANSITION_PERIOD;

            private const int DAY_PERIOD = NightStart + STATIC_PERIOD * 3;

            public DayNightCycle(int timeDelay, int startTime) 
            {
                Repeat = true;
                Playing = true;

                startTime %= DAY_PERIOD;

                CombatScene.Time = startTime;
                CurrentKeyframe = startTime;
                tick = startTime * timeDelay;

                Color startColor = new Color(NightColor);

                for (int i = 0; i < DAY_PERIOD; i++) 
                {
                    Keyframe frame = new Keyframe(i * timeDelay);
                    Color colorDif = new Color(0, 0, 0, 0);

                    if (i >= NightEnd && i < MorningStart) 
                    {
                        colorDif = (MorningColor - NightColor) / 64;
                    }
                    if (i >= MorningEnd && i < MiddayStart)
                    {
                        colorDif = (MiddayColor - MorningColor)/ 64;
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

                    frame.Action = (_) =>
                    {
                        CombatScene.EnvironmentColor.Add(colorDif);
                        CombatScene.Time = (CombatScene.Time + 1) % DAY_PERIOD;
                        //PrintTime();
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
                Console.Write($"{CombatScene.Time}");

                string identifer = "";
                if (CombatScene.Time == NightStart) 
                {
                    identifer = " Night start";
                }
                switch (CombatScene.Time) 
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

            public static bool IsNight() 
            {
                return CombatScene.Time < MorningStart || CombatScene.Time > EveningEnd;
            }
        }
    }
}
