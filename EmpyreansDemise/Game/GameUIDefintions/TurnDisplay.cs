﻿using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.UI
{
    public class TurnDisplay : UIObject
    {
        public List<Unit> Units = new List<Unit>();
        private List<UIObject> UnitObjects = new List<UIObject>();

        public TurnDisplay() 
        {
            BaseComponent = new UIBlock();
            BaseComponent.SetAllInline(0);

            BaseComponent.SetColor(_Colors.Transparent);

            AddChild(BaseComponent);

            ValidateObject(this);
        }

        private readonly object _settingUnitsLock = new object();
        public void SetUnits(List<Unit> units, CombatScene scene) 
        {
            lock (_settingUnitsLock) 
            {
                Units = units;

                foreach(var unitObj in UnitObjects)
                {
                    BaseComponent.RemoveChild(unitObj);
                    scene.Tick -= unitObj.Tick;
                }

                UnitObjects.Clear();

                int unitSpacing = 50;

                for (int i = 0; i < Units.Count; i++) 
                {
                    BaseObject obj = Units[i].CreateBaseObject();

                    obj._currentAnimation.Reset();
                    obj._currentAnimation.Pause();

                    UIObject uiObj = new UIObject();

                    obj.BaseFrame.ScaleX(0.18f / WindowConstants.AspectRatio);
                    obj.BaseFrame.ScaleY(0.18f);

                    obj.EnableLighting = false;

                    UnitObjects.Add(uiObj);
                    scene.Tick += uiObj.Tick;

                    Vector3 pos;

                    if (BaseComponent.Children.Count > 0)
                    {
                        pos = BaseComponent.Children[^1].GetAnchorPosition(UIAnchorPosition.RightCenter) + new Vector3(unitSpacing, 0, 0);
                    }
                    else 
                    {
                        pos = BaseComponent.GetAnchorPosition(UIAnchorPosition.Center);
                    }

                    UIBlock block = new UIBlock(default, new UIScale(0.18f, 0.18f));
                    block.MultiTextureData.MixTexture = false;
                    block.SetColor(_Colors.UILightGray - new Vector4(0, 0, 0, 0.25f));

                    block.BaseObjects.Insert(0, obj);
                    block._baseObject = obj;
                    block.BaseObjects.RemoveAt(1);

                    block.SetPosition(obj.Position);

                    uiObj.BaseComponent = block;

                    BaseComponent.AddChild(uiObj);

                    uiObj.AddChild(block);

                    uiObj.SetPositionFromAnchor(pos, UIAnchorPosition.Center);

                    if (i != Units.Count - 1) 
                    {
                        UIBlock chevron = new UIBlock(default, new UIScale(0.15f, 0.1f), default, (int)UISheetIcons.Chevron);
                        chevron.MultiTextureData.MixTexture = false;

                        chevron.SetPositionFromAnchor(pos + new Vector3(unitSpacing * 0.7f, 0, 0), UIAnchorPosition.Center);
                        //chevron.SetColor(Units[i + 1].StatusBarComp.HealthBar.BarColor);

                        uiObj.AddChild(chevron);
                    }

                    int index = i;
                    uiObj.Clickable = true;
                    uiObj.Click += (s, e) =>
                    {
                        Vector2i clusterPos = TileMapHelpers.PointToClusterPosition(Units[index].Info.TileMapPosition);

                        if (VisionManager.ConsolidatedVision[scene.VisibleTeam].TryGetValue(Units[index].Info.TileMapPosition, out var inVision) && inVision > 0) 
                        {
                            scene.SmoothPanCameraToUnit(Units[index], 1);
                        }
                    };
                }

                PositionUnits();
            }
        }

        public void PositionUnits() 
        {
            if (UnitObjects.Count <= 1)
                return;

            Vector3 posDiff = (UnitObjects[0].Position - UnitObjects[^1].Position) / 2;

            for (int i = 0; i < UnitObjects.Count; i++) 
            {
                UnitObjects[i].SetPosition(UnitObjects[i].Position + posDiff);
            }
        }

        public void ClearUnits() 
        {
            Units = null;

            BaseComponent.RemoveChildren();

            UnitObjects.Clear();
        }

        public void SetCurrentUnit(int currUnit) 
        {
            for (int i = 0; i < UnitObjects.Count; i++) 
            {
                UnitObjects[i].BaseComponent._baseObject._currentAnimation.Reset();
                UnitObjects[i].BaseComponent._baseObject._currentAnimation.Pause();
            }

            UnitObjects[currUnit].BaseComponent._baseObject._currentAnimation.Reset();
            UnitObjects[currUnit].BaseComponent._baseObject._currentAnimation.Play();
        }
    }
}
