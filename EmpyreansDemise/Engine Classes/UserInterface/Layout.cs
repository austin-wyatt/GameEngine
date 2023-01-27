using Empyrean.Engine_Classes.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.UserInterface
{
    public class Layout : UIElement
    {
        /// <summary>
        /// The integer whose bits represent a float value of ~0.996
        /// </summary>
        const int MAX_DEPTH = 1065300000;
        /// <summary>
        /// The integer whose bits represent a float value of ~0
        /// </summary>
        const int MIN_DEPTH = 9000000;
        public static Layout ROOT = new Layout()
        {
            _depthAllotment = MAX_DEPTH - MIN_DEPTH,
            _absoluteDepth = MIN_DEPTH
        };

        const int DEFAULT_DEPTH_ALLOTMENT = 4;

        public List<UIElement> Children = new List<UIElement>();

        /// <summary>
        /// The amount of depth indices that this layout can apportion.
        /// If the layout does not have a sufficiently large allotment, it
        /// must request more from its parent
        /// </summary>
        protected int _depthAllotment;

        

        protected virtual void PositionElements()
        {
            //position all child elements according to the current layout
        }

        protected void CalculateDimensions()
        {
            throw new NotImplementedException();
            //Dimensions = ...
            //calculate dimensions by iterating through all children and getting the min/max anchor offsets for each corner
        }

        public void LayoutInvalidated()
        {
            UIDimensions prevDimensions = Dimensions;

            PositionElements();
            CalculateDimensions();

            if(prevDimensions != Dimensions)
            {
                InvalidateLayout();
            }
        }

        public void AddChild(UIElement child, int zIndex = -1, int requestedDepthAllotment = DEFAULT_DEPTH_ALLOTMENT)
        {
            lock (Children)
            {
                int currentDepth = Children[^1]._absoluteDepth;
                
                Children.Add(child);

                child.Parent = this;
                child.ZIndex = zIndex == -1 ? child.ZIndex : zIndex;

                if(Children.Count > 1 && Children[^1].ZIndex < Children[^2].ZIndex)
                {
                    Children.Sort((a, b) => a.ZIndex - b.ZIndex);
                }

                if (currentDepth + requestedDepthAllotment > _depthAllotment)
                {
                    RequestGreaterAllotment();
                }

                LayoutInvalidated();
            }
        }

        public void RemoveChild(UIElement child)
        {
            lock (Children)
            {
                if (Children.Remove(child))
                {
                    LayoutInvalidated();
                }
            }
        }

        public bool _generatesOwnRenderBatch = false;
        public RenderBatch GenerateRenderBatch()
        {
            //HOW THIS PROCESS WORKS
            //

            //walk through children and generate render batches

            //if a layout has the flag _generatesOwnRenderBatch add their 
            //tree's batch as a child batch but do not trigger a regeneration
            //(do propagate the depth though)

            throw new NotImplementedException();
        }
        protected void PropagateDepth()
        {
            _absoluteDepth = _depthOffset + Parent._absoluteDepth;

            int currDepth = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i]._depthOffset = currDepth;
                Children[i]._absoluteDepth = _absoluteDepth + currDepth;

                Layout layout = Children[i] as Layout;
                if (layout != null)
                {
                    currDepth += layout._depthAllotment;
                }
                else
                {
                    currDepth++;
                }

                if(currDepth > _depthAllotment)
                    RequestGreaterAllotment();
            }
        }
        protected void RequestGreaterAllotment()
        {
            Parent.ProcessAllotmentRequest(this);
        }

        protected void ProcessAllotmentRequest(Layout child)
        {
            Layout layout = Children[^1] as Layout;

            int finalChildAllotment = layout != null ? layout._depthAllotment : 0;

            int currDepth = Children[^1]._depthOffset + finalChildAllotment;

            if(currDepth + child._depthAllotment > _depthAllotment)
            {
                if (this == ROOT)
                    throw new Exception();

                RequestGreaterAllotment();
            }

            child._depthAllotment *= 2;

            if(child != Children[^1])
            {
                //If the child requesting more depth was not the final element
                //then we must recalculate depths
                PropagateDepth();
            }
        } 
    }
}
