using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Units;
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
        Default = 21,
        Grass = 22,
        Outline = 20,

        Fog_1 = 80,
        Fog_2 = 81,
        Fog_3 = 90,
        Fog_4 = 91,
    }
    public class BaseTile : GameObject
    {
        public Vector4 DefaultColor = default;
        public BaseTileAnimationType DefaultAnimation = BaseTileAnimationType.SolidWhite;
        public BaseTileAnimationType CurrentAnimation = BaseTileAnimationType.SolidWhite;
        public TilePoint TilePoint;

        public new ObjectType ObjectType = ObjectType.Tile;

        public TileClassification TileClassification = TileClassification.Ground;

        public TileType TileType = TileType.Grass;
        public Vector4 Color; //color that will be applied to the tile on the dynamic texture
        public Vector4 OutlineColor = Colors.Black; //outline color that will be applied to the dynamic texture
        public bool Outline = true; //whether the tile should be outline on the dynamic texture


        public bool InFog = true;
        public bool BlocksVision = false;
        public bool Selected = false;

        public Dictionary<UnitTeam, bool> Explored = new Dictionary<UnitTeam, bool>();

        private Vector4 _fogColorOffset = new Vector4(0.5f, 0.5f, 0.5f, 0);

        public BaseObject _tileObject;

        public BaseTile AttachedTile; //for selection tiles 
        public TileMap TileMap;

        public BaseTile()
        {
            FillExploredDictionary();
        }
        public BaseTile(Vector3 position, TilePoint point)
        {
            Name = "BaseTile";
            TilePoint = point;

            BaseObject BaseTile = new BaseObject(BASE_TILE_ANIMATION.List, ObjectID, "Base Tile " + ObjectID, default, EnvironmentObjects.BASE_TILE.Bounds);
            DefaultColor = BaseTile.BaseFrame.Color;
            BaseTile.BaseFrame.CameraPerspective = true;

            BaseTile.OutlineParameters.SetAllInline(2);
            BaseTile.OutlineParameters.InlineColor = Colors.Black;
            BaseTile.OutlineParameters.OutlineColor = Colors.Red;

            Hoverable = true;
            Clickable = true;

            BaseObjects.Add(BaseTile);
            _tileObject = BaseTile;

            //_tileObject.BaseFrame.ScaleAddition(1);

            FillExploredDictionary();
            SetPosition(position);
        }

        public static implicit operator TilePoint(BaseTile tile) => tile.TilePoint;

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

        protected void FillExploredDictionary()
        {
            foreach (UnitTeam team in Enum.GetValues(typeof(UnitTeam)))
            {
                Explored.Add(team, false);
            }
        }


        public void SetFog(bool inFog, UnitTeam team = UnitTeam.Ally, UnitTeam previousTeam = UnitTeam.Ally)
        {
            if (inFog != InFog) 
            {
                InFog = inFog;

                TileMap.TilesToUpdate.Add(this);
                TileMap.DynamicTextureInfo.TextureChanged = true;
            }

            if (inFog && !Explored[team])
            {
                Outline = false;
            }
            else
            {
                Outline = true;
            }

            InFog = inFog;
            MultiTextureData.MixTexture = inFog;

            if (inFog && !Explored[team] && !Selected && !Hovered) //Outline the tile if it's not in fog (with some exceptions)
            {
                _tileObject.OutlineParameters.InlineThickness = 0;
            }
            else
            {
                _tileObject.OutlineParameters.InlineThickness = _tileObject.OutlineParameters.BaseInlineThickness;
            }
        }

        public void SetExplored(bool explored = true, UnitTeam team = UnitTeam.Ally, UnitTeam previousTeam = UnitTeam.Ally)
        {
            if (explored != Explored[team] || Explored[team] != Explored[previousTeam])
            {
                Explored[team] = explored;

                TileMap.TilesToUpdate.Add(this);
                TileMap.DynamicTextureInfo.TextureChanged = true;
            }

            Explored[team] = explored;
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

    public class TilePoint
    {
        public int X;
        public int Y;

        public TileMap ParentTileMap;

        public bool _visited = false; //using for pathing

        public TilePoint(int x, int y, TileMap map) 
        {
            X = x;
            Y = y;
            ParentTileMap = map;
        }

        public TilePoint(Vector2i coords, TileMap map)
        {
            X = coords.X;
            Y = coords.Y;
            ParentTileMap = map;
        }

        public BaseTile GetTile() 
        {
            return ParentTileMap[this];
        }

        public static bool operator ==(TilePoint a, TilePoint b) => a.X == b.X && a.Y == b.Y && a.ParentTileMap.TileMapCoords == b.ParentTileMap.TileMapCoords;
        public static bool operator !=(TilePoint a, TilePoint b) => !(a == b);

        public override string ToString()
        {
            return "TilePoint {" + X + ", " + Y + "}";
        }
        public override bool Equals(object obj)
        {
            return obj is TilePoint point &&
                   X == point.X &&
                   Y == point.Y &&
                   EqualityComparer<TileMap>.Default.Equals(ParentTileMap, point.ParentTileMap);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, ParentTileMap);
        }
    }
}
