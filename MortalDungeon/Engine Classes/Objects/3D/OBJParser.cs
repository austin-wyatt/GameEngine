using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    internal class Object3D 
    {
        internal string Name;
        internal float[] Vertices;
        internal float[] TextureCoords;
        internal float[] Normals;
        internal Face[] Faces;
        internal readonly int ObjectID = _verticeType++;

        private static int _verticeType = 1;
    }

    internal struct Face 
    {
        internal VVtN X => Values[0];  
        internal VVtN Y => Values[1];
        internal VVtN Z => Values[2];

        internal VVtN[] Values; 

        internal Face(VVtN x, VVtN y, VVtN z) 
        {
            Values = new VVtN[] { x, y, z };
        }
    }

    /// <summary>
    /// Vertex/Texture/Normal coordinate group
    /// </summary>
    internal struct VVtN 
    {
        internal int Vertex;
        internal int VertexTexture;
        internal int Normal;

        internal VVtN(int v, int vt, int normal) 
        {
            Vertex = v;
            VertexTexture = vt;
            Normal = normal;
        }
    }

    internal static class OBJParser
    {
        internal static Object3D ParseOBJ(string filename) 
        {
            Object3D obj = new Object3D();
            string[] lines = new string[0];

            try
            {
                lines = System.IO.File.ReadAllLines(filename);
            }
            catch (Exception e) 
            {
                Console.WriteLine($"Error caught in OBJParser.ParseOBJ: {e.Message}");
            }
            List<float> vertices = new List<float>();
            List<float> texCoords = new List<float>();
            List<float> normals = new List<float>();

            List<Face> faces = new List<Face>();

            foreach (string line in lines) 
            {
                if (line.Length == 0)
                    continue;

                string[] temp = line.Replace("  ", " ").Split(' ');

                //comment
                if (temp[0] == "#")
                    continue;

                if (temp[0] == "o")
                    obj.Name = line.Substring(2);

                //if (temp[0] == "g")
                //    obj.Name = line.Substring(2);

                if (temp[0] == "v") 
                {
                    for (int i = 1; i < temp.Length; i++) 
                    {
                        vertices.Add(float.Parse(temp[i]));
                    }
                }

                if (temp[0] == "vt")
                {
                    for (int i = 1; i < temp.Length; i++)
                    {
                        texCoords.Add(float.Parse(temp[i]));
                    }
                }

                if (temp[0] == "vn")
                {
                    for (int i = 1; i < temp.Length; i++)
                    {
                        normals.Add(float.Parse(temp[i]));
                    }
                }

                if (temp[0] == "f")
                {
                    List<VVtN> VVts = new List<VVtN>();
                    for (int i = 1; i < temp.Length; i++)
                    {
                        string[] data = temp[i].Split('/');

                        VVts.Add(new VVtN(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2])));
                    }

                    faces.Add(new Face(VVts[0], VVts[1], VVts[2]));
                }
            }

            obj.Vertices = vertices.ToArray();
            obj.TextureCoords = texCoords.ToArray();
            obj.Normals = normals.ToArray();
            obj.Faces = faces.ToArray();

            return obj;
        }
    }
}
