using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Abilities.AbilityDefinitions;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles.HelperTiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
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
        Selection = 2,

        Stone_1 = 20,
        Stone_2 = 21,
        Stone_3 = 22,
        Gravel = 23,
        WoodPlank = 24,

        Default = 41,
        Grass = 42,
        AltGrass = 43,
        Water = 44,
        AltWater = 45,
        Outline = 40,

        Dirt = 63,
        Grass_2 = 64,
        Dead_Grass = 65,

        Fog_1 = 160,
        Fog_2 = 161,
        Fog_3 = 180,
        Fog_4 = 181,
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
        public Vector4 DefaultColor = default;
        public BaseTileAnimationType DefaultAnimation = BaseTileAnimationType.SolidWhite;
        public BaseTileAnimationType CurrentAnimation = BaseTileAnimationType.SolidWhite;
        public TilePoint TilePoint;

        public new ObjectType ObjectType = ObjectType.Tile;

        public TileProperties Properties;

        public Vector4 Color = _Colors.White; //color that will be applied to the tile on the dynamic texture
        public Vector4 OutlineColor = _Colors.Black; //outline color that will be applied to the dynamic texture
        public bool Outline = false; //whether the tile should be outline on the dynamic texture
        public bool NeverOutline = false; //whether this tile should never be outlined (used for contiguous tiles like water)

        //public Dictionary<UnitTeam, bool> InFog = new Dictionary<UnitTeam, bool>();
        public bool Selected = false;

        //public Dictionary<UnitTeam, bool> Explored = new Dictionary<UnitTeam, bool>();

        private Vector4 _fogColorOffset = new Vector4(0.5f, 0.5f, 0.5f, 0);

        public BaseObject _tileObject;

        public Structure Structure;

        public Cliff Cliff;

        public HeightIndicatorTile HeightIndicator;
        public TileMap TileMap;

        public TileChunk Chunk;

        public bool Updating = false;


        public new bool HasContextMenu = true;

        public BaseTile()
        {
            //FillExploredDictionary();
        }
        public BaseTile(Vector3 position, TilePoint point)
        {
            Name = "Tile";
            TilePoint = point;

            //BaseObject BaseTile = new BaseObject(BASE_TILE_ANIMATION.List, ObjectID, "Base Tile " + ObjectID, default, EnvironmentObjects.BASE_TILE.Bounds);
            //DefaultColor = BaseTile.BaseFrame.BaseColor;
            //BaseTile.BaseFrame.CameraPerspective = true;

            BaseObject BaseTile = _3DObjects.CreateBaseObject(new SpritesheetObject((int)TileType.Grass, Spritesheets.TileSheet), _3DObjects.Hexagon, default);
            DefaultColor = _Colors.White;
            BaseTile.BaseFrame.CameraPerspective = true;
            BaseTile.BaseFrame.ScaleAll(0.5f);
            //BaseTile.BaseFrame.RotateZ(-30);

            BaseTile.Bounds = new Bounds(EnvironmentObjects.BaseTileBounds_2x, BaseTile.BaseFrame);

            Hoverable = true;
            Clickable = true;

            AddBaseObject(BaseTile);
            _tileObject = BaseTile;

            Properties = new TileProperties(this) 
            {
                Type = TileType.Grass,
                Classification = TileClassification.Ground
            };

            //_tileObject.BaseFrame.ScaleAddition(1);

            //FillExploredDictionary();
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


        public bool InFog(UnitTeam team)
        {
            if (Properties.AlwaysVisible || (VisionManager.RevealAll && team == VisionManager.Scene.VisibleTeam && !Properties.MustExplore))
                return false;

            if (VisionManager.ConsolidatedVision.TryGetValue(team, out var dict))
            {
                if(dict.TryGetValue(TilePoint, out bool value))
                {
                    return !value;
                }
            }

            return true;
        }

        public bool InVision(UnitTeam team)
        {
            return !InFog(team);
        }

        public bool Explored(UnitTeam team)
        {
            return true;
        }


        public void SetExplored(bool explored = true, UnitTeam team = UnitTeam.PlayerUnits)
        {
            //if (explored != Explored[team])
            //{
            //    Explored[team] = explored;

            //    if(team == GetScene().VisibleTeam)
            //    {
            //        Update();
            //    }
            //}
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

        public void OnSteppedOn(Unit unit) 
        {
            foreach(var effect in TileEffectManager.GetTileEffectsOnTilePoint(this))
            {
                effect.OnSteppedOn(unit, this);
            }
        }

        public void OnSteppedOff(Unit unit)
        {
            foreach (var effect in TileEffectManager.GetTileEffectsOnTilePoint(this))
            {
                effect.OnSteppedOff(unit, this);
            }
        }

        public void OnTurnStart(Unit unit)
        {
            foreach (var effect in TileEffectManager.GetTileEffectsOnTilePoint(this))
            {
                effect.OnTurnStart(unit, this);
            }
        }

        public void OnTurnEnd(Unit unit)
        {
            foreach (var effect in TileEffectManager.GetTileEffectsOnTilePoint(this))
            {
                effect.OnTurnEnd(unit, this);
            }
        }

        public override void CleanUp()
        {
            base.CleanUp();

            GetScene().Tick -= Tick;

            if (Structure != null) 
            {
                Structure.CleanUp();
                RemoveStructure(Structure);
            }

            foreach(var unit in UnitPositionManager.GetUnitsOnTilePoint(TilePoint))
            {
                GetScene().RemoveUnit(unit);
                unit?.CleanUp();
            }
        }

        public void AddStructure<T>(T structure) where T : Structure 
        {
            if (Structure != null) 
            {
                RemoveStructure(Structure);
                //return;
            }

            GetScene().AddStructure(structure);

            Chunk.Structures.Add(structure);
            Structure = structure;
        }

        public void RemoveStructure<T>(T structure) where T : Structure
        {
            GetScene().RemoveStructure(structure);

            Chunk.Structures.Remove(structure);
            Structure = null;
        }

        public void Update()
        {
            TileMap.UpdateTile(this);
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

            if (scene.CurrentUnit == null)
                return "";

            if (tile.InFog(scene.CurrentUnit.AI.Team) && !tile.Explored(scene.CurrentUnit.AI.Team))
            {
                tooltip = "Unexplored tile";
            }
            else 
            {
                int coordX = tile.TilePoint.X + tile.TilePoint.ParentTileMap.TileMapCoords.X * tile.TilePoint.ParentTileMap.Width;
                int coordY = tile.TilePoint.Y + tile.TilePoint.ParentTileMap.TileMapCoords.Y * tile.TilePoint.ParentTileMap.Height;

                Vector3 cubeCoord = tile.TileMap.OffsetToCube(tile.TilePoint);

                var tileMapPos = FeatureEquation.FeaturePointToTileMapCoords(new FeaturePoint(tile));

                tooltip = $"Type: {tile.Properties.Type.Name()} \n";
                tooltip += $"Coordinates: {coordX}, {coordY} \n";
                tooltip += $"Offset: {cubeCoord.X}, {cubeCoord.Y}, {cubeCoord.Z} \n";
                tooltip += $"Tile Map: {tileMapPos.X}, {tileMapPos.Y} \n";
                tooltip += $"Position: {tile.BaseObject.BaseFrame.Position.X}, {tile.BaseObject.BaseFrame.Position.Y}, {tile.BaseObject.BaseFrame.Position.Z} \n";
                //tooltip += $"Elevation: {tile.Properties.Height}\n";
                //tooltip += $"Movement Cost: {tile.Properties.MovementCost}\n";

                if (tile.Structure != null) 
                {
                    tooltip += $"Structure\n* Name: {tile.Structure.Type.Name()}\n";
                    tooltip += $"* Height: {tile.Structure.Info.Height}\n";
                }
            }

            return tooltip;
        }


        public int GetVisionHeight() 
        {
            return Structure != null && !Structure.Passable && !Structure.Info.Transparent ? Structure.Info.Height + Properties.Height : Properties.Height;
        }

        public int GetPathableHeight()
        {
            return Structure != null && Structure.Pathable && !Structure.Passable ? Structure.Info.Height + Properties.Height : Properties.Height;
        }

        public bool StructurePathable()
        {
            return Structure == null || (Structure != null && Structure.Pathable);
        }

        public CombatScene GetScene() 
        {
            return TilePoint.ParentTileMap.Controller.Scene;
        }

        public void OnRightClick(ContextManager<MouseUpFlags> flags)
        {
            CombatScene scene = GetScene();

            bool isCurrentUnit = false;
            if (scene.CurrentUnit != null)
            {
                int distance = TileMap.GetDistanceBetweenPoints(scene.CurrentUnit.Info.Point, TilePoint);
                isCurrentUnit = scene.CurrentUnit.AI.ControlType == ControlType.Controlled;

                int interactDistance = 5;
                int inspectDistance = 10;

                List<GameObject> objects = new List<GameObject>();

                if (Structure != null && Structure.HasContextMenu && distance <= interactDistance && isCurrentUnit)
                {
                    objects.Add(Structure);
                }

                foreach (var unit in UnitPositionManager.GetUnitsOnTilePoint(TilePoint))
                {
                    if (unit.HasContextMenu && distance <= inspectDistance && isCurrentUnit)
                    {
                        objects.Add(unit);
                    }
                }
                    

                if (HasContextMenu && isCurrentUnit)
                {
                    Name = Properties.Type.Name();
                    //objects.Add(this);
                }
            }

            #region right click movement
            if (scene._selectedUnits.Count > 1)
            {
                var mainUnit = scene._selectedUnits.Find(u => u.AI.ControlType == ControlType.Controlled);

                if (mainUnit != null)
                {
                    if (scene.ContextManager.GetFlag(GeneralContextFlags.RightClickMovementEnabled))
                    {
                        GroupMove groupMove = new GroupMove(mainUnit);
                        groupMove.OnTileClicked(TileMap, this);

                        //if (unit.Info._movementAbility.Moving)
                        //{
                        //    unit.Info._movementAbility.CancelMovement();
                        //}
                        //else
                        //{
                        //    unit.Info._movementAbility.MoveToTile(this);
                        //}
                    }
                    else
                    {
                        (Tooltip moveMenu, UIList moveList) = UIHelpers.GenerateContextMenuWithList("Move");

                        moveList.AddItem("Move here", (item) =>
                        {
                            scene.CloseContextMenu();
                            GroupMove groupMove = new GroupMove(mainUnit);
                            groupMove.OnTileClicked(TileMap, this);
                        });

                        scene.OpenContextMenu(moveMenu);
                    }
                }
            }
            else if (scene._selectedUnits.Count == 1 && isCurrentUnit) 
            {
                if (scene.ContextManager.GetFlag(GeneralContextFlags.RightClickMovementEnabled))
                {
                    Unit unit = scene._selectedUnits[0];
                    if (unit.Info._movementAbility.Moving)
                    {
                        unit.Info._movementAbility.CancelMovement();
                    }
                    else
                    {
                        unit.Info._movementAbility.MoveToTile(this);
                    }
                }
                else 
                {
                    (Tooltip moveMenu, UIList moveList) = UIHelpers.GenerateContextMenuWithList("Move");

                    Unit unit = scene._selectedUnits[0]; ;
                    moveList.AddItem("Move here", (item) =>
                    {
                        scene.CloseContextMenu();
                        unit.Info._movementAbility.MoveToTile(this);
                    });

                    scene.OpenContextMenu(moveMenu);
                }
            }
            #endregion


            Sound sound = new Sound(Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(0.6f, 0.6f) };
            sound.Play();

            scene._debugSelectedTile = this;
        }

        public override Tooltip CreateContextMenu()
        {
            Tooltip menu = new Tooltip();

            TextComponent header = new TextComponent();
            header.SetTextScale(0.1f);
            header.SetColor(_Colors.UITextBlack);
            header.SetText("Tile " + ObjectID);

            TextComponent description = new TextComponent();
            description.SetTextScale(0.05f);
            description.SetColor(_Colors.UITextBlack);
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

        public TileMap ParentTileMap;

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

    public class TileProperties 
    {
        private TileType _type;
        public TileType Type 
        {
            get => _type;
            set 
            {
                _type = value;
                Tile.BaseObject._currentAnimation.CurrentFrame.SpritesheetPosition = (int)value;
            } 
        }

        public TileClassification Classification;

        public List<TileOverlay> TileOverlays = new List<TileOverlay>();

        public bool MustExplore = false;
        public bool AlwaysVisible = false;

        public float DamageOnEnter = 0;
        public float Slow = 0;
        public bool BlocksVision = false;
        public int Height = 0; //the tile's height for vision and movement purposes
        public float MovementCost = 1; //how expensive this tile is to move across compared to normal

        public BaseTile Tile;
        public TileProperties(BaseTile tile) 
        {
            Tile = tile;
        }
    }

    
}
