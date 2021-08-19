using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles.HelperTiles;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using static MortalDungeon.Engine_Classes.Scenes.Scene;

namespace MortalDungeon.Game.Tiles
{
    public enum TileClassification //ground, terrain, etc 
    {
        Ground, //doesn't inhibit movement in any way
        Terrain, //inhibits movement, cannot be attacked
        AttackableTerrain, //inhibits movement, can be attacked
        Water //inhibits movement, cannot be attacked
    }

    public enum TileType //tree, grass, water, etc. Special interactions would be created for each of these (interactions would depend on ability/unit/etc)
    {
        Stone_1 = 10,
        Stone_2 = 11,
        Stone_3 = 12,
        Gravel = 13,
        WoodPlank = 14,

        Default = 21,
        Grass = 22,
        AltGrass = 23,
        Water = 24,
        AltWater = 25,
        Outline = 20,

        Dirt = 33,
        Grass_2 = 34,

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

        public TileProperties Properties;

        public Vector4 Color = Colors.White; //color that will be applied to the tile on the dynamic texture
        public Vector4 OutlineColor = Colors.Black; //outline color that will be applied to the dynamic texture
        public bool Outline = false; //whether the tile should be outline on the dynamic texture
        public bool NeverOutline = false; //whether this tile should never be outlined (used for contiguous tiles like water)

        public bool InFog = true;
        public bool Selected = false;

        public Dictionary<UnitTeam, bool> Explored = new Dictionary<UnitTeam, bool>();

        private Vector4 _fogColorOffset = new Vector4(0.5f, 0.5f, 0.5f, 0);

        public BaseObject _tileObject;

        public BaseTile AttachedTile; //for selection tiles 
        public Structure Structure;
        public Cliff Cliff;

        public HeightIndicatorTile HeightIndicator;
        public TileMap TileMap;

        public TileChunk Chunk;

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

            Properties = new TileProperties() 
            {
                Type = TileType.Grass,
                Classification = TileClassification.Ground
            };

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
                //Explored.Add(team, false);
                Explored.Add(team, true);
            }
        }


        public void SetFog(bool inFog, UnitTeam team = UnitTeam.Ally, UnitTeam previousTeam = UnitTeam.Ally)
        {
            if (inFog != InFog) 
            {
                InFog = inFog;

                Update();
            }

            TileMap.RemoveHeightIndicatorTile(this);

            if (inFog && !Explored[team])
            {
                Outline = false;
            }
            else
            {
                Outline = !NeverOutline;
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

                Update();
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

            //if (hovered)
            //{
            //    _tileObject.OutlineParameters.OutlineThickness = _tileObject.OutlineParameters.BaseOutlineThickness;
            //    _tileObject.OutlineParameters.OutlineColor = Colors.Red;
            //}
            //else
            //{
            //    _tileObject.OutlineParameters.OutlineThickness = 0;
            //}
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

                for (int i = 0; i < _onHoverEndActions.Count; i++)
                {
                    _onHoverEndActions[i]?.Invoke();
                }
            }
        }

        public void Update()
        {
            TileMap.TilesToUpdate.Add(this);
            TileMap.DynamicTextureInfo.TextureChanged = true;
        }

        public void ClearCliff() 
        {
            if (Cliff != null) 
            {
                Cliff.ClearCliff();
                Cliff = null;
            }
        }

        public static string GetTooltipString(BaseTile tile, CombatScene scene) 
        {
            string tooltip;

            if (tile.InFog && !tile.Explored[scene.CurrentUnit.Team])
            {
                tooltip = "Unexplored tile";
            }
            else 
            {
                int coordX = tile.TilePoint.X + tile.TilePoint.ParentTileMap.TileMapCoords.X * tile.TilePoint.ParentTileMap.Width;
                int coordY = tile.TilePoint.Y + tile.TilePoint.ParentTileMap.TileMapCoords.Y * tile.TilePoint.ParentTileMap.Height;

                tooltip = $"Type: {TileTypeToString(tile.Properties.Type)} \n";
                tooltip += $"Coordinates: {coordX}, {coordY} \n";
                tooltip += $"Elevation: {tile.Properties.Height}\n";

                if (tile.Structure != null) 
                {
                    tooltip += $"Structure\n* Name: {tile.Structure.Name}\n";
                    tooltip += $"* Height: {tile.Structure.Height}\n";
                }
            }

            return tooltip;
        }

        public static string TileTypeToString(TileType type) 
        {
            string val;

            switch (type) 
            {
                case TileType.Water:
                case TileType.AltWater:
                    val = "Water";
                    break;
                case TileType.Grass:
                case TileType.AltGrass:
                    val = "Grass";
                    break;
                default:
                    val = type.ToString();
                    break;
            }

            return val;
        }

        public int GetVisionHeight() 
        {
            return Structure != null && !Structure.Passable ? Structure.Height + Properties.Height : Properties.Height;
        }

        public int GetPathableHeight()
        {
            return Structure != null && Structure.Pathable && !Structure.Passable ? Structure.Height + Properties.Height : Properties.Height;
        }

        internal bool StructurePathable()
        {
            return Structure == null || (Structure != null && Structure.Pathable);
        }

        internal CombatScene GetScene() 
        {
            return TilePoint.ParentTileMap.Controller.Scene;
        }

        public void OnRightClick(ContextManager<MouseUpFlags> flags)
        {
            CombatScene scene = GetScene();

            if (Structure != null)
            {
                Tooltip structContextMenu = Structure.CreateContextMenu();

                if (structContextMenu != null)
                {
                    scene.OpenContextMenu(structContextMenu);
                }
                else
                {
                    scene.OpenContextMenu(CreateContextMenu());
                }
            }
            else 
            {
                scene.OpenContextMenu(CreateContextMenu());
            }
        }

        public override Tooltip CreateContextMenu()
        {
            Tooltip menu = new Tooltip();

            TextComponent header = new TextComponent();
            header.SetTextScale(0.1f);
            header.SetColor(Colors.UITextBlack);
            header.SetText("Tile " + ObjectID);

            TextComponent description = new TextComponent();
            description.SetTextScale(0.05f);
            description.SetColor(Colors.UITextBlack);
            description.SetText(GetTooltipString(this, GetScene()));

            menu.AddChild(header);
            menu.AddChild(description);

            UIDimensions letterScale = header._textField.Letters[0].GetDimensions();

            //initially position the objects so that the tooltip can be fitted
            header.SetPositionFromAnchor(menu.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10 + letterScale.Y / 2, 0), UIAnchorPosition.TopLeft);
            description.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            menu.Margins = new UIDimensions(0, 60);

            menu.FitContents();

            //position the objects again once the menu has been fitted to the correct size
            header.SetPositionFromAnchor(menu.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10 + letterScale.Y / 2, 0), UIAnchorPosition.TopLeft);
            description.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            menu.Clickable = true;

            return menu;
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

        public bool IsValidTile() 
        {
            return ParentTileMap.IsValidTile(X, Y);
        }

        public static bool operator ==(TilePoint a, TilePoint b) => Equals(a, b);
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
                   ParentTileMap.TileMapCoords == point.ParentTileMap.TileMapCoords &&
                   EqualityComparer<TileMap>.Default.Equals(ParentTileMap, point.ParentTileMap);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, ParentTileMap);
        }
    }

    public class TileProperties 
    {
        public TileType Type;
        public TileClassification Classification;
        public float DamageOnEnter = 0;
        public float Slow = 0;
        public bool BlocksVision = false;
        public int Height = 0; //the tile's height for vision and movement purposes

        public TileProperties() 
        {
        }
    }
}
