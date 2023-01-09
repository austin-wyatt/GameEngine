using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Lighting
{

    /// <summary>
    /// Eventually this should be expanded to refer to PBR lighting materials
    /// </summary>
    public class Material
    {
        public Texture Diffuse;
        public Texture Specular;
        public float Shininess = 0;

        public Material() 
        {

        }
        public Material(Material oldMaterial) 
        {
            Diffuse = oldMaterial.Diffuse;
            Specular = oldMaterial.Specular;
            Shininess = oldMaterial.Shininess;
        }
    }
}
