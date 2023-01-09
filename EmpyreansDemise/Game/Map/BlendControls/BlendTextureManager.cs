using Empyrean.Engine_Classes;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Empyrean.Game.Map.BlendControls
{
    /// <summary>
    /// Loads and keeps track of all of the loaded blend control image files. <para/>
    /// Attempts to keep frequently used images loaded in memory while disposing 
    /// of infrequently used images
    /// </summary>
    public static class BlendTextureManager
    {
        private static Dictionary<TileType, Texture> TextureMap = new Dictionary<TileType, Texture>();
        private static Dictionary<TileType, string> TileTypeFileMap = new Dictionary<TileType, string>()
        {
            { TileType.Grass,  "Resources/Textures/Grass.png" },
            { TileType.Dirt,  "Resources/Textures/Dirt.png" },
            { TileType.Stone_1,  "Resources/Textures/Stone_1.png" },
            { TileType.Stone_2,  "Resources/Textures/Stone_2.png" },
        };

        private static Dictionary<TileType, long> TileTimestamps = new Dictionary<TileType, long>();

        /// <summary>
        /// This must be called from the main thread
        /// </summary>
        public static Texture GetTileTexture(TileType type)
        {
            if(TextureMap.TryGetValue(type, out Texture texture))
            {
                TileTimestamps.AddOrSet(type, DateTime.Now.Ticks);

                return texture;
            }
            else
            {
                var tex = Texture.LoadFromFile(TileTypeFileMap[type]);

                TextureMap.Add(type, tex);
                TileTimestamps.AddOrSet(type, DateTime.Now.Ticks);

                return tex;
            }
        }

        private static void UnloadTexture(TileType type)
        {
            if(TextureMap.TryGetValue(type, out Texture texture))
            {
                if(Thread.CurrentThread.ManagedThreadId == WindowConstants.MainThreadId)
                {
                    texture.DisposeImmediate();
                }
                else
                {
                    Window.QueueToRenderCycle(texture.DisposeImmediate);
                }

                TextureMap.Remove(type);
            }

            TileTimestamps.Remove(type);
        }
    }
}
