using Empyrean.Game.Combat;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;
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

    public class EndTurnAction : IAIAction
    {
        public float Weight { get; set; }

        public EndTurnAction()
        {
            CalculateWeight();
        }

        public async Task<bool> ActionChosen()
        {
            TileMapManager.Scene.CompleteTurn();
            return true;
        }

        public void CalculateWeight()
        {
            Weight = 1;
        }
    }
    public class SearchForEnemyAction : IAIAction
    {
        public float Weight { get; set; }

        public Unit CurrentUnit;

        public List<InformationMorsel> LastLocationMorsels = new List<InformationMorsel>();

        public SearchForEnemyAction(Unit unit)
        {
            CurrentUnit = unit;
            CalculateWeight();
        }

        public async Task<bool> ActionChosen()
        {
            //Find a morsel that is relatively close by for a living unit and walk to it's last known location.
            //If the AI is within a couple tiles of its last known location then choose a random direction and move there

            //The movement to the last known point doesn't need to be a full movement there, just a best possible path with the energy
            //the unit has. Additionally, if a unit is spotted while moving then the movement should be canceled.
            return false;
        }

        public void CalculateWeight()
        {
            List<UnitTeam> enemyTeams = new List<UnitTeam>();

            bool unitInVision = false;

            foreach(var team in TileMapManager.Scene.ActiveTeams)
            {
                if(team.GetRelation(CurrentUnit.AI.Team) == Relation.Hostile)
                {
                    enemyTeams.Add(team);
                }
            }

            foreach(var enemyTeam in enemyTeams)
            {
                if(TileMapManager.Scene.CombatState.UnitInformation.TryGetValue(enemyTeam, out var morsels))
                {
                    foreach(var morsel in morsels)
                    {
                        if (morsel.Unit.Info.Visible(CurrentUnit.AI.GetTeam()))
                        {
                            unitInVision = true;
                            return;
                        }
                        else
                        {
                            LastLocationMorsels.Add(morsel.ActionMorsel);
                        }
                    }
                }
            }

            if (!unitInVision)
            {
                Weight = 10; //Very high weight to ensure the AI searches when no units are visible
            }
        }
    }

    public static class AIBrain
    {
        public static async void TakeAITurn(Unit unit)
        {
            while (await MakeNextUnitAction(unit))
            {

            }

            Console.WriteLine($"Unit {unit.Name} ended turn with {unit.GetResF(ResF.MovementEnergy)} energy");

            TileMapManager.Scene.CompleteTurn();
        }

        /// <summary>
        /// Determines and enacts the unit's next action. <para/>
        /// Returns true if an action was enacted. False if no actions were deemed feasible
        /// and the turn should be ended.
        /// </summary>
        public static async Task<bool> MakeNextUnitAction(Unit unit)
        {
            List<IAIAction> aiActions = new List<IAIAction>();

            aiActions.Add(new SearchForEnemyAction(unit));

            //check base actions
            // - Retreat from enemy
            // - Search for enemy (implement in first batch)
            // - Surrender
            // - Group with allies
            // - Make space
            // - Hunker down


            //check abilities
            foreach (var ability in unit.Info.Abilities)
            {
                if (!ability.HasMovementParams)
                {
                    //hook into some template weight gathering function of the ability
                    List<IAIAction> desiredTargets = ability.GetDesiredTargets();

                    foreach(var target in desiredTargets)
                    {
                        if(target.Weight > 1)
                        {
                            aiActions.Add(target);
                        }
                    }
                }
            }

            aiActions.Sort((a, b) => b.Weight.CompareTo(a.Weight));

            foreach(var action in aiActions)
            {
                if (await action.ActionChosen())
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
