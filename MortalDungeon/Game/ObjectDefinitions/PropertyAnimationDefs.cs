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
        internal class BounceAnimation : PropertyAnimation
        {
            internal BounceAnimation(RenderableObject baseFrame, int bounceFrameDelay = 1)
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

        internal class LiftAnimation : PropertyAnimation
        {
            internal LiftAnimation(RenderableObject baseFrame)
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

        internal class DayNightCycle : PropertyAnimation 
        {
            private static Color NightColor = new Color(0.1f, 0.1f, 0.2f, 1f);
            private static Color MorningColor = new Color(0.5f, 0.5f, 0.43f, 1f);
            private static Color MiddayColor = new Color(0.52f, 0.52f, 0.52f, 1f);
            private static Color EveningColor = new Color(0.5f, 0.47f, 0.42f, 1f);


            private const int STATIC_PERIOD = 24;
            private const int TRANSITION_PERIOD = 64;

            private const int NightEnd = 0;
            internal const int MorningStart = NightEnd + TRANSITION_PERIOD;
            private const int MorningEnd = MorningStart + STATIC_PERIOD;
            internal const int MiddayStart = MorningEnd + TRANSITION_PERIOD;
            private const int MiddayEnd = MiddayStart + STATIC_PERIOD * 5;
            internal const int EveningStart = MiddayEnd + TRANSITION_PERIOD;
            private const int EveningEnd = EveningStart + STATIC_PERIOD;
            internal const int NightStart = EveningEnd + TRANSITION_PERIOD;

            private const int DAY_PERIOD = NightStart + STATIC_PERIOD * 3;

            public const int HOUR = STATIC_PERIOD;

            internal DayNightCycle(int timeDelay, int startTime) 
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

                    frame.Action = () =>
                    {
                        CombatScene.EnvironmentColor.Add(colorDif);
                        CombatScene.Time = tick % DAY_PERIOD;
                        //PrintTime();
                    };

                    Keyframes.Add(frame);
                }



                CombatScene.EnvironmentColor.R = startColor.R;
                CombatScene.EnvironmentColor.G = startColor.G;
                CombatScene.EnvironmentColor.B = startColor.B;
                CombatScene.EnvironmentColor.A = startColor.A;
            }

            internal void PrintTime() 
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

            internal static bool IsNight() 
            {
                return CombatScene.Time < MorningStart || CombatScene.Time > EveningEnd;
            }
        }
    }
}
