using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities
{
    public class GroupedMovementParams
    {
        public List<MovementParams> Params;

        public static GroupedMovementParams Empty = new GroupedMovementParams()
        {
            Params = new List<MovementParams>()
        };
    }

    public struct MovementParams
    {
        public Ability Ability;
        public List<Vector3i> MovementOffsets;
        public bool IsTeleport;
        public bool NeedsVision;

        /// <summary>
        /// A value [0:1] dictating how willing the unit is to use this ability in general movement
        /// </summary>
        public float Willingness;

        public int MaxUses;
        public float EnergyCost;
        public float ActionCost;

        public override bool Equals(object obj)
        {
            return obj is MovementParams @params &&
                   EqualityComparer<Ability>.Default.Equals(Ability, @params.Ability);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Ability);
        }
    }

    public interface IMovementAbility
    {
        /// <summary>
        /// Builds the MovementParams that the nav mesh needs to work the ability's movement
        /// into the pathfinding operation.
        /// </summary>
        public MovementParams GetMovementParams();
    }
}
