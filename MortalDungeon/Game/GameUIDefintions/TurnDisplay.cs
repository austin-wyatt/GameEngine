using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI
{
    public class TurnDisplay : UIObject
    {
        public List<Unit> Units = new List<Unit>();
        private List<UIObject> UnitObjects = new List<UIObject>();

        public TurnDisplay() 
        {
            BaseComponent = new UIBlock();
            BaseComponent.SetColor(Colors.Transparent);

            AddChild(BaseComponent);

            ValidateObject(this);
        }

        public void SetUnits(List<Unit> units) 
        {
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

                BaseComponent.AddChild(uiObj);
            }

            PositionUnits();
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
