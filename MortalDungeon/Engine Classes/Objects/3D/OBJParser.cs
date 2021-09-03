using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class Object3D 
    {
        public string Name;
        public float[] Vertices;
        public float[] TextureCoords;
        public Face[] Faces;
        public readonly int ObjectID = _verticeType++;

        private static int _verticeType = 1;
    }

    public struct Face 
    {
        public VVt X => Values[0];  
        public VVt Y => Values[1];
        public VVt Z => Values[2];

        public VVt[] Values; 

        public Face(VVt x, VVt y, VVt z) 
        {
            Values = new VVt[] { x, y, z };
        }
    }

    /// <summary>
    /// Vertex/Texture coordinate pair
    /// </summary>
    public struct VVt 
    {
        public int Vertex;
        public int VertexTexture;

        public VVt(int v, int vt) 
        {
            Vertex = v;
            VertexTexture = vt;
        }
    }

    public static class OBJParser
    {
        public static Object3D ParseOBJ(string filename) 
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

            List<Face> faces = new List<Face>();

            foreach (string line in lines) 
            {
                string[] temp = line.Split(' ');

                //comment
                if (temp[0] == "#")
                    continue;

                if (temp[0] == "o")
                    obj.Name = line.Substring(2);

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

                if (temp[0] == "f")
                {
                    List<VVt> VVts = new List<VVt>();
                    for (int i = 1; i < temp.Length; i++)
                    {
                        string[] data = temp[i].Split('/');

                        VVts.Add(new VVt(int.Parse(data[0]), int.Parse(data[1])));
                    }

                    faces.Add(new Face(VVts[0], VVts[1], VVts[2]));
                }
            }

            obj.Vertices = vertices.ToArray();
            obj.TextureCoords = texCoords.ToArray();
            obj.Faces = faces.ToArray();

            return obj;
        }
    }
}
