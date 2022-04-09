using MortalDungeon.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
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

        public void LoadTexture()
        {
            TextureLoadBatcher.LoadTexture(this);
        }
    }
}
