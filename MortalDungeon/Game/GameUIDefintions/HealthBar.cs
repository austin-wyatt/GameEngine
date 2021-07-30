using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public class HealthBar : UIObject
    {
        public float _healthPercent = 1;
        private UIObject _healthBar;
        public HealthBar(Vector3 position, UIScale scale) 
        {
            Size = scale;
            Position = position;

            BaseComponent = new UIBlock(Position, Size);


            _healthBar = new UIBlock(Position, Size);
            _healthBar.SetColor(new Vector4(0, 1, 0, 0.25f));
            _healthBar.MultiTextureData.MixTexture = false;
            _healthBar.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.TopLeft);

            BaseComponent.AddChild(_healthBar);


            AddChild(BaseComponent);
        }

        public void SetHealthPercent(float percent) 
        {
            if (percent < 0)
            {
                _healthPercent = 0;
            }
            else 
            {
                _healthPercent = percent;
            }

            _healthBar.SetSize(new UIScale(Size.X * _healthPercent, Size.Y));
            _healthBar.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
        }

        public override void SetSize(UIScale size)
        {
            base.SetSize(size);

            SetHealthPercent(_healthPercent);
        }
    }
}
