﻿using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Abilities
{
    internal class Move : Ability
    {
        List<TileClassification> TraversableTypes = new List<TileClassification>();

        public Action _moveCancelAction = null;
        internal Move(Unit castingUnit, int range = 6)
        {
            Type = AbilityTypes.Move;
            Range = range;
            CastingUnit = castingUnit;
            CanTargetThroughFog = true;
            BreakStealth = false;

            UnitTargetParams.IsHostile = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            DamageType = DamageType.NonDamaging;

            EnergyCost = 1;
            ActionCost = 0;

            MaxCharges = 0;
            Charges = 0;

            ChargesLostOnUse = 0;

            HasHoverEffect = true;

            Name = "Move";

            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.WalkingBoot, Spritesheets.IconSheet, true);

            TraversableTypes.Add(TileClassification.Ground);
        }


        internal override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            base.GetValidTileTargets(tileMap);

            Range = CastingUnit.Info.Energy / GetEnergyCost(); //special case for general move ability
            

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

        internal override void OnSelect(CombatScene scene, TileMap currentMap)
        {
            Units = scene._units;
            Range = CastingUnit.Info.Energy / GetEnergyCost(); //special case for general move ability
        }

        internal override void EnactEffect()
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

            try
            {
                if (CurrentTiles.Count > 0)
                {
                    //Task.Run(() =>
                    //{
                    PropertyAnimation moveAnimation = new PropertyAnimation(CastingUnit.BaseObjects[0].BaseFrame);

                    //float speed = CastingUnit.Info.Speed;
                    float speed = 0.1f;

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
                                Action = () =>
                                {
                                    CastingUnit.SetPosition(CastingUnit.Position - distanceToTravel);
                                }
                            };

                            moveAnimation.Keyframes.Add(frame);
                        }

                        Keyframe endOfTileMoveKeyframe = new Keyframe((i - 1) * moveDelay * moveIncrements + moveDelay * moveIncrements);

                        BaseTile currentTile = CurrentTiles[i];
                        endOfTileMoveKeyframe.Action = () =>
                        {
                            if (!ApplyTileEnergyCost(currentTile)) 
                            {
                                moveAnimation.Finish();
                                return;
                            }

                            Task.Run(() =>
                            {
                                CastingUnit.SetTileMapPosition(currentTile);

                                Scene.Controller.CullObjects();
                            });

                            currentTile.Properties.Type.SimplifiedType().FootstepSound().Play();

                            if (_moveCanceled)
                            {
                                _moveCanceled = false;

                                moveAnimation.Finish();
                                _moveCancelAction?.Invoke();
                            }
                        };

                        moveAnimation.Keyframes.Add(endOfTileMoveKeyframe);


                        if (Settings.MovementTurbo && i == CurrentTiles.Count - 1)
                        {
                            moveAnimation.Keyframes.RemoveRange(0, moveAnimation.Keyframes.Count - 1);
                            moveAnimation.Keyframes[0].ActivationTick = 5;

                            tileAPosition = CastingUnit.Info.TileMapPosition.Position;
                            tileBPosition = CurrentTiles[^1].Position;

                            distanceToTravel = tileAPosition - tileBPosition;
                            CastingUnit.SetPosition(CastingUnit.Position - distanceToTravel);
                        }

                        tileAPosition = tileBPosition;
                    }

                    CastingUnit.AddPropertyAnimation(moveAnimation);
                    moveAnimation.Play();
                    Moving = true;

                    moveAnimation.OnFinish = () =>
                    {
                        CastingUnit.RemovePropertyAnimation(moveAnimation.AnimationID);
                        EffectEnded();

                        if (_moveCanceled) 
                        {
                            _moveCancelAction?.Invoke();
                        }

                        Moving = false;
                        _moveCanceled = false;
                    };
                    //});
                }
            }
            catch (Exception e)
            {
                EffectEnded();

                Console.WriteLine($"Error in Move.EnactEffect: {e.Message}");
                return;
            }

            Casted();

            SelectedTile = null;
        }

        internal bool Moving = false;
        private bool _moveCanceled = false;
        internal void CancelMovement() 
        {
            if (Moving) 
            {
                _moveCanceled = true;
            }
        }

        internal override void OnCast()
        {
            TileMap.DeselectTiles();

            base.OnCast();

            CurrentTiles.Clear();
        }

        internal override void OnAICast()
        {
            base.OnAICast();

            CurrentTiles.Clear();
        }

        internal override void ApplyEnergyCost()
        {
            //if (CastingUnit.AI.ControlType == ControlType.Controlled)
            //{
            //    Scene.EnergyDisplayBar.HoverAmount(0);

            //    float energyCost = GetPathMovementCost(CurrentTiles) * GetEnergyCost();

            //    Scene.EnergyDisplayBar.AddEnergy(-energyCost);
            //}
            //else 
            //{
            //    float energyCost = GetPathMovementCost(CurrentTiles) * GetEnergyCost();

            //    CastingUnit.Info.Energy -= energyCost;
            //}
        }

        protected bool ApplyTileEnergyCost(BaseTile tile) 
        {
            if (!Scene.InCombat)
                return true;

            if (CastingUnit.AI.ControlType == ControlType.Controlled)
            {
                Scene.EnergyDisplayBar.HoverAmount(0);

                float energyCost = tile.Properties.MovementCost * GetEnergyCost();

                if (CastingUnit.Info.Energy - energyCost < 0) 
                {
                    return false;
                }

                Scene.EnergyDisplayBar.AddEnergy(-energyCost);
                CastingUnit.Info.Energy -= energyCost;
            }
            else
            {
                float energyCost = tile.Properties.MovementCost * GetEnergyCost();

                if (CastingUnit.Info.Energy - energyCost < 0)
                {
                    return false;
                }

                CastingUnit.Info.Energy -= energyCost;
            }

            return true;
        }

        internal override void OnTileClicked(TileMap map, BaseTile tile)
        {
            if (_path.Count > 0)
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
        private bool _evaluatingPath = false;
        internal override void OnHover(BaseTile tile, TileMap map)
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
                if (!_evaluatingPath) 
                {
                    Task.Run(() => EvaluateHoverPath(tile, map));
                }
            }
        }

        internal void EvaluateHoverPath(BaseTile tile, TileMap map) 
        {
            _evaluatingPath = true;

            SelectedTile = tile;
            List<BaseTile> tiles = new List<BaseTile>();
            TileMap.PathToPointParameters param;

            //attempt to extend the selection from the current path
            if (_path.Count > 0)
            {
                bool selectedTileInPath = _path.Exists(p => p.AttachedTile != null && p.AttachedTile.ObjectID == tile.ObjectID);

                if (_path[^1].AttachedTile == null) return;

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
                foreach(var t in tiles)
                {
                    if (t == null)
                        break;

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
                }

                for (int i = 0; i < _path.Count; i++) 
                {
                    _baseSelectionColor = _path[i]._tileObject.OutlineParameters.InlineColor;

                    _path[i]._tileObject.OutlineParameters.InlineColor = _pathColor;
                    _path[i]._tileObject.OutlineParameters.InlineThickness = 5;
                }

                //_path.ForEach(p =>
                //{
                //    _baseSelectionColor = p._tileObject.OutlineParameters.InlineColor;

                //    p._tileObject.OutlineParameters.InlineColor = _pathColor;
                //    p._tileObject.OutlineParameters.InlineThickness = 5;
                //});
            }

            float energyCost = GetPathMovementCost(tiles) * GetEnergyCost();

            if (energyCost > CastingUnit.Info.Energy)
            {
                tiles.Clear();
                ClearSelectedTiles();
            }

            Scene.EnergyDisplayBar.HoverAmount(GetPathMovementCost(tiles) * GetEnergyCost());

            _evaluatingPath = false;
        }

        internal (float cost, int moves) GetCostToPoint(TilePoint point, float customRange = -1) 
        {
            float range;

            range = CastingUnit.Info.Energy / GetEnergyCost();

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
            try
            {
                for (int i = 0; i < _path.Count; i++)
                {
                    if (_path[i] == null)
                        continue;

                    ClearTile(_path[i]);
                }

                _pathTilesToDelete.ForEach(i =>
                {
                    _path[i].TilePoint.ParentTileMap.DeselectTile(_path[i]);
                });

                _path.Clear();
                _pathTilesToDelete.Clear();
            }
            catch (Exception e) 
            {
                Console.WriteLine($"Exception caught in Move.ClearSelectedTiles: {e.Message}");
            }
        }

        private void ClearTile(BaseTile tile)
        {
            tile._tileObject.OutlineParameters.InlineColor = _baseSelectionColor;
            tile._tileObject.OutlineParameters.InlineThickness = tile._tileObject.OutlineParameters.BaseInlineThickness;
        }

        internal override void OnRightClick()
        {
            base.OnRightClick();
            ClearSelectedTiles();
        }

        internal override void OnAbilityDeselect()
        {
            base.OnAbilityDeselect();

            ClearSelectedTiles();
            SelectedTile = null;
        }

        internal override float GetEnergyCost()
        {
            if (CastingUnit != null)
            {
                return CastingUnit.Info.EnergyCostMultiplier * CastingUnit.Info.SpeedMultiplier * EnergyCost + CastingUnit.Info.EnergyAddition + CastingUnit.Info.SpeedAddition;
            }

            return EnergyCost;
        }
    }

}
