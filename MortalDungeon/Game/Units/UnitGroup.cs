using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Units
{
    internal class UnitGroup
    {
        internal Unit PrimaryUnit;
        internal List<Unit> SecondaryUnitsInGroup = new List<Unit>();

        internal CombatScene Scene;

        internal UnitGroup(CombatScene scene) 
        {
            Scene = scene;
        }

        internal void SetPrimaryUnit(Unit unit) 
        {
            PrimaryUnit = unit;
        }

        internal void AddUnitToGroup(Unit unit) 
        {
            if ((PrimaryUnit == null && !Scene.InCombat) || unit == PrimaryUnit)
                return;

            SecondaryUnitsInGroup.Add(unit);

            unit.RemoveFromTile();

            if(unit.StatusBarComp != null)
                unit.StatusBarComp.SetRender(false);

            Scene.RemoveUnit(unit, true);
            Scene.DecollateUnit(unit);
        }

        internal void DissolveGroup(bool force = false, Action onGroupDissolved = null) 
        {
            GenericSelectGround ability = new GenericSelectGround(PrimaryUnit, 4) { MustCast = force, ActionCost = 0, MaxCharges = 0, EnergyCost = 0 };


            int currUnit = SecondaryUnitsInGroup.Count - 1;

            void GroundSelectUnitAction(BaseTile tile) 
            {
                Scene._units.AddImmediate(SecondaryUnitsInGroup[currUnit]);

                SecondaryUnitsInGroup[currUnit].SetPosition(tile.Position + SecondaryUnitsInGroup[currUnit].TileOffset);
                SecondaryUnitsInGroup[currUnit].SetTileMapPosition(tile);

                if (SecondaryUnitsInGroup[currUnit].StatusBarComp != null)
                    SecondaryUnitsInGroup[currUnit].StatusBarComp.SetRender(true);

                SecondaryUnitsInGroup.RemoveAt(currUnit);
                currUnit--;

                if(currUnit >= 0) 
                {
                    ability.OnGroundSelected = (tile) => Task.Run(() => GroundSelectUnitAction(tile));
                    ability.MustCast = false;
                    Scene.DeselectAbility();
                    ability.MustCast = force;
                    Scene.SelectAbility(ability, PrimaryUnit);

                    if(ability.AffectedTiles.Count == 0) 
                    {
                        ability.MustCast = false;
                        Scene.DeselectAbility();
                    }
                }
                else 
                {
                    ability.OnGroundSelected = null;
                    ability.MustCast = false;
                    Scene.DeselectAbility();

                    onGroupDissolved?.Invoke();
                }
            }

            ability.OnGroundSelected = (tile) => Task.Run(() => GroundSelectUnitAction(tile));

            Scene.DeselectAbility();
            Scene.SetAbilityInProgress(false);
            Scene.SelectAbility(ability, PrimaryUnit);
            Scene.SetAbilityInProgress(true);

            if (ability.AffectedTiles.Count == 0)
            {
                ability.MustCast = false;
                Scene.DeselectAbility();
                Scene.SetAbilityInProgress(false);
            }
        }
    }
}
