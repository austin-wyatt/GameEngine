using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Combat;
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
    public class Move : Ability
    {
        public NavType NavType = NavType.Base;

        public Action _moveCancelAction = null;
        public Move(Unit castingUnit, int range = 6)
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

            CastingMethod |= CastingMethod.Movement;

            EnergyCost = 1;
            ActionCost = 0;

            MaxCharges = 0;
            Charges = 0;

            ChargesLostOnUse = 0;

            HasHoverEffect = true;

            //Name = "Move";

            OneUsePerTurn = false;

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)IconSheetIcons.WalkingBoot },
                Spritesheet = (int)TextureName.IconSpritesheet
            });
        }

        public override void OnSelect(CombatScene scene, TileMap currentMap)
        {
            Units = scene._units;
            Range = CastingUnit.Info.Energy / GetEnergyCost(); //special case for general move ability
        }

        private object _currentTilesLock = new object();
        public override void EnactEffect()
        {
            BeginEffect();

            List<Tile> tiles;

            lock (_currentTilesLock)
            {
                tiles = new List<Tile>(CurrentTiles);
            }

            if (_path.Count > 0)
            {
                int startingIndex = 0;
                lock (_currentTilesLock)
                {
                    for (int i = startingIndex; i < _path.Count; i++)
                    {
                        CurrentTiles.Add(_path[i]);
                        tiles.Add(_path[i]);
                    }
                }

                ClearSelectedTiles();
            }

            Casted();

            try
            {
                if (tiles.Count > 0)
                {
                    if (tiles.Exists(t => t == null)) 
                    {
                        EffectEnded();
                        return;
                    }

                    if(tiles[0].Properties.MovementCost * GetEnergyCost() > CastingUnit.Info.Energy) 
                    {
                        EffectEnded();
                        return;
                    }

                    if(Moving)
                    {
                        //EffectEnded();
                        //return;
                        _moveCanceled = true;
                    }

                    PropertyAnimation moveAnimation = new PropertyAnimation(CastingUnit.BaseObjects[0].BaseFrame);

                    //float speed = CastingUnit.Info.Speed;
                    float speed = 0.1f;

                    Vector3 tileAPosition = tiles[0].Position;
                    Vector3 tileBPosition;

                    int moveIncrements = (int)(20 * speed);

                    int moveDelay = 1; //in ticks

                    for (int i = 1; i < tiles.Count; i++)
                    {
                        tileBPosition = tiles[i].Position;

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

                        Tile currentTile = tiles[i];
                        endOfTileMoveKeyframe.Action = () =>
                        {
                            if (!ApplyTileEnergyCost(currentTile)) 
                            {
                                moveAnimation.Finish();
                                return;
                            }

                            CastingUnit.SetTileMapPosition(currentTile);

                            //Scene.Controller.CullObjects();

                            CalculationThread.AddCalculation(() =>
                            {
                                currentTile.Properties.Type.SimplifiedType().FootstepSound().Play();
                            });
                            

                            if (_moveCanceled)
                            {
                                _moveCanceled = false;

                                moveAnimation.Finish();
                                _moveCancelAction?.Invoke();
                            }
                        };

                        moveAnimation.Keyframes.Add(endOfTileMoveKeyframe);


                        if (Settings.MovementTurbo && i == tiles.Count - 1)
                        {
                            moveAnimation.Keyframes.RemoveRange(0, moveAnimation.Keyframes.Count - 1);
                            moveAnimation.Keyframes[0].ActivationTick = 5;

                            tileAPosition = CastingUnit.Info.TileMapPosition.Position;
                            tileBPosition = tiles[^1].Position;

                            distanceToTravel = tileAPosition - tileBPosition;
                            CastingUnit.SetPosition(CastingUnit.Position - distanceToTravel);
                        }

                        tileAPosition = tileBPosition;
                    }
                    Moving = true;
                    CastingUnit.AddPropertyAnimation(moveAnimation);
                    moveAnimation.Play();

                    moveAnimation.OnFinish = () =>
                    {
                        CastingUnit.SetPositionOffset(CastingUnit.Info.TileMapPosition.Position);
                        CastingUnit.RemovePropertyAnimation(moveAnimation.AnimationID);
                        EffectEnded();

                        if (_moveCanceled) 
                        {
                            _moveCancelAction?.Invoke();
                        }

                        Moving = false;
                        _moveCanceled = false;
                    };
                }
            }
            catch (Exception e)
            {
                EffectEnded();

                Console.WriteLine($"Error in Move.EnactEffect: {e.Message}");
                return;
            }

            SelectedTile = null;
        }

        public bool Moving = false;
        private bool _moveCanceled = false;
        public void CancelMovement() 
        {
            if (Moving) 
            {
                _moveCanceled = true;
            }
        }

        public override void OnCast()
        {
            TileMap.Controller.DeselectTiles();

            base.OnCast();

            lock(_currentTilesLock)
                CurrentTiles.Clear();
        }

        public override void OnAICast()
        {
            base.OnAICast();

            lock (_currentTilesLock)
                CurrentTiles.Clear();
        }

        public override void ApplyEnergyCost()
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

        protected bool ApplyTileEnergyCost(Tile tile) 
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

        public override void OnTileClicked(TileMap map, Tile tile)
        {
            if (_path.Count > 0)
            {
                SelectedTile = tile;
                EnactEffect();
                Scene._selectedAbility = null;

                map.Controller.DeselectTiles();
            }
        }

        private Vector4 _pathColor = new Vector4(0.8f, 0.59f, 0.14f, 1);
        private Vector4 _baseSelectionColor = new Vector4();
        private List<Tile> _path = new List<Tile>();
        private List<BaseTile> _hoverPath = new List<BaseTile>();
        private List<int> _pathTilesToDelete = new List<int>();
        private bool _evaluatingPath = false;
        public override void OnHover(Tile tile, TileMap map)
        {
            if (tile.TilePoint == CastingUnit.Info.TileMapPosition) 
            {
                ClearSelectedTiles();
                Scene.EnergyDisplayBar.HoverAmount(0);
                SelectedTile = null;
                return;
            }

            if (SelectedTile == null || tile != SelectedTile)
            {
                if (!_evaluatingPath) 
                {
                    //Task.Run(() => EvaluateHoverPath(tile, map));
                    CalculationThread.AddCalculation(() =>
                    {
                        EvaluateHoverPath(tile, map);
                    });
                    //EvaluateHoverPath(tile, map);
                }
            }
        }

        public void EvaluateHoverPath(Tile tile, TileMap map, bool ignoreRange = false)
        {
            if (_evaluatingPath)
                return;

            float range = Range;

            if (ignoreRange)
            {
                range = 1000;
            }


            _evaluatingPath = true;

            SelectedTile = tile;
            List<Tile> tiles = new List<Tile>();

            //attempt to extend the selection from the current path
            if (_path.Count > 0)
            {
                bool selectedTileInPath = _path.Exists(p => p != null && p == tile);

                if (_path[^1] == null)
                {
                    _evaluatingPath = false;
                    return;
                }

                //get the path from the last tile to the hovered point
                TileMapManager.NavMesh.GetPathToPoint(_path[^1].ToFeaturePoint(), SelectedTile.ToFeaturePoint(), NavType, out tiles, 
                    range - _path.Count + 1, pathingUnit: CastingUnit);

                if (tiles.Count == 0 && _path.Count - 1 == range && _path.Count > 1 && !selectedTileInPath)
                {
                    TileMapManager.NavMesh.GetPathToPoint(_path[^2].ToFeaturePoint(), SelectedTile.ToFeaturePoint(), NavType, out tiles,
                        range - _path.Count + 2, pathingUnit: CastingUnit);
                    if (tiles.Count != 0)
                    {
                        for (int i = 0; i < _path.Count - 2; i++)
                        {
                            tiles.Insert(i, _path[i]);
                        }
                    }
                    else
                    {
                        //the path is too long so we use the default case here
                        TileMapManager.NavMesh.GetPathToPoint(CastingUnit.Info.TileMapPosition.ToFeaturePoint(), SelectedTile.ToFeaturePoint(), NavType, out tiles,
                        range, pathingUnit: CastingUnit);
                    }
                }
                else if (tiles.Count != 0 || (_path.Count > 1 && _path[^2].TilePoint == tile.TilePoint))
                {
                    bool backtracking = false;
                    for (int i = 0; i < _path.Count - 1; i++)
                    {
                        //if the path does exist make sure we aren't backtracking
                        if (tiles.Exists(t => _path[i] != null && t == _path[i]))
                        {
                            tiles.Clear();
                            //remove the last tile since we are backtracking
                            for (int j = _path.Count - 1; j > i; j--)
                            {
                                if (_pathTilesToDelete.Contains(j))
                                {
                                    _pathTilesToDelete.Remove(j);
                                    _path[j].TilePoint.ParentTileMap.Controller.DeselectTile(_hoverPath[j]);
                                }
                                ClearTile(_hoverPath[j]);
                                _path.Remove(_path[j]);
                                _hoverPath.Remove(_hoverPath[j]);
                            }


                            //if backtracking, fill the tiles list with the path up to that point
                            for (int j = 0; j < _path.Count; j++)
                            {
                                tiles.Add(_path[j]);
                            }

                            backtracking = true;
                        }
                    }

                    if (!backtracking)
                    {
                        //if we aren't backtracking then fill in the tiles we need
                        for (int i = 0; i < _path.Count - 1; i++)
                        {
                            tiles.Insert(i, _path[i]);
                        }
                    }
                }
                else
                {
                    //default case, if a path doesn't exist attempt to get one normally
                    TileMapManager.NavMesh.GetPathToPoint(CastingUnit.Info.TileMapPosition.ToFeaturePoint(), SelectedTile.ToFeaturePoint(), NavType, out tiles,
                        range, pathingUnit: CastingUnit);
                }

            }
            else
            {
                //default case
                TileMapManager.NavMesh.GetPathToPoint(CastingUnit.Info.TileMapPosition.ToFeaturePoint(), SelectedTile.ToFeaturePoint(), NavType, out tiles,
                        range, pathingUnit: CastingUnit);
            }

            ClearSelectedTiles();

            if (tiles.Count > 0)
            {
                int dist = TileMap.GetDistanceBetweenPoints(tiles[0], CastingUnit.Info.TileMapPosition);
                if (dist > 1)
                {
                    _evaluatingPath = false;
                    return;
                }

                foreach (var t in tiles)
                {
                    if (t == null)
                        break;

                    _hoverPath.Add(map.Controller.SelectTile(t, TileSelectionType.Full));
                    _path.Add(t);
                    _pathTilesToDelete.Add(_path.Count - 1);
                }

                for (int i = 0; i < _hoverPath.Count; i++) 
                {
                    _baseSelectionColor = _hoverPath[i]._tileObject.BaseFrame.InterpolatedColor;

                    _hoverPath[i]._tileObject.BaseFrame.SetBaseColor(_Colors.Red + new Vector4(0, i * 0.04f, 0, 0));
                }
            }

            float energyCost = GetPathMovementCost(tiles) * GetEnergyCost();

            if (energyCost > CastingUnit.Info.Energy && !ignoreRange)
            {
                tiles.Clear();
                ClearSelectedTiles();
            }

            Scene.EnergyDisplayBar.HoverAmount(GetPathMovementCost(tiles) * GetEnergyCost());

            _evaluatingPath = false;
        }

        public (float cost, int moves) GetCostToPoint(TilePoint point, float customRange = -1) 
        {
            float range;

            range = CastingUnit.Info.Energy / GetEnergyCost();

            if (customRange != -1) 
            {
                range = customRange;
            }

            List<Tile> tiles;

            TileMapManager.NavMesh.GetPathToPoint(CastingUnit.Info.TileMapPosition.ToFeaturePoint(), point.ToFeaturePoint(), NavType, out tiles,
                        range, pathingUnit: CastingUnit);


            return (GetPathMovementCost(tiles), tiles.Count);
        }

        private float GetPathMovementCost(List<Tile> tiles) 
        {
            float value = 0;

            for (int i = 1; i < tiles.Count; i++) 
            {
                if (tiles[i] == null)
                    continue;

                value += tiles[i].Properties.MovementCost;
            }

            return value;
        }

        private void ClearSelectedTiles()
        {
            try
            {
                for (int i = 0; i < _hoverPath.Count; i++)
                {
                    if (_hoverPath[i] == null)
                        continue;

                    ClearTile(_hoverPath[i]);
                }

                _pathTilesToDelete.ForEach(i =>
                {
                    _hoverPath[i].TilePoint.ParentTileMap.Controller.DeselectTile(_hoverPath[i]);
                });

                _path.Clear();
                _hoverPath.Clear();
                _pathTilesToDelete.Clear();
            }
            catch (Exception e) 
            {
                Console.WriteLine($"Exception caught in Move.ClearSelectedTiles: {e.Message}");
            }
        }

        private void ClearTile(BaseTile tile)
        {
            tile._tileObject.BaseFrame.SetBaseColor(_baseSelectionColor);
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
                return CastingUnit.Info.BuffManager.GetValue(BuffEffect.EnergyCostMultiplier) *
                       CastingUnit.Info.BuffManager.GetValue(BuffEffect.MovementEnergyMultiplier) * 
                       (EnergyCost + 
                       CastingUnit.Info.BuffManager.GetValue(BuffEffect.EnergyCostAdditive) + 
                       CastingUnit.Info.BuffManager.GetValue(BuffEffect.MovementEnergyAdditive));
            }

            return EnergyCost;
        }


        private object _moveToTileLock = new object();
        public void MoveToTile(Tile tile, bool ignoreRange = true) 
        {
            Units = TileMapManager.Scene._units;
            Range = GetRange();

            Task.Run(() =>
            {
                lock (_moveToTileLock) 
                {
                    EvaluateHoverPath(tile, tile.TileMap, ignoreRange: ignoreRange);
                    if(_path.Count > 0) 
                    {
                        EnactEffect();
                    }
                }
            });
        }

        public bool CheckPathToTile(Tile tile, bool ignoreRange = true)
        {
            EvaluateHoverPath(tile, tile.TileMap, ignoreRange: ignoreRange);
            if (_path.Count > 0)
            {
                return true;
            }

            return false;
        }

        public float GetRange()
        {
            return CastingUnit.Info.Energy / GetEnergyCost();
        }
    }

}
