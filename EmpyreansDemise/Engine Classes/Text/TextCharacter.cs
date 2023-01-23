using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Text
{
    public class TextCharacter : Transformations3D
    {
        public Glyph Glyph;

        public Vector4 Color = new Vector4(1);

        public Vector3 CharPosition = new Vector3();

        public TextCharacter(Glyph glyph)
        {
            Glyph = glyph;
        }

        public void SetCharacter(int newCharacter, FontInfo font)
        {
            Glyph = GlyphLoader.GetGlyph(newCharacter, font);
        }

        public UIDimensions GetDimensions()
        {
            var dim = new UIDimensions(WindowConstants.ConvertGlobalToScreenSpaceCoordinates(Glyph.Size));

            dim.X *= CurrentScale.X;
            dim.Y *= CurrentScale.Y;

            return dim;
        }

        public virtual UIDimensions GetAnchorOffset(UIAnchorPosition anchorPosition)
        {
            UIDimensions dimensions = GetDimensions();
            UIDimensions returnDim = new UIDimensions();

            switch (anchorPosition)
            {
                case UIAnchorPosition.TopCenter:
                    returnDim.Y -= dimensions.Y / 2;
                    break;
                case UIAnchorPosition.TopLeft:
                    returnDim.Y -= dimensions.Y / 2;
                    returnDim.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.TopRight:
                    returnDim.Y -= dimensions.Y / 2;
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.LeftCenter:
                    returnDim.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.RightCenter:
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.BottomCenter:
                    returnDim.Y += dimensions.Y / 2;
                    break;
                case UIAnchorPosition.BottomLeft:
                    returnDim.Y += dimensions.Y / 2;
                    returnDim.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.BottomRight:
                    returnDim.Y += dimensions.Y / 2;
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.Center:
                default:
                    break;
            }

            return returnDim;
        }

        public virtual void SetPositionFromAnchor(Vector3 position, UIAnchorPosition anchor = UIAnchorPosition.Center)
        {
            UIDimensions anchorOffset = GetAnchorOffset(anchor);

            SetPosition(position - anchorOffset);
        }
        /// <summary>
        /// Shorthand for SetPositionFromAnchor
        /// </summary>
        public void SAP(Vector3 position, UIAnchorPosition anchor = UIAnchorPosition.Center)
        {
            SetPositionFromAnchor(position, anchor);
        }

        public override void SetPosition(Vector3 position)
        {
            CharPosition = position;

            //screen space
            Vector3 bottomLeft = GetAnchorPosition(UIAnchorPosition.BottomLeft, position);

            //global
            bottomLeft = WindowConstants.ConvertScreenSpaceToGlobalCoordinates(bottomLeft);

            float floorDif = bottomLeft.X - (int)bottomLeft.X;

            const float fiveOverSix = 5f / 6;
            const float oneOverSix = 1f / 6;
            const float oneOverThree = 1f / 3;
            const float twoOverThree = 2f / 3;

            //if (floorDif >= 0.5f)
            //{
            //    //if (floorDif >= fiveOverSix)
            //    //{
            //    //    bottomLeft.X = (int)bottomLeft.X + 1;
            //    //}
            //    //else
            //    //{
            //    //    bottomLeft.X = (int)bottomLeft.X + twoOverThree;
            //    //}

            //    bottomLeft.X = (int)bottomLeft.X + 1;
            //}
            //else
            //{
            //    //if (floorDif <= oneOverSix)
            //    //{
            //    //    bottomLeft.X = (int)bottomLeft.X;
            //    //}
            //    //else
            //    //{
            //    //    bottomLeft.X = (int)bottomLeft.X + oneOverThree;
            //    //}

            //    bottomLeft.X = (int)bottomLeft.X;
            //}

            bottomLeft.X = (int)bottomLeft.X;

            floorDif = bottomLeft.Y - (int)bottomLeft.Y;

            bottomLeft.Y = floorDif > 0.5f ? (int)bottomLeft.Y + 1 : (int)bottomLeft.Y;

            //screen space
            bottomLeft = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(bottomLeft);

            var dim = GetAnchorOffset(UIAnchorPosition.BottomLeft);
            bottomLeft.X -= dim.X;
            bottomLeft.Y -= dim.Y;

            base.SetPosition(bottomLeft);
        }

        /// <summary>
        /// Shorthand for GetAnchorPosition
        /// </summary>
        public Vector3 GAP(UIAnchorPosition anchorPosition)
        {
            return GetAnchorPosition(anchorPosition);
        }

        /// <summary>
        /// Shorthand for GetAnchorPosition
        /// </summary>
        public Vector3 GAP(UIAnchorPosition anchorPosition, Vector3 position)
        {
            return GetAnchorPosition(anchorPosition, position);
        }

        public Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition)
        {
            return GetAnchorPosition(anchorPosition, CharPosition);
        }
        public virtual Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition, Vector3 position)
        {
            UIDimensions dimensions = GetDimensions();
            Vector3 anchorPos = new Vector3(position);

            switch (anchorPosition)
            {
                case UIAnchorPosition.TopCenter:
                    anchorPos.Y -= dimensions.Y / 2;
                    break;
                case UIAnchorPosition.TopLeft:
                    anchorPos.Y -= dimensions.Y / 2;
                    anchorPos.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.TopRight:
                    anchorPos.Y -= dimensions.Y / 2;
                    anchorPos.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.LeftCenter:
                    anchorPos.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.RightCenter:
                    anchorPos.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.BottomCenter:
                    anchorPos.Y += dimensions.Y / 2;
                    break;
                case UIAnchorPosition.BottomLeft:
                    anchorPos.Y += dimensions.Y / 2;
                    anchorPos.X -= dimensions.X / 2;
                    break;
                case UIAnchorPosition.BottomRight:
                    anchorPos.Y += dimensions.Y / 2;
                    anchorPos.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.Center:
                default:
                    break;
            }

            return anchorPos;
        }

        public float NextCharXPosition()
        {
            Vector3 leftCenter = GAP(UIAnchorPosition.LeftCenter);

            float screenAdvance = (float)Glyph.Advance / WindowConstants.ClientSize.Y *
                    WindowConstants.ScreenUnits.Y * CurrentScale.X / WindowConstants.AspectRatio;

            return leftCenter.X + screenAdvance;
        }
    }
}
