﻿using Empyrean.Engine_Classes;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Objects
{
    public enum ObjectIDs
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
    public static class CursorObjects
    {
        public static readonly ObjectDefinition MAIN_CURSOR = new ObjectDefinition(
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

    public static class TestObjects
    {

        public static readonly ObjectDefinition TEST_SPRITESHEET = new ObjectDefinition(
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

    public static class EnvironmentObjects
    {
        public static readonly ObjectDefinition FIRE_BASE = new SpritesheetObject(2, Spritesheets.TestSheet).CreateObjectDefinition(ObjectIDs.FIRE_BASE);
        public static readonly float[] BaseTileBounds = new float[]{
        0.26093745f, -0.44166672f, 0.0f,
        -0.253125f, -0.44166672f, 0.0f,
        -0.484375f, -0.008333325f, 0.0f,
        -0.24843752f, 0.41388887f, 0.0f,
        0.2578125f, 0.41388887f, 0.0f,
        0.49843752f, -0.0055555105f, 0.0f,
        };
        public static readonly float[] BaseTileBounds_2x = new float[]{
        0.26093745f * 2, -0.44166672f * 2, 0.0f,
        -0.253125f * 2, -0.44166672f * 2, 0.0f,
        -0.484375f * 2, -0.008333325f * 2, 0.0f,
        -0.24843752f * 2, 0.41388887f * 2, 0.0f,
        0.2578125f * 2, 0.41388887f * 2, 0.0f,
        0.49843752f * 2, -0.0055555105f * 2, 0.0f,
        };

        public static readonly ObjectDefinition BASE_TILE = new SpritesheetObject(11, Spritesheets.TestSheet).CreateObjectDefinition(ObjectIDs.BASE_TILE, BaseTileBounds, true);

        public static readonly float[] UIBlockBounds = new float[] 
        {
            -0.5f, -0.5f, 0.0f,
            -0.5f, 0.5f, 0.0f,
            0.5f, 0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
        };

        public static readonly float[] QuadBounds = UIBlockBounds;
    }

    public static class _3DObjects 
    {
        public static Object3D WallObj = OBJParser.ParseOBJ("Resources/Wall.obj");
        public static Object3D WallCornerObj = OBJParser.ParseOBJ("Resources/WallCorner.obj");
        public static Object3D Ball = OBJParser.ParseOBJ("Resources/Ball.obj");
        //public static Object3D Monkey = OBJParser.ParseOBJ("Resources/Monkey.obj");
        public static Object3D Cube = OBJParser.ParseOBJ("Resources/Cube.obj");
        public static Object3D Wall3D = OBJParser.ParseOBJ("Resources/WallObj.obj");
        public static Object3D WallCorner3D = OBJParser.ParseOBJ("Resources/WallCornerObj.obj");
        public static Object3D Tent = OBJParser.ParseOBJ("Resources/3D models/Tent.obj");
        //public static Object3D Grass = OBJParser.ParseOBJ("Resources/3D models/Grass.obj");
        public static Object3D TilePillar = OBJParser.ParseOBJ("Resources/3D models/TilePillar.obj");
        public static Object3D Hexagon = OBJParser.ParseOBJ("Resources/3D models/Hexagon.obj");

        public static RenderableObject CreateObject(SpritesheetObject spritesheet, Object3D obj) 
        {
            RenderableObject testObj = new RenderableObject(spritesheet.Create3DObjectDefinition(obj), new Vector4(1, 1, 1, 1), Shaders.FAST_DEFAULT_SHADER_DEFERRED);
            testObj.CameraPerspective = true;

            return testObj;
        }

        public static BaseObject CreateBaseObject(SpritesheetObject spritesheet, Object3D obj, Vector3 position)
        {
            BaseObject testObj = new BaseObject(CreateObject(spritesheet, obj), 0, "", position);

            testObj.EnableLighting = true;

            Engine_Classes.Rendering.Renderer.LoadTextureFromBaseObject(testObj);

            return testObj;
        }

        public static void PrintObjectVertices(Object3D obj) 
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


    public class LineObject
    {
        Vector3 Point1;
        Vector3 Point2;
        float Thickness;
        public LineObject(Vector3 point1, Vector3 point2, float thickness = 0.01f)
        {
            Point1 = point1;
            Point2 = point2;
            Thickness = thickness;
        }

        public ObjectDefinition CreateLineDefinition()
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

    public class SpritesheetObject
    {
        public int SpritesheetPosition = 0;
        public Vector2 SideLengths = new Vector2(1, 1); //allows multiple spreadsheet tiles to be used to define a texture
        public Spritesheet Spritesheet;

        public SpritesheetObject(int position, Spritesheet spritesheet, int xLength = 1, int yLength = -1)
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

        public ObjectDefinition CreateObjectDefinition(bool fastRendering, ObjectIDs ID = ObjectIDs.Unknown, float[] bounds = null) 
        {
            return CreateObjectDefinition(ID, bounds, fastRendering);
        }

        public static float[] QuadVertices = 
            new float[] {
                0.5f, 0.5f, 0.0f,
                    1f, 1f, // tex coords top right
                    0, 0, 1, // normal (facing up)
                0.5f, -0.5f, 0.0f,
                    1f, 0.0f, // tex coords bottom right
                    0, 0, 1, // normal
                -0.5f, -0.5f, 0.0f,
                    0.0f, 0.0f, // tex coords bottom left
                    0, 0, 1, // normal
                -0.5f, 0.5f, 0.0f,
                    0.0f, 1f, // tex coords top left
                    0, 0, 1, // normal
            };

        public static float[] QuadVerticesBase =
            new float[] {
                0.5f, 0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                -0.5f, 0.5f, 0.0f,
            };

        public static uint[] VertexDrawOrder = 
            new uint[]{
                0, 1, 3,
                1, 2, 3,
            };

        public static float[] DefaultBounds = 
            new float[]{
                0.5f, 0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                -0.5f, 0.5f, 0.0f,
            };

        public ObjectDefinition CreateObjectDefinition(ObjectIDs ID = ObjectIDs.Unknown, float[] bounds = null, bool fastRendering = true)
        {

            ObjectDefinition returnDef = new ObjectDefinition(
                QuadVertices,
                VertexDrawOrder,
                4,
                new TextureInfo(Spritesheet, new int[] { SpritesheetPosition }),
                default,
                bounds != null ? bounds : DefaultBounds,
                false
            );


            returnDef.SpritesheetPosition = SpritesheetPosition;
            returnDef.SideLengths = new Vector2(SideLengths.X, SideLengths.Y);

            returnDef.VerticeType = -1;

            return returnDef;
        }

        public ObjectDefinition Create3DObjectDefinition(Object3D object3D, float[] bounds = null, bool fastRendering = true, bool invertTexture = false)
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
