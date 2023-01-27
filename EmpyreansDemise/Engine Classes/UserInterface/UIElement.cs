using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.UserInterface
{
    public class UIElement : Transformations3D
    {
        public bool Render = false;
        public UIVisual Visual = new UIVisual(VisualType.Color);

        public Layout Parent;

        public UIDimensions Dimensions;

        public int ZIndex = 0;

        #region depth declarations
        /// <summary>
        /// The depth that gets passed to the shader to be reevaluated as a float
        /// </summary>
        public int _absoluteDepth = 0;
        /// <summary>
        /// The depth offset of the element from their nearest parent layout
        /// </summary>
        public int _depthOffset = 0;
        #endregion 

        public void SetRender(bool value)
        {
            Render = value;
        }

        public void InvalidateLayout()
        {
            Parent.LayoutInvalidated();
        }

        public void InvalidateRender()
        {
            //Actions that invalidate a render
            //(ie actions that will cause the render batches to be rebuilt):
            //
            //Changing the Visual type
            //Changing color between opaque and transparent
        }

        public override void CalculateTransformations()
        {
            if (Visual.TryGetVisualTransform(out var transform))
            {
                Transformations = Scale * transform.Scale * 
                    Rotation * transform.Rotation * 
                    Translation * transform.Translation;
            }
            else
            {
                base.CalculateTransformations();
            }
        }

        //Insert UIObject functionality here:
        //Positioning
        //Color setting
        //Events
        //Bounds checks
    }
}
