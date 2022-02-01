using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.Items;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Player
{

    public static class PlayerParty
    {
        public static List<Unit> UnitsInParty = new List<Unit>();
        
        public static Unit PrimaryUnit;

        public static bool Grouped = false; //Add to unit save info

        public static List<int> UnitCreationList = new List<int>() { 5, 5, 5, 5, 5 };

        public static CombatScene Scene;

        public static Inventory Inventory = new Inventory();

        /// <summary>
        /// Attempts to group the units in the UnitsInParty list. <para/>
        /// If a unit is too far from the designated primary unit or one of the units is in combat the grouping fails. <para/>
        /// Returns whether grouping the units succeeded or not.
        /// </summary>
        public static bool GroupUnits(Unit primaryUnit)
        {
            if (Scene.InCombat || primaryUnit == null)
                return false;

            PrimaryUnit = primaryUnit;

            foreach(var unit in UnitsInParty)
            {
                if(unit != PrimaryUnit)
                {
                    EntityManager.UnloadEntity(unit.EntityHandle);
                }
            }

            PrimaryUnit.StatusBarComp._nameBox.SetColor(_Colors.Blue);

            return true;
        }

        /// <summary>
        /// Takes the current unit group and attemps to ungroup them either automatically or manually <para/>
        /// Returns whether ungrouping the units succeeded or not.
        /// </summary>
        public static bool UngroupUnits(BaseTile targetTile, bool autoPlace = false)
        {
            if (autoPlace)
            {
                return PlaceUnits(targetTile);
            }
            else
            {
                GenericSelectGround ability = new GenericSelectGround(PrimaryUnit, 4) { MustCast = true, ActionCost = 0, MaxCharges = 0, EnergyCost = 0 };

                List<Unit> units = new List<Unit>(UnitsInParty);

                if(targetTile.UnitOnTile == PrimaryUnit && PrimaryUnit != null)
                {
                    units.Remove(PrimaryUnit);
                }

                int currUnit = 0;

                void GroundSelectUnitAction(BaseTile tile)
                {
                    void deselectAbility()
                    {
                        ability.OnGroundSelected = null;
                        ability.MustCast = false;
                        Scene.DeselectAbility();
                    }

                    

                    EntityManager.LoadEntity(units[currUnit].EntityHandle, tile.ToFeaturePoint());

                    currUnit++;

                    //if (currUnit == units.Count)
                    //{
                    //    deselectAbility();

                    //    PrimaryUnit.StatusBarComp._nameBox.SetColor(_Colors.Black);
                    //    PrimaryUnit = null;
                    //    return;
                    //}

                    if (currUnit < units.Count)
                    {
                        ability.OnGroundSelected = (tile) => Task.Run(() => GroundSelectUnitAction(tile));
                        //ability.OnGroundSelected = (tile) => GroundSelectUnitAction(tile);
                        ability.MustCast = false;
                        Scene.DeselectAbility();
                        ability.MustCast = true;
                        Scene.SelectAbility(ability, PrimaryUnit);

                        if (ability.AffectedTiles.Count == 0)
                        {
                            ability.MustCast = false;
                            Scene.DeselectAbility();

                            return;
                        }
                    }
                    else
                    {
                        PrimaryUnit.StatusBarComp._nameBox.SetColor(_Colors.Black);
                        PrimaryUnit = null;
                    }
                }

                ability.OnGroundSelected = (tile) => Task.Run(() => GroundSelectUnitAction(tile));
                //ability.OnGroundSelected = (tile) => GroundSelectUnitAction(tile);

                Scene.DeselectAbility();
                Scene.SetAbilityInProgress(false);
                Scene.SelectAbility(ability, PrimaryUnit);
                Scene.SetAbilityInProgress(true);

                if (ability.AffectedTiles.Count < units.Count - 1)
                {
                    ability.MustCast = false;
                    Scene.DeselectAbility();
                    Scene.SetAbilityInProgress(false);
                    
                    return false;
                }
            }

            return true;
        }

        public static void InitializeParty()
        {
            if (UnitsInParty.Count > 0)
                return;

            foreach(var item in UnitCreationList)
            {
                var creationInfo = UnitCreationInfoSerializer.LoadUnitCreationInfoFromFile(item);

                var unit = creationInfo.CreateUnit(Scene);

                unit.Info.PartyMember = true;

                EntityManager.AddEntity(new Entity(unit));

                UnitsInParty.Add(unit);
            }
        }

        public static bool PlaceUnits(BaseTile center, int aoe = 5)
        {
            //get flood filled tile aoe around the center point and place all units randomly around there.
            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(center, aoe)
            {
                TraversableTypes = new List<TileClassification>() { TileClassification.Ground },
                Units = Scene._units,
            };

            List<BaseTile> validTiles = center.TileMap.FindValidTilesInRadius(param);

            if(validTiles.Count < UnitsInParty.Count)
            {
                return false;
            }

            int tileIndex = 0;

            if(PrimaryUnit != null)
            {
                if (!PrimaryUnit.EntityHandle.Loaded)
                {
                    EntityManager.LoadEntity(PrimaryUnit.EntityHandle, validTiles[tileIndex++].ToFeaturePoint());
                }
                else
                {
                    PrimaryUnit.SetTileMapPosition(validTiles[tileIndex++]);
                    PrimaryUnit.SetPosition(PrimaryUnit.Info.TileMapPosition.Position + PrimaryUnit.TileOffset);
                }
            }

            for(int i = 0; i < UnitsInParty.Count; i++)
            {
                if(UnitsInParty[i] != PrimaryUnit)
                {
                    if(!UnitsInParty[i].EntityHandle.Loaded)
                    {
                        EntityManager.LoadEntity(UnitsInParty[i].EntityHandle, validTiles[tileIndex++].ToFeaturePoint());
                    }
                    else
                    {
                        UnitsInParty[i].SetTileMapPosition(validTiles[tileIndex++]);
                        UnitsInParty[i].SetPosition(UnitsInParty[i].Info.TileMapPosition.Position + UnitsInParty[i].TileOffset);
                    }
                }
            }

            return true;
        }
    }
}
