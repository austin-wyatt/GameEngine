using Empyrean.Definitions.BlendControls;
using Empyrean.Engine_Classes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Tiles.Meshes;
using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Map
{

    [XmlInclude(typeof(BlendPath))]
    [XmlInclude(typeof(ImageBlendControl))]
    [Serializable]
    public abstract class BlendControl
    {
        public FeaturePoint Origin;
        public string Name;

        public TileType Red = TileType.None;
        public TileType Green = TileType.None;
        public TileType Blue = TileType.None;

        /// <summary>
        /// The order in which this control should be loaded <para/>
        /// The higher the load order the more "on top" the control will be
        /// compared to other controls (since it will be loaded last).
        /// </summary>
        public int LoadOrder;

        public BlendControl() { }

        public abstract void ApplyControl();


        //protected static float COLOR_OFFSET = 0.5f;
        protected static float COLOR_OFFSET = 1;

        protected static float GetOffsetForPaletteLocation(PaletteLocation paletteLocation)
        {
            switch (paletteLocation)
            {
                //case PaletteLocation.Red1:
                //case PaletteLocation.Green1:
                //case PaletteLocation.Blue1:
                //    return 0;
                //case PaletteLocation.Red2:
                //case PaletteLocation.Green2:
                //case PaletteLocation.Blue2:
                //    return 0.5f;
                default:
                    return 0;
            }
        }

        private static int GetBitOffsetForPaletteLocation(PaletteLocation paletteLocation)
        {
            switch (paletteLocation)
            {
                //case PaletteLocation.Red1:
                //case PaletteLocation.Green1:
                //case PaletteLocation.Blue1:
                //    return 0;
                //case PaletteLocation.Red2:
                //case PaletteLocation.Green2:
                //case PaletteLocation.Blue2:
                //    return 4;
                default:
                    return 0;
            }
        }


        private static int FIRST_HALF = 15;
        private static int SECOND_HALF = 240;

        protected static int GetCombinedColor(PaletteLocation loc, int newColor, int oldColor)
        {
            int newOffset = GetBitOffsetForPaletteLocation(loc);
            int oldOffset = 4 - newOffset;

            newColor /= 2;
            newColor <<= newOffset;

            oldColor &= newOffset == 1 ? ~FIRST_HALF : ~SECOND_HALF;

            oldColor += newColor;

            return oldColor;
        }

        protected static void SetColorByLocation(ref int r, ref int g, ref int b, int color, PaletteLocation location)
        {
            switch (location)
            {
                case PaletteLocation.Red1:
                    r = color;
                    return;
                case PaletteLocation.Green1:
                    g = color;
                    return;
                case PaletteLocation.Blue1:
                    b = color;
                    return;
            }
        }
    }
}
