using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public class Move : Ability
    {
        List<TileClassification> TraversableTypes = new List<TileClassification>();

        public Move(Unit castingUnit, int range = 6)
        {
            Type = AbilityTypes.Move;
            Range = range;
            CastingUnit = castingUnit;
            CanTargetEnemy = false;
            CanTargetAlly = false;
            CanTargetThroughFog = true;

            EnergyCost = 1;

            HasHoverEffect = true;

            Name = "Move";

            Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.WalkingBoot, Spritesheets.IconSheet, true);

            TraversableTypes.Add(TileClassification.Ground);
        }


        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default)
        {
            Range = Scene.EnergyDisplayBar.CurrentEnergy / GetEnergyCost(); //special case for general move ability

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(CastingUnit.Info.TileMapPosition, Range)
            {
                TraversableTypes = TraversableTypes,
                Units = units,
                CastingUnit = CastingUnit,
                AbilityType = Type,
                CheckTileLower = true
            };

            AffectedTiles = tileMap.FindValidTilesInRadius(param);
            Units = units;

            TrimTiles(AffectedTiles, units);
            return AffectedTiles;
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            if (_path.Count > 0)
            {
                int startingIndex = 0;

                for (int i = startingIndex; i < _path.Count; i++)
                {
                    CurrentTiles.Add(_path[i].AttachedTile);
                }

                ClearSelectedTiles();
            }


            if (CurrentTiles.Count > 0)
            {
                PropertyAnimation moveAnimation = new PropertyAnimation(CastingUnit.BaseObjects[0].BaseFrame);

                float speed = CastingUnit.Info.Speed;

                Vector3 tileAPosition = CurrentTiles[0].Position;
                Vector3 tileBPosition;

                int moveIncrements = (int)(20 * speed);
                int moveDelay = 1; //in ticks

                for (int i = 1; i < CurrentTiles.Count; i++)
                {
                    tileBPosition = CurrentTiles[i].Position;

                    Vector3 distanceToTravel = tileAPosition - tileBPosition;
                    distanceToTravel.X /= moveIncrements;
                    distanceToTravel.Y /= moveIncrements;
                    distanceToTravel.Z /= moveIncrements;

                    for (int j = 0; j < moveIncrements; j++)
                    {
                        Keyframe frame = new Keyframe((i - 1) * moveDelay * moveIncrements + moveDelay * j)
                        {
                            Action = (display) =>
                            {
                                CastingUnit.SetPosition(CastingUnit.Position - distanceToTravel);
                            }
                        };

                        moveAnimation.Keyframes.Add(frame);
                    }

                    Keyframe endOfTileMoveKeyframe = new Keyframe((i - 1) * moveDelay * moveIncrements + moveDelay * moveIncrements);

                    BaseTile currentTile = CurrentTiles[i];
                    endOfTileMoveKeyframe.Action = ( display) =>
                    {
                        bool updateAll = false;
                        //if (CastingUnit.TileMapPosition.GetPathableHeight() != currentTile.GetPathableHeight()) 
                        //{
                        //    updateAll = true;
                        //}

                        CastingUnit.SetTileMapPosition(currentTile);

                        TileMap.Controller.TileMaps.ForEach(m => m.Tiles.ForEach(tile =>
                        {
                            //tile.SetFog(true, CastingUnit.AI.Team);
                            tile.SetFog(true, UnitTeam.Ally);

                            if (updateAll)
                                tile.Update();
                        }));

                        Units.ForEach(unit =>
                        {
                            //if (unit.AI.Team == CastingUnit.AI.Team)
                            if (unit.AI.Team == UnitTeam.Ally)
                            {
                                List<Unit> allOtherUnits = Units.FindAll(u => u.AI.Team != unit.AI.Team);
                                List<BaseTile> tiles = TileMap.GetVisionInRadius(unit.Info.TileMapPosition, unit.Info.VisionRadius, new List<TileClassification>() { TileClassification.Terrain }, Units.FindAll(u => u.Info.TileMapPosition != unit.Info.TileMapPosition));

                                tiles.ForEach(tile =>
                                {
                                    //tile.SetExplored(true, CastingUnit.AI.Team);
                                    //tile.SetFog(false, CastingUnit.AI.Team);
                                    tile.SetExplored(true, UnitTeam.Ally);
                                    tile.SetFog(false, UnitTeam.Ally);
                                });

                                Scene.HideObjectsInFog(allOtherUnits);
                            }
                        });
                    };

                    moveAnimation.Keyframes.Add(endOfTileMoveKeyframe);

                    tileAPosition = tileBPosition;
                }

                CastingUnit.AddPropertyAnimation(moveAnimation);
                moveAnimation.Play();

                moveAnimation.OnFinish = () =>
                {
                    CastingUnit.RemovePropertyAnimation(moveAnimation.AnimationID);
                    EffectEnded();
                };
            }

            Casted();
            
            //CastingUnit.TileMapPosition = SelectedTile.TileIndex;
            SelectedTile = null;
        }

        public override void OnCast()
        {
            Scene.EnergyDisplayBar.HoverAmount(0);

            float energyCost = GetPathMovementCost(CurrentTiles) * GetEnergyCost();

            //if (energyCost == 0) //special cases will go here
            //{
            //    energyCost = 1;
            //}

            Scene.EnergyDisplayBar.AddEnergy(-energyCost);

            TileMap.DeselectTiles();
            CurrentTiles.Clear();

            base.OnCast();
        }

        public override void OnAICast()
        {
            float energyCost = GetPathMovementCost(CurrentTiles) * GetEnergyCost();

            CastingUnit.Info.Energy -= energyCost;

            CurrentTiles.Clear();

            base.OnAICast();
        }

        public override void OnTileClicked(TileMap map, BaseTile tile)
        {
            if (AffectedTiles.Exists(t => t.TilePoint == tile.TilePoint))
            {
                SelectedTile = tile;
                EnactEffect();
                Scene._selectedAbility = null;

                map.DeselectTiles();
            }
        }

        private Vector4 _pathColor = new Vector4(0.8f, 0.59f, 0.14f, 1);
        private Vector4 _baseSelectionColor = new Vector4();
        private List<BaseTile> _path = new List<BaseTile>();
        private List<int> _pathTilesToDelete = new List<int>();
        public override void OnHover(BaseTile tile, TileMap map)
        {
            if (tile.TilePoint == CastingUnit.Info.TileMapPosition) 
            {
                ClearSelectedTiles();
                Scene.EnergyDisplayBar.HoverAmount(0);
                SelectedTile = null;
                return;
            }


            if (SelectedTile == null || tile.ObjectID != SelectedTile.ObjectID)
            {
                SelectedTile = tile;
                List<BaseTile> tiles = new List<BaseTile>();
                TileMap.PathToPointParameters param;

                //attempt to extend the selection from the current path
                if (_path.Count > 0)
                {
                    bool selectedTileInPath = _path.Exists(p => p.AttachedTile != null && p.AttachedTile.ObjectID == tile.ObjectID);

                    //get the path from the last tile to the hovered point
                    param = new TileMap.PathToPointParameters(_path[^1].AttachedTile.TilePoint, SelectedTile.TilePoint, Range - _path.Count + 1)
                    {
                        TraversableTypes = TraversableTypes,
                        Units = Units,
                        CastingUnit = CastingUnit,
                        AbilityType = Type,
                        CheckTileLower = true
                    };
                    tiles = TileMap.GetPathToPoint(param);

                    if (tiles.Count == 0 && _path.Count - 1 == Range && _path.Count > 1 && !selectedTileInPath)
                    {
                        param.StartingPoint = _path[^2].AttachedTile.TilePoint;
                        param.Depth = Range - _path.Count + 2;

                        tiles = TileMap.GetPathToPoint(param);
                        if (tiles.Count != 0)
                        {
                            for (int i = 0; i < _path.Count - 2; i++)
                            {
                                tiles.Insert(i, _path[i].AttachedTile);
                            }
                        }
                        else
                        {
                            //the path is too long so we use the default case here
                            param.StartingPoint = CastingUnit.Info.TileMapPosition;
                            param.Depth = Range;

                            tiles = TileMap.GetPathToPoint(param);
                        }
                    }
                    else if (tiles.Count != 0 || (_path.Count > 1 && _path[^2].AttachedTile.TilePoint == tile.TilePoint))
                    {
                        bool backtracking = false;
                        for (int i = 0; i < _path.Count - 1; i++)
                        {
                            //if the path does exist make sure we aren't backtracking
                            if (tiles.Exists(t => t.ObjectID == _path[i].AttachedTile.ObjectID))
                            {
                                tiles.Clear();
                                //remove the last tile since we are backtracking
                                for (int j = _path.Count - 1; j > i; j--)
                                {
                                    if (_pathTilesToDelete.Contains(j))
                                    {
                                        _pathTilesToDelete.Remove(j);
                                        _path[j].TilePoint.ParentTileMap.DeselectTile(_path[j]);
                                    }
                                    ClearTile(_path[j]);
                                    _path.Remove(_path[j]);
                                }


                                //if backtracking, fill the tiles list with the path up to that point
                                for (int j = 0; j < _path.Count; j++)
                                {
                                    tiles.Add(_path[j].AttachedTile);
                                }

                                backtracking = true;
                            }
                        }

                        if (!backtracking)
                        {
                            //if we aren't backtracking then fill in the tiles we need
                            for (int i = 0; i < _path.Count - 1; i++)
                            {
                                tiles.Insert(i, _path[i].AttachedTile);
                            }
                        }
                    }
                    else
                    {
                        //if a path doesn't exist attempt to get one normally
                        param.StartingPoint = CastingUnit.Info.TileMapPosition;
                        param.Depth = Range;

                        tiles = TileMap.GetPathToPoint(param);
                    }

                }
                else
                {
                    param = new TileMap.PathToPointParameters(CastingUnit.Info.TileMapPosition, SelectedTile.TilePoint, Range)
                    {
                        TraversableTypes = TraversableTypes,
                        Units = Units,
                        CastingUnit = CastingUnit,
                        AbilityType = Type,
                        CheckTileLower = true
                    };

                    tiles = TileMap.GetPathToPoint(param);
                }

                ClearSelectedTiles();

                if (tiles.Count > 0)
                {
                    tiles.ForEach(t =>
                    {
                        if (t.AttachedTile != null)
                        {
                            _path.Add(t.AttachedTile);
                        }
                        else
                        {
                            map.SelectTile(t);
                            _path.Add(t.AttachedTile);
                            _pathTilesToDelete.Add(_path.Count - 1);
                        }
                    });

                    _path.ForEach(p =>
                    {
                        _baseSelectionColor = p._tileObject.OutlineParameters.InlineColor;

                        p._tileObject.OutlineParameters.InlineColor = _pathColor;
                        p._tileObject.OutlineParameters.InlineThickness = 5;
                    });
                }

                float energyCost = GetPathMovementCost(tiles) * GetEnergyCost();

                if (energyCost > Scene.EnergyDisplayBar.CurrentEnergy) 
                {
                    tiles.Clear();
                    ClearSelectedTiles();
                }

                Scene.EnergyDisplayBar.HoverAmount(GetPathMovementCost(tiles) * GetEnergyCost());
            }
        }

        public (float cost, int moves) GetCostToPoint(TilePoint point, float customRange = -1) 
        {
            float range;

            if (CastingUnit.AI.ControlType == ControlType.Controlled)
            {
                range = Scene.EnergyDisplayBar.CurrentEnergy / GetEnergyCost();
            }
            else 
            {
                range = CastingUnit.Info.Energy / GetEnergyCost();
            }

            if (customRange != -1) 
            {
                range = customRange;
            }

            TileMap.PathToPointParameters param = new TileMap.PathToPointParameters(CastingUnit.Info.TileMapPosition, point, range)
            {
                TraversableTypes = TraversableTypes,
                Units = Units,
                CastingUnit = CastingUnit,
                AbilityType = Type,
                CheckTileLower = true,
                IgnoreTargetUnit = true
            };

            List<BaseTile> tiles = TileMap.GetPathToPoint(param);


            return (GetPathMovementCost(tiles), tiles.Count);
        }

        private float GetPathMovementCost(List<BaseTile> tiles) 
        {
            float value = 0;

            for (int i = 1; i < tiles.Count; i++) 
            {
                value += tiles[i].Properties.MovementCost;
            }

            return value;
        }

        private void ClearSelectedTiles()
        {
            _path.ForEach(t =>
            {
                ClearTile(t);
            });

            _pathTilesToDelete.ForEach(i =>
            {
                _path[i].TilePoint.ParentTileMap.DeselectTile(_path[i]);
            });

            _path.Clear();
            _pathTilesToDelete.Clear();
        }

        private void ClearTile(BaseTile tile)
        {
            tile._tileObject.OutlineParameters.InlineColor = _baseSelectionColor;
            tile._tileObject.OutlineParameters.InlineThickness = tile._tileObject.OutlineParameters.BaseInlineThickness;
        }

        public override void OnRightClick()
        {
            base.OnRightClick();
            ClearSelectedTiles();
        }

        public override void OnAbilityDeselect()
        {
            base.OnAbilityDeselect();

            ClearSelectedTiles();
            SelectedTile = null;
        }

        public override float GetEnergyCost()
        {
            if (CastingUnit != null)
            {
                return CastingUnit.Info.EnergyCostMultiplier * CastingUnit.Info.SpeedMultiplier * EnergyCost + CastingUnit.Info.EnergyAddition + CastingUnit.Info.SpeedAddition;
            }

            return EnergyCost;
        }
    }

}
