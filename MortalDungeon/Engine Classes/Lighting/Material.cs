using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.Lighting
{

    /// <summary>
    /// Eventually this should be expanded to refer to PBR lighting materials
    /// </summary>
    internal class Material
    {
        internal Texture Diffuse;
        internal Texture Specular;
        internal float Shininess = 0;

        internal Material() 
        {

        }
        internal Material(Material oldMaterial) 
        {
            Diffuse = oldMaterial.Diffuse;
            Specular = oldMaterial.Specular;
            Shininess = oldMaterial.Shininess;
        }
    }
}
