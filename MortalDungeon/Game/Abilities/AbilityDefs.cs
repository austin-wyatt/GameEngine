using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.GameObjects;
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

            TraversableTypes.Add(TileClassification.Ground);
        }


        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default)
        {
            AffectedTiles = tileMap.FindValidTilesInRadius(CastingUnit.TileMapPosition, Range, TraversableTypes, units, CastingUnit, Type);
            TileMap = tileMap;
            Units = units;

            TrimTiles(AffectedTiles, units, true);
            return AffectedTiles;
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            CurrentTiles = TileMap.GetPathToPoint(CastingUnit.TileMapPosition, SelectedTile.TileIndex, Range, TraversableTypes, Units, CastingUnit, Type);

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

            AffectedTiles.ForEach(tile =>
            {
                tile.SetAnimation(tile.DefaultAnimation);
                tile.SetColor(tile.DefaultColor);
            });
            CastingUnit.TileMapPosition = SelectedTile.TileIndex;
        }
    }

    public class BasicMelee : Ability
    {
        public BasicMelee(Unit castingUnit, int range = 1)
        {
            Type = AbilityTypes.MeleeAttack;
            Range = range;
            CastingUnit = castingUnit;
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default)
        {
            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(CastingUnit.TileMapPosition, Range, new List<TileClassification> { TileClassification.Ground, TileClassification.AttackableTerrain }, units, CastingUnit);
            TileMap = tileMap;

            TrimTiles(validTiles, units);
            return validTiles;
        }

        public override void EnactEffect()
        {
            base.EnactEffect();
        }
    }
}
