using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public interface IHasPosition
    {
        public Vector3 Position { get; set; }
        public void SetPosition(Vector3 position);
    }
}
