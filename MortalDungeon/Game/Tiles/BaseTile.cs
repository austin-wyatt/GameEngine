using System;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;

namespace MortalDungeon.Game.Tiles
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

        public new ObjectType ObjectType = ObjectType.Tile;

        public TileClassification TileClassification = TileClassification.Ground;
        public TileType TileType = TileType.Default;

        public bool InFog = false;
        public bool BlocksVision = false;
        public bool Selected = false;

        public bool Explored = false;

        private Vector4 _fogColorOffset = new Vector4(0.5f, 0.5f, 0.5f, 0);

        private Vector4 _selectedColor = new Vector4(0.75f, 0, 0, 1);

        public BaseObject _tileObject;



        public BaseTile() { }
        public BaseTile(Vector3 position, int id)
        {
            Name = "BaseTile";

            BaseObject BaseTile = new BaseObject(BASE_TILE_ANIMATION.List, id, "Base Tile " + id, default, EnvironmentObjects.BASE_TILE.Bounds);
            DefaultColor = BaseTile.BaseFrame.Color;
            BaseTile.BaseFrame.CameraPerspective = true;

            BaseTile.OutlineParameters.SetAllInline(2);
            BaseTile.OutlineParameters.InlineColor = Colors.Black;
            BaseTile.OutlineParameters.OutlineColor = Colors.Red;

            Hoverable = true;
            Clickable = true;

            TileIndex = id;

            BaseObjects.Add(BaseTile);
            _tileObject = BaseTile;

            //_tileObject.BaseFrame.ScaleAddition(1);

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

        public Vector4 SetFogColor() 
        {
            SetColor(DefaultColor - _fogColorOffset);
            return DefaultColor - _fogColorOffset;
        }


        public void SetFog(bool inFog) 
        {
            InFog = inFog;
            MultiTextureData.MixTexture = inFog;

            if (inFog && !Explored && !Selected && !Hovered) //Outline the tile if it's not in fog (with some exceptions)
            {
                _tileObject.OutlineParameters.InlineThickness = 0;
            }
            else 
            {
                _tileObject.OutlineParameters.InlineThickness = _tileObject.OutlineParameters.BaseInlineThickness;
            }
        }

        public void SetExplored(bool explored = true) 
        {
            Explored = explored;
            if (explored)
            {
                MultiTextureData.MixPercent = 0.5f;
            }
            else 
            {
                MultiTextureData.MixPercent = 1f;
            }
        }

        public void SetHovered(bool hovered) 
        {
            Hovered = hovered;

            if (hovered)
            {
                _tileObject.OutlineParameters.OutlineThickness = _tileObject.OutlineParameters.BaseOutlineThickness;
                _tileObject.OutlineParameters.OutlineColor = Colors.Red;
            }
            else 
            {
                _tileObject.OutlineParameters.OutlineThickness = 0;
            }
        }

        public void SetSelected(bool selected)
        {
            Selected = selected;

            if (selected)
            {
                _tileObject.OutlineParameters.OutlineThickness = _tileObject.OutlineParameters.BaseOutlineThickness;
                _tileObject.OutlineParameters.OutlineColor = Colors.Blue;
            }
            else
            {
                _tileObject.OutlineParameters.OutlineThickness = 0;
            }
        }

        public override void OnHover()
        {
            if (!Hovered)
            {
                base.OnHover();

                SetHovered(true);
            }
        }

        public override void HoverEnd()
        {
            if (Hovered)
            {
                base.HoverEnd();

                SetHovered(false);
            }
        }
    }
}
