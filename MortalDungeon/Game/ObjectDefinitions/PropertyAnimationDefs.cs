using MortalDungeon.Engine_Classes;
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
    }
}
