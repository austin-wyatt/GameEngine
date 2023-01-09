using Empyrean.Engine_Classes;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Movement
{
    public abstract class MoveAnimation
    {
        public MoveContract MoveContract;
        public MoveAnimation(MoveContract contract)
        {
            MoveContract = contract;
        }

        public abstract Task EnactMovement(Unit objToMove);
    }
}
