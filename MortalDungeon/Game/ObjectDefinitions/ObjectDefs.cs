using MortalDungeon.Engine_Classes;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Objects
{
    internal enum ObjectIDs
    {
        Unknown = -1,
        CURSOR = 0,
        HEXAGON_TILE,
        BUTTON,
        GRASS,
        FIRE_BASE,
        BASE_TILE,
        CHARACTER
    }

    //Where static object defs are defined for usage with renderable objects, animations, etc
    internal static class CursorObjects
    {
        internal static readonly ObjectDefinition MAIN_CURSOR = new ObjectDefinition(
            new float[]{
                0.5f, 0.5f, 0.0f, 1.0f, 0.0f, // top right
                0, 0, 1, // normal (facing up)
                 0.5f, -0.5f, 0.0f, 1.0f, 1.0f, // bottom right
                 0, 0, 1, // normal (facing up)
                -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, // bottom left
                0, 0, 1, // normal (facing up)
                -0.5f, 0.5f, 0.0f, 0.0f, 0.0f, // top left
                0, 0, 1, // normal (facing up)
            },
            new uint[]{
                0, 1, 3,
                1, 2, 3
            },
            4,
            new TextureInfo("Resources/Cursor.png"),
            new Vector3(-1.5f, 1f, 0)
        );
    }

    internal static class TestObjects
    {

        internal static readonly ObjectDefinition TEST_SPRITESHEET = new ObjectDefinition(
            new float[]{
            0.5f, 0.5f, 0.0f, 0.2f, 0.0f,
            0.5f, -0.5f, 0.0f, 0.2f, 0.1f,
            -0.5f, -0.5f, 0.0f, 0.1f, 0.1f,
            -0.5f, 0.5f, 0.0f, 0.1f, 0.0f,
            0f, 0f, 0f, 0.15f, 0.05f
            },
            new uint[]{
            0, 1, 3,
            1, 2, 3
            },
            5,
            new TextureInfo(Spritesheets.TestSheet, new int[] { 1 }),
            new Vector3(),
            new float[]{
            0.5f, 0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            -0.5f, 0.5f, 0.0f,
            0f, 0f, 0f,
            }
        );
    }

    internal static class EnvironmentObjects
    {
        internal static readonly ObjectDefinition FIRE_BASE = new SpritesheetObject(2, Spritesheets.TestSheet).CreateObjectDefinition(ObjectIDs.FIRE_BASE);
        internal static readonly float[] BaseTileBounds = new float[]{
        0.26093745f, -0.44166672f, 0.0f,
        -0.253125f, -0.44166672f, 0.0f,
        -0.484375f, -0.008333325f, 0.0f,
        -0.24843752f, 0.41388887f, 0.0f,
        0.2578125f, 0.41388887f, 0.0f,
        0.49843752f, -0.0055555105f, 0.0f,
        };
        internal static readonly ObjectDefinition BASE_TILE = new SpritesheetObject(11, Spritesheets.TestSheet).CreateObjectDefinition(ObjectIDs.BASE_TILE, BaseTileBounds, true);

        internal static readonly float[] UIBlockBounds = new float[] 
        {
            -0.5f, -0.5f, 0.0f,
            -0.5f, 0.5f, 0.0f,
            0.5f, 0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
        };
    }

    internal static class _3DObjects 
    {
        internal static Object3D WallObj = OBJParser.ParseOBJ("Resources/Wall.obj");
        internal static Object3D WallCornerObj = OBJParser.ParseOBJ("Resources/WallCorner.obj");
        internal static Object3D Ball = OBJParser.ParseOBJ("Resources/Ball.obj");
        //internal static Object3D Monkey = OBJParser.ParseOBJ("Resources/Monkey.obj");
        internal static Object3D Cube = OBJParser.ParseOBJ("Resources/Cube.obj");
        internal static Object3D Wall3D = OBJParser.ParseOBJ("Resources/WallObj.obj");
        internal static Object3D WallCorner3D = OBJParser.ParseOBJ("Resources/WallCornerObj.obj");
        internal static Object3D Tent = OBJParser.ParseOBJ("Resources/3D models/Tent.obj");

        internal static RenderableObject CreateObject(SpritesheetObject spritesheet, Object3D obj) 
        {
            RenderableObject testObj = new RenderableObject(spritesheet.Create3DObjectDefinition(obj), new Vector4(1, 1, 1, 1), Shaders.FAST_DEFAULT_SHADER);
            testObj.CameraPerspective = true;

            return testObj;
        }

        internal static BaseObject CreateBaseObject(SpritesheetObject spritesheet, Object3D obj, Vector3 position)
        {
            BaseObject testObj = new BaseObject(CreateObject(spritesheet, obj), 0, "", position);

            testObj.EnableLighting = true;

            Engine_Classes.Rendering.Renderer.LoadTextureFromBaseObject(testObj);

            return testObj;
        }

        internal static void PrintObjectVertices(Object3D obj) 
        {
            Console.Write("[");

            foreach (var face in obj.Faces)
            {
                foreach (var vvtn in face.Values)
                {
                    int vertexCoord = (vvtn.Vertex - 1) * 3;

                    Console.Write(obj.Vertices[vertexCoord] + ", ");
                    Console.Write(obj.Vertices[vertexCoord + 1] + ", ");
                    Console.Write(obj.Vertices[vertexCoord + 2] + ", ");
                }
            }

            Console.Write("]");
        }
    }


    internal class LineObject
    {
        Vector3 Point1;
        Vector3 Point2;
        float Thickness;
        internal LineObject(Vector3 point1, Vector3 point2, float thickness = 0.01f)
        {
            Point1 = point1;
            Point2 = point2;
            Thickness = thickness;
        }

        internal ObjectDefinition CreateLineDefinition()
        {
            return new ObjectDefinition(
            new float[] {
            Point1.X, Point1.Y, Point1.Z, 0.4f, 0.0f,
            Point1.X, Point1.Y + Thickness, Point1.Z, 0.4f, 0.1f,
            Point2.X, Point2.Y, Point2.Z, 0.3f, 0.1f,
            Point2.X, Point2.Y + Thickness, Point2.Z, 0.3f, 0.0f,
            },
            new uint[]{
            0, 1, 2,
            1, 2, 3
            },
            4,
            new TextureInfo(Spritesheets.TestSheet, new int[] { 0 }),
            default,
            null,
            false
            );
        }
    }

    internal class SpritesheetObject
    {
        internal int SpritesheetPosition = 0;
        internal Vector2 SideLengths = new Vector2(1, 1); //allows multiple spreadsheet tiles to be used to define a texture
        internal Spritesheet Spritesheet;

        internal SpritesheetObject(int position, Spritesheet spritesheet, int xLength = 1, int yLength = -1)
        {
            SpritesheetPosition = position;
            Spritesheet = spritesheet;
            if (yLength == -1)
            {
                SideLengths.X = xLength;
                SideLengths.Y = xLength;
            }
            else 
            {
                SideLengths.X = xLength;
                SideLengths.Y = yLength;
            }
        }

        internal ObjectDefinition CreateObjectDefinition(bool fastRendering, ObjectIDs ID = ObjectIDs.Unknown, float[] bounds = null) 
        {
            return CreateObjectDefinition(ID, bounds, fastRendering);
        }
        internal ObjectDefinition CreateObjectDefinition(ObjectIDs ID = ObjectIDs.Unknown, float[] bounds = null, bool fastRendering = true, bool invertTexture = false)
        {

            float[] defaultBounds = new float[]{
                0.5f, 0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                -0.5f, 0.5f, 0.0f,
            };

            //float aspectRatio = SideLengths.X / SideLengths.Y;
            float aspectRatio = 1;

            float[] vertices;

            if (invertTexture)
            {
                vertices = new float[] {
                0.5f * aspectRatio, 0.5f, 0.0f, 
                    1f, 0.0f, // tex coords top right
                    0, 0, 1, // normal (facing up)
                0.5f * aspectRatio, -0.5f, 0.0f, 
                    1f, 1f, // tex coords bottom right
                    0, 0, 1, // normal (facing up)
                -0.5f * aspectRatio, -0.5f, 0.0f, 
                    0.0f, 1f, // tex coords  bottom left
                    0, 0, 1, // normal (facing up)
                -0.5f * aspectRatio, 0.5f, 0.0f, 
                    0.0f, 0.0f, // tex coords  top left
                    0, 0, 1, // normal (facing up)
                };
            }
            else 
            {
                vertices = new float[] {
                0.5f * aspectRatio, 0.5f, 0.0f, 
                    1f, 1f, // tex coords top right
                    0, 0, 1, // normal (facing up)
                0.5f * aspectRatio, -0.5f, 0.0f, 
                    1f, 0.0f, // tex coords bottom right
                    0, 0, 1, // normal
                -0.5f * aspectRatio, -0.5f, 0.0f, 
                    0.0f, 0.0f, // tex coords bottom left
                    0, 0, 1, // normal
                -0.5f * aspectRatio, 0.5f, 0.0f, 
                    0.0f, 1f, // tex coords top left
                    0, 0, 1, // normal
                };
            }


            ObjectDefinition returnDef = new ObjectDefinition(
                vertices,
                new uint[]{
                    0, 1, 3,
                    1, 2, 3,
                },
                4,
                new TextureInfo(Spritesheet, new int[] { SpritesheetPosition }),
                default,
                bounds != null ? bounds : defaultBounds,
                false
            );


            returnDef.SpritesheetPosition = SpritesheetPosition;
            returnDef.SideLengths = new Vector2(SideLengths.X, SideLengths.Y);

            returnDef.VerticeType = -aspectRatio;

            return returnDef;
        }

        internal ObjectDefinition Create3DObjectDefinition(Object3D object3D, float[] bounds = null, bool fastRendering = true, bool invertTexture = false)
        {
            float[] defaultBounds = new float[]{
                0.5f, 0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                -0.5f, 0.5f, 0.0f,
            };

            const int VERTICES_AND_TEX_COORDS_AND_NORMAL_LENGTH = 8;
            const int VERTICES_LENGTH = 3;
            const int TEX_COORD_LENGTH = 2;

            float[] vertices = new float[object3D.Faces.Length * 3 * VERTICES_AND_TEX_COORDS_AND_NORMAL_LENGTH]; //n faces, 3 vvtn's per face, 8 values per vvtn


            uint[] indices = new uint[object3D.Faces.Length * VERTICES_LENGTH];
            int index = 0;


            foreach (var face in object3D.Faces) 
            {
                foreach (var vvtn in face.Values) 
                {
                    indices[index] = (uint)index;

                    int vertexCoord = (vvtn.Vertex - 1) * VERTICES_LENGTH;
                    int textureCoord = (vvtn.VertexTexture - 1) * TEX_COORD_LENGTH;
                    int normalCoord = (vvtn.Normal - 1) * VERTICES_LENGTH;

                    vertices[index * VERTICES_AND_TEX_COORDS_AND_NORMAL_LENGTH] = object3D.Vertices[vertexCoord]; //vertex X
                    vertices[index * VERTICES_AND_TEX_COORDS_AND_NORMAL_LENGTH + 1] = object3D.Vertices[vertexCoord + 1]; //vertex Y
                    vertices[index * VERTICES_AND_TEX_COORDS_AND_NORMAL_LENGTH + 2] = object3D.Vertices[vertexCoord + 2]; //vertex Z
                    vertices[index * VERTICES_AND_TEX_COORDS_AND_NORMAL_LENGTH + 3] = object3D.TextureCoords[textureCoord]; //texture coord X
                    vertices[index * VERTICES_AND_TEX_COORDS_AND_NORMAL_LENGTH + 4] = object3D.TextureCoords[textureCoord + 1]; //texture coord Y
                    vertices[index * VERTICES_AND_TEX_COORDS_AND_NORMAL_LENGTH + 5] = object3D.Normals[normalCoord]; //normal X
                    vertices[index * VERTICES_AND_TEX_COORDS_AND_NORMAL_LENGTH + 6] = object3D.Normals[normalCoord + 1]; //normal Y
                    vertices[index * VERTICES_AND_TEX_COORDS_AND_NORMAL_LENGTH + 7] = object3D.Normals[normalCoord + 2]; //normal Z

                    index++;
                }
            }


            ObjectDefinition returnDef = new ObjectDefinition(
                vertices,
                indices,
                indices.Length,
                new TextureInfo(Spritesheet, new int[] { SpritesheetPosition }),
                default,
                bounds != null ? bounds : defaultBounds,
                false
            );


            returnDef.VerticeType = object3D.ObjectID;
            returnDef.SpritesheetPosition = SpritesheetPosition;
            returnDef.SideLengths = new Vector2(SideLengths.X, SideLengths.Y);

            //returnDef.SpritesheetPosition = 0;
            //returnDef.SideLengths = new Vector2(Spritesheet.Columns, Spritesheet.Rows);

            return returnDef;
        }
    }
}
