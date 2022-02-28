using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Serializers
{
    [Serializable]
    public class FeatureUnit : ISerializable
    {
        public int UnitId;
        public AffectedPoint AffectedPoint;

        public void CompleteDeserialization()
        {
            AffectedPoint.CompleteDeserialization();
        }

        public void PrepareForSerialization()
        {
            AffectedPoint.PrepareForSerialization();
        }
    }
}
