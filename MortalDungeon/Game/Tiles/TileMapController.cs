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
        Selection
    }

    public class TileMapController
    {
        public static readonly Texture TileSpritesheet = Texture.LoadFromFile("Resources/TileSpritesheet.png");
        public static readonly Texture TileOverlaySpritesheet = Texture.LoadFromFile("Resources/TileOverlaySpritesheet.png");

        public CombatScene Scene;

        public QueuedList<BaseTile> SelectionTiles = new QueuedList<BaseTile>(); //these tiles will be place above the currently selected tiles
        private const int MAX_SELECTION_TILES = 1000;
        public int _amountOfSelectionTiles = 0;
        private readonly List<BaseTile> _selectionTilePool = new List<BaseTile>();

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

                baseTile.Properties.Type = (TileType)1;

                //baseTile.BaseObject.EnableLighting = false;

                _selectionTilePool.Add(baseTile);
            }

            GameObject.LoadTextures(_selectionTilePool);

            HoveredTile = new BaseTile(new Vector3(0, 0, 0.01f), new TilePoint(-1, -1, null));
            HoveredTile.SetRender(false);

            HoveredTile.Properties.Type = (TileType)1;
            HoveredTile.SetColor(_Colors.TranslucentRed);

            //HoveredTile.BaseObject.EnableLighting = false;

            GameObject.LoadTexture(HoveredTile);

            _hoveredTileList = new List<BaseTile>() { HoveredTile };
        }

        public List<BaseTile> GetSelectionTilePool()
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

        public BaseTile SelectTile(BaseTile tile, TileSelectionType type = TileSelectionType.Full)
        {
            //Console.WriteLine(_amountOfSelectionTiles + " tiles in use");

            if (_amountOfSelectionTiles == _selectionTilePool.Count)
                _amountOfSelectionTiles--;

            if (_amountOfSelectionTiles < 0)
            {
                Console.WriteLine("TileMap.SelectTile: Less than 0 selection tiles ");
                _amountOfSelectionTiles = 0;
            }

            float zOffset = 0.009f;

            switch (type)
            {
                case TileSelectionType.Full:
                    _selectionTilePool[_amountOfSelectionTiles].BaseObject.BaseFrame.SpritesheetPosition = 1;
                    break;
                case TileSelectionType.Selection:
                    _selectionTilePool[_amountOfSelectionTiles].BaseObject.BaseFrame.SpritesheetPosition = (int)TileType.Selection;
                    zOffset = 0.0091f;
                    break;
                //add other zOffsets here to prevent z fighting when a tile is selected multiple times
            }

            Vector3 pos = new Vector3
            {
                X = tile.Position.X,
                Y = tile.Position.Y,
                Z = tile.Position.Z + zOffset
            };

            _selectionTilePool[_amountOfSelectionTiles].TilePoint.ParentTileMap = tile.TileMap;
            _selectionTilePool[_amountOfSelectionTiles].TileMap = tile.TileMap;
            _selectionTilePool[_amountOfSelectionTiles].SetPosition(pos);
            _selectionTilePool[_amountOfSelectionTiles].SetRender(true);


            SelectionTiles.Add(_selectionTilePool[_amountOfSelectionTiles]);

            _amountOfSelectionTiles++;

            return _selectionTilePool[_amountOfSelectionTiles - 1];
        }

        public void DeselectTile(BaseTile selectionTile)
        {
            selectionTile.BaseObject.BaseFrame.SetBaseColor(_Colors.White);

            SelectionTiles.Remove(selectionTile);

            selectionTile.SetRender(false);
            _amountOfSelectionTiles--;
        }

        public void DeselectTiles()
        {
            for (int i = 0; i < _amountOfSelectionTiles; i++)
            {
                _selectionTilePool[i].SetRender(false);
            }

            SelectionTiles.Clear();

            _amountOfSelectionTiles = 0;
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
