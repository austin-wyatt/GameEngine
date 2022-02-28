using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Tiles.TileMaps;
using System.Diagnostics;
using MortalDungeon.Game.Units;
using MortalDungeon.Game.Entities;
using System.Linq;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Serializers;
using System.Threading.Tasks;
using System.Threading;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;

namespace MortalDungeon.Game.Tiles
{
    public enum TileSelectionType
    {
        Full,
        Selection,
        Stone,
        Stone_2
    }

    public class TileMapController
    {
        public static readonly Texture TileSpritesheet = Texture.LoadFromFile("Resources/TileSpritesheet.png");
        public static readonly Texture TileOverlaySpritesheet = Texture.LoadFromFile("Resources/TileOverlaySpritesheet.png");

        public static Dictionary<TileSelectionType, int> SelectionTypeToSpritesheetMap = new Dictionary<TileSelectionType, int>
        {
            { TileSelectionType.Full, 0 },
            { TileSelectionType.Selection, (int)TileType.Selection },
            { TileSelectionType.Stone, (int)TileType.Stone_1 },
            { TileSelectionType.Stone_2, (int)TileType.Stone_2 },
        };

        public CombatScene Scene;

        public HashSet<BaseTile> SelectionTiles = new HashSet<BaseTile>(); //these tiles will be place above the currently selected tiles
        private const int MAX_SELECTION_TILES = 1000;
        private readonly Stack<BaseTile> _selectionTilePool = new Stack<BaseTile>();

        public BaseTile HoveredTile;
        private List<BaseTile> _hoveredTileList = new List<BaseTile>();

        public TileMapController(CombatScene scene = null) 
        {
            Scene = scene;

            InitializeHelperTiles();
        }

        private void InitializeHelperTiles() 
        {
            BaseTile baseTile = new BaseTile();
            Vector3 tilePosition = new Vector3(0, 0, 0.008f);

            for (int i = 0; i < MAX_SELECTION_TILES; i++)
            {
                baseTile = new BaseTile(tilePosition, new TilePoint(i, -1, null));
                baseTile.SetRender(false);

                baseTile.DefaultColor = _Colors.TranslucentBlue;
                baseTile.SetColor(_Colors.TranslucentBlue);

                baseTile.Properties.Type = (TileType)3;

                //baseTile.BaseObject.EnableLighting = false;

                _selectionTilePool.Push(baseTile);
            }

            GameObject.LoadTextures(_selectionTilePool);

            HoveredTile = new BaseTile(new Vector3(0, 0, 0.01f), new TilePoint(-1, -1, null));
            HoveredTile.SetRender(false);

            HoveredTile.Properties.Type = (TileType)3;
            HoveredTile.SetColor(new Vector4(1, 0, 0, 0.25f));

            //HoveredTile.BaseObject.EnableLighting = false;

            GameObject.LoadTexture(HoveredTile);

            _hoveredTileList = new List<BaseTile>() { HoveredTile };
        }

        public IEnumerable<BaseTile> GetSelectionTilePool()
        {
            return _selectionTilePool;
        }

        public void SelectTiles(List<BaseTile> tiles, TileSelectionType type = TileSelectionType.Full)
        {
            //if (tiles.Count > MAX_SELECTION_TILES)
            //    throw new Exception("Attempted to select " + tiles.Count + " tiles while the maximum was " + MAX_SELECTION_TILES + " in tile map " + ObjectID);

            for (int i = 0; i < tiles.Count; i++)
            {
                SelectTile(tiles[i], type);
            }
        }


        public object _selectLock = new object();
        public BaseTile SelectTile(BaseTile tile, TileSelectionType type = TileSelectionType.Full)
        {
            lock (_selectLock)
            {
                if (_selectionTilePool.Count == 0)
                {
                    Vector3 tilePosition = new Vector3(0, 0, 0.008f);
                    for (int i = 0; i < 100; i++)
                    {
                        var baseTile = new BaseTile(tilePosition, new TilePoint(i, -1, null));
                        baseTile.SetRender(false);

                        baseTile.DefaultColor = _Colors.TranslucentBlue;
                        baseTile.SetColor(_Colors.TranslucentBlue);

                        _selectionTilePool.Push(baseTile);

                        TextureLoadBatcher.LoadTexture(baseTile);
                    }
                }

                float zOffset = 0.009f;

                var selectionTile = _selectionTilePool.Pop();
                selectionTile.BaseObject.BaseFrame.SpritesheetPosition = SelectionTypeToSpritesheetMap[type];

                switch (type)
                {
                    case TileSelectionType.Selection:
                        zOffset = 0.0091f;
                        break;
                    case TileSelectionType.Stone:
                        zOffset = 0.0092f;
                        break;
                    case TileSelectionType.Stone_2:
                        zOffset = 0.0097f;
                        break;
                        //add other zOffsets here to prevent z fighting when a tile is selected multiple times
                }

                Vector3 pos = new Vector3
                {
                    X = tile.Position.X,
                    Y = tile.Position.Y,
                    Z = tile.Position.Z + zOffset
                };

                selectionTile.TilePoint.X = tile.TilePoint.X;
                selectionTile.TilePoint.Y = tile.TilePoint.Y;
                selectionTile.TilePoint.Layer = tile.TilePoint.Layer;
                selectionTile.TilePoint.ParentTileMap = tile.TileMap;
                selectionTile.TileMap = tile.TileMap;
                selectionTile.SetPosition(pos);
                selectionTile.SetRender(true);


                SelectionTiles.Add(selectionTile);

                return selectionTile;
            }
        }

        public void DeselectTile(BaseTile selectionTile)
        {
            lock (_selectLock)
            {
                selectionTile.BaseObject.BaseFrame.SetBaseColor(_Colors.White);

                if (SelectionTiles.Remove(selectionTile) && ((SelectionTiles.Count + _selectionTilePool.Count) < MAX_SELECTION_TILES))
                {
                    _selectionTilePool.Push(selectionTile);
                }

                selectionTile.SetRender(false);
            }
        }

        public void DeselectTiles()
        {
            lock (_selectLock)
            {
                List<BaseTile> tilesToRemove = new List<BaseTile>();

                foreach(var tile in SelectionTiles)
                {
                    tilesToRemove.Add(tile);
                    tile.SetRender(false);
                    tile.BaseObject.BaseFrame.SetBaseColor(_Colors.White);
                }

                for(int i = 0; i < tilesToRemove.Count; i++)
                {
                    if (SelectionTiles.Remove(tilesToRemove[i]) && ((SelectionTiles.Count + _selectionTilePool.Count) < MAX_SELECTION_TILES))
                    {
                        _selectionTilePool.Push(tilesToRemove[i]);
                    }
                }
            }
        }

        public void DeselectTiles(TileSelectionType type)
        {
            lock (_selectLock)
            {
                List<BaseTile> tilesToRemove = new List<BaseTile>();

                foreach (var tile in SelectionTiles)
                {
                    if(SelectionTypeToSpritesheetMap[type] == tile.BaseObject.BaseFrame.SpritesheetPosition)
                    {
                        tilesToRemove.Add(tile);
                        tile.SetRender(false);
                        tile.BaseObject.BaseFrame.SetBaseColor(_Colors.White);
                    }
                }

                for (int i = 0; i < tilesToRemove.Count; i++)
                {
                    if (SelectionTiles.Remove(tilesToRemove[i]) && ((SelectionTiles.Count + _selectionTilePool.Count) < MAX_SELECTION_TILES))
                    {
                        _selectionTilePool.Push(tilesToRemove[i]);
                    }
                }
            }
        }

        public List<BaseTile> GetHoveredTile()
        {
            return _hoveredTileList;
        }

        public void HoverTile(BaseTile tile)
        {
            Vector3 pos = new Vector3
            {
                X = tile.Position.X,
                Y = tile.Position.Y,
                Z = tile.Position.Z + 0.01f
            };

            if (!tile.Hovered)
            {
                EndHover();
                _hoveredTile = tile;
            }

            tile.OnHover();

            HoveredTile.SetPosition(pos);
            HoveredTile.SetRender(true);
        }

        public BaseTile _hoveredTile = null;
        public void EndHover()
        {
            if (_hoveredTile != null)
            {
                _hoveredTile.OnHoverEnd();
                _hoveredTile = null;
            }

            HoveredTile.SetRender(false);
        }

        //public TileMapPoint GlobalPositionToMapPoint(Vector3 position)
        //{
        //    if (TileMaps.Count == 0 || Scene.ContextManager.GetFlag(GeneralContextFlags.TileMapLoadInProgress))
        //        return null;

        //    Vector3 camPos = WindowConstants.ConvertLocalToScreenSpaceCoordinates(position.Xy);

        //    var map = TileMaps[0];

        //    Vector3 dim = map.GetTileMapDimensions();

        //    Vector3 mapPos = map.Position;

        //    Vector3 offsetPos = camPos - mapPos;

        //    TileMapPoint point = new TileMapPoint((int)Math.Floor(offsetPos.X / dim.X) + map.TileMapCoords.X, (int)Math.Floor(offsetPos.Y / dim.Y) + map.TileMapCoords.Y);

        //    return point;
        //} 
    }
}
