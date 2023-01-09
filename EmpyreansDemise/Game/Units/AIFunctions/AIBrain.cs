using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Game.Combat;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Empyrean.Game.Units.AIFunctions
{
    public interface IAIAction
    {
        public float Weight { get; set; }

        /// <summary>
        /// Movement and feasibility checks should occur here. <para/>
        /// Returns true if the action was deemed to be feasible and enacted.
        /// </summary>
        public Task<bool> ActionChosen();
        public void CalculateWeight();
    }


    /// <summary>
    /// An object that contains the unioned move tiles of a given target unit and the current AI <para/>
    /// Specifically checks the ability line tiles for now
    /// </summary>
    public class AvailableMovePaths
    {
        public HashSet<NavTileWithParent> UnionedTiles = new HashSet<NavTileWithParent>();
        public InformationMorsel AssociatedMorsel;

        public Dictionary<int, HashSet<NavTileWithParent>> UnionedTilesByDistanceFromUnit = new Dictionary<int, HashSet<NavTileWithParent>>();

        public AvailableMovePaths(InformationMorsel morsel, HashSet<NavTileWithParent> allMoves)
        {
            AssociatedMorsel = morsel;

            if(TileMapManager.Scene.CombatState.UnimpededUnitSightlines.TryGetValue(morsel.Unit, out List<LineOfTiles> foundTiles))
            {
                NavTileWithParent tempNavTile = new NavTileWithParent();
                tempNavTile.NavTile = new NavTile();

                for(int i = 0; i < foundTiles.Count; i++)
                {
                    for(int j = foundTiles[i].AbilityLineHeightIndex; j < foundTiles[i].Tiles.Count; j++)
                    {
                        tempNavTile.NavTile.Tile = foundTiles[i].Tiles[j];
                        if (allMoves.TryGetValue(tempNavTile, out NavTileWithParent foundNavTile))
                        {
                            UnionedTiles.Add(foundNavTile);

                            int distance = CubeMethods.GetDistanceBetweenPoints(morsel.Unit.Info.TileMapPosition, foundNavTile.NavTile.Tile);
                            HashSet<NavTileWithParent> navTiles;
                            if(UnionedTilesByDistanceFromUnit.TryGetValue(distance, out navTiles))
                            {

                            }
                            else
                            {
                                navTiles = new HashSet<NavTileWithParent>();
                                UnionedTilesByDistanceFromUnit.Add(distance, navTiles);
                            }

                            navTiles.Add(foundNavTile);
                        }
                    }
                }
            }
        }
    }

    public class AIAction
    {
        public float Weight = 0;

        public Func<Task<bool>> DoAction = null;
        public Func<bool> FeasibilityCheck = null;
    }


    //public class SearchForEnemyAction : IAIAction
    //{
    //    public float Weight { get; set; }

    //    public Unit CurrentUnit;

    //    public List<InformationMorsel> LastLocationMorsels = new List<InformationMorsel>();

    //    public SearchForEnemyAction(Unit unit)
    //    {
    //        CurrentUnit = unit;
    //        CalculateWeight();
    //    }

    //    public async Task<bool> ActionChosen()
    //    {
    //        //Find a morsel that is relatively close by for a living unit and walk to it's last known location.
    //        //If the AI is within a couple tiles of its last known location then choose a random direction and move there

    //        //The movement to the last known point doesn't need to be a full movement there, just a best possible path with the energy
    //        //the unit has. Additionally, if a unit is spotted while moving then the movement should be canceled.
    //        return false;
    //    }

    //    public void CalculateWeight()
    //    {
    //        List<UnitTeam> enemyTeams = new List<UnitTeam>();

    //        bool unitInVision = false;

    //        foreach(var team in TileMapManager.Scene.ActiveTeams)
    //        {
    //            if(team.GetRelation(CurrentUnit.AI.Team) == Relation.Hostile)
    //            {
    //                enemyTeams.Add(team);
    //            }
    //        }

    //        foreach(var enemyTeam in enemyTeams)
    //        {
    //            if(TileMapManager.Scene.CombatState.UnitInformation.TryGetValue(enemyTeam, out var morsels))
    //            {
    //                foreach(var morsel in morsels)
    //                {
    //                    if (morsel.Unit.Info.Visible(CurrentUnit.AI.GetTeam()))
    //                    {
    //                        unitInVision = true;
    //                        return;
    //                    }
    //                    else
    //                    {
    //                        LastLocationMorsels.Add(morsel.ActionMorsel);
    //                    }
    //                }
    //            }
    //        }

    //        if (!unitInVision)
    //        {
    //            Weight = 10; //Very high weight to ensure the AI searches when no units are visible
    //        }
    //    }
    //}

    public static class AIBrain
    {
        public static Stopwatch AITimer = Stopwatch.StartNew();
        public const int AI_THINKING_BUDGET_MS = 200;

        private const int MIN_WAIT_TIME_MS = 10;
        public static async void TakeAITurn(Unit unit)
        {
            AITimer.Restart();

            while (await MakeNextUnitAction(unit))
            {
                int thinkingTime = AI_THINKING_BUDGET_MS - (int)AITimer.ElapsedMilliseconds;

                Console.WriteLine("AI took " + AITimer.Elapsed.TotalMilliseconds + "ms to evaluate move");

                if(thinkingTime > MIN_WAIT_TIME_MS)
                {
                    Thread.Sleep(thinkingTime);
                }



                AITimer.Restart();
            }

            Console.WriteLine($"Unit {unit.Name} ended turn with {unit.GetResF(ResF.MovementEnergy)} energy, " +
                $"{unit.GetResF(ResF.ActionEnergy)} action energy, and {unit.GetResI(ResI.Stamina)} stamina.");

            TileMapManager.Scene.CompleteTurn();
        }

        /// <summary>
        /// Determines and enacts the unit's next action. <para/>
        /// Returns true if an action was enacted. False if no actions were deemed feasible
        /// and the turn should be ended.
        /// </summary>
        public static async Task<bool> MakeNextUnitAction(Unit unit)
        {
            List<AIAction> aiActions = new List<AIAction>();

            //aiActions.Add(new SearchForEnemyAction(unit));

            //check base actions
            // - Retreat from enemy
            // - Search for enemy (implement in first batch)
            // - Surrender
            // - Group with allies
            // - Make space
            // - Hunker down

            HashSet<NavTileWithParent> possibleMovesSet = new HashSet<NavTileWithParent>();
            TileMapManager.NavMesh.NavFloodFill(unit.Info.TileMapPosition.ToFeaturePoint(), unit.Info._movementAbility.NavType, ref possibleMovesSet,
                unit.Info._movementAbility.Range, unit);

            Dictionary<Unit, AvailableMovePaths> availableMovePaths = new Dictionary<Unit, AvailableMovePaths>();

            //Allows each move path to only be generated as needed when an action is potentially being tested instead
            //of generating a move paths for every single morsel 
            AvailableMovePaths getAvailablePathsToUnit(InformationMorsel morsel)
            {
                AvailableMovePaths path;
                if (availableMovePaths.TryGetValue(morsel.Unit, out path))
                {
                    
                }
                else
                {
                    path = new AvailableMovePaths(morsel, possibleMovesSet);
                    availableMovePaths.Add(morsel.Unit, path);
                }

                return path;
            }

            //check abilities
            foreach (var ability in unit.Info.Abilities)
            {
                if (!ability.IsForMovement && ability.CanCast())
                {
                    List<AIAction> desiredTargets = ability.AITargetSelection.GetPotentialActions(getAvailablePathsToUnit);

                    foreach(var target in desiredTargets)
                    {
                        if(target.Weight > 1)
                        {
                            aiActions.Add(target);
                        }
                    }
                }
            }

            foreach (var item in unit.Info.Equipment.EquippedItems.Values)
            {
                if (!item.ItemAbility.IsForMovement && item.ItemAbility.CanCast())
                {
                    List<AIAction> desiredTargets = item.ItemAbility.AITargetSelection.GetPotentialActions(getAvailablePathsToUnit);

                    foreach (var target in desiredTargets)
                    {
                        if (target.Weight > 1)
                        {
                            aiActions.Add(target);
                        }
                    }
                }
            }

            aiActions.Sort((a, b) => b.Weight.CompareTo(a.Weight));





            foreach(var action in aiActions)
            {
                if (await action.DoAction.Invoke())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Placeholder, this should create a list of movement actions (movement actions
        /// being a sequence of actions such as "use movement ability to tile X,Y then 
        /// use teleport ability from X,Y to tile A,B"). <para/>
        /// If the actions are feasible (ie they pass a feasibility check stage) 
        /// then the movements will be enacted and true will be returned.
        /// If no movement is found then false will be returned.
        /// </summary>
        public static async Task<bool> MovementCheck(Unit unit, FeaturePoint destination, Abilities.GroupedMovementParams movementParams)
        {
            if(TileMapManager.NavMesh.GetPathToPoint(unit.Info.TileMapPosition.ToFeaturePoint(), destination, NavType.Base,
                out var tileList, unit.Info._movementAbility.GetRange(), considerCaution: true))
            {
                unit.Info._movementAbility.CurrentTiles = tileList;
                unit.Info._movementAbility.EnactEffect();


                return await unit.Info._movementAbility.EffectEndedAsync.Task;
            }

            return false;
        }
    }
}
