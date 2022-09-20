using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Combat;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities
{
    public class Move : Ability
    {
        public NavType NavType = NavType.Base;

        public Action _moveCancelAction = null;

        public Tile _immediateHoverTile = null;

        public float EnergyCost = 1;

        public override float Range { get => CastingUnit.GetResF(ResF.MovementEnergy) / GetEnergyCost(); }

        public Move(Unit castingUnit, int range = 6)
        {
            Type = AbilityTypes.Move;
            Range = range;
            CastingUnit = castingUnit;
            BreakStealth = false;

            DamageType = DamageType.NonDamaging;

            CastingMethod |= CastingMethod.Movement;

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

            SelectionInfo = new SelectionInfo(this);
        }

        public override void OnSelect(CombatScene scene, TileMap currentMap)
        {
            if(_immediateHoverTile != null)
            {
                EvaluateHoverPath(_immediateHoverTile, _immediateHoverTile.TileMap);
            }
            else if(Scene._tileMapController._hoveredTile != null)
            {
                EvaluateHoverPath(Scene._tileMapController._hoveredTile, Scene._tileMapController._hoveredTile.TileMap);
            }
        }

        public List<Tile> CurrentTiles = new List<Tile>();

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
                    float speed;
                    float moveIncrement;
                    int moveIncrements;
                    int moveDelay;
                    int i;
                    float j;
                    Vector3 distanceToTravel = new Vector3();

                    for (i = 0; i < tiles.Count; i++)
                    {
                        if(tiles[i] == null)
                        {
                            EffectEnded();
                            return;
                        }
                    }

                    if(tiles[0].Properties.MovementCost * GetEnergyCost() > CastingUnit.GetResF(ResF.MovementEnergy)) 
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
                    //speed = 1f;
                    speed = 0.2f;

                    Vector3 tileAPosition = tiles[0].Position;
                    Vector3 tileBPosition;

                    moveIncrements = (int)(20 * speed);
                    moveIncrement = (float)1 / moveIncrements;

                    moveDelay = 1; //in ticks

                    for (i = 1; i < tiles.Count; i++)
                    {
                        tileBPosition = tiles[i]._position;

                        //distanceToTravel.Sub(ref tileAPosition, ref tileBPosition);
                        //distanceToTravel.X /= moveIncrements;
                        //distanceToTravel.Y /= moveIncrements;
                        //distanceToTravel.Z /= moveIncrements;

                        for (j = 0; j < 1; j += moveIncrement)
                        {
                            float x;
                            float y;
                            float z;

                            if (j == 0 || j == 1 - moveIncrement)
                            {
                                x = GMath.LnLerp(tileAPosition.X, tileBPosition.X, j);
                                y = GMath.LnLerp(tileAPosition.Y, tileBPosition.Y, j);
                                z = GMath.LnLerp(tileAPosition.Z, tileBPosition.Z, j);
                            }
                            else
                            {
                                x = MathHelper.Lerp(tileAPosition.X, tileBPosition.X, j);
                                y = MathHelper.Lerp(tileAPosition.Y, tileBPosition.Y, j);
                                z = MathHelper.Lerp(tileAPosition.Z, tileBPosition.Z, j);
                            }

                            Keyframe frame = new Keyframe((int)((i - 1) * moveDelay * moveIncrements + moveDelay * j * moveIncrements + i))
                            {
                                Action = () =>
                                {
                                    CastingUnit.SetPositionOffset(x, y, z);
                                }
                            };

                            moveAnimation.Keyframes.Add(frame);

                            //Console.WriteLine(frame.ActivationTick);
                        }
                        
                        Keyframe endOfTileMoveKeyframe = new Keyframe((i - 1) * moveDelay * moveIncrements + moveDelay * moveIncrements + i);

                        //Console.WriteLine(endOfTileMoveKeyframe.ActivationTick);

                        Tile currentTile = tiles[i];
                        endOfTileMoveKeyframe.Action = () =>
                        {
                            if (!ApplyTileEnergyCost(currentTile)) 
                            {
                                moveAnimation.Finish();
                                return;
                            }

                            CastingUnit.SetPositionOffset(currentTile._position);
                            CastingUnit.SetTileMapPosition(currentTile);

                            Sound sound = currentTile.Properties.Type.SimplifiedType().FootstepSound();
                            sound.SetPosition(CastingUnit.BaseObject.BaseFrame._position.X, 
                                CastingUnit.BaseObject.BaseFrame._position.Y, 
                                CastingUnit.BaseObject.BaseFrame._position.Z);

                            CalculationThread.AddCalculation(sound.Play);

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
                            tileBPosition = tiles[^1]._position;

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
                        CastingUnit.SetPositionOffset(CastingUnit.Info.TileMapPosition._position);
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

            if (CastingUnit.AI.GetControlType() == ControlType.Controlled)
            {
                Scene.EnergyDisplayBar.HoverAmount(0);

                float energyCost = tile.Properties.MovementCost * GetEnergyCost();

                if (CastingUnit.GetResF(ResF.MovementEnergy) - energyCost < 0) 
                {
                    return false;
                }

                Scene.EnergyDisplayBar.AddEnergy(-energyCost);
                CastingUnit.AddResF(ResF.MovementEnergy, -energyCost);
            }
            else
            {
                float energyCost = tile.Properties.MovementCost * GetEnergyCost();

                if (CastingUnit.GetResF(ResF.MovementEnergy) - energyCost < 0)
                {
                    return false;
                }

                CastingUnit.AddResF(ResF.MovementEnergy, -energyCost);
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
        public List<Tile> _path = new List<Tile>();
        private List<Tile> _hoverPath = new List<Tile>();
        private HashSet<int> _pathTilesToDelete = new HashSet<int>();
        private bool _evaluatingPath = false;
        private Tile SelectedTile = null;
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
                    EvaluateHoverPath(tile, map);

                    //CalculationThread.AddCalculation(() =>
                    //{
                    //    Stopwatch stopwatch = Stopwatch.StartNew();
                    //    EvaluateHoverPath(tile, map);

                    //    Console.WriteLine($"Hover path evaluated in {stopwatch.ElapsedMilliseconds}ms");
                    //});
                }
            }
        }

        public void EvaluateHoverPath(Tile tile, TileMap map, bool ignoreRange = false, bool highlightTiles = true, bool allowEndInUnit = false)
        {
            if (_evaluatingPath)
                return;

            _immediateHoverTile = null;

            float range = Range;

            if (ignoreRange)
            {
                range = 100;
            }

            int j;
            int i;
            int x;

            _evaluatingPath = true;

            SelectedTile = tile;
            List<Tile> tiles;

            //attempt to extend the selection from the current path
            if (_path.Count > 0)
            {
                bool selectedTileInPath = false;

                for (i = 0; i < _path.Count; i++)
                {
                    if(_path[i] != null && _path[i] == tile)
                    {
                        selectedTileInPath = true;
                        break;
                    }
                }

                if (_path[^1] == null)
                {
                    _evaluatingPath = false;
                    return;
                }

                //get the path from the last tile to the hovered point
                TileMapManager.NavMesh.GetPathToPoint(_path[^1].ToFeaturePoint(), SelectedTile.ToFeaturePoint(), NavType, out tiles, 
                    range - _path.Count + 1, pathingUnit: CastingUnit, allowEndInUnit: allowEndInUnit);

                if (tiles.Count == 0 && _path.Count - 1 == range && _path.Count > 1 && !selectedTileInPath)
                {
                    tiles.Clear();
                    Tile.TileListPool.FreeObject(ref tiles);

                    TileMapManager.NavMesh.GetPathToPoint(_path[^2].ToFeaturePoint(), SelectedTile.ToFeaturePoint(), NavType, out tiles,
                        range - _path.Count + 2, pathingUnit: CastingUnit, allowEndInUnit: allowEndInUnit);
                    if (tiles.Count != 0)
                    {
                        for (i = 0; i < _path.Count - 2; i++)
                        {
                            tiles.Insert(i, _path[i]);
                        }
                    }
                    else
                    {
                        tiles.Clear();
                        Tile.TileListPool.FreeObject(ref tiles);

                        //the path is too long so we use the default case here
                        TileMapManager.NavMesh.GetPathToPoint(CastingUnit.Info.TileMapPosition.ToFeaturePoint(), SelectedTile.ToFeaturePoint(), NavType, out tiles,
                        range, pathingUnit: CastingUnit, allowEndInUnit: allowEndInUnit);
                    }
                }
                else if (tiles.Count != 0 || (_path.Count > 1 && _path[^2].TilePoint == tile.TilePoint))
                {
                    bool backtracking = false;
                    for (i = 0; i < _path.Count - 1; i++)
                    {
                        bool exists = false;
                        for(x = 0; x < tiles.Count; x++)
                        {
                            if(_path[i] != null && tiles[x] == _path[i])
                            {
                                exists = true;
                                break;
                            }
                        }

                        //if the path does exist make sure we aren't backtracking
                        if (exists)
                        {
                            tiles.Clear();
                            //remove the last tile since we are backtracking
                            for (j = _path.Count - 1; j > i; j--)
                            {
                                if (_pathTilesToDelete.Contains(j))
                                {
                                    _pathTilesToDelete.Remove(j);
                                    _path[j].TilePoint.ParentTileMap.Controller.DeselectTile(_hoverPath[j]);
                                }
                                _path.Remove(_path[j]);
                                _hoverPath.Remove(_hoverPath[j]);
                            }


                            //if backtracking, fill the tiles list with the path up to that point
                            for (j = 0; j < _path.Count; j++)
                            {
                                tiles.Add(_path[j]);
                            }

                            backtracking = true;
                        }
                    }

                    if (!backtracking)
                    {
                        //if we aren't backtracking then fill in the tiles we need
                        for (i = 0; i < _path.Count - 1; i++)
                        {
                            tiles.Insert(i, _path[i]);
                        }
                    }
                }
                else
                {
                    tiles.Clear();
                    Tile.TileListPool.FreeObject(ref tiles);

                    //default case, if a path doesn't exist attempt to get one normally
                    TileMapManager.NavMesh.GetPathToPoint(CastingUnit.Info.TileMapPosition.ToFeaturePoint(), SelectedTile.ToFeaturePoint(), NavType, out tiles,
                        range, pathingUnit: CastingUnit, allowEndInUnit: allowEndInUnit);
                }

            }
            else
            {
                //default case
                TileMapManager.NavMesh.GetPathToPoint(CastingUnit.Info.TileMapPosition.ToFeaturePoint(), SelectedTile.ToFeaturePoint(), NavType, out tiles,
                        range, pathingUnit: CastingUnit, allowEndInUnit: allowEndInUnit);
            }

            ClearSelectedTiles();

            if (tiles.Count > 0)
            {
                int dist = FeatureEquation.GetDistanceBetweenPoints(tiles[0].ToFeaturePoint(), CastingUnit.Info.TileMapPosition.ToFeaturePoint());
                if (dist > 1)
                {
                    _evaluatingPath = false;
                    return;
                }

                for (i = 0; i < tiles.Count; i++)
                {
                    if (tiles[i] == null)
                        break;


                    _path.Add(tiles[i]);

                    if (highlightTiles)
                    {
                        map.Controller.SelectTile(tiles[i], TileSelectionType.Full);
                        _hoverPath.Add(tiles[i]);
                        _pathTilesToDelete.Add(_path.Count - 1);
                    }
                }

                for (i = 0; i < _hoverPath.Count; i++) 
                {
                    _hoverPath[i].SelectionColor = _Colors.Red + new Vector4(0, i * 0.04f, 0, 0);
                    _hoverPath[i].CalculateDisplayedColor();
                }
            }

            float energyCost = GetPathMovementCost(tiles) * GetEnergyCost();

            if (energyCost > CastingUnit.GetResF(ResF.MovementEnergy) && !ignoreRange)
            {
                tiles.Clear();
                ClearSelectedTiles();
            }

            Scene.EnergyDisplayBar.HoverAmount(GetPathMovementCost(tiles) * GetEnergyCost());

            tiles.Clear();
            Tile.TileListPool.FreeObject(ref tiles);

            _evaluatingPath = false;
        }

        public (float cost, int moves) GetCostToPoint(TilePoint point, float customRange = -1) 
        {
            float range;

            range = CastingUnit.GetResF(ResF.MovementEnergy) / GetEnergyCost();

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
                foreach(var i in _pathTilesToDelete)
                {
                    TileMapManager.Scene._tileMapController.DeselectTile(_hoverPath[i]);
                }

                _path.Clear();
                _hoverPath.Clear();
                _pathTilesToDelete.Clear();
            }
            catch (Exception e) 
            {
                Console.WriteLine($"Exception caught in Move.ClearSelectedTiles: {e.Message}");
            }
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
            _immediateHoverTile = null;
        }

        public float GetEnergyCost()
        {
            if (CastingUnit != null)
            {
                return CastingUnit.Info.BuffManager.GetValue(BuffEffect.EnergyCostMultiplier) *
                       CastingUnit.Info.BuffManager.GetValue(BuffEffect.MovementEnergyCostMultiplier) * 
                       (EnergyCost + 
                       CastingUnit.Info.BuffManager.GetValue(BuffEffect.EnergyCostAdditive) + 
                       CastingUnit.Info.BuffManager.GetValue(BuffEffect.MovementEnergyAdditive));
            }

            return EnergyCost;
        }


        private object _moveToTileLock = new object();
        public void MoveToTile(Tile tile, bool ignoreRange = true) 
        {
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
            return CastingUnit.GetResF(ResF.MovementEnergy) / GetEnergyCost();
        }
    }

}
