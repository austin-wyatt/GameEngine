using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.TextHandling;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.UI
{
    public class UnitStatusBar : UIObject
    {
        public Camera _camera;
        public Unit _unit;

        public Text _nameBox;
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
            //BaseComponent.SetColor(_Colors.UILightGray);
            BaseComponent.SetColor(_Colors.Transparent);

            Name = "UnitStatusBar";

            Text nameBox = new Text(unit.Name, Text.DEFAULT_FONT, 32, Brushes.White);
            nameBox.SetPositionFromAnchor(BaseComponent.Position, UIAnchorPosition.Center);
            nameBox.SetColor(_Colors.Black);

            _nameBox = nameBox;

            _nameBox.Name = "NameBox";

            //_mainTextBox = textBox;
            //BaseComponent.AddChild(textBox);
            BaseComponent.AddChild(nameBox, 100);


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

            UpdateInfo();

            UpdateInfoBarScales(scale);
            UpdateUnitStatusPosition();
        }

        private object _updateLock = new object();
        public void UpdateUnitStatusPosition([Optional]Matrix4 cameraMatrices) 
        {
            if (_unit.BaseObjects.Count == 0)
                return;

            if (_unit.Cull)
                return;

            lock (_updateLock)
            {
                Vector4 unitPos = new Vector4(0, 0, 0, 1);
                if(cameraMatrices != Matrix4.Zero)
                {
                    unitPos *= _unit.GetDisplay().Transformations * cameraMatrices;
                }
                else
                {
                    unitPos *= _unit.GetDisplay().Transformations * _camera.GetViewMatrix() * _camera.ProjectionMatrix;
                }

                unitPos.X /= unitPos.W;
                unitPos.Y /= unitPos.W;
                unitPos.Z /= unitPos.W;

                if (WillDisplay)
                {
                    _nameBox.SetRender(true);
                    //BaseComponent.SetColor(_Colors.UILightGray);
                    BaseComponent.SetColor(_Colors.Transparent);
                    //BaseComponent.SetAllInline(2);
                    BaseComponent.SetAllInline(0);

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
                        _nameBox.SetTextScale(0.065f);
                        UpdateInfoBarScales(zoomScale);
                    }
                    else if (_camera.Position.Z >= 4 && _camera.Position.Z < 8)
                    {
                        zoomScale = new UIScale(0.18f, 0.045f);
                        unitPos.Y += 0.17f;

                        SetSize(zoomScale);
                        _nameBox.SetTextScale(0.055f);
                        UpdateInfoBarScales(zoomScale);
                    }
                    else if (_camera.Position.Z >= 8 && _camera.Position.Z < 10)
                    {
                        zoomScale = new UIScale(0.15f, 0.040f);
                        unitPos.Y += 0.12f;

                        SetSize(zoomScale);
                        _nameBox.SetTextScale(0.045f);
                    }
                    else if (_camera.Position.Z >= 10)
                    {
                        zoomScale = new UIScale(0.1f, 0.04f);

                        SetSize(zoomScale);

                        _nameBox.SetRender(false);
                        BaseComponent.SetColor(_Colors.Transparent);
                        BaseComponent.SetAllInline(0);
                        //SetRender(false);
                    }
                    else if (_camera.Position.Z >= 10)
                    {
                        SetRender(false);
                    }

                    UpdateInfoBarScales(zoomScale);


                    Vector3 screenSpace = WindowConstants.ConvertLocalToScreenSpaceCoordinates(unitPos.Xy);

                    screenSpace.Z = Position.Z;
                    //screenSpace.Z = 0.01f;

                    BaseComponent.SetPosition(screenSpace);

                    _nameBox.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.Center), UIAnchorPosition.Center);
                    HealthBar.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.TopLeft);
                    ShieldBar.SetPositionFromAnchor(HealthBar.GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.TopLeft);

                    //ForceTreeRegeneration();
                }
            }
        }

        public void SetWillDisplay(bool display) 
        {
            if(WillDisplay != display) 
            {
                WillDisplay = display;
                SetRender(display);

                if (display)
                {
                    UpdateUnitStatusPosition();
                }
            }
        }

        public void SetIsTurn(bool isTurn) 
        {

        }

        public void UpdateInfo() 
        {
            HealthBar.SetHealthPercent(_unit.Info.Health / _unit.Info.MaxHealth, _unit.AI.Team);
            ShieldBar.SetCurrentShields(_unit.Info.CurrentShields);

            if(_nameBox.TextString != _unit.Name)
            {
                _nameBox.SetText(_unit.Name);
            }
        }

        public override void OnCameraMove()
        {
            base.OnCameraMove();
            //UpdateUnitStatusPosition();

            //Task.Run(() =>
            //{
            //    UpdateUnitStatusPosition();
            //});
        }

        private UIScale _healthBarScale = new UIScale();
        private UIScale _shieldBarScale = new UIScale();
        private void UpdateInfoBarScales(UIScale scale) 
        {
            _healthBarScale.X = scale.X;
            _healthBarScale.Y = scale.Y / 2;
            _shieldBarScale.X = scale.X;
            _shieldBarScale.Y = scale.Y / 1.5f;

            HealthBar.SetSize(_healthBarScale);
            ShieldBar.SetSize(_shieldBarScale);
        }
    }
}
