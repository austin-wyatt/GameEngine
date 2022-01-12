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
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Structures;
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

        public Dictionary<UnitTeam, bool> InFog = new Dictionary<UnitTeam, bool>();
        public bool Selected = false;

        public Dictionary<UnitTeam, bool> Explored = new Dictionary<UnitTeam, bool>();

        private Vector4 _fogColorOffset = new Vector4(0.5f, 0.5f, 0.5f, 0);

        public BaseObject _tileObject;

        public BaseTile AttachedTile; //for selection tiles 
        public Structure Structure;
        public Unit UnitOnTile;

        public Cliff Cliff;

        public HeightIndicatorTile HeightIndicator;
        public TileMap TileMap;

        public TileChunk Chunk;

        public bool Updating = false;

        public List<TileEffect> TileEffects = new List<TileEffect>();


        public new bool HasContextMenu = true;

        public BaseTile()
        {
            FillExploredDictionary();
        }
        public BaseTile(Vector3 position, TilePoint point)
        {
            Name = "Tile";
            TilePoint = point;

            BaseObject BaseTile = new BaseObject(BASE_TILE_ANIMATION.List, ObjectID, "Base Tile " + ObjectID, default, EnvironmentObjects.BASE_TILE.Bounds);
            DefaultColor = BaseTile.BaseFrame.BaseColor;
            BaseTile.BaseFrame.CameraPerspective = true;

            BaseTile.OutlineParameters.SetAllInline(2);
            BaseTile.OutlineParameters.InlineColor = _Colors.Black;
            BaseTile.OutlineParameters.OutlineColor = _Colors.Red;

            Hoverable = true;
            Clickable = true;

            AddBaseObject(BaseTile);
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
                InFog.Add(team, true);
            }
        }


        public void SetFog(bool inFog, UnitTeam team = UnitTeam.PlayerUnits)
        {
            if (inFog != InFog[team])
            {
                InFog[team] = inFog;

                Update();

                if (inFog && !Explored[team])
                {
                    Outline = false;
                }
                else
                {
                    Outline = !NeverOutline;
                }
            }
        }

        public bool InVision(UnitTeam team)
        {
            Vector2i clusterPos = GetScene()._tileMapController.PointToClusterPosition(this);

            return VisionMap.InVision(clusterPos.X, clusterPos.Y, team);
        }

        public void SetExplored(bool explored = true, UnitTeam team = UnitTeam.PlayerUnits)
        {
            if (explored != Explored[team])
            {
                Explored[team] = explored;

                Update();
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
            for(int i = 0; i < TileEffects.Count; i++) 
            {
                TileEffects[i].OnSteppedOn(unit, this);
            }
        }

        public void OnTurnStart(Unit unit)
        {
            for (int i = 0; i < TileEffects.Count; i++)
            {
                TileEffects[i].OnTurnStart(unit, this);
            }
        }

        public void OnTurnEnd(Unit unit)
        {
            for (int i = 0; i < TileEffects.Count; i++)
            {
                TileEffects[i].OnTurnEnd(unit, this);
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

            if (UnitOnTile != null) 
            {
                GetScene().RemoveUnit(UnitOnTile);
                UnitOnTile.CleanUp();
                UnitOnTile = null;
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
            if (Updating) return;

            Updating = true;
            TileMap.UpdateTile(this);

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

            if (scene.CurrentUnit == null)
                return "";

            if (tile.InFog[scene.CurrentUnit.AI.Team] && !tile.Explored[scene.CurrentUnit.AI.Team])
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
                isCurrentUnit = scene.CurrentUnit.AI.Team == UnitTeam.PlayerUnits && scene.CurrentUnit.AI.ControlType == ControlType.Controlled;

                int interactDistance = 5;
                int inspectDistance = 10;

                List<GameObject> objects = new List<GameObject>();

                if (Structure != null && Structure.HasContextMenu && distance <= interactDistance && isCurrentUnit)
                {
                    objects.Add(Structure);
                }

                if (UnitOnTile != null && UnitOnTile.HasContextMenu && distance <= inspectDistance && isCurrentUnit)
                {
                    objects.Add(UnitOnTile);
                }

                if (HasContextMenu && isCurrentUnit)
                {
                    Name = Properties.Type.Name();
                    //objects.Add(this);
                }
            }

            #region right click movement
            if (scene.CurrentUnit != null && isCurrentUnit) 
            {
                if (scene.ContextManager.GetFlag(GeneralContextFlags.RightClickMovementEnabled))
                {
                    Unit unit = scene.CurrentUnit;
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

                    Unit unit = scene.CurrentUnit;
                    moveList.AddItem("Move here", (item) =>
                    {
                        scene.CloseContextMenu();
                        unit.Info._movementAbility.MoveToTile(this);
                    });

                    scene.OpenContextMenu(moveMenu);
                }
            }
            #endregion


            //if (objects.Count > 1)
            //{
            //    (Tooltip menu, UIList list) = UIHelpers.GenerateContextMenuWithList("Options");

            //    for (int i = 0; i < objects.Count; i++)
            //    {
            //        int index = i;
            //        list.AddItem(objects[i].Name, (item) =>
            //        {
            //            scene.CloseContextMenu();
            //            scene.OpenContextMenu(objects[index].CreateContextMenu());

            //            Sound sound = new Sound(Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(0.75f, 0.75f) };
            //            sound.Play();
            //        });
            //    }

            //    scene.OpenContextMenu(menu);
            //}
            //else if (objects.Count == 1) 
            //{
            //    scene.CloseContextMenu();
            //    scene.OpenContextMenu(objects[0].CreateContextMenu());
            //}

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

        public bool _visited = false; //using for pathing

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
        public TileType Type;
        public TileClassification Classification;

        public List<TileOverlay> TileOverlays = new List<TileOverlay>();

        public bool MustExplore = false;

        public float DamageOnEnter = 0;
        public float Slow = 0;
        public bool BlocksVision = false;
        public int Height = 0; //the tile's height for vision and movement purposes
        public float MovementCost = 1; //how expensive this tile is to move across compared to normal

        public TileProperties() 
        {
        }
    }

    
}
