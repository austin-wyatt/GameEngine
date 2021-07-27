﻿using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
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

            HasHoverEffect = true;

            Name = "Move";

            TraversableTypes.Add(TileClassification.Ground);
        }


        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default)
        {
            AffectedTiles = tileMap.FindValidTilesInRadius(CastingUnit.TileMapPosition, Range, TraversableTypes, units, CastingUnit, Type);
            TileMap = tileMap;
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
                //if (_path[0].AttachedTile.TileIndex == CastingUnit.TileMapPosition)
                //{
                //    startingIndex++;
                //}

                CurrentTiles.Clear();
                for (int i = startingIndex; i < _path.Count; i++)
                {
                    CurrentTiles.Add(_path[i].AttachedTile);
                }

                ClearSelectedTiles();
            }
            else
            {
                CurrentTiles = TileMap.GetPathToPoint(CastingUnit.TileMapPosition, SelectedTile.TileIndex, Range, TraversableTypes, Units, CastingUnit, Type);
            }


            if (CurrentTiles.Count > 0)
            {
                PropertyAnimation moveAnimation = new PropertyAnimation(CastingUnit.GetDisplay(), CastingUnit.NextAnimationID);

                Vector3 tileAPosition = CurrentTiles[0].Position;
                Vector3 tileBPosition;

                int moveIncrements = 20;
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
                        Keyframe frame = new Keyframe((i - 1) * moveDelay * moveIncrements + moveDelay * j);

                        frame.Action = display =>
                        {
                            CastingUnit.SetPosition(CastingUnit.Position - distanceToTravel);
                        };

                        moveAnimation.Keyframes.Add(frame);
                    }

                    Keyframe endOfTileMoveKeyframe = new Keyframe((i - 1) * moveDelay * moveIncrements + moveDelay * moveIncrements);

                    BaseTile currentTile = CurrentTiles[i];
                    endOfTileMoveKeyframe.Action = display =>
                    {
                        TileMap.Tiles.ForEach(tile =>
                        {
                            tile.SetFog(true);
                        });

                        Units.ForEach(unit =>
                        {
                            List<BaseTile> tiles = TileMap.GetVisionInRadius(currentTile.TileIndex, unit.VisionRadius, new List<TileClassification>() { TileClassification.Terrain }, Units.FindAll(u => u.TileMapPosition != unit.TileMapPosition));

                            tiles.ForEach(tile =>
                            {
                                tile.SetFog(false);
                                tile.SetExplored();
                            });
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
                };
            }

            //AffectedTiles.ForEach(tile =>
            //{
            //    tile.SetAnimation(tile.DefaultAnimation);
            //    tile.SetColor(tile.DefaultColor);
            //});
            CastingUnit.TileMapPosition = SelectedTile.TileIndex;
            SelectedTile = null;
        }

        public override void OnSelect(CombatScene scene, TileMap currentMap)
        {
            base.OnSelect(scene, currentMap);

            AffectedTiles = GetValidTileTargets(currentMap, scene._units);


            //currentMap.SelectTiles(AffectedTiles);

            AffectedTiles.ForEach(tile =>
            {
                if (tile.Explored || !(tile.InFog))
                    currentMap.SelectTile(tile);
            });
        }

        public override void OnTileClicked(TileMap map, BaseTile tile)
        {
            if (AffectedTiles.Exists(t => t.TileIndex == tile.TileIndex))
            {
                SelectedTile = tile;
                EnactEffect();
                Scene._selectedAbility = null;

                map.DeselectTiles();
            }
        }

        public override void OnUnitClicked(Unit unit)
        {
            if (CastingUnit.ObjectID == unit.ObjectID)
            {
                Scene.DeselectAbility();
            }
        }

        private Vector4 _pathColor = Colors.Red;
        private Vector4 _baseSelectionColor = new Vector4();
        private List<BaseTile> _path = new List<BaseTile>();
        private List<int> _pathTilesToDelete = new List<int>();
        public override void OnHover(BaseTile tile, TileMap map)
        {
            if (tile.TileIndex == CastingUnit.TileMapPosition)
                return;

            if (SelectedTile == null || tile.ObjectID != SelectedTile.ObjectID)
            {
                SelectedTile = tile;
                List<BaseTile> tiles = new List<BaseTile>();


                //attempt to extend the selection from the current path
                if (_path.Count > 0)
                {
                    bool selectedTileInPath = _path.Exists(p => p.AttachedTile != null && p.AttachedTile.ObjectID == tile.ObjectID);

                    //get the path from the last tile to the hovered point
                    tiles = TileMap.GetPathToPoint(_path[_path.Count - 1].AttachedTile.TileIndex, SelectedTile.TileIndex, Range - _path.Count + 1, TraversableTypes, Units, CastingUnit, Type);

                    if (tiles.Count == 0 && _path.Count - 1 == Range && _path.Count > 1 && !selectedTileInPath)
                    {
                        tiles = TileMap.GetPathToPoint(_path[_path.Count - 2].AttachedTile.TileIndex, SelectedTile.TileIndex, Range - _path.Count + 2, TraversableTypes, Units, CastingUnit, Type);
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
                            tiles = TileMap.GetPathToPoint(CastingUnit.TileMapPosition, SelectedTile.TileIndex, Range, TraversableTypes, Units, CastingUnit, Type);
                        }
                    }
                    else if (tiles.Count != 0 || (_path.Count > 1 && _path[_path.Count - 2].AttachedTile.TileIndex == tile.TileIndex))
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
                                        _path[j].TileMap.DeselectTile(_path[j]);
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
                        tiles = TileMap.GetPathToPoint(CastingUnit.TileMapPosition, SelectedTile.TileIndex, Range, TraversableTypes, Units, CastingUnit, Type);
                    }

                }
                else
                {
                    tiles = TileMap.GetPathToPoint(CastingUnit.TileMapPosition, SelectedTile.TileIndex, Range, TraversableTypes, Units, CastingUnit, Type);
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
            }
        }

        private void ClearSelectedTiles()
        {
            _path.ForEach(t =>
            {
                ClearTile(t);
            });

            _pathTilesToDelete.ForEach(i =>
            {
                _path[i].TileMap.DeselectTile(_path[i]);
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
            Scene.DeselectAbility();
            ClearSelectedTiles();
        }

        public override void OnAbilityDeselect()
        {
            base.OnAbilityDeselect();

            ClearSelectedTiles();
        }
    }

}