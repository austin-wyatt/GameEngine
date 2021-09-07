using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public UnitAI(Unit unit) 
        {
            _unit = unit;
            Dispositions = new Dispositions(_unit);
        }

        public void SetTeam(UnitTeam team) 
        {
            Team = team;
        }

        public void TakeTurn() 
        {
            //List<BaseTile> tilesInVision = Scene.GetTeamVision(_unit.AI.Team);

            _unit.Info.Energy = _unit.Info.CurrentEnergy;

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
                Task.Run(() => GetAction().EnactEffect());
                //GetAction().EnactEffect();
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

            float cost = 0;
            for(int i = 0; i < tiles.Count; i++)
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

            UnitAIAction tempAction;
            foreach (AIAction action in Enum.GetValues(typeof(AIAction)))
            {
                UnitAIAction unitAction = new UnitAIAction(_unit);

                Dispositions.DispositionList.ForEach(disp =>
                {
                    tempAction = disp.GetAction(action);

                    if (tempAction != null)
                    {
                        actions.Add(tempAction);
                    }
                });
            }


            actions.ForEach(a =>
            {
                if (a.Weight > selectedAction.Weight) 
                {
                    selectedAction = a;
                }
            });

            Console.WriteLine($"{_unit.Name} chose action {selectedAction.GetType().Name} with weight {selectedAction.Weight}. {_unit.Info.Energy} Energy remaining.");

            return selectedAction;
        }

        private static Dictionary<int, Relation> TeamRelations = new Dictionary<int, Relation>();
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

        public Dispositions(Unit unit) 
        {

        }

        public void Add<T>(T disposition) where T : Disposition
        {
            DispositionList.Add(disposition);
        }
    }

    public class Disposition 
    {
        public float Weight = 1;
        protected Unit _unit;

        protected CombatScene Scene => _unit.Scene;
        protected TileMap Map => _unit.GetTileMap();

        public Disposition(Unit unit) 
        {
            _unit = unit;
        }

        public virtual UnitAIAction GetAction(AIAction action) 
        {
            return null;
        }

        public static Ability GetBestAbility(List<Ability> abilities, Unit unit)
        {
            Ability ability = null;

            abilities.ForEach(a =>
            {
                if (ability == null && a.GetEnergyCost() < unit.Info.Energy)
                    ability = a;

                if (ability != null && a.Grade > ability.Grade && a.GetEnergyCost() <= unit.Info.Energy)
                {
                    ability = a;
                }
            });

            return ability;
        }

        public enum CheckEnum
        {
            NotSet,
            True,
            False
        }
        public class UnitSearchParams 
        {
            public CheckEnum Dead = CheckEnum.NotSet;
            public CheckEnum IsHostile = CheckEnum.NotSet;
            public CheckEnum IsFriendly = CheckEnum.NotSet;
            public CheckEnum IsNeutral = CheckEnum.NotSet;

            public UnitSearchParams() { }

            public bool CheckUnit(Unit unit, Unit castingUnit) 
            {
                if (Dead != CheckEnum.NotSet) 
                {
                    if (unit.Info.Dead != Dead.BoolValue()) 
                    {
                        return false;
                    }
                }

                Relation relation = unit.AI.Team.GetRelation(castingUnit.AI.Team);

                if (IsHostile != CheckEnum.NotSet) 
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

                if (IsFriendly != CheckEnum.NotSet)
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

                if (IsNeutral != CheckEnum.NotSet)
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

                return true;
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
    }

    public class UnitAIAction 
    {
        public BaseTile TargetedTile;
        public Unit TargetedUnit;
        public Unit CastingUnit;
        public Ability Ability;

        public float Weight = 0;

        protected CombatScene Scene => CastingUnit.Scene;
        protected TileMap Map => CastingUnit.GetTileMap();
        protected TilePoint TilePosition => CastingUnit.Info.TileMapPosition.TilePoint;
        protected BaseTile Tile => CastingUnit.Info.TileMapPosition;

        public UnitAIAction(Unit castingUnit, Ability ability = null, BaseTile tile = null, Unit unit = null) 
        {
            CastingUnit = castingUnit;

            TargetedTile = tile;
            TargetedUnit = unit;

            Ability = ability;
        }
        public virtual void EnactEffect() 
        {
            Scene.CompleteTurn();
        }
    }
}
