using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes.Rendering;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities;
using Empyrean.Game.Abilities.AbilityDefinitions;
using Empyrean.Game.Map;
using Empyrean.Game.Objects;
using Empyrean.Game.Structures;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;

namespace Empyrean.Game.Tiles
{
    public enum TileClassification //ground, terrain, etc 
    {
        Ground, //doesn't inhibit movement in any way
        ImpassableGround, //inhibits movement
        ImpassableAir, //inhibits flying movement
        Water, //inhibits movement, cannot be attacked
        Lava,
    }

    public enum TileType //tree, grass, water, etc. Special interactions would be created for each of these (interactions would depend on ability/unit/etc)
    {
        None,
        Selection = 2,
        Fill = 3,

        Grass = 5,
        Stone_1 = 6,
        Stone_2 = 7,
        Stone_3 = 12,
        Gravel = 23,
        WoodPlank = 24,

        Default = 41,
        AltGrass = 43,
        Water = 44,
        AltWater = 45,
        Outline = 40,

        Dirt = 8,
        Grass_2 = 64,
        Dead_Grass = 65,
    }

    public enum SimplifiedTileType 
    {
        Unknown,
        Grass,
        Water,
        Stone,
        Wood
    }

    public class BaseTile : GameObject
    {
        public BaseTileAnimationType DefaultAnimation = BaseTileAnimationType.SolidWhite;
        public BaseTileAnimationType CurrentAnimation = BaseTileAnimationType.SolidWhite;
        public TilePoint TilePoint;

        public new ObjectType ObjectType = ObjectType.Tile;

        public BaseTileProperties Properties;

        public Vector4 Color = _Colors.White; //color that will be applied to the tile on the dynamic texture
        public Vector4 OutlineColor = _Colors.Black; //outline color that will be applied to the dynamic texture
        public bool Outline = false; //whether the tile should be outline on the dynamic texture
        public bool NeverOutline = false; //whether this tile should never be outlined (used for contiguous tiles like water)

        public bool Selected = false;

        public BaseObject _tileObject;

        public Structure Structure;

        public TileMap TileMap;

        public TileChunk Chunk;

        public new bool HasContextMenu = true;

        public static ObjectPool<List<BaseTile>> ListPool = new ObjectPool<List<BaseTile>>();

        public BaseTile()
        {

        }
        public BaseTile(Vector3 position, TilePoint point)
        {
            Name = "Tile";
            TilePoint = point;

            BaseObject BaseTile = _3DObjects.CreateBaseObject(new SpritesheetObject((int)TileType.Grass, Spritesheets.TileSheet), _3DObjects.Hexagon, default);
            BaseTile.BaseFrame.CameraPerspective = true;
            BaseTile.BaseFrame.ScaleAll(0.5f);

            BaseTile.Bounds = new Bounds(EnvironmentObjects.BaseTileBounds_2x, BaseTile.BaseFrame);

            Hoverable = true;
            Clickable = true;

            AddBaseObject(BaseTile);
            _tileObject = BaseTile;

            Properties = new BaseTileProperties(this) 
            {
                Type = TileType.Grass,
                Classification = TileClassification.Ground
            };

            SetPosition(position);
        }

        public static implicit operator TilePoint(BaseTile tile) => tile.TilePoint;


        public bool InFog(UnitTeam team)
        {
            if (Properties.AlwaysVisible || (VisionManager.RevealAll && team == VisionManager.Scene.VisibleTeam && !Properties.MustExplore))
                return false;

            if (VisionManager.ConsolidatedVision.TryGetValue(team, out var dict))
            {
                if(dict.TryGetValue(TilePoint, out int value))
                {
                    return !(value > 0);
                }
            }

            return true;
        }

        public bool Explored(UnitTeam team)
        {
            return true;
        }

        public bool BlocksType(BlockingType type)
        {
            for(int i = 0; i < Properties.BlockingTypes.Count; i++)
            {
                if (Properties.BlockingTypes[i].HasFlag(type))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetHovered(bool hovered)
        {
            Hovered = hovered;
        }

        public void SetSelected(bool selected)
        {
            Selected = selected;

            if (selected)
            {
                _tileObject.OutlineParameters.OutlineThickness = _tileObject.OutlineParameters.BaseOutlineThickness;
                _tileObject.OutlineParameters.OutlineColor = _Colors.Blue;
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

        public override void OnHoverEnd()
        {
            if (Hovered)
            {
                base.OnHoverEnd();

                SetHovered(false);

                HoverEndEvent(this);
            }
        }

        //public void OnSteppedOn(Unit unit) 
        //{
        //    foreach(var effect in TileEffectManager.GetTileEffectsOnTilePoint(this))
        //    {
        //        effect.OnSteppedOn(unit, this);
        //    }
        //}

        //public void OnSteppedOff(Unit unit)
        //{
        //    foreach (var effect in TileEffectManager.GetTileEffectsOnTilePoint(this))
        //    {
        //        effect.OnSteppedOff(unit, this);
        //    }
        //}

        //public void OnTurnStart(Unit unit)
        //{
        //    foreach (var effect in TileEffectManager.GetTileEffectsOnTilePoint(this))
        //    {
        //        effect.OnTurnStart(unit, this);
        //    }
        //}

        //public void OnTurnEnd(Unit unit)
        //{
        //    foreach (var effect in TileEffectManager.GetTileEffectsOnTilePoint(this))
        //    {
        //        effect.OnTurnEnd(unit, this);
        //    }
        //}

        //internal void SetHeight(float height)
        //{
        //    SetPosition(new Vector3(Position.X, Position.Y, height * 0.2f));

        //    Properties.Height = height;
        //    if(Structure != null)
        //    {
        //        Structure.SetPositionOffset(new Vector3(Structure._actualPosition.X, Structure._actualPosition.Y, Position.Z));
        //        TileMapManager.Scene.RenderDispatcher.DispatchAction(TileMapManager.Scene._structureDispatchObject, TileMapManager.Scene.CreateStructureInstancedRenderData);
        //    }

        //    var unitsOnTile = UnitPositionManager.GetUnitsOnTilePoint(TilePoint);
        //    foreach(var unit in unitsOnTile)
        //    {
        //        unit.SetPositionOffset(Position);
        //    }

        //    if (!TileMapManager.Scene.ContextManager.GetFlag(GeneralContextFlags.TileMapManagerLoading))
        //    {
        //        TileMapManager.DispatchTilePillarUpdate(TileMap);
        //        TileMapManager.NavMesh.UpdateNavMeshForTile(this);
        //    }

        //    Update();
        //}

        public override void CleanUp()
        {
            base.CleanUp();
        }

        public void AddStructure<T>(T structure) where T : Structure 
        {
            if (Structure != null) 
            {
                RemoveStructure(Structure);
                //return;
            }

            TileMapManager.Scene.AddStructure(structure);

            Chunk.Structures.Add(structure);
            Structure = structure;
        }

        public void RemoveStructure<T>(T structure) where T : Structure
        {
            TileMapManager.Scene.RemoveStructure(structure);

            Chunk.Structures.Remove(structure);
            Structure = null;
        }

        public void Update()
        {
            //TileMap.UpdateTile();
        }

        public static string GetTooltipString(BaseTile tile, CombatScene scene) 
        {
            string tooltip = "";

            if (scene.CurrentUnit == null)
                return "";

            //if (tile.InFog(scene.CurrentUnit.AI.Team) && !tile.Explored(scene.CurrentUnit.AI.Team))
            //{
            //    tooltip = "Unexplored tile";
            //}
            //else 
            //{
            //    int coordX = tile.TilePoint.X + tile.TilePoint.ParentTileMap.TileMapCoords.X * tile.TilePoint.ParentTileMap.Width;
            //    int coordY = tile.TilePoint.Y + tile.TilePoint.ParentTileMap.TileMapCoords.Y * tile.TilePoint.ParentTileMap.Height;

            //    Vector3 cubeCoord = tile.TileMap.OffsetToCube(tile.TilePoint);


            //    var tileMapPos = FeatureEquation.FeaturePointToTileMapCoords(new FeaturePoint(tile));

            //    tooltip = $"Type: {tile.Properties.Type.Name()} \n";
            //    tooltip += $"Coordinates: {coordX}, {coordY} \n";
            //    tooltip += $"Offset: {cubeCoord.X}, {cubeCoord.Y}, {cubeCoord.Z} \n";
            //    tooltip += $"Tile Map: {tileMapPos.X}, {tileMapPos.Y} \n";
            //    tooltip += $"Position: {tile.BaseObject.BaseFrame.Position.X}, {tile.BaseObject.BaseFrame.Position.Y}, {tile.BaseObject.BaseFrame.Position.Z} \n";
            //    //tooltip += $"Elevation: {tile.Properties.Height}\n";
            //    //tooltip += $"Movement Cost: {tile.Properties.MovementCost}\n";

            //    if (tile.Structure != null) 
            //    {
            //        tooltip += $"Structure\n* Name: {tile.Structure.Type.Name()}\n";
            //        tooltip += $"* Height: {tile.Structure.Info.Height}\n";
            //    }
            //}

            return tooltip;
        }


        public float GetVisionHeight() 
        {
            return Structure != null && !Structure.Passable && !Structure.Info.Transparent ? Structure.Info.Height + Properties.Height : Properties.Height;
        }

        public float GetPathableHeight()
        {
            return Structure != null && Structure.Pathable && !Structure.Passable ? Structure.Info.Height + Properties.Height : Properties.Height;
        }

        public bool StructurePathable()
        {
            return Structure == null || (Structure != null && Structure.Pathable);
        }

        public override Tooltip CreateContextMenu()
        {
            //Tooltip menu = new Tooltip();

            //TextComponent header = new TextComponent();
            //header.SetTextScale(0.1f);
            //header.SetColor(_Colors.UITextBlack);
            //header.SetText("Tile " + ObjectID);

            //TextComponent description = new TextComponent();
            //description.SetTextScale(0.05f);
            //description.SetColor(_Colors.UITextBlack);
            //description.SetText(GetTooltipString(this, TileMapManager.Scene));

            //menu.AddChild(header);
            //menu.AddChild(description);

            //UIDimensions letterScale = header._textField.Letters[0].GetDimensions();

            ////initially position the objects so that the tooltip can be fitted
            //header.SetPositionFromAnchor(menu.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10 + letterScale.Y / 2, 0), UIAnchorPosition.TopLeft);
            //description.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            //menu.Margins = new UIDimensions(0, 60);

            //menu.FitContents();

            ////position the objects again once the menu has been fitted to the correct size
            //header.SetPositionFromAnchor(menu.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10 + letterScale.Y / 2, 0), UIAnchorPosition.TopLeft);
            //description.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

            //menu.Clickable = true;

            //return menu;
            return null;
        }

        public FeaturePoint ToFeaturePoint()
        {
            return new FeaturePoint(this);
        }

        public void LoadTexture()
        {
            Renderer.LoadTextureFromGameObj(this, false);
        }
    }

    public class TilePoint
    {
        public int X;
        public int Y;
        public int Layer = 0;

        public TileMap ParentTileMap;
        public TileMapPoint MapPoint;

        private bool _visited = false;
        public bool Visited 
        { 
            get => _visited; 
            set  
            {
                _visited = value;
                if (_visited)
                {
                    TileMapHelpers.AddVisitedTile(this);
                }
            }
        } //using for pathing

        public TilePoint() { }
        public TilePoint(int x, int y, TileMap map) 
        {
            X = x;
            Y = y;
            ParentTileMap = map;
            MapPoint = map?.TileMapCoords ?? TileMapPoint.Empty;
        }

        public TilePoint(Vector2i coords, TileMap map)
        {
            X = coords.X;
            Y = coords.Y;
            ParentTileMap = map;
            MapPoint = map.TileMapCoords;
        }

        public TilePoint(FeaturePoint point)
        {
            if (point.X < 0)
            {
                X = TileMapManager.TILE_MAP_DIMENSIONS.X - Math.Abs(point.X) % TileMapManager.TILE_MAP_DIMENSIONS.X;
                X = X == TileMapManager.TILE_MAP_DIMENSIONS.X ? 0 : X;
            }
            else
            {
                X = Math.Abs(point.X % TileMapManager.TILE_MAP_DIMENSIONS.X);
            }

            if (point.Y < 0)
            {
                Y = TileMapManager.TILE_MAP_DIMENSIONS.Y - Math.Abs(point.Y) % TileMapManager.TILE_MAP_DIMENSIONS.Y;
                Y = Y == TileMapManager.TILE_MAP_DIMENSIONS.Y ? 0 : Y;
            }
            else
            {
                Y = Math.Abs(point.Y) % TileMapManager.TILE_MAP_DIMENSIONS.Y;
            }

            MapPoint = point.ToTileMapPoint();
        }

        public Tile GetBaseTile() 
        {
            return ParentTileMap.GetTile(this);
        }

        public BaseTile GetTile()
        {
            throw new NotImplementedException();
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
                   Layer == point.Layer &&
                   MapPoint == point.MapPoint;
        }

        public long GetUniqueHash()
        {
            return ((long)X << 32) + Y;
        }

        public FeaturePoint ToFeaturePoint() 
        {
            return new FeaturePoint(this);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, ParentTileMap);
        }
    }

    [Flags]
    public enum BlockingType
    {
        None,
        Vision,
        Abilities,
    }

    public class BaseTileProperties 
    {
        private TileType _type;
        public TileType Type 
        {
            get => _type;
            set 
            {
                _type = value;
                BaseTile.BaseObject._currentAnimation.CurrentFrame.SpritesheetPosition = (int)value;
            } 
        }

        public TileClassification Classification;

        public List<BlockingType> BlockingTypes = new List<BlockingType>();

        public bool MustExplore = false;
        public bool AlwaysVisible = false;

        public float DamageOnEnter = 0;
        public float Slow = 0;
        public bool BlocksVision = false;
        public float Height = 0; //the tile's height for vision and movement purposes
        public float MovementCost = 1; //how expensive this tile is to move across compared to normal

        public BaseTile BaseTile;
        public BaseTileProperties(BaseTile tile) 
        {
            BaseTile = tile;
        }
    }

    
}
