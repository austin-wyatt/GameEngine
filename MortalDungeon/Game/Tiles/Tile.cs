using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Abilities.AbilityDefinitions;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles.Meshes;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MortalDungeon.Game.Tiles
{
    public class Tile : IHoverable, IHasPosition
    {
        public TilePoint TilePoint;

        public TileProperties Properties;

        public Vector4 Color = _Colors.White; //color that will be applied to the tile on the dynamic texture

        public bool Selected = false;

        public Structure Structure;

        public TileMap TileMap;

        public TileChunk Chunk;

        public bool HasContextMenu = true;

        public Vector3 Position { get; set; }

        public bool Hovered = false;

        public bool HasTimedHoverEffect = false;

        public TileBounds TileBounds = new TileBounds();

        /// <summary>
        /// Assigned when the MeshChunk is created by the TileChunk
        /// </summary>
        public MeshTile MeshTileHandle = null;

        public Tile()
        {

        }
        public Tile(Vector3 position, TilePoint point)
        {
            TilePoint = point;

            //BaseTile.Bounds = new Bounds(EnvironmentObjects.BaseTileBounds_2x, BaseTile.BaseFrame);

            Properties = new TileProperties(this)
            {
                Type = TileType.Grass,
                Classification = TileClassification.Ground
            };

            SetPosition(position);
        }

        public static implicit operator TilePoint(Tile tile) => tile.TilePoint;


        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public bool InFog(UnitTeam team)
        {
            if (Properties.AlwaysVisible || (VisionManager.RevealAll && team == VisionManager.Scene.VisibleTeam && !Properties.MustExplore))
                return false;

            if (VisionManager.ConsolidatedVision.TryGetValue(team, out var dict))
            {
                if (dict.TryGetValue(TilePoint, out int value))
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
            for (int i = 0; i < Properties.BlockingTypes.Count; i++)
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
        }

        public void OnHover()
        {
            if (!Hovered)
            {
                SetHovered(true);
                HoverEvent(this);
            }
        }

        public void OnHoverEnd()
        {
            if (Hovered)
            {
                SetHovered(false);
                HoverEndEvent(this);
            }
        }

        public void OnTimedHover()
        {
            TimedHoverEvent(this);
        }

        #region Event actions
        public delegate void TileEventHandler(Tile obj);

        public event TileEventHandler OnCleanUp;
        public event TileEventHandler HoverEnd;
        public event TileEventHandler Hover;
        public event TileEventHandler TimedHover;

        protected void HoverEndEvent(Tile obj)
        {
            HoverEnd?.Invoke(obj);
        }

        protected void CleanUpEvent(Tile obj)
        {
            OnCleanUp?.Invoke(obj);
        }

        protected void HoverEvent(Tile obj)
        {
            Hover?.Invoke(obj);
        }

        protected void TimedHoverEvent(Tile obj)
        {
            TimedHover?.Invoke(obj);
        }

        #endregion

        public void OnSteppedOn(Unit unit)
        {
            foreach (var effect in TileEffectManager.GetTileEffectsOnTilePoint(this))
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

        internal void SetHeight(float height)
        {
            SetPosition(new Vector3(Position.X, Position.Y, height * 0.2f));

            Properties.Height = height;
            if (Structure != null)
            {
                Structure.SetPositionOffset(new Vector3(Structure._actualPosition.X, Structure._actualPosition.Y, Position.Z));
                TileMapManager.Scene.RenderDispatcher.DispatchAction(TileMapManager.Scene._structureDispatchObject, TileMapManager.Scene.CreateStructureInstancedRenderData);
            }

            var unitsOnTile = UnitPositionManager.GetUnitsOnTilePoint(TilePoint);
            foreach (var unit in unitsOnTile)
            {
                unit.SetPositionOffset(Position);
            }

            if (!TileMapManager.Scene.ContextManager.GetFlag(GeneralContextFlags.TileMapManagerLoading))
            {
                TileMapManager.DispatchTilePillarUpdate(TileMap);
                TileMapManager.NavMesh.UpdateNavMeshForTile(this);
            }

            Update();
        }

        public void CleanUp()
        {
            CleanUpEvent(this);
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
            Chunk.UpdateTile();
            //TileMap.UpdateTile();
        }

        //public static string GetTooltipString(BaseTile tile, CombatScene scene)
        //{
        //    string tooltip;

        //    if (scene.CurrentUnit == null)
        //        return "";

        //    if (tile.InFog(scene.CurrentUnit.AI.Team) && !tile.Explored(scene.CurrentUnit.AI.Team))
        //    {
        //        tooltip = "Unexplored tile";
        //    }
        //    else
        //    {
        //        int coordX = tile.TilePoint.X + tile.TilePoint.ParentTileMap.TileMapCoords.X * tile.TilePoint.ParentTileMap.Width;
        //        int coordY = tile.TilePoint.Y + tile.TilePoint.ParentTileMap.TileMapCoords.Y * tile.TilePoint.ParentTileMap.Height;

        //        Vector3 cubeCoord = tile.TileMap.OffsetToCube(tile.TilePoint);

        //        var tileMapPos = FeatureEquation.FeaturePointToTileMapCoords(new FeaturePoint(tile));

        //        tooltip = $"Type: {tile.Properties.Type.Name()} \n";
        //        tooltip += $"Coordinates: {coordX}, {coordY} \n";
        //        tooltip += $"Offset: {cubeCoord.X}, {cubeCoord.Y}, {cubeCoord.Z} \n";
        //        tooltip += $"Tile Map: {tileMapPos.X}, {tileMapPos.Y} \n";
        //        tooltip += $"Position: {tile.BaseObject.BaseFrame.Position.X}, {tile.BaseObject.BaseFrame.Position.Y}, {tile.BaseObject.BaseFrame.Position.Z} \n";
        //        //tooltip += $"Elevation: {tile.Properties.Height}\n";
        //        //tooltip += $"Movement Cost: {tile.Properties.MovementCost}\n";

        //        if (tile.Structure != null)
        //        {
        //            tooltip += $"Structure\n* Name: {tile.Structure.Type.Name()}\n";
        //            tooltip += $"* Height: {tile.Structure.Info.Height}\n";
        //        }
        //    }

        //    return tooltip;
        //}


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

        public void OnRightClick(ContextManager<Scene.MouseUpFlags> flags)
        {
            CombatScene scene = TileMapManager.Scene;

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
                    //Name = Properties.Type.Name();
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
                        //TODO
                        //unit.Info._movementAbility.MoveToTile(this);
                    }
                }
                else
                {
                    (Tooltip moveMenu, UIList moveList) = UIHelpers.GenerateContextMenuWithList("Move");

                    Unit unit = scene._selectedUnits[0]; ;
                    moveList.AddItem("Move here", (item) =>
                    {
                        scene.CloseContextMenu();
                        //TODO
                        //unit.Info._movementAbility.MoveToTile(this);
                    });

                    scene.OpenContextMenu(moveMenu);
                }
            }
            #endregion


            Sound sound = new Sound(Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(0.6f, 0.6f) };
            sound.Play();
        }

        public void SetColor(Vector4 color)
        {
            Color = color;
        }

        //public override Tooltip CreateContextMenu()
        //{
        //    Tooltip menu = new Tooltip();

        //    TextComponent header = new TextComponent();
        //    header.SetTextScale(0.1f);
        //    header.SetColor(_Colors.UITextBlack);
        //    header.SetText("Tile " + ObjectID);

        //    TextComponent description = new TextComponent();
        //    description.SetTextScale(0.05f);
        //    description.SetColor(_Colors.UITextBlack);
        //    description.SetText(GetTooltipString(this, TileMapManager.Scene));

        //    menu.AddChild(header);
        //    menu.AddChild(description);

        //    UIDimensions letterScale = header._textField.Letters[0].GetDimensions();

        //    //initially position the objects so that the tooltip can be fitted
        //    header.SetPositionFromAnchor(menu.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10 + letterScale.Y / 2, 0), UIAnchorPosition.TopLeft);
        //    description.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

        //    menu.Margins = new UIDimensions(0, 60);

        //    menu.FitContents();

        //    //position the objects again once the menu has been fitted to the correct size
        //    header.SetPositionFromAnchor(menu.GetAnchorPosition(UIAnchorPosition.TopLeft) + new Vector3(10, 10 + letterScale.Y / 2, 0), UIAnchorPosition.TopLeft);
        //    description.SetPositionFromAnchor(header.GetAnchorPosition(UIAnchorPosition.BottomLeft) + new Vector3(0, 20, 0), UIAnchorPosition.TopLeft);

        //    menu.Clickable = true;

        //    return menu;
        //}

        public FeaturePoint ToFeaturePoint()
        {
            return new FeaturePoint(this);
        }
    }

    public class TileProperties
    {
        private TileType _type;
        public TileType Type { 
            get => _type; 
            set
            {
                _type = value;
                DisplayInfo = new TileDisplayInfo(_type);
                Tile.Update();
            } 
        }

        public TileDisplayInfo DisplayInfo = new TileDisplayInfo();

        public TileClassification Classification;

        public List<BlockingType> BlockingTypes = new List<BlockingType>();

        public bool MustExplore = false;
        public bool AlwaysVisible = false;

        public float DamageOnEnter = 0;
        public float Slow = 0;
        public bool BlocksVision = false;
        public float Height = 0; //the tile's height for vision and movement purposes
        public float MovementCost = 1; //how expensive this tile is to move across compared to normal

        public Tile Tile;
        public TileProperties(Tile tile)
        {
            Tile = tile;
        }
    }

    public struct TileDisplayInfo
    {
        public int SpritesheetPos;
        public Spritesheet TileSpritesheet;
        public SimpleTexture Texture;

        public TileDisplayInfo(TileType type)
        {
            SpritesheetPos = (int)type % 25;
            TileSpritesheet = Spritesheets.TileSheets[(int)type / 25];
            Texture = new SimpleTexture(TileSpritesheet)
            {
                GenerateMipMaps = false,
                Nearest = true
            };
        }
    }

    public class TileBounds : IBounds
    {
        /// <summary>
        /// The maximum dimensions of a mesh tile
        /// </summary>
        public Vector3 TileDimensions;


        public bool Contains(Vector2 point, Camera camera = null)
        {
            throw new NotImplementedException();
        }

        public bool Contains3D(Vector3 pointNear, Vector3 pointFar, Camera camera)
        {
            throw new NotImplementedException();
        }

        public Vector3 GetDimensionData()
        {
            throw new NotImplementedException();
        }

        public PointF GetTransformedPoint(float x, float y, float z, Camera camera = null)
        {
            throw new NotImplementedException();
        }
    }
}


