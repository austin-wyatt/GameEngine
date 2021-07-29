using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public class UnitStatusBar : UIObject
    {
        public Camera _camera;
        public Unit _unit;

        public TextBox _textBox;
        public UnitStatusBar(Unit unit, Camera camera) 
        {
            Vector4 unitPos = new Vector4(unit.Position, 1) * unit.GetDisplay().Transformations * camera.GetViewMatrix() * camera.ProjectionMatrix;
            unitPos.X /= unitPos.W;
            unitPos.Y /= unitPos.W;
            unitPos.Z /= unitPos.W;

            Vector3 screenSpace = WindowConstants.ConvertLocalToScreenSpaceCoordinates(unitPos.Xy);

            UIScale scale = new UIScale(0.55f, 0.1f);

            BaseComponent = new UIBlock(screenSpace, scale);

            BaseComponent.MultiTextureData.MixTexture = false;
            BaseComponent.SetColor(Colors.UILightGray);


            TextBox textBox = new TextBox(new Vector3(), scale, unit.Name, 0.07f, true);
            textBox.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.TopCenter), UIAnchorPosition.TopCenter);
            textBox.BaseComponent.SetColor(Colors.Transparent);
            textBox.BaseComponent.GetBaseObject().OutlineParameters.SetAllInline(0);
            textBox.GetBaseObject().RenderData = new RenderData() { AlphaThreshold = 1 };
            textBox.BaseComponent.MultiTextureData.MixTexture = false;
            textBox.TextField.SetColor(new Vector4(0.1f, 0.1f, 0.1f, 1));

            _textBox = textBox;

            BaseComponent.AddChild(textBox);

            _camera = camera;
            _unit = unit;

            AddChild(BaseComponent);

            unit.StatusBarComp = this;

            UpdateUnitStatusPosition();
        }

        public void UpdateUnitStatusPosition() 
        {
            Vector4 unitPos = new Vector4(0, 0, 0, 1) * _unit.GetDisplay().Transformations * _camera.GetViewMatrix() * _camera.ProjectionMatrix;
            unitPos.X /= unitPos.W;
            unitPos.Y /= unitPos.W;
            unitPos.Z /= unitPos.W;

            if (_camera.Position.Z < 2)
            {
                Render = false;
            }
            else 
            {
                Render = true;
            }

            if (_camera.Position.Z < 4 && _camera.Position.Z > 2)
            {
                unitPos.Y += 0.3f;
                SetSize(new UIScale(0.55f, 0.1f));
                _textBox.TextField.SetTextScale(0.07f);
            }
            else if (_camera.Position.Z >= 4 && _camera.Position.Z < 10)
            {
                unitPos.Y += 0.13f;
                SetSize(new UIScale(0.4f, 0.075f));
                _textBox.TextField.SetTextScale(0.04f);
            }
            else if (_camera.Position.Z >= 10)
            {
                //unitPos.Y += 0.15f;
                Render = false;
            }

            
            Vector3 screenSpace = WindowConstants.ConvertLocalToScreenSpaceCoordinates(unitPos.Xy);

            BaseComponent.SetPosition(screenSpace);
        }

        public override void OnCameraMove()
        {
            base.OnCameraMove();

            UpdateUnitStatusPosition();
        }
    }
}
