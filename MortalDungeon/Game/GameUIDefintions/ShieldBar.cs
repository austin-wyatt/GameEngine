using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MortalDungeon.Game.UI
{
    public class ShieldBar : UIObject
    {
        enum ShieldAnimations 
        {
            Whole,
            Broken
        }

        private int _maxShieldsDisplayed = 10;

        public List<UIBlock> Shields = new List<UIBlock>();
        public ShieldBar(Vector3 position, UIScale scale)
        {
            Size = scale;
            Position = position;

            BaseComponent = new UIBlock(Position, Size);
            BaseComponent.MultiTextureData.MixTexture = false;
            //BaseComponent.SetColor(Colors.UILightGray);
            //BaseComponent._baseObject.OutlineParameters.SetAllInline(1);
            BaseComponent.SetColor(Colors.Transparent);
            BaseComponent._baseObject.OutlineParameters.SetAllInline(0);


            for (int i = 0; i < _maxShieldsDisplayed; i++) 
            {
                UIBlock shield = new UIBlock(new Vector3(), new UIScale(Size.Y, Size.Y), default, 27);
                shield.MultiTextureData.MixTexture = false;

                shield._baseObject.Animations.Add((AnimationType)ShieldAnimations.Broken, CreateBrokenShieldAnimation());

                if (i == 0)
                {
                    shield.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.BottomLeft);
                }
                else 
                {
                    shield.SetPositionFromAnchor(Shields[Shields.Count - 1].GetAnchorPosition(UIAnchorPosition.BottomRight), UIAnchorPosition.BottomLeft);
                }

                shield.SetRender(false);

                Shields.Add(shield);
                BaseComponent.AddChild(shield);
            }


            AddChild(BaseComponent);
        }

        public override void SetSize(UIScale size)
        {
            base.SetSize(size);

            for (int i = 0; i < Shields.Count; i++) 
            {
                Shields[i].SetSize(new UIScale(Size.Y, Size.Y));
                if (i == 0)
                {
                    Shields[i].SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.BottomLeft);
                }
                else 
                {
                    Shields[i].SetPositionFromAnchor(Shields[i - 1].GetAnchorPosition(UIAnchorPosition.BottomRight), UIAnchorPosition.BottomLeft);
                }
            }
        }

        public void SetShieldsBroken() 
        {
            Shields.ForEach(shield =>
            {
                shield._baseObject.SetAnimation((AnimationType)ShieldAnimations.Broken);
            });
        }

        public void SetShieldsWhole()
        {
            Shields.ForEach(shield =>
            {
                shield._baseObject.SetAnimation((AnimationType)ShieldAnimations.Whole);
            });
        }

        public void SetCurrentShields(int shieldCount) 
        {
            if (shieldCount < 0)
            {
                shieldCount = Math.Abs(shieldCount);
                SetShieldsBroken();
            }
            else 
            {
                SetShieldsWhole();
            }

            for (int i = 0; i < _maxShieldsDisplayed; i++) 
            {
                if (i < shieldCount)
                {
                    Shields[i].SetRender(true);
                }
                else 
                {
                    Shields[i].SetRender(false);
                }
            }
        }

        private Animation CreateBrokenShieldAnimation() 
        {
            Animation tempAnimation;

            RenderableObject brokenShield = new RenderableObject(new SpritesheetObject(28, Spritesheets.UISheet, 1, 1).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

            tempAnimation = new Animation()
            {
                Frames = new List<RenderableObject>() { brokenShield },
                Frequency = 0,
                Repeats = 0
            };

            return tempAnimation;
        }
    }
}
