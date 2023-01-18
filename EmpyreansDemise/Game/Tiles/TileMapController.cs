using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles.TileMaps;
using System.Diagnostics;
using Empyrean.Game.Units;
using Empyrean.Game.Entities;
using System.Linq;
using Empyrean.Engine_Classes;
using Empyrean.Game.Serializers;
using System.Threading.Tasks;
using System.Threading;
using Empyrean.Engine_Classes.Rendering;
using Empyrean.Game.Objects;
using Empyrean.Objects;

namespace Empyrean.Game.Tiles
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

        public HashSet<Tile> SelectedTiles = new HashSet<Tile>();

        public static ObjectPool<IndividualMesh> TargetIndicatorPool = new ObjectPool<IndividualMesh>(30);

        public Dictionary<Tile, IndividualMesh> TargetedTiles = new Dictionary<Tile, IndividualMesh>();

        public TileMapController(CombatScene scene = null) 
        {
            Scene = scene;
        }


        public void SelectTiles(List<Tile> tiles, TileSelectionType type = TileSelectionType.Full)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                SelectTile(tiles[i], type);
            }
        }


        public object _selectLock = new object();
        public void SelectTile(Tile tile, TileSelectionType type = TileSelectionType.Full)
        {
            lock (_selectLock)
            {
                SelectedTiles.Add(tile);
                tile.SetSelected(true);
            }
        }

        public void DeselectTile(Tile tile)
        {
            lock (_selectLock)
            {
                SelectedTiles.Remove(tile);
                tile.SetSelected(false);
            }
        }

        public void DeselectTiles(ICollection<Tile> tiles)
        {
            lock (_selectLock)
            {
                foreach(var tile in tiles)
                {
                    SelectedTiles.Remove(tile);
                    tile.SetSelected(false);
                }
            }
        }

        public void DeselectTiles()
        {
            lock (_selectLock)
            {
                List<Tile> tilesToRemove = Tile.TileListPool.GetObject();

                foreach(var tile in SelectedTiles)
                {
                    tilesToRemove.Add(tile);
                    tile.SetSelected(false);
                }

                for(int i = tilesToRemove.Count - 1; i >= 0; i--)
                {
                    SelectedTiles.Remove(tilesToRemove[i]);

                    tilesToRemove.RemoveAt(i);
                }

                Tile.TileListPool.FreeObject(ref tilesToRemove);
            }
        }

        public void DeselectTiles(TileSelectionType type)
        {
            //lock (_selectLock)
            //{
            //    List<Tile> tilesToRemove = Tile.TileListPool.GetObject();

            //    foreach (var tile in SelectedTiles)
            //    {
            //        tilesToRemove.Add(tile);
            //        tile.SetSelected(false);
            //    }

            //    for (int i = tilesToRemove.Count - 1; i >= 0; i--)
            //    {
            //        SelectedTiles.Remove(tilesToRemove[i]);

            //        tilesToRemove.RemoveAt(i);
            //    }

            //    Tile.TileListPool.FreeObject(ref tilesToRemove);
            //}
        }


        public void HoverTile(Tile tile)
        {
            if (!tile.Hovered)
            {
                EndHover();
                _hoveredTile = tile;
            }

            tile.OnHover();
        }

        public Tile _hoveredTile = null;
        public void EndHover()
        {
            if (_hoveredTile != null)
            {
                _hoveredTile.OnHoverEnd();
                _hoveredTile = null;
            }
        }

        public IndividualMesh TargetTile(Tile tile)
        {
            if (TargetedTiles.TryGetValue(tile, out var mesh))
            {
                mesh.FillFromMeshTile(tile.MeshTileHandle);
                mesh.SetTranslation(WindowConstants.ConvertScreenSpaceToLocalCoordinates(tile._position) + new Vector3(0, 0, 0.001f));
                return mesh;
            }

            IndividualMesh tileIndicator = TargetIndicatorPool.GetObject();

            tileIndicator.FillFromMeshTile(tile.MeshTileHandle);
            tileIndicator.SetTranslation(WindowConstants.ConvertScreenSpaceToLocalCoordinates(tile._position) + new Vector3(0, 0, 0.001f));

            if (tileIndicator.Texture == null)
            {
                tileIndicator.Texture = new SimpleTexture(Textures.X_Marking);
                tileIndicator.Texture.LoadTexture();
            }

            TargetedTiles.Add(tile, tileIndicator);
            Scene.IndividualMeshes.Add(tileIndicator);
            return tileIndicator;
        }

        public void UntargetTile(Tile tile)
        {
            if (TargetedTiles.TryGetValue(tile, out var mesh))
            {
                Scene.IndividualMeshes.Remove(mesh);
                TargetedTiles.Remove(tile);

                TargetIndicatorPool.FreeObject(mesh);
                return;
            }
        }
    }
}
