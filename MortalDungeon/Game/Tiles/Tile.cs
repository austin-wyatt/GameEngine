using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities;
using Empyrean.Game.Abilities.AbilityDefinitions;
using Empyrean.Game.Map;
using Empyrean.Game.Structures;
using Empyrean.Game.Tiles.Meshes;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Empyrean.Game.Tiles
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

        public Vector3 _position = new Vector3();
        public Vector3 Position { get => _position; set => _position = value; }

        public bool Hovered = false;

        public bool HasTimedHoverEffect = false;

        public TileBounds TileBounds;

        /// <summary>
        /// Assigned when the MeshChunk is created by the TileChunk
        /// </summary>
        public MeshTile MeshTileHandle = null;

        public Tile()
        {
            TileBounds = new TileBounds(this);
        }
        public Tile(Vector3 position, TilePoint point)
        {
            TilePoint = point;

            //BaseTile.Bounds = new Bounds(EnvironmentObjects.BaseTileBounds_2x, BaseTile.BaseFrame);

            Properties = new TileProperties(this)
            {
                Classification = TileClassification.Ground
            };

            Properties.SetType(TileType.Grass);

            TileBounds = new TileBounds(this);

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

        public Vector4 HoverColor = new Vector4(1, 0, 0, 1f);
        public float HoverMixPercent = 0.5f;
        public const float BaseHoverMixPercent = 0.5f;
        public void SetHovered(bool hovered)
        {
            Hovered = hovered;

            if (!Hovered)
            {
                HoverMixPercent = BaseHoverMixPercent;
            }

            CalculateDisplayedColor();
        }

        public Vector4 SelectionColor = new Vector4(0, 0, 1, 1f);
        public float SelectionMixPercent = 0.5f;
        public const float BaseSelectionMixPercent = 0.5f;
        public void SetSelected(bool selected)
        {
            Selected = selected;

            if (!Selected)
            {
                SelectionMixPercent = BaseSelectionMixPercent;
            }

            CalculateDisplayedColor();
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
            SetPosition(new Vector3(Position.X, Position.Y, height));

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

            MeshTileHandle?.SetHeight(height);
            Update(TileUpdateType.Vertex);

            if (!TileMapManager.Scene.ContextManager.GetFlag(GeneralContextFlags.TileMapManagerLoading))
            {
                TileMapManager.NavMesh.UpdateNavMeshForTile(this);
            }
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

        public void Update(TileUpdateType updateType)
        {
            Chunk?.Update(updateType);
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

            bool playSound = true;

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

                //if (mainUnit != null)
                //{
                //    if (scene.ContextManager.GetFlag(GeneralContextFlags.RightClickMovementEnabled))
                //    {
                //        Move move = new Move(mainUnit);
                //        move.EvaluateHoverPath(this, TileMap, ignoreRange: true, highlightTiles: false);
                //        move.OnTileClicked(TileMap, this);
                //    }
                //    else
                //    {
                //        (Tooltip moveMenu, UIList moveList) = UIHelpers.GenerateContextMenuWithList("Move");

                //        moveList.AddItem("Move here", (item) =>
                //        {
                //            scene.CloseContextMenu();
                //            Move move = new Move(mainUnit);
                //            move.EvaluateHoverPath(this, TileMap, ignoreRange: true, highlightTiles: false);
                //            move.OnTileClicked(TileMap, this);
                //        });

                //        scene.OpenContextMenu(moveMenu);
                //    }
                //}
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
                        playSound = false;

                        unit.Info._movementAbility._immediateHoverTile = this;
                        unit.Scene.SelectAbility(unit.Info._movementAbility, unit);
                        //unit.Info._movementAbility.EvaluateHoverPath(this, TileMap, ignoreRange: true, highlightTiles: false);
                        //unit.Info._movementAbility.OnTileClicked(TileMap, this);
                    }
                }
                else
                {
                    (Tooltip moveMenu, UIList moveList) = UIHelpers.GenerateContextMenuWithList("Move");

                    Unit unit = scene._selectedUnits[0]; ;
                    moveList.AddItem("Move here", (item) =>
                    {
                        scene.CloseContextMenu();
                        unit.Info._movementAbility.EvaluateHoverPath(this, TileMap, ignoreRange: true, highlightTiles: false);
                        unit.Info._movementAbility.OnTileClicked(TileMap, this);
                    });

                    scene.OpenContextMenu(moveMenu);
                }
            }
            #endregion


            if (playSound)
            {
                Sound sound = new Sound(Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(0.6f, 0.6f) };
                sound.Play();
            }
        }

        public void CalculateDisplayedColor()
        {
            if(Hovered && Selected)
            {
                SetColor((HoverColor + SelectionColor) * 0.5f, SetColorFlag.Hover, (HoverMixPercent + SelectionMixPercent) * 0.5f);
            }
            else if (Hovered)
            {
                SetColor(HoverColor, SetColorFlag.Hover, HoverMixPercent);
            }
            else if (Selected)
            {
                SetColor(SelectionColor, SetColorFlag.Selected, SelectionMixPercent);
            }
            else
            {
                SetColor(Color);
            }
        }

        public float ColorMixPercent = 0;
        public void SetColor(Vector4 color, SetColorFlag flag = SetColorFlag.Base, float mixPercent = 0)
        {
            if(flag == SetColorFlag.Base)
            {
                Color = color;
                ColorMixPercent = mixPercent;
            }
            
            MeshTileHandle?.SetColor(ref color, mixPercent);
            Update(TileUpdateType.Vertex);
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

        public void ToFeaturePoint(ref FeaturePoint featurePoint)
        {
            featurePoint.Initialize(this);
        }

        public static ObjectPool<List<Tile>> TileListPool = new ObjectPool<List<Tile>>();
    }

    public class TileProperties
    {
        public TileType Type = (TileType)(-1);

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

        /// <summary>
        /// Sets the type of the tile
        /// </summary>
        /// <param name="type"></param>
        /// <param name="loadTexture">
        /// Indicates whether the texture should be loaded if the type is on a different spritesheet
        /// </param>
        /// <param name="fromFeature">
        /// Indicates whether the type was set from a feature equation. If both this and loadTexture
        /// are true, the tile will be added to the TileMapManager.TilesRequiringTextureUpdates list
        /// which is resolved at the end of the tile map load cycle.
        /// </param>
        /// <param name="updateChunk">
        /// Indicates whether the chunk should be updated with the new texture info
        /// </param>
        public void SetType(TileType type, bool loadTexture = true, bool fromFeature = false, bool updateChunk = true)
        {
            void UpdateChunk()
            {
                if (fromFeature)
                {
                    TileMapManager.TilesRequiringTextureUpdates.Add(Tile);
                }
                else if (Tile.Chunk != null)
                {
                    Tile.MeshTileHandle.UpdateTextureInfo();
                    Tile.Update(TileUpdateType.Textures);
                }
            }

            if (Type == type)
                return;

            Type = type;

            if (DisplayInfo.TileSpritesheet == null)
            {
                DisplayInfo = new TileDisplayInfo(Type);
            }
            else
            {
                DisplayInfo.SetDisplayInfo(Type, out bool textureLoadRequired);

                if(loadTexture && textureLoadRequired)
                {
                    TextureLoadBatcher.LoadTexture(DisplayInfo.Texture);

                    if (updateChunk)
                    {
                        UpdateChunk();
                    }
                }
                else if(updateChunk)
                {
                    UpdateChunk();
                }
            }
        }
    }

    public struct TileDisplayInfo
    {
        public Spritesheet TileSpritesheet;
        public SimpleTexture Texture;

        public TileDisplayInfo(TileType type)
        {
            TileSpritesheet = TileSheets[type];
            Texture = new SimpleTexture(TileSpritesheet)
            {
                GenerateMipMaps = true,
                Nearest = true
            };
        }

        public void SetDisplayInfo(TileType type, out bool textureLoadRequired)
        {
            TileSpritesheet = TileSheets[type];
            Texture = new SimpleTexture(TileSpritesheet)
            {
                GenerateMipMaps = true,
                Nearest = true
            };

            textureLoadRequired = true;
        }

        public static Dictionary<TileType, Spritesheet> TileSheets = new Dictionary<TileType, Spritesheet>() 
        {
            { TileType.Dirt, Textures.Dirt },
            { TileType.Grass, Textures.Grass },
            { TileType.Stone_1, Textures.Stone_1 },
            { TileType.Stone_2, Textures.Stone_2 },

        };
    }

    public class TileBounds : IBounds
    {
        public static readonly Vector3 TileDimensions = 
            new Vector3(
                1 * WindowConstants.ScreenUnits.X * 0.5f, 
                0.8660254f * WindowConstants.ScreenUnits.Y * 0.5f, 
                0
            );
        /// <summary>
        /// The maximum dimensions of a mesh tile.
        /// </summary>
        public static readonly Vector3 MeshTileDimensions = new Vector3(1, 0.8660254f, 0);

        public Tile Tile;

        public TileBounds(Tile tile)
        {
            Tile = tile;
        }


        public bool Contains(float x, float y, Camera camera = null)
        {
            int intersections = 0;

            if (Tile.MeshTileHandle == null)
                return false;

            int offset = Tile.MeshTileHandle.GetVertexOffset();

            PointF point3 = new PointF();
            PointF point4 = new PointF();

            for(int i = 0; i < MeshTile.BOUNDING_VERTICES.Length - 1; i++)
            {
                GetTransformedPointInPlace(ref point3, Tile.MeshTileHandle.VerticesHandle[offset + MeshTile.BOUNDING_VERTICES[i]],
                    Tile.MeshTileHandle.VerticesHandle[offset + MeshTile.BOUNDING_VERTICES[i] + 1], 
                    Tile.MeshTileHandle.VerticesHandle[offset + MeshTile.BOUNDING_VERTICES[i] + 2], camera);
                GetTransformedPointInPlace(ref point4, Tile.MeshTileHandle.VerticesHandle[offset + MeshTile.BOUNDING_VERTICES[i + 1]],
                    Tile.MeshTileHandle.VerticesHandle[offset + MeshTile.BOUNDING_VERTICES[i + 1] + 1], 
                    Tile.MeshTileHandle.VerticesHandle[offset + MeshTile.BOUNDING_VERTICES[i] + 2], camera);

                if (MiscOperations.GFG.get_line_intersection(x, y, x, y + 1000, point3.X, point3.Y, point4.X, point4.Y))
                {
                    intersections++;
                }
            }

            if (intersections % 2 == 0)
            {
                return false;
            }

            return true;
        }

        public bool Contains3D(Vector3 pointNear, Vector3 pointFar, Camera camera)
        {
            if (Tile.MeshTileHandle == null)
            {
                return false;
            }

            //first get the point at the Z position of the object
            float xUnit = pointFar.X - pointNear.X;
            float yUnit = pointFar.Y - pointNear.Y;

            float percentageAlongLine = (Tile.MeshTileHandle.Weights[^1] - pointNear.Z) / (pointFar.Z - pointNear.Z);

            float x = pointNear.X + xUnit * percentageAlongLine;
            float y = pointNear.Y + yUnit * percentageAlongLine;

            //check bounds of object
            return Contains(x, y, camera);
        }

        public Vector3 GetDimensionData()
        {
            throw new NotImplementedException();
        }

        public PointF GetTransformedPoint(float x, float y, float z, Camera camera = null)
        {
            return new PointF(x + Tile.Chunk.MeshChunk.Mesh.Position.X, y + Tile.Chunk.MeshChunk.Mesh.Position.Y);
        }

        public void GetTransformedPointInPlace(ref PointF point, float x, float y, float z, Camera camera = null)
        {
            point.X = x + Tile.Chunk.MeshChunk.Mesh.Position.X;
            point.Y = y + Tile.Chunk.MeshChunk.Mesh.Position.Y;
        }
    }
}


