using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI
{
    internal class TurnDisplay : UIObject
    {
        internal List<Unit> Units = new List<Unit>();
        private List<UIObject> UnitObjects = new List<UIObject>();

        internal TurnDisplay() 
        {
            BaseComponent = new UIBlock();
            BaseComponent.SetColor(Colors.Transparent);

            AddChild(BaseComponent);

            ValidateObject(this);
        }

        private bool _settingUnits = false;
        internal void SetUnits(List<Unit> units, CombatScene scene) 
        {
            if (_settingUnits)
                return;

            _settingUnits = true;
            
            Units = units;

            BaseComponent.RemoveChildren();

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

                UnitObjects.Add(uiObj);

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
                block.SetColor(Colors.UILightGray - new Vector4(0, 0, 0, 0.25f));

                block.BaseObjects.Insert(0, obj);
                block._baseObject = obj;

                block.SetPosition(obj.Position);

                uiObj.BaseComponent = block;

                uiObj.AddChild(block);

                uiObj.SetPositionFromAnchor(pos, UIAnchorPosition.Center);

                if (i != Units.Count - 1) 
                {
                    UIBlock chevron = new UIBlock(default, new UIScale(0.15f, 0.1f), default, (int)UISheetIcons.Chevron);
                    chevron.MultiTextureData.MixTexture = false;

                    chevron.SetPositionFromAnchor(pos + new Vector3(unitSpacing * 0.7f, 0, 0), UIAnchorPosition.Center);
                    chevron.SetColor(Units[i + 1].StatusBarComp.HealthBar.BarColor);

                    uiObj.AddChild(chevron);
                }

                int index = i;
                uiObj.Clickable = true;
                uiObj.OnClickAction = () =>
                {
                    Vector2i clusterPos = scene._tileMapController.PointToClusterPosition(Units[index].Info.TileMapPosition);

                    if (VisionMap.InVision(clusterPos.X, clusterPos.Y, UnitTeam.PlayerUnits)) 
                    {
                        Vector4 pos = Units[index].BaseObject.BaseFrame.Position;

                        scene.SmoothPanCamera(new Vector3(pos.X, pos.Y - scene._camera.Position.Z / 5, scene._camera.Position.Z), 1);
                    }
                };


                BaseComponent.AddChild(uiObj);
            }

            PositionUnits();

            _settingUnits = false;
        }

        internal void PositionUnits() 
        {
            if (UnitObjects.Count <= 1)
                return;

            Vector3 posDiff = (UnitObjects[0].Position - UnitObjects[^1].Position) / 2;

            for (int i = 0; i < UnitObjects.Count; i++) 
            {
                UnitObjects[i].SetPosition(UnitObjects[i].Position + posDiff);
            }
        }

        internal void ClearUnits() 
        {
            Units = null;

            BaseComponent.RemoveChildren();

            UnitObjects.Clear();
        }

        internal void SetCurrentUnit(int currUnit) 
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
