using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    internal class Tooltip : UIObject
    {
        internal UIDimensions Margins = new UIDimensions();
        internal Tooltip() 
        {
            UIBlock mainBlock = new UIBlock();
            mainBlock.MultiTextureData.MixPercent = 0.1f;
            mainBlock.SetColor(Colors.UILightGray);

            BaseComponent = mainBlock;

            AddChild(BaseComponent, -10);
        }

        internal override void AddChild(UIObject uiObj, int zIndex = -1)
        {
            base.AddChild(uiObj, zIndex);

            FitContents();
        }

        internal void FitContents(bool useMargin = true, UIDimensions margin = default) 
        {
            UIDimensions dimTopLeft = new UIDimensions(9999,9999);
            UIDimensions dimBottomRight = new UIDimensions(-9999,-9999);

            Vector3 pos;
            for (int i = 0; i < Children.Count; i++) 
            {
                if (Children[i].ObjectID != BaseComponent.ObjectID) 
                {
                    pos = Children[i].GetAnchorPosition(UIAnchorPosition.TopLeft);

                    if (pos.X < dimTopLeft.X) 
                    {
                        dimTopLeft.X = pos.X;
                    }
                    if (pos.Y < dimTopLeft.Y) 
                    {
                        dimTopLeft.Y = pos.Y;
                    }

                    pos = Children[i].GetAnchorPosition(UIAnchorPosition.BottomRight);

                    if (pos.X > dimBottomRight.X)
                    {
                        dimBottomRight.X = pos.X;
                    }
                    if (pos.Y > dimBottomRight.Y)
                    {
                        dimBottomRight.Y = pos.Y;
                    }
                }
            }

            if (dimTopLeft.X != 9999) 
            {
                if (useMargin) 
                {
                    if (margin == null)
                    {
                        dimBottomRight += new UIDimensions(20, 20);
                    }
                    else 
                    {
                        dimBottomRight += margin;
                    }
                }

                UIDimensions tooltipDim = dimBottomRight - dimTopLeft;

                tooltipDim.X *= 2 * WindowConstants.AspectRatio;
                tooltipDim.Y *= 2;

                tooltipDim += Margins;

                SetSize(tooltipDim);
            }
        }
    }
}
