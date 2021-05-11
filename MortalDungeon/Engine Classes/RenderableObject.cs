using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace MortalDungeon
{
    public enum ObjectRenderType 
    {
        Color,
        Texture
    }

    public struct ShaderInfo 
    {
        public ShaderInfo(string vertex, string fragment) 
        {
            Vertex = vertex;
            Fragment = fragment;
        }
        public string Vertex;
        public string Fragment;
    }

    public class RenderableObject
    {
        public float[] Vertices;
        //when a renderable object is loaded into a scene it's texture needs to be added to the texture list
        public string Texture = "";
        public float[] Color;
        public ObjectRenderType RenderType = ObjectRenderType.Color;
        public uint[] VerticesDrawOrder;
        public int Points;

        //Every renderable object begins at the origin and is placed from there.
        public Vector4 LocalPosition = new Vector4(0, 0, 0, 1.0f);
        public Vector4 GlobalPosition = new Vector4(0, 0, 0, 1.0f);

        public int Stride;

        //Textures and shaders will be loaded separately then assigned to the object
        public Shader ShaderReference;
        public Texture TextureReference;


        //transformations
        public Matrix4 Translation = Matrix4.Identity;
        public Matrix4 Rotation = Matrix4.Identity;
        public Matrix4 Scale = Matrix4.Identity;

        public RenderableObject(float[] vertices, uint[] verticesDrawOrder, int points, string texture, float[] color, ObjectRenderType renderType, Shader shaderReference) 
        {
            Points = points;
            Vertices = CenterVertices(vertices);
            Texture = texture;
            Color = color;
            RenderType = renderType;
            VerticesDrawOrder = verticesDrawOrder;
            ShaderReference = shaderReference;

            Stride = GetVerticesSize(vertices) / Points;
        }

        public int GetRenderDataOffset() 
        {
            switch (RenderType) 
            {
                case ObjectRenderType.Color:
                    return 3 * sizeof(float);
                case ObjectRenderType.Texture:
                    return 3 * sizeof(float);
                default:
                    return 0;
            }
        }

        public int GetVerticesSize()
        {
            return Vertices.Length * sizeof(float);
        }

        public int GetVerticesSize(float[] vertices)
        {
            return vertices.Length * sizeof(float);
        }

        public int GetVerticesDrawOrderSize()
        {
            return VerticesDrawOrder.Length * sizeof(uint);
        }


        //TRANSLATE FUNCTIONS
        public void TranslateX(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation[0] += f;

            SetTranslation(currentTranslation);
        }
        public void TranslateY(float f) 
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation[1] += f;

            SetTranslation(currentTranslation);
        }
        public void TranslateZ(float f)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation[2] += f;

            SetTranslation(currentTranslation);
        }
        public void Translate(Vector3 translation)
        {
            Vector3 currentTranslation = Translation.ExtractTranslation();
            currentTranslation[0] += translation[0];
            currentTranslation[1] += translation[1];
            currentTranslation[2] += translation[2];

            SetTranslation(currentTranslation);
        }

        //SCALE FUNCTIONS
        public void ScaleAll (float f) 
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale[0] *= f;
            currentScale[1] *= f;
            currentScale[2] *= f;

            SetScale(currentScale);
        }
        public void ScaleX (float f) 
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale[0] *= f;

            SetScale(currentScale);
        }
        public void ScaleY(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale[1] *= f;

            SetScale(currentScale);
        }
        public void ScaleZ(float f)
        {
            Vector3 currentScale = Scale.ExtractScale();
            currentScale[2] *= f;

            SetScale(currentScale);
        }

        //ROTATE FUNCTIONS


        //TRANSFORMATION SETTERS
        public void SetRotation(Vector3 translations)
        {
            Translation = Matrix4.CreateTranslation(translations);
        }
        public void SetScale(Vector3 scale)
        {
            Scale = Matrix4.CreateScale(scale);
        }
        public void SetTranslation(Vector3 translations) 
        {
            Translation = Matrix4.CreateTranslation(translations);
        }
        

        //TRANSFORMATION RESETTERS
        public void ResetRotation()
        {
            Rotation = Matrix4.Identity;
        }
        public void ResetScale()
        {
            Scale = Matrix4.Identity;
        }
        public void ResetTranslation()
        {
            Translation = Matrix4.Identity;
        }
        
        
        //Centers the vertices of the renderable object when defined (might want to move this to a different area at some point)
        private float[] CenterVertices(float[] vertices) 
        {
            //vertices will be stored in [x, y, z, textureX, textureY] format
            int stride = vertices.Length / Points;

            float centerX = 0.0f;
            float centerY = 0.0f;
            float centerZ = 0.0f;



            for (int i = 0; i < Points; i++) {
                centerX += vertices[i * stride + 0];
                centerY += vertices[i * stride + 1];
                centerZ += vertices[i * stride + 2];
            }

            centerX /= Points;
            centerY /= Points;
            centerZ /= Points;

            for (int i = 0; i < Points; i++)
            {
                vertices[i * stride + 0] -= centerX;
                vertices[i * stride + 1] -= centerY;
                vertices[i * stride + 2] -= centerZ;
            }

            return vertices;
        } 
    }
}
