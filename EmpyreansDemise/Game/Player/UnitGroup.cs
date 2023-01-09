using Empyrean.Definitions.Buffs;
using Empyrean.Engine_Classes;
using Empyrean.Game.Abilities;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Player
{
    /// <summary>
    /// Represents a group of units. This overrides their abilities and forces them to move together.
    /// </summary>
    public class UnitGroup
    {
        public bool PlayerGroup = false;

        public List<Unit> Units = new List<Unit>();

        public List<Ability> GroupAbilities = new List<Ability>();

        public Unit Leader;

        public Vector4 GroupColor = Vector4.One;

        private static List<Vector4> PossibleGroupColors = new List<Vector4>()
        {
            _Colors.Tan,
            _Colors.LessAggressiveRed,
            _Colors.Green,
            _Colors.Blue,
            _Colors.Purple
        };

        private static HashSet<Vector4> UsedGroupColors = new HashSet<Vector4>();

        public UnitGroup(List<Unit> units)
        {
            CreateGroup(units);
        }

        private List<Buff> _groupedDebuffs = new List<Buff>();

        public void CreateGroup(List<Unit> units)
        {
            Units = new List<Unit>(units);
            
            _groupedDebuffs.Clear();

            for (int i = 0; i < units.Count; i++)
            {
                _groupedDebuffs.Add(new GroupedDebuff());

                units[i].Info.BuffManager.AddBuff(_groupedDebuffs[i]);

                units[i].Info.Group = this;
            }

            Leader = units[0];

            for(int i = 0; i < PossibleGroupColors.Count; i++)
            {
                if (!UsedGroupColors.Contains(PossibleGroupColors[i]))
                {
                    GroupColor = PossibleGroupColors[i];
                    UsedGroupColors.Add(GroupColor);
                    break;
                }
            }

            TileMapManager.Scene.UpdateUnitStatusBars();
        }

        public void DissolveGroup()
        {
            for (int i = 0; i < Units.Count; i++)
            {
                Units[i].Info.BuffManager.RemoveBuff(_groupedDebuffs[i]);

                Units[i].Info.Group = null;
            }

            _groupedDebuffs.Clear();

            if(GroupColor != Vector4.One)
            {
                UsedGroupColors.Remove(GroupColor);
            }

            TileMapManager.Scene.UpdateUnitStatusBars();
        }
    }
}
