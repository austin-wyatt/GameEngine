using System;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;

namespace MortalDungeon.Game.GameObjects
{
    public enum TileClassification //ground, terrain, etc 
    {
        Ground, //doesn't inhibit movement in any way
        Terrain, //inhibits movement, cannot be attacked
        AttackableTerrain, //inhibits movement, can be attacked
        Water //inhibits movement, cannot be attacked
    }

    public enum TileType //tree, grass, water, etc. Special interactions would be created for each of these (interactions would depend on ability/unit/etc
    {
        Default,
        Grass,
        Tree,
        Water
    }
    public class BaseTile : GameObject
    {
        public Vector4 DefaultColor = default;
        public BaseTileAnimationType DefaultAnimation = BaseTileAnimationType.SolidWhite;
        public BaseTileAnimationType CurrentAnimation = BaseTileAnimationType.SolidWhite;
        public int TileIndex = 0;
        public int TileMapClassifier = 0; //indicates which tilemap this tile belongs to (in case multiple tile maps are ever used at once

        public TileClassification TileClassification = TileClassification.Ground;
        public TileType TileType = TileType.Default;

        public bool InFog = false;
        public bool BlocksVision = false;

        private Vector4 fogColorOffset = new Vector4(0.5f, 0.5f, 0.5f, 0);

        public BaseTile() { }
        public BaseTile(Vector3 position, int id)
        {
            Name = "BaseTile";

            BaseObject BaseTile = new BaseObject(BASE_TILE_ANIMATION.List, id, "Base Tile " + id, default, EnvironmentObjects.BASE_TILE.Bounds);
            BaseTile.BaseFrame.CameraPerspective = true;
            DefaultColor = BaseTile.BaseFrame.Color;
            BaseTile.SetAnimation((int)BaseTileAnimationType.Grass);
            DefaultAnimation = BaseTileAnimationType.Grass;

            TileIndex = id;

            BaseObjects.Add(BaseTile);

            SetPosition(position);
        }

        public override bool Equals(object obj)
        {
            dynamic d = obj;

            if (obj.GetType() == d.GetType())
            {
                return TileIndex == d.TileIndex && TileMapClassifier == d.TileMapClassifier;
            }
            else 
            {
                return false;
            }
        }

        public void SetAnimation(AnimationType type, Action onFinish = null)
        {
            BaseObjects[0].SetAnimation(type, onFinish);
            CurrentAnimation = (BaseTileAnimationType)type;
        }

        public void SetAnimation(BaseTileAnimationType type, Action onFinish = null)
        {
            BaseObjects[0].SetAnimation((int)type, onFinish);
            CurrentAnimation = type;
        }

        public void SetFogColor() 
        {
            SetColor(DefaultColor - fogColorOffset);
        }
    }
}
