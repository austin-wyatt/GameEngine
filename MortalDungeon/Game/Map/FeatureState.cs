using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Map
{
    public static class FeatureState
    {
        //the int component is the feature's ID from the HashCoordinates function
        public static Dictionary<int, FeatureStateData> States = new Dictionary<int, FeatureStateData>(); 
    }

    public struct FeatureStateData
    {
        public bool Visited;
        public bool Cleared;
    }
}
