using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Units;
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

        UnitTeam _team = UnitTeam.Ally;
        public HealthBar(Vector3 position, UIScale scale) 
        {
            Size = scale;
            Position = position;

            BaseComponent = new UIBlock(Position, Size);

            //HasTimedHoverEffect = true;
            //Hoverable = true;


            _healthBar = new UIBlock(Position, Size);
            _healthBar.SetColor(new Vector4(0, 1, 0, 0.25f));
            _healthBar.MultiTextureData.MixTexture = false;
            _healthBar.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.TopLeft);

            BaseComponent.AddChild(_healthBar);


            AddChild(BaseComponent);
        }

        public void SetHealthPercent(float percent, UnitTeam team = UnitTeam.Unknown) 
        {
            if (team == UnitTeam.Unknown)
            {
                team = _team;
            }
            else 
            {
                _team = team;
            }


            switch (team) 
            {
                case UnitTeam.Ally:
                    _healthBar.SetColor(new Vector4(0, 1, 0, 0.25f));
                    break;
                case UnitTeam.Enemy:
                    _healthBar.SetColor(new Vector4(1, 0, 0, 0.25f));
                    break;
                case UnitTeam.Neutral:
                    _healthBar.SetColor(new Vector4(0.93f, 0.83f, 0.56f, 0.75f));
                    break;
            }

            if (percent < 0)
            {
                _healthPercent = 0;
            }
            else 
            {
                _healthPercent = percent;
            }

            BaseComponent.SetSize(Size);
            _healthBar.SetSize(new UIScale(Size.X * _healthPercent, Size.Y));
            _healthBar.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
        }

        public override void SetSize(UIScale size)
        {
            base.SetSize(size);

            SetHealthPercent(_healthPercent);

        }

        public override void SetInlineColor(Vector4 color)
        {
            base.SetInlineColor(color);
            _healthBar.SetInlineColor(color);
        }
    }
}
