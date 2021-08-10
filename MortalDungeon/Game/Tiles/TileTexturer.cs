using MortalDungeon.Engine_Classes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace MortalDungeon.Game.Tiles
{
    public static class TileTexturer
    {
        private const int tile_width = 62; //individual tile width
        private const int tile_width_partial = 46; //stacked width
        private const int tile_height = 54; //individual tile height
        private const int tile_height_partial = 27; //stacked height

        private const int cell_height = 64;
        private const int cell_width = 64;

        private const int floats_per_pixel = 4; //RGBA values

        public static void InitializeTexture(TileMap map)
        {
            //int width = tile_width + (map.Width - 1) * tile_width_partial;
            //int height = tile_height + (map.Height - 1) * tile_height_partial;

            //float[] textureData = new float[width * height * floats_per_pixel];

            int width = cell_width * map.Width;
            int height = (cell_height) * map.Width + cell_height / 2;

            float[] textureData = new float[width * height * floats_per_pixel];

            map.DynamicTexture = Texture.LoadFromArray(textureData, new Vector2i(width, height));

            GenerateTexture(map, map.Tiles);
        }

        public static void UpdateTexture(TileMap map) 
        {
            GenerateTexture(map, map.TilesToUpdate);
        }

        private static void GenerateTexture(TileMap map, List<BaseTile> tiles) 
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            float[] textureData = map.DynamicTexture.ImageData.ImageData;

            //int width = tile_width + (map.Width - 1) * tile_width_partial;
            //int height = tile_height + (map.Height - 1) * tile_height_partial;

            int width = cell_width * map.Width;

            BaseTile tile;
            Vector2i tileTexPos;

            //initial pass
            for (int i = 0; i < tiles.Count; i++)
            {
                tile = tiles[i];
                tileTexPos = GetOriginPoint(tile);

                int spritesheetY = (int)((int)tile.TileType * 0.1f);
                int spritesheetX = (int)tile.TileType - spritesheetY * 10;


                ApplySpritesheetColorToImage(spritesheetX, spritesheetY, textureData, tileTexPos, width);

                
            }

            //outline pass
            for (int i = 0; i < tiles.Count; i++)
            {
                tile = tiles[i];
                tileTexPos = GetOriginPoint(tile);

                if (tile.Outline)
                {
                    int spritesheetY = (int)((int)TileType.Outline * 0.1f);
                    int spritesheetX = (int)TileType.Outline - spritesheetY * 10;

                    ApplyCustomColorToImage(spritesheetX, spritesheetY, textureData, tileTexPos, width, tile.OutlineColor);
                }
            }



            map.DynamicTexture.UpdateTextureArray();

            Console.WriteLine("Texture generated in " + timer.ElapsedMilliseconds + " milliseconds");
        }

        private static void ApplySpritesheetColorToImage(int spritesheetX, int spritesheetY, float[] textureData, Vector2i tileTexPos, int width) 
        {
            for (int y = 0; y < cell_height; y++)
            {
                for (int x = 0; x < cell_width; x++)
                {
                    Color4 color = TileMapController.TileBitmap.GetPixel(spritesheetX * cell_width + x, spritesheetY * cell_height + y);

                    if (color.A != 0)
                    {
                        int pos = (tileTexPos.Y + y) * width * floats_per_pixel + (tileTexPos.X + x) * floats_per_pixel;
                        textureData[pos] = color.R;
                        textureData[pos + 1] = color.G;
                        textureData[pos + 2] = color.B;
                        textureData[pos + 3] = color.A;
                    }
                }
            }
        }

        private static void ApplyCustomColorToImage(int spritesheetX, int spritesheetY, float[] textureData, Vector2i tileTexPos, int width, Vector4 newColor)
        {
            for (int y = 0; y < cell_height; y++)
            {
                for (int x = 0; x < cell_width; x++)
                {
                    Color4 color = TileMapController.TileBitmap.GetPixel(spritesheetX * cell_width + x, spritesheetY * cell_height + y);

                    if (color.A != 0)
                    {
                        int pos = (tileTexPos.Y + y) * width * floats_per_pixel + (tileTexPos.X + x) * floats_per_pixel;
                        textureData[pos] = newColor.X;
                        textureData[pos + 1] = newColor.Y;
                        textureData[pos + 2] = newColor.Z;
                        textureData[pos + 3] = newColor.W;
                    }
                }
            }
        }

        //get the tiles origin point on the texture
        private static Vector2i GetOriginPoint(BaseTile tile) 
        {
            Vector2i val = new Vector2i();

            int yOffset = tile.TilePoint.X % 2 == 1 ? 1 : 0;

            val.X = tile.TilePoint.X * tile_width_partial;
            val.Y = (tile.TilePoint.Y + 1) * tile_height - yOffset * tile_height_partial;
            //val.Y = tile.TilePoint.Y * tile_height;

            //should get us close enough for now, fine tune later

            return val;
        }
    }
}
