using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Movement
{
    public enum MoveIntent
    {
        Intentional,
        Forced
    }

    public class MoveContract
    {
        public List<MoveNode> Moves = new List<MoveNode>();
        public MoveIntent Intent;

        public bool Viable = false;

        public MoveAnimation MoveAnimation;
    }
}
