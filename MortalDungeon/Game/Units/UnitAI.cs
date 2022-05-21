using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Abilities;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units.AIFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Empyrean.Game.Units
{
    public enum UnitTeam
    {
        Unknown,
        PlayerUnits,
        BadGuys,
        Skeletons,
    }

    public enum Relation 
    {
        Friendly,
        Hostile,
        Neutral
    }

    public enum ControlType 
    {
        Controlled,
        Basic_AI,
    }

    [Serializable]
    public class UnitAI : ISerializable
    {
        public UnitTeam Team = UnitTeam.Unknown;
        public ControlType ControlType = ControlType.Controlled;

        public OverrideContainer<UnitTeam> TeamOverride = new OverrideContainer<UnitTeam>();
        public OverrideContainer<ControlType> ControlTypeOverride = new OverrideContainer<ControlType>();

        //public Dispositions Dispositions;

        [XmlIgnore]
        private Unit _unit;

        [XmlIgnore]
        private CombatScene Scene => _unit.Scene;

        [XmlIgnore]
        private TileMap Map => _unit.GetTileMap();

        [XmlIgnore]
        private TilePoint TilePosition => _unit.Info.TileMapPosition.TilePoint;

        [XmlIgnore]
        private Tile Tile => _unit.Info.TileMapPosition;

        public bool Fighting = true; //if a unit surrenders they will no longer be considered fighting

        public float Bloodthirsty = 0;
        public float Virtuous = 0;
        public float Cowardly = 0;
        public float MovementAversion = 0.2f;

        public Feelings Feelings;

        public UnitAI() { }

        public UnitAI(Unit unit) 
        {
            _unit = unit;
            Feelings = new Feelings(unit);
        }



        public float GetPathMovementCost(List<Tile> tiles)
        {
            float value = 0;

            for (int i = 1; i < tiles.Count; i++)
            {
                value += tiles[i].Properties.MovementCost;
            }

            return value;
        }

        public List<Tile> GetAffordablePath(List<Tile> tiles) 
        {
            List<Tile> returnList = new List<Tile>();

            if (tiles.Count > 0)
                returnList.Add(tiles[0]);

            float cost = 0;
            for(int i = 1; i < tiles.Count; i++)
            {
                if (cost + tiles[i].Properties.MovementCost > _unit.GetResF(ResF.MovementEnergy))
                {
                    return returnList;
                }
                else 
                {
                    cost += tiles[i].Properties.MovementCost;
                    returnList.Add(tiles[i]);
                }
            }

            return returnList;
        }

        public UnitTeam GetTeam()
        {
            if(TeamOverride.TryGetValue(out var team))
            {
                return team;
            }

            return Team;
        }

        public ControlType GetControlType()
        {
            if (ControlTypeOverride.TryGetValue(out var controlType))
            {
                return controlType;
            }

            return ControlType;
        }

        private static Dictionary<long, Relation> TeamRelations = new Dictionary<long, Relation>();
        public static void SetTeamRelation(UnitTeam a, UnitTeam b, Relation relation) 
        {
            TeamRelations.AddOrSet(a.Hash(b), relation);
        }

        public static Relation GetTeamRelation(UnitTeam a, UnitTeam b) 
        {
            if (TeamRelations.TryGetValue(a.Hash(b), out Relation value))
            {
                return value;
            }
            else 
            {
                return Relation.Neutral;
            }
        }

        public static Dictionary<long, Relation> GetTeamRelationsDictionary()
        {
            return TeamRelations;
        }

        public void PrepareForSerialization()
        {

        }

        public void CompleteDeserialization()
        {

        }

        public static void AttachUnitToAI(UnitAI ai, Unit unit)
        {
            ai._unit = unit;
        }
    }


    public enum AIAction 
    {
        MoveCloser,
        MoveFarther,
        

        AttackEnemyMelee,
        AttackEnemyRanged,
        KillEnemy,
        DebuffEnemy,

        HealAlly,
        BuffAlly,

        HoldPosition,
        MoveOutOfVision,
        Hide,

        EndTurn
    }

    /// <summary>
    /// Determines the preferred strategy of the AI controlled unit
    /// </summary>
    public enum AIDisposition
    {
        MeleeDamageDealer,
        Healer,
        Support,
        RangedDamageDealer,
        Coward,
        Flanker,
        Assassin
    }
    public class Dispositions 
    {
        public List<Disposition> DispositionList = new List<Disposition>();

        public Dispositions() { }

        public Dispositions(Unit unit) 
        {

        }

        public void Add<T>(T disposition) where T : Disposition
        {
            DispositionList.Add(disposition);
        }
    }

    public enum UnitCheckEnum
    {
        /// <summary>
        /// Value is not taken into account
        /// </summary>
        NotSet,
        /// <summary>
        /// Value must be true
        /// </summary>
        True,
        /// <summary>
        /// Value must be false
        /// </summary>
        False,
        /// <summary>
        /// At least one soft true or soft false value must match
        /// </summary>
        SoftTrue,
        /// <summary>
        /// At least one soft true or soft false value must match
        /// </summary>
        SoftFalse
    }
    public class Disposition 
    {
        public float Weight = 1;
        /// <summary>
        /// Fatigue makes the AI less likely to take a course of action the more often it is selected
        /// </summary>
        public float Fatigue = 0;

        /// <summary>
        /// Turn fatigue generally builds up faster and will dissaude the AI from repeating the single "optimal" action in a single turn.
        /// </summary>
        public float TurnFatigue = 0;
        protected Unit _unit;

        protected CombatScene Scene => _unit.Scene;
        protected TileMap Map => _unit.GetTileMap();

        public Disposition() { }
        public Disposition(Unit unit) 
        {
            _unit = unit;
        }

        public virtual UnitAIAction GetAction(AIAction action) 
        {
            return null;
        }

        public virtual void OnActionSelected(AIAction action) 
        {

        }

        public static Ability GetBestAbility(List<Ability> abilities, Unit unit)
        {
            Ability ability = null;

            abilities.ForEach(a =>
            {
                if (ability == null && a.CanCast())
                    ability = a;

                if (ability != null && a.Grade > ability.Grade && a.CanCast())
                {
                    ability = a;
                }
            });

            return ability;
        }

        /// <summary>
        /// All hard params must be satisfied and (if present) at least one soft param must be satisfied
        /// </summary>
        




        public static Unit GetClosestUnit(Unit castingUnit, float seekRange = -1, UnitSearchParams searchParams = null)
        {
            Unit unit = null;

            if (searchParams == null) 
            {
                searchParams = UnitSearchParams._;
            }

            castingUnit.Scene._units.ForEach(u =>
            {
                if (searchParams.CheckUnit(u, castingUnit))
                {
                    (float moveCost, int moves) = castingUnit.Info._movementAbility.GetCostToPoint(u.Info.TileMapPosition.TilePoint, seekRange);

                    if (moveCost != 0)
                    {
                        if (unit != null)
                        {
                            (float unitMoveCost, int unitMoves) = castingUnit.Info._movementAbility.GetCostToPoint(unit.Info.TileMapPosition.TilePoint, seekRange);
                            if (unitMoveCost > moveCost && unitMoves != 0)
                            {
                                unit = u;
                            }
                        }
                        else
                        {
                            if (moves > 0)
                            {
                                unit = u;
                            }
                        }
                    }
                }
            });

            return unit;
        }

        public static List<Unit> GetReasonablyCloseUnits(Unit castingUnit, float seekRange = -1, UnitSearchParams searchParams = null)
        {
            List<Unit> units = new List<Unit>();

            if (searchParams == null)
            {
                searchParams = UnitSearchParams._;
            }

            castingUnit.Scene._units.ForEach(u =>
            {
                if (searchParams.CheckUnit(u, castingUnit))
                {
                    (float moveCost, int moves) = castingUnit.Info._movementAbility.GetCostToPoint(u.Info.TileMapPosition.TilePoint, seekRange);

                    if (moves != 0)
                    {
                        units.Add(u);
                    }
                }
            });

            return units;
        }
    }

    public class UnitAIAction 
    {
        public Tile TargetedTile;
        public Unit TargetedUnit;
        public Unit CastingUnit;
        public Ability Ability;

        public AIAction ActionType;

        public float Weight = 0;

        public bool HasAction = false;
        public float PathCost = 0;

        public Action EffectAction = null;

        protected CombatScene Scene => CastingUnit.Scene;
        protected TileMap Map => CastingUnit.GetTileMap();
        protected TilePoint TilePosition => CastingUnit.Info.TileMapPosition.TilePoint;
        protected Tile Tile => CastingUnit.Info.TileMapPosition;

        public UnitAIAction(Unit castingUnit, AIAction actionType, Ability ability = null, Tile tile = null, Unit unit = null) 
        {
            CastingUnit = castingUnit;

            TargetedTile = tile;
            TargetedUnit = unit;

            Ability = ability;

            ActionType = actionType;
        }
        public virtual void EnactEffect() 
        {
            EffectAction?.Invoke();
        }
    }

    public class UnitSearchParams
    {
        public UnitCheckEnum Dead = UnitCheckEnum.NotSet;
        public UnitCheckEnum IsHostile = UnitCheckEnum.NotSet;
        public UnitCheckEnum IsFriendly = UnitCheckEnum.NotSet;
        public UnitCheckEnum IsNeutral = UnitCheckEnum.NotSet;
        public UnitCheckEnum Self = UnitCheckEnum.NotSet;
        public UnitCheckEnum IsControlled = UnitCheckEnum.NotSet;
        public UnitCheckEnum InVision = UnitCheckEnum.NotSet;

        public UnitSearchParams() { }

        public bool CheckUnit(Unit unit, Unit castingUnit)
        {
            bool softCheck = false;

            bool softCheckUsed = false;

            #region Dead Check
            if (Dead != UnitCheckEnum.NotSet && !Dead.IsSoft())
            {
                if (unit.Info.Dead != Dead.BoolValue())
                {
                    return false;
                }
            }
            else if (Dead.IsSoft())
            {
                if (unit.Info.Dead == Dead.BoolValue())
                {
                    softCheck = true;
                }

                softCheckUsed = true;
            }

            Relation relation = unit.AI.GetTeam().GetRelation(castingUnit.AI.GetTeam());
            #endregion

            #region Hostile Check
            if (IsHostile != UnitCheckEnum.NotSet && !IsHostile.IsSoft())
            {
                if (relation == Relation.Hostile && !IsHostile.BoolValue())
                {
                    return false;

                }
                else if (relation != Relation.Hostile && IsHostile.BoolValue())
                {
                    return false;
                }
            }
            else if (IsHostile.IsSoft())
            {
                if (relation == Relation.Hostile && IsHostile.BoolValue())
                {
                    softCheck = true;
                }
                else if (relation != Relation.Hostile && !IsHostile.BoolValue())
                {
                    softCheck = true;
                }

                softCheckUsed = true;
            }
            #endregion

            #region Friendly Check
            if (IsFriendly != UnitCheckEnum.NotSet && !IsFriendly.IsSoft())
            {
                if (relation == Relation.Friendly && !IsFriendly.BoolValue())
                {
                    return false;
                }
                else if (relation != Relation.Friendly && IsFriendly.BoolValue())
                {
                    return false;
                }
            }
            else if (IsFriendly.IsSoft())
            {
                if (relation == Relation.Friendly && IsFriendly.BoolValue())
                {
                    softCheck = true;
                }
                else if (relation != Relation.Friendly && !IsFriendly.BoolValue())
                {
                    softCheck = true;
                }

                softCheckUsed = true;
            }
            #endregion

            #region Neutral Check
            if (IsNeutral != UnitCheckEnum.NotSet && !IsNeutral.IsSoft())
            {
                if (relation == Relation.Neutral && !IsNeutral.BoolValue())
                {
                    return false;
                }
                else if (relation != Relation.Neutral && IsNeutral.BoolValue())
                {
                    return false;
                }
            }
            else if (IsNeutral.IsSoft())
            {
                if (relation == Relation.Neutral && IsNeutral.BoolValue())
                {
                    softCheck = true;
                }
                else if (relation != Relation.Neutral && !IsNeutral.BoolValue())
                {
                    softCheck = true;
                }

                softCheckUsed = true;
            }
            #endregion

            #region Self Check
            if (Self != UnitCheckEnum.NotSet && !Self.IsSoft())
            {
                if (unit == castingUnit != Self.BoolValue())
                {
                    return false;
                }
            }
            else if (Self.IsSoft())
            {
                if (unit == castingUnit == Self.BoolValue())
                {
                    softCheck = true;
                }

                softCheckUsed = true;
            }
            #endregion

            #region Controlled Check
            if (IsControlled != UnitCheckEnum.NotSet && !IsControlled.IsSoft())
            {
                if (unit.AI.GetControlType() == ControlType.Controlled != IsControlled.BoolValue())
                {
                    return false;
                }
            }
            else if (IsControlled.IsSoft())
            {
                if (unit.AI.GetControlType() == ControlType.Controlled == IsControlled.BoolValue())
                {
                    softCheck = true;
                }

                softCheckUsed = true;
            }
            #endregion

            #region Vision Check
            if (InVision != UnitCheckEnum.NotSet && !InVision.IsSoft())
            {
                if (unit.Info.Visible(castingUnit.AI.GetTeam()) != InVision.BoolValue())
                {
                    return false;
                }
            }
            else if (InVision.IsSoft())
            {
                if (unit.Info.Visible(castingUnit.AI.GetTeam()) == InVision.BoolValue())
                {
                    softCheck = true;
                }

                softCheckUsed = true;
            }
            #endregion

            if (!softCheckUsed) return true;
            else return softCheck;
        }

        public static UnitSearchParams _ = new UnitSearchParams();
    }
}
