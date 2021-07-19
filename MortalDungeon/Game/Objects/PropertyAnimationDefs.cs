using MortalDungeon.Engine_Classes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Objects
{
    namespace PropertyAnimations
    {
        public enum PropertyAnimationIDs 
        {
            Unknown,
            Bounce
        }
        public class BounceAnimation : PropertyAnimation
        {
            public BounceAnimation(RenderableObject baseFrame)
            {
                BaseFrame = baseFrame;
                BaseTranslation = baseFrame.Translation.ExtractTranslation();
                BaseColor = new Vector4(baseFrame.Color);

                AnimationID = (int)PropertyAnimationIDs.Bounce;

                Repeat = true;
                Playing = false;



                for (int i = 0; i < 16; i++) 
                {
                    Keyframe temp = new Keyframe(i * 2);

                    if (i < 8)
                    {
                        temp.Action = (baseFrame) => baseFrame.TranslateY(0.003f);
                    }
                    else  
                    {
                        temp.Action = (baseFrame) => baseFrame.TranslateY(-0.003f);
                    }

                    Keyframes.Add(temp);
                }
            }
        }
    }
}
