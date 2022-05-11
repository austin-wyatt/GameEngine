using Empyrean.Engine_Classes.Rendering;
using Empyrean.Objects;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public class SimpleTexture
    {
        public string TextureName;
        public string FileName;
        public int TextureId;

        public Texture Texture;
        public bool TextureLoaded = false;

        public bool GenerateMipMaps = true;
        public bool Nearest = true;

        public SimpleTexture(Spritesheet spritesheet)
        {
            if(spritesheet != null)
            {
                FileName = spritesheet.File;
                TextureId = spritesheet.TextureId;
                TextureName = spritesheet.Name;
            }
        }

        public SimpleTexture(string filename, int texId)
        {
            FileName = filename;
            TextureId = texId;
            TextureName = filename;
        }

        public void LoadTexture()
        {
            TextureLoadBatcher.LoadTexture(this);
        }

        public void LoadTextureImmediate()
        {
            Renderer.LoadTextureFromSimple(this);
        }
    }
}
