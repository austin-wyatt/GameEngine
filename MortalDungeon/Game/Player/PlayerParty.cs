using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Abilities;
using Empyrean.Game.Entities;
using Empyrean.Game.Items;
using Empyrean.Game.Map;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Empyrean.Game.Player
{

    public static class PlayerParty
    {
        public static HashSet<Unit> UnitsInParty = new HashSet<Unit>();
        
        public static Unit PrimaryUnit;

        public static bool Grouped = false; //Add to unit save info

        public static List<int> UnitCreationList = new List<int>() { 5, 5, 5 };

        public static CombatScene Scene;

        public static Inventory Inventory = new Inventory();



        /// <summary>
        /// Checks whether the units can be grouped.
        /// </summary>
        public static bool CanGroupUnits()
        {
            if (Scene.InCombat)
            {
                return false;
            }

            //TODO, check unit proximity from primary unit

            return true;
        }


        public static void InitializeParty()
        {
            if (UnitsInParty.Count > 0)
                return;

            int permanentID = int.MinValue;
            foreach(var item in UnitCreationList)
            {
                var creationInfo = UnitInfoBlockManager.GetUnit(item);

                var unit = creationInfo.CreateUnit(Scene, firstLoad: true);

                unit.Info.PartyMember = true;

                unit.SetPermanentId(permanentID);
                permanentID++;

                EntityManager.AddEntity(new Entity(unit));

                UnitsInParty.Add(unit);
            }
        }

        public static bool PlaceUnits(Tile center, int aoe = 5)
        {
            //get flood filled tile aoe around the center point and place all units randomly around there.
            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(center, aoe)
            {
                Units = Scene._units,
            };

            List<Tile> validTiles = center.TileMap.FindValidTilesInRadius(param);

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
                    PrimaryUnit.SetPositionOffset(validTiles[tileIndex++].Position);
                    PrimaryUnit.SetTileMapPosition(validTiles[tileIndex++]);
                }
            }

            foreach(var unit in UnitsInParty)
            {
                if(unit != PrimaryUnit)
                {
                    if(!unit.EntityHandle.Loaded)
                    {
                        EntityManager.LoadEntity(unit.EntityHandle, validTiles[tileIndex++].ToFeaturePoint());
                    }
                    else
                    {
                        unit.SetPositionOffset(validTiles[tileIndex++].Position);
                        unit.SetTileMapPosition(validTiles[tileIndex++]);
                    }
                }
            }

            return true;
        }

        public static void EnterCombat()
        {
            VisionManager.SetRevealAll(false);
        }

        public static void ExitCombat()
        {
            VisionManager.SetRevealAll(true);
        }

        /// <summary>
        /// Checks whether a member of the player party will be unloaded by a tile map load
        /// </summary>
        public static bool CheckPartyMemberWillBeUnloaded()
        {
            //add check for whether any party member will be unloaded

            return false;
        }
    }
}
