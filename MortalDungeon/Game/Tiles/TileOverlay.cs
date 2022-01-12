using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Tiles
{
    public enum TileOverlayType
    {
        Outline,
        Grass_1,
        Grass_2,
        Grass_3,
    }

    public class TileOverlay
    {
        public TileOverlayType TileOverlayType;
        public float MixPercent;

        public TileOverlay() { }
        public TileOverlay(TileOverlayType type, float mixPercent)
        {
            TileOverlayType = type;
            MixPercent = mixPercent;
        }
    }
}
