using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.UI
{
    public class StaminaBar : UIObject
    {
        public float _focusPercent = 1;
        private UIObject _focusBar;

        public Vector4 BarColor = _Colors.Tan;

        private static ObjectPool<UIBlock> _staminaPipPool = new ObjectPool<UIBlock>(10);

        public static Vector4 FULL_COLOR = _Colors.Tan;
        public static Vector4 EMPTY_COLOR = _Colors.DarkTan;
        public static Vector4 ENERGIZED_COLOR = _Colors.Red;

        public StaminaBar(Vector3 position, UIScale scale)
        {
            Size = scale;
            Position = position;

            BaseComponent = new UIBlock(Position, Size);
            BaseComponent.SetColor(_Colors.Transparent);
            BaseComponent.SetAllInline(0);

            for(int i = 0;i < 10; i++)
            {
                UIBlock pip = new UIBlock(default, new UIScale(Size.Y * 3, Size.Y * 3), spritesheetPosition: (int)IconSheetIcons.StaminaPip, spritesheet: Spritesheets.IconSheet);
                pip.SetAllInline(0);
                _staminaPipPool.FreeObject(ref pip);

                pip.HasTimedHoverEffect = true;
                pip.Hoverable = true;
                pip.TimedHover += (s) =>
                {
                    OnTimedHover(pip);
                };
            }
            
            AddChild(BaseComponent);
        }

        public List<UIBlock> Pips = new List<UIBlock>();

        public void SetStaminaAmount(int stamina, int maxStamina)
        {
            for(int i = 0; i < Pips.Count; i++)
            {
                _staminaPipPool.FreeObject(Pips[i]);
            }
            Pips.Clear();
            BaseComponent.RemoveChildren();

            UIBlock prevPip = null;

            for(int i = 0; i < Math.Max(stamina, maxStamina); i++)
            {
                UIBlock pip = _staminaPipPool.GetObject();

                Pips.Add(pip);

                if (i == 0)
                {
                    pip.SAP(BaseComponent.GAP(UIAnchorPosition.TopLeft) + new Vector3(0, -14, 0), UIAnchorPosition.TopLeft);
                }
                else
                {
                    pip.SAP(prevPip.GAP(UIAnchorPosition.RightCenter) + new Vector3(1, 0, 0), UIAnchorPosition.LeftCenter);
                }

                if(i < stamina)
                {
                    if (i > maxStamina)
                    {
                        pip.SetColor(_Colors.Red);
                    }
                    else
                    {
                        pip.SetColor(_Colors.Tan);
                    }
                }
                else
                {
                    pip.SetColor(_Colors.DarkTan);
                }

                BaseComponent.AddChild(pip);
                prevPip = pip;
            }
        }

        public override void SetSize(UIScale size)
        {
            base.SetSize(size);

            _staminaPipPool.EmptyPool();

            for (int i = 0; i < 10; i++)
            {
                UIBlock pip = new UIBlock(default, new UIScale(Size.Y * 3, Size.Y * 3), spritesheetPosition: (int)IconSheetIcons.StaminaPip, spritesheet: Spritesheets.IconSheet);
                pip.SetAllInline(0);
                _staminaPipPool.FreeObject(ref pip);

                pip.HasTimedHoverEffect = true;
                pip.Hoverable = true;
                pip.TimedHover += (s) =>
                {
                    OnTimedHover(pip);
                };
            }
        }
    }
}
