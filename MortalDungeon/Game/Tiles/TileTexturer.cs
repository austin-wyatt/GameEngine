using MortalDungeon.Engine_Classes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MortalDungeon.Engine_Classes.Rendering;

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

        private static Random random = new Random();

        public static void InitializeTexture(TileMap map)
        {
            //int width = tile_width + (map.Width - 1) * tile_width_partial;
            //int height = tile_height + (map.Height - 1) * tile_height_partial;

            //float[] textureData = new float[width * height * floats_per_pixel];

            int width = cell_width * map.Width;
            int height = (cell_height) * map.Width + cell_height / 2;

            float[] textureData = new float[width * height * floats_per_pixel];
            map.DynamicTexture = Texture.LoadFromArray(textureData, new Vector2i(width, height));

            map.Tiles.ForEach(tile => map.TilesToUpdate.Add(tile));

            map.DynamicTextureInfo.PixelBufferObject = Renderer.CreatePixelBufferObject(map.DynamicTexture.ImageData.ImageData);
            map.DynamicTexture.ImageData.ImageData = null;

            //map.DynamicTexture.UpdateTextureArray(new Vector2i(0, 0), map.DynamicTexture.ImageData.ImageDimensions, map);
        }

        public static Vector2 GetTextureProportions(TileMap map) 
        {
            int width = cell_width * map.Width;
            int height = (cell_height) * map.Width + cell_height / 2;

            return new Vector2((float)width / (tile_width + tile_width_partial * (map.Width - 1)),
                (float)height / (tile_height * (map.Height) + tile_height_partial));
        }

        private static void GenerateTexture(TileMap map, HashSet<BaseTile> tiles) 
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            float[] textureData = map.DynamicTexture.ImageData.ImageData;

            //int width = tile_width + (map.Width - 1) * tile_width_partial;
            //int height = tile_height + (map.Height - 1) * tile_height_partial;

            int width = cell_width * map.Width;

            //BaseTile tile;
            Vector2i tileTexPos;


            //add min and max changed calculation here

            //initial pass
            foreach (BaseTile tile in tiles) 
            {
                tileTexPos = GetOriginPoint(tile);

                UpdateDynamicTextureBounds(map, tileTexPos);

                int spritesheetY = (int)((int)tile.TileType * 0.1f);
                int spritesheetX = (int)tile.TileType - spritesheetY * 10;

                if (tile.InFog)
                {
                    int num = random.Next() % 4;
                    TileType fogType;

                    switch (num)
                    {
                        default:
                            fogType = TileType.Fog_1;
                            break;
                        case 1:
                            fogType = TileType.Fog_2;
                            break;
                        case 2:
                            fogType = TileType.Fog_3;
                            break;
                        case 3:
                            fogType = TileType.Fog_4;
                            break;
                    }

                    int overlayY = (int)((int)fogType * 0.1f);
                    int overlayX = (int)fogType - overlayY * 10;

                    float mixPercent = tile.InFog && map.Controller.Scene.CurrentUnit != null && tile.Explored[map.Controller.Scene.CurrentUnit.Team] ? 0.5f : 1;

                    ApplySpritesheetColorToImage(spritesheetX, spritesheetY, textureData, tileTexPos, width, overlayX, overlayY, mixPercent);
                }
                else
                {
                    ApplySpritesheetColorToImage(spritesheetX, spritesheetY, textureData, tileTexPos, width);
                }
            }

            //outline pass
            foreach (BaseTile tile in tiles) 
            {
                if (tile.Outline && !(tile.InFog && !(map.Controller.Scene.CurrentUnit != null && tile.Explored[map.Controller.Scene.CurrentUnit.Team])))
                {
                    tileTexPos = GetOriginPoint(tile);
                    int spritesheetY = (int)((int)TileType.Outline * 0.1f);
                    int spritesheetX = (int)TileType.Outline - spritesheetY * 10;

                    ApplyCustomColorToImage(spritesheetX, spritesheetY, textureData, tileTexPos, width, tile.OutlineColor);
                }
            }


            //map.DynamicTexture.UpdateTextureArray();

            Console.WriteLine("Texture generated in " + timer.ElapsedMilliseconds + " milliseconds");
        }

        private static void ApplySpritesheetColorToImage(int spritesheetX, int spritesheetY, float[] textureData, Vector2i tileTexPos, int width,
            int overlayX = -1, int overlayY = -1, float mixPercent = 0.5f) 
        {
            for (int y = 0; y < cell_height; y++)
            {
                for (int x = 0; x < cell_width; x++)
                {
                    Color4 color = TileMapController.TileBitmap.GetPixel(spritesheetX * cell_width + x, spritesheetY * cell_height + y);

                    if (overlayX != -1)
                    {
                        Color4 overlayColor = TileMapController.TileBitmap.GetPixel(overlayX * cell_width + x, overlayY * cell_height + y);
                        if (color.A != 0)
                        {
                            int pos = (tileTexPos.Y + y) * width * floats_per_pixel + (tileTexPos.X + x) * floats_per_pixel;
                            textureData[pos] = color.R * (1 - mixPercent) + mixPercent * overlayColor.R;
                            textureData[pos + 1] = color.G * (1 - mixPercent) + mixPercent * overlayColor.G;
                            textureData[pos + 2] = color.B * (1 - mixPercent) + mixPercent * overlayColor.B;
                            textureData[pos + 3] = color.A * (1 - mixPercent) + mixPercent * overlayColor.A;
                        }
                    }
                    else 
                    {
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




        public unsafe static void UpdateTexture(TileMap map, float* dataPointer)
        {
            GenerateTexture(map, map.TilesToUpdate, dataPointer);

            map.DynamicTextureInfo.TextureChanged = false;
        }

        private static int _operations = 0;
        private unsafe static void GenerateTexture(TileMap map, HashSet<BaseTile> tiles, float* dataPointer)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            //int width = tile_width + (map.Width - 1) * tile_width_partial;
            //int height = tile_height + (map.Height - 1) * tile_height_partial;

            int width = cell_width * map.Width;

            //BaseTile tile;
            Vector2i tileTexPos;

            Console.WriteLine("Generating texture");

            //add min and max changed calculation here

            //initial pass
            foreach (BaseTile tile in tiles) 
            {
                tileTexPos = GetOriginPoint(tile);

                UpdateDynamicTextureBounds(map, tileTexPos);

                int spritesheetY = (int)((int)tile.TileType * 0.1f);
                int spritesheetX = (int)tile.TileType - spritesheetY * 10;

                if (tile.InFog)
                {
                    int num = random.Next() % 4;
                    TileType fogType;

                    switch (num)
                    {
                        default:
                            fogType = TileType.Fog_1;
                            break;
                        case 1:
                            fogType = TileType.Fog_2;
                            break;
                        case 2:
                            fogType = TileType.Fog_3;
                            break;
                        case 3:
                            fogType = TileType.Fog_4;
                            break;
                    }

                    int overlayY = (int)((int)fogType * 0.1f);
                    int overlayX = (int)fogType - overlayY * 10;

                    float mixPercent = tile.InFog && map.Controller.Scene.CurrentUnit != null && tile.Explored[map.Controller.Scene.CurrentUnit.Team] ? 0.5f : 1;

                    ApplySpritesheetColorToImage(spritesheetX, spritesheetY, dataPointer, tileTexPos, width, overlayX, overlayY, mixPercent);
                }
                else
                {
                    ApplySpritesheetColorToImage(spritesheetX, spritesheetY, dataPointer, tileTexPos, width);
                }
            }

            Console.WriteLine(timer.ElapsedMilliseconds);

            foreach (BaseTile tile in tiles)
            {
                if (tile.Outline)
                {
                    tileTexPos = GetOriginPoint(tile);
                    int spritesheetY = (int)((int)TileType.Outline * 0.1f);
                    int spritesheetX = (int)TileType.Outline - spritesheetY * 10;

                    ApplyCustomColorToImage(spritesheetX, spritesheetY, dataPointer, tileTexPos, width, tile.OutlineColor);
                }
            }

            //map.DynamicTexture.UpdateTextureArray();

            Console.WriteLine("Texture generated in " + timer.ElapsedMilliseconds + " milliseconds using " + _operations + " operations");
            _operations = 0;
        }

        private unsafe static void ApplySpritesheetColorToImage(int spritesheetX, int spritesheetY, float* textureData, Vector2i tileTexPos, int width,
            int overlayX = -1, int overlayY = -1, float mixPercent = 0.5f)
        {

            int spritesheetWidth = spritesheetX * cell_width;
            int spritesheetHeight = spritesheetY * cell_height;

            int overlayWidth = overlayX * cell_width;
            int overlayHeight = overlayY * cell_height;

            int width_times_fpp = width * floats_per_pixel;

            Color4 color;
            Color4 overlayColor;
            for (int y = 0; y < cell_height; y++)
            {
                for (int x = 0; x < cell_width; x++)
                {
                    color = TileMapController.TileBitmap.GetPixel(spritesheetWidth + x, spritesheetHeight + y);

                    if (overlayX != -1)
                    {
                        overlayColor = TileMapController.TileBitmap.GetPixel(overlayWidth + x, overlayHeight + y);
                        if (color.A != 0)
                        {
                            int pos = (tileTexPos.Y + y) * width_times_fpp + (tileTexPos.X + x) * floats_per_pixel;
                            textureData[pos] = color.R * (1 - mixPercent) + mixPercent * overlayColor.R;
                            textureData[pos + 1] = color.G * (1 - mixPercent) + mixPercent * overlayColor.G;
                            textureData[pos + 2] = color.B * (1 - mixPercent) + mixPercent * overlayColor.B;
                            textureData[pos + 3] = color.A * (1 - mixPercent) + mixPercent * overlayColor.A;

                            _operations += 7;
                        }
                    }
                    else
                    {
                        if (color.A != 0)
                        {
                            int pos = (tileTexPos.Y + y) * width_times_fpp + (tileTexPos.X + x) * floats_per_pixel;
                            textureData[pos] = color.R;
                            textureData[pos + 1] = color.G;
                            textureData[pos + 2] = color.B;
                            textureData[pos + 3] = color.A;

                            _operations += 6;
                        }
                    }
                }
            }
        }
        private unsafe static void ApplyCustomColorToImage(int spritesheetX, int spritesheetY, float* textureData, Vector2i tileTexPos, int width, Vector4 newColor)
        {
            int spritesheetWidth = spritesheetX * cell_width;
            int spritesheetHeight = spritesheetY * cell_height;

            int width_times_fpp = width * floats_per_pixel;

            for (int y = 0; y < cell_height; y++)
            {
                for (int x = 0; x < cell_width; x++)
                {
                    Color4 color = TileMapController.TileBitmap.GetPixel(spritesheetWidth + x, spritesheetHeight + y);

                    if (color.A != 0)
                    {
                        int pos = (tileTexPos.Y + y) * width_times_fpp + (tileTexPos.X + x) * floats_per_pixel;
                        textureData[pos] = newColor.X;
                        textureData[pos + 1] = newColor.Y;
                        textureData[pos + 2] = newColor.Z;
                        textureData[pos + 3] = newColor.W;

                        _operations += 6;
                    }
                }
            }
        }



        private static void UpdateDynamicTextureBounds(TileMap map, Vector2i tileTexPos) 
        {
            return;
            //if the texture hasn't been changed at all
            if (!map.DynamicTextureInfo.TextureChanged)
            {
                map.DynamicTextureInfo.MinChangedBounds.X = tileTexPos.X;
                map.DynamicTextureInfo.MinChangedBounds.Y = tileTexPos.Y;
                map.DynamicTextureInfo.MaxChangedBounds.X = tileTexPos.X + cell_width;
                map.DynamicTextureInfo.MaxChangedBounds.Y = tileTexPos.Y + cell_height;
            }
            else
            {
                if (tileTexPos.X < map.DynamicTextureInfo.MinChangedBounds.X)
                {
                    map.DynamicTextureInfo.MinChangedBounds.X = tileTexPos.X;
                }

                if (tileTexPos.Y < map.DynamicTextureInfo.MinChangedBounds.Y)
                {
                    map.DynamicTextureInfo.MinChangedBounds.Y = tileTexPos.Y;
                }

                if (tileTexPos.X + cell_width > map.DynamicTextureInfo.MaxChangedBounds.X)
                {
                    map.DynamicTextureInfo.MaxChangedBounds.X = tileTexPos.X + cell_width;
                }

                if (tileTexPos.Y + cell_height > map.DynamicTextureInfo.MaxChangedBounds.Y)
                {
                    map.DynamicTextureInfo.MaxChangedBounds.Y = tileTexPos.Y + cell_height;
                }
            }

            //map.DynamicTextureInfo.MinChangedBounds.X = 0;
            //map.DynamicTextureInfo.MinChangedBounds.Y = 0;
            //map.DynamicTextureInfo.MaxChangedBounds.X = cell_width * map.Width;
            //map.DynamicTextureInfo.MaxChangedBounds.Y = (cell_height) * map.Width + cell_height / 2;
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
