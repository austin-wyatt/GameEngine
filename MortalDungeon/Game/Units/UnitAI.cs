using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Units
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


    public class UnitAI
    {
        public UnitTeam Team = UnitTeam.Skeletons;
        public ControlType ControlType = ControlType.Controlled;

        public Dispositions Dispositions;

        private Unit _unit;

        private CombatScene Scene => _unit.Scene;

        private TileMap Map => _unit.GetTileMap();

        private TilePoint TilePosition => _unit.Info.TileMapPosition.TilePoint;

        private BaseTile Tile => _unit.Info.TileMapPosition;

        public bool Fighting = true; //if a unit surrenders they will no longer be considered fighting

        public UnitAI() { }

        public UnitAI(Unit unit) 
        {
            _unit = unit;
            Dispositions = new Dispositions(_unit);
        }


        public void TakeTurn() 
        {
            //List<BaseTile> tilesInVision = Scene.GetTeamVision(_unit.AI.Team);

            if (Scene.CurrentUnit != _unit)
                return;

            foreach (var disp in Dispositions.DispositionList) 
            {
                disp.TurnFatigue = 0;
            }

            _depth = 0;
            BeginNextAction();
        }

        private int _depth = 0;
        public void BeginNextAction() 
        {
            if (_depth > 1000)
            {
                Console.WriteLine("UnitAI infinite loop broken.");
                Scene.CompleteTurn();
            }
            else 
            {
                Task.Run(() =>
                {
                    //Stopwatch timer = new Stopwatch();
                    //timer.Start();
                    GetAction().EnactEffect();

                    //Console.WriteLine($"AI effect completed in {timer.ElapsedMilliseconds}ms");
                });
                _depth++;
            }
        }

        public void EndTurn() 
        {
            new AI.EndTurn(_unit).EnactEffect();
        }


        public float GetPathMovementCost(List<BaseTile> tiles)
        {
            float value = 0;

            for (int i = 1; i < tiles.Count; i++)
            {
                value += tiles[i].Properties.MovementCost;
            }

            return value;
        }

        public List<BaseTile> GetAffordablePath(List<BaseTile> tiles) 
        {
            List<BaseTile> returnList = new List<BaseTile>();

            if (tiles.Count > 0)
                returnList.Add(tiles[0]);

            float cost = 0;
            for(int i = 1; i < tiles.Count; i++)
            {
                if (cost + tiles[i].Properties.MovementCost > _unit.Info.Energy)
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


        public UnitAIAction GetAction()
        {
            Dictionary<AIAction, UnitAIAction> actionValues = new Dictionary<AIAction, UnitAIAction>();

            UnitAIAction selectedAction = new AI.EndTurn(_unit) { Weight = 1 };

            List<UnitAIAction> actions = new List<UnitAIAction> { selectedAction };
            List<Disposition> actionDispositions = new List<Disposition>() { new Disposition(_unit) };

            UnitAIAction tempAction;
            foreach (AIAction action in Enum.GetValues(typeof(AIAction)))
            {
                Dispositions.DispositionList.ForEach(disp =>
                {
                    tempAction = disp.GetAction(action);

                    if (tempAction != null)
                    {
                        tempAction.Weight -= disp.Fatigue;
                        tempAction.Weight -= disp.TurnFatigue;

                        actions.Add(tempAction);
                        actionDispositions.Add(disp);
                    }
                });
            }

            int selectedActionIndex = 0;
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].Weight > selectedAction.Weight) 
                {
                    selectedAction = actions[i];
                    selectedActionIndex = i;
                }
                if (actions[i].Weight == selectedAction.Weight) 
                {
                    if(GlobalRandom.NextFloat() >= 0.5f) 
                    {
                        selectedAction = actions[i];
                        selectedActionIndex = i;
                    }
                }
            }

            Console.WriteLine($"{_unit.Name} chose action {selectedAction.GetType().Name} with weight {selectedAction.Weight}. {_unit.Info.Energy} Movement remaining. {_unit.Info.ActionEnergy} Actions remaining.");


            float fatigueBuildup = GlobalRandom.NextFloat(0.01f, 0.05f);

            Disposition selectedDisposition = actionDispositions[selectedActionIndex];
            selectedDisposition.OnActionSelected(selectedAction.ActionType);

            selectedDisposition.Fatigue += fatigueBuildup;
            var dispHashSet = actionDispositions.ToHashSet();

            dispHashSet.Remove(selectedDisposition);

            foreach (var item in dispHashSet) 
            {
                if (item.Fatigue > 0) 
                {
                    item.Fatigue -= 0.01f;
                }
                if (item.Fatigue < 0) 
                {
                    item.Fatigue = 0;
                }
            }


            return selectedAction;
        }

        private static Dictionary<long, Relation> TeamRelations = new Dictionary<long, Relation>();
        public static void SetTeamRelation(UnitTeam a, UnitTeam b, Relation relation) 
        {
            TeamRelations.Add(a.Hash(b), relation);
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
        public class UnitSearchParams 
        {
            public UnitCheckEnum Dead = UnitCheckEnum.NotSet;
            public UnitCheckEnum IsHostile = UnitCheckEnum.NotSet;
            public UnitCheckEnum IsFriendly = UnitCheckEnum.NotSet;
            public UnitCheckEnum IsNeutral = UnitCheckEnum.NotSet;
            public UnitCheckEnum Self = UnitCheckEnum.NotSet;

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

                Relation relation = unit.AI.Team.GetRelation(castingUnit.AI.Team);
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

                if (!softCheckUsed) return true;
                else return softCheck;
            }

            public static UnitSearchParams _ = new UnitSearchParams();
        }




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
        public BaseTile TargetedTile;
        public Unit TargetedUnit;
        public Unit CastingUnit;
        public Ability Ability;

        public AIAction ActionType;

        public float Weight = 0;

        protected CombatScene Scene => CastingUnit.Scene;
        protected TileMap Map => CastingUnit.GetTileMap();
        protected TilePoint TilePosition => CastingUnit.Info.TileMapPosition.TilePoint;
        protected BaseTile Tile => CastingUnit.Info.TileMapPosition;

        public UnitAIAction(Unit castingUnit, AIAction actionType, Ability ability = null, BaseTile tile = null, Unit unit = null) 
        {
            CastingUnit = castingUnit;

            TargetedTile = tile;
            TargetedUnit = unit;

            Ability = ability;

            ActionType = actionType;
        }
        public virtual void EnactEffect() 
        {
            Scene.CompleteTurn();
        }
    }
}
