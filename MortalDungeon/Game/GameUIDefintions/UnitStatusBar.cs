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

        public TextComponent _mainTextBox;
        public TextBox _turnDisplay;
        public HealthBar HealthBar;
        public ShieldBar ShieldBar;

        public bool WillDisplay = true;

        public UnitStatusBar(Unit unit, Camera camera) 
        {
            Vector4 unitPos = new Vector4(unit.Position, 1) * unit.GetDisplay().Transformations * camera.GetViewMatrix() * camera.ProjectionMatrix;
            unitPos.X /= unitPos.W;
            unitPos.Y /= unitPos.W;
            unitPos.Z /= unitPos.W;

            Vector3 screenSpace = WindowConstants.ConvertLocalToScreenSpaceCoordinates(unitPos.Xy);

            UIScale scale = new UIScale(0.3f, 0.1f);

            BaseComponent = new UIBlock(screenSpace, scale);

            BaseComponent.MultiTextureData.MixTexture = false;
            BaseComponent.SetColor(Colors.UILightGray);


            TextComponent nameBox = new TextComponent();
            nameBox.SetText(unit.Name);
            nameBox.SetColor(Colors.UITextBlack);
            nameBox.SetPositionFromAnchor(BaseComponent.Position, UIAnchorPosition.Center);

            _mainTextBox = nameBox;

            //_mainTextBox = textBox;
            //BaseComponent.AddChild(textBox);
            BaseComponent.AddChild(nameBox);


            _turnDisplay = new TextBox(new Vector3(), new UIScale(scale.X / 4, scale.Y / 2), " ", 0.07f, false, new UIDimensions(0, 0));
            _turnDisplay.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.BottomLeft);
            _turnDisplay.BaseComponent.SetColor(Colors.Transparent);
            _turnDisplay.BaseComponent.GetBaseObject().OutlineParameters.SetAllInline(0);
            _turnDisplay.GetBaseObject().RenderData = new RenderData() { AlphaThreshold = 1 };
            _turnDisplay.BaseComponent.MultiTextureData.MixTexture = false;

            BaseComponent.AddChild(_turnDisplay);


            HealthBar = new HealthBar(new Vector3(), scale);
            HealthBar.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.TopLeft);
            HealthBar.SetHealthPercent(1, unit.AI.Team);

            BaseComponent.AddChild(HealthBar);


            ShieldBar = new ShieldBar(new Vector3(), scale);
            ShieldBar.MultiTextureData.MixTexture = false;
            ShieldBar.SetCurrentShields(unit.Info.CurrentShields);

            BaseComponent.AddChild(ShieldBar);



            _camera = camera;
            _unit = unit;

            AddChild(BaseComponent);

            unit.StatusBarComp = this;

            UpdateInfoBarScales(scale);
            UpdateUnitStatusPosition();
        }

        public void UpdateUnitStatusPosition() 
        {
            Vector4 unitPos = new Vector4(0, 0, 0, 1) * _unit.GetDisplay().Transformations * _camera.GetViewMatrix() * _camera.ProjectionMatrix;
            unitPos.X /= unitPos.W;
            unitPos.Y /= unitPos.W;
            unitPos.Z /= unitPos.W;

            if (WillDisplay)
            {
                if (_camera.Position.Z < 2)
                {
                    SetRender(false);
                }
                else
                {
                    SetRender(true);
                }

                UIScale zoomScale = Size;

                if (_camera.Position.Z < 4 && _camera.Position.Z > 2)
                {
                    //zoomScale = new UIScale(0.5f, 0.085f);
                    zoomScale = new UIScale(0.3f, 0.06f);

                    unitPos.Y += 0.25f;
                    SetSize(zoomScale);
                    _mainTextBox.SetTextScale(0.03f);
                    UpdateInfoBarScales(zoomScale);
                }
                else if (_camera.Position.Z >= 4 && _camera.Position.Z < 8)
                {
                    zoomScale = new UIScale(0.18f, 0.045f);
                    unitPos.Y += 0.17f;

                    SetSize(zoomScale);
                    _mainTextBox.SetTextScale(0.02f);
                    UpdateInfoBarScales(zoomScale);
                }
                else if (_camera.Position.Z >= 8 && _camera.Position.Z < 10)
                {
                    zoomScale = new UIScale(0.15f, 0.040f);
                    unitPos.Y += 0.12f;

                    SetSize(zoomScale);
                    _mainTextBox.SetTextScale(0.02f);
                }
                else if (_camera.Position.Z >= 10)
                {
                    SetRender(false);
                }

                UpdateInfoBarScales(zoomScale);


                Vector3 screenSpace = WindowConstants.ConvertLocalToScreenSpaceCoordinates(unitPos.Xy);

                BaseComponent.SetPosition(screenSpace);

                _mainTextBox.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);
                _turnDisplay.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.BottomLeft);
                HealthBar.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.TopLeft);
                ShieldBar.SetPositionFromAnchor(HealthBar.GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.TopLeft);
            }
        }

        public void SetWillDisplay(bool display) 
        {
            WillDisplay = display;
            SetRender(display);

            if (display)
            {
                UpdateUnitStatusPosition();
            }
        }

        public void SetIsTurn(bool isTurn) 
        {
            if (isTurn)
            {
                _turnDisplay.TextField.SetText("*");
                _turnDisplay.TextField._textField.Letters[0].BaseObjects[0].OutlineParameters.SetAllInline(1);
            }
            else 
            {
                _turnDisplay.TextField._textField.SetTextString(" ");
                _turnDisplay.TextField._textField.Letters[0].BaseObjects[0].OutlineParameters.SetAllInline(0);
            }
        }

        public override void OnCameraMove()
        {
            base.OnCameraMove();

            UpdateUnitStatusPosition();
        }

        private void UpdateInfoBarScales(UIScale scale) 
        {
            HealthBar.SetSize(new UIScale(scale.X, scale.Y / 2));
            ShieldBar.SetSize(new UIScale(scale.X, scale.Y / 1.5f));

            Console.WriteLine(scale);
        }
    }
}
