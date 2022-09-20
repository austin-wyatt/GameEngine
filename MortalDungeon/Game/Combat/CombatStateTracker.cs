using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Combat
{
    public enum MorselType
    {
        All,
        Action,
        Turn
    }

    public class CombatStateTracker
    {
        public Dictionary<UnitTeam, HashSet<UnitMorsels>> UnitInformation = new Dictionary<UnitTeam, HashSet<UnitMorsels>>();

        public static CombatScene Scene;

        public Dictionary<Unit, List<LineOfTiles>> UnimpededUnitSightlines = new Dictionary<Unit, List<LineOfTiles>>();


        public async Task StartCombat()
        {
            UnimpededUnitSightlines.Clear();

            AddMorselForAll();

            foreach(var unit in Scene.UnitsInCombat)
            {
                await CalculateUnimpededLinesToUnit(unit);
            }

            Scene.UnitMoved += CalculateUnimpededLinesToUnit;
            Scene.UnitAddedToCombat += CalculateUnimpededLinesToUnit;
        }

        public void EndCombat()
        {
            UnitInformation.Clear();
            UnimpededUnitSightlines.Clear();

            Scene.UnitMoved -= CalculateUnimpededLinesToUnit;
            Scene.UnitAddedToCombat -= CalculateUnimpededLinesToUnit;
        }

        public void AddMorselForAll()
        {
            foreach (var unit in Scene.UnitsInCombat)
            {
                CreateMorsel(unit, MorselType.All);
            }
        }

        public void CreateMorsel(Unit unit, MorselType morselType)
        {
            foreach(var team in Scene.ActiveTeams)
            {
                if(VisionManager.ConsolidatedVision.TryGetValue(team, out var vision))
                {
                    if(vision.TryGetValue(unit.Info.TileMapPosition, out int visionCount))
                    {
                        if(visionCount > 0)
                        {
                            HashSet<UnitMorsels> morsels;
                            UnitMorsels newMorsel = new UnitMorsels(unit);

                            if (UnitInformation.TryGetValue(team, out morsels)) 
                            {
                                if (morsels.TryGetValue(newMorsel, out var foundMorsel))
                                {
                                    newMorsel = foundMorsel;
                                }
                                else
                                {
                                    morsels.Add(newMorsel);
                                }
                            }
                            else
                            {
                                morsels = new HashSet<UnitMorsels>();
                                morsels.Add(newMorsel);

                                UnitInformation.Add(team, morsels);
                            }


                            switch (morselType)
                            {
                                case MorselType.Action:
                                    newMorsel.CreateActionMorsel();
                                    break;
                                case MorselType.Turn:
                                    newMorsel.CreateTurnMorsel();
                                    break;
                                case MorselType.All:
                                    newMorsel.CreateTurnMorsel();
                                    newMorsel.CreateActionMorsel();
                                    break;

                            }
                        }
                    }
                }
            }
        }

        public static int UNIT_SIGHTLINE_LENGTH = 10;
        private static ObjectPool<List<LineOfTiles>> _lineOfTileListPool = new ObjectPool<List<LineOfTiles>>();
        public async Task CalculateUnimpededLinesToUnit(Unit unit)
        {
            if(UnimpededUnitSightlines.TryGetValue(unit, out var list))
            {
                for(int i = 0; i < list.Count; i++)
                {
                    list[i].Tiles.Clear();
                    Tile.TileListPool.FreeObject(list[i].Tiles);
                }

                list.Clear();
                _lineOfTileListPool.FreeObject(list);
                UnimpededUnitSightlines.Remove(unit);
            }

            var lineOfTilesList = _lineOfTileListPool.GetObject();

            UnimpededUnitSightlines.Add(unit, lineOfTilesList);

            var ringOfTiles = Tile.TileListPool.GetObject();

            TileMap map = unit.Info.TileMapPosition.TileMap;
            map.GetRingOfTiles(unit.Info.TileMapPosition, ringOfTiles, UNIT_SIGHTLINE_LENGTH);

            

            for (int i = 0; i < ringOfTiles.Count; i++)
            {
                var lineOfTiles = Tile.TileListPool.GetObject();

                map.GetLineOfTiles(ringOfTiles[i], unit.Info.TileMapPosition.TilePoint, lineOfTiles);

                LineOfTiles filledLine = new LineOfTiles(lineOfTiles);
                filledLine.CalculateLineIndices();
                lineOfTilesList.Add(filledLine);

                //for(int j = 0; j < filledLine.Tiles.Count; j++)
                //{
                //    if(j >= filledLine.AbilityLineNoHeightIndex)
                //    {
                //        filledLine.Tiles[j].SetColor(_Colors.Blue);
                //    }
                //}
            }

            ringOfTiles.Clear();
            Tile.TileListPool.FreeObject(ringOfTiles);
        }
    }

    public class LineOfTiles
    {
        public List<Tile> Tiles = new List<Tile>();

        /// <summary>
        /// The index that the ability line begins at <para/>
        /// Specifies a line where the heights are never too great to break a direct line
        /// </summary>
        public int AbilityLineHeightIndex = -1;

        /// <summary>
        /// The index that the ability line begins at <para/>
        /// Specifies a line where the heights are ignored.
        /// </summary>
        public int AbilityLineNoHeightIndex = -1;


        /// <summary>
        /// The index that the vision line begins at
        /// </summary>
        public int VisionLineIndex = -1;

        public LineOfTiles(List<Tile> tiles)
        {
            Tiles = tiles;
        }

        public void CalculateLineIndices()
        {
            if (Tiles.Count < 2)
                return;

            float baseHeight = Tiles[Tiles.Count - 1].GetVisionHeight();

            Tile currentTile;

            for(int i = Tiles.Count - 2; i >= 0; i--)
            {
                currentTile = Tiles[i];

                bool currTileHigher = (baseHeight - currentTile.GetVisionHeight()) <= -VisionManager.HEIGHT_VISION_CUTOFF;
                //bool currTileLower = (baseHeight - currentTile.GetVisionHeight()) >= VisionManager.HEIGHT_VISION_CUTOFF * 0.5f;

                bool blocksAbility = currentTile.BlocksType(BlockingType.Abilities);
                bool blocksVision = currentTile.BlocksType(BlockingType.Vision);

                if (AbilityLineHeightIndex == -1 && (currTileHigher || blocksAbility)) 
                {
                    AbilityLineHeightIndex = i + 1;
                }

                if (AbilityLineNoHeightIndex == -1 && blocksAbility)
                {
                    AbilityLineNoHeightIndex = i + 1;
                }

                if (VisionLineIndex == -1 && (currTileHigher || blocksVision))
                {
                    VisionLineIndex = i + 1;
                }
            }

            AbilityLineHeightIndex = AbilityLineHeightIndex == -1 ? 0 : AbilityLineHeightIndex;
            AbilityLineNoHeightIndex = AbilityLineNoHeightIndex == -1 ? 0 : AbilityLineNoHeightIndex;
            VisionLineIndex = VisionLineIndex == -1 ? 0 : VisionLineIndex;
        }
    }
}
