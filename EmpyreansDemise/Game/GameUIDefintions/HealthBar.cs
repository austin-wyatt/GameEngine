using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.UI
{
    public class HealthBar : UIObject
    {
        public float _healthPercent = 1;
        private UIObject _healthBar;

        private UnitTeam _team = UnitTeam.PlayerUnits;

        public Vector4 BarColor = new Vector4(0, 0.5f, 0, 1f);

        public HealthBar(Vector3 position, UIScale scale) 
        {
            Size = scale;
            Position = position;

            BaseComponent = new UIBlock(Position, Size);

            //HasTimedHoverEffect = true;
            //Hoverable = true;


            _healthBar = new UIBlock(Position, Size);
            _healthBar.SetColor(BarColor);
            _healthBar.MultiTextureData.MixTexture = false;
            _healthBar.MultiTextureData.MixPercent = 0.25f;
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

            Relation relation = UnitTeam.PlayerUnits.GetRelation(team);


            switch (relation) 
            {
                case Relation.Friendly:
                    if(team == UnitTeam.PlayerUnits)
                    {
                        BarColor = new Vector4(0, 0.5f, 0, 1f);
                    }
                    else
                    {
                        BarColor = new Vector4(0, 0.75f, 0, 1f);
                    }
                        
                    _healthBar.SetColor(BarColor);
                    break;
                case Relation.Hostile:
                    BarColor = new Vector4(0.5f, 0, 0, 1f);
                    _healthBar.SetColor(BarColor);
                    break;
                case Relation.Neutral:
                    BarColor = new Vector4(0.93f, 0.83f, 0.56f, 1f);
                    _healthBar.SetColor(BarColor);
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
