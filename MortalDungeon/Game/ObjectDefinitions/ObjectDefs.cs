using MortalDungeon.Engine_Classes;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Objects
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
                 0.5f, -0.5f, 0.0f, 1.0f, 1.0f, // bottom right
                -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, // bottom left
                -0.5f, 0.5f, 0.0f, 0.0f, 0.0f // top left
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

    public static class ButtonObjects
    {
        public static readonly ObjectDefinition BUTTON_SPRITESHEET = new ObjectDefinition(
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
            -0.47812498f, -0.26111114f, 0.0f,
            -0.4765625f, 0.22777778f, 0.0f,
            0.48125005f, 0.22500002f, 0.0f,
            0.48281252f, -0.2527778f, 0.0f,
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
        public static readonly ObjectDefinition BASE_TILE = new SpritesheetObject(11, Spritesheets.TestSheet).CreateObjectDefinition(ObjectIDs.BASE_TILE, BaseTileBounds, true);

        public static readonly float[] UIBlockBounds = new float[] 
        {
            -0.5f, -0.5f, 0.0f,
            -0.5f, 0.5f, 0.0f,
            0.5f, 0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
        };
    }

    public static class _3DObjects 
    {
        public static Object3D WallObj = OBJParser.ParseOBJ("Resources/Wall.obj");
        public static Object3D WallCornerObj = OBJParser.ParseOBJ("Resources/WallCorner_2.obj");

        public static RenderableObject CreateObject(SpritesheetObject spritesheet, Object3D obj) 
        {
            RenderableObject testObj = new RenderableObject(spritesheet.Create3DObjectDefinition(obj), new Vector4(1, 1, 1, 1), Shaders.FAST_DEFAULT_SHADER);
            testObj.CameraPerspective = true;

            return testObj;
        }

        public static BaseObject CreateBaseObject(SpritesheetObject spritesheet, Object3D obj, Vector3 position)
        {
            BaseObject testObj = new BaseObject(CreateObject(spritesheet, obj), 0, "", position);

            Engine_Classes.Rendering.Renderer.LoadTextureFromBaseObject(testObj);

            return testObj;
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
        public ObjectDefinition CreateObjectDefinition(ObjectIDs ID = ObjectIDs.Unknown, float[] bounds = null, bool fastRendering = true, bool invertTexture = false)
        {

            float[] defaultBounds = new float[]{
                0.5f, 0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                -0.5f, 0.5f, 0.0f,
            };

            float aspectRatio = SideLengths.X / SideLengths.Y;

            float[] vertices;

            if (invertTexture)
            {
                vertices = new float[] {
                0.5f * aspectRatio, 0.5f, 0.0f, 1f, 0.0f, // top right
                0.5f * aspectRatio, -0.5f, 0.0f, 1f, 1f, // bottom right
                -0.5f * aspectRatio, -0.5f, 0.0f, 0.0f, 1f, // bottom left
                -0.5f * aspectRatio, 0.5f, 0.0f, 0.0f, 0.0f, // top left
                };
            }
            else 
            {
                vertices = new float[] {
                0.5f * aspectRatio, 0.5f, 0.0f, 1f, 1f, // top right
                0.5f * aspectRatio, -0.5f, 0.0f, 1f, 0.0f, // bottom right
                -0.5f * aspectRatio, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
                -0.5f * aspectRatio, 0.5f, 0.0f, 0.0f, 1f, // top left
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

            const int VERTICES_AND_TEX_COORDS_LENGTH = 5;
            const int VERTICES_LENGTH = 3;
            const int TEX_COORD_LENGTH = 2;

            float[] vertices = new float[object3D.Vertices.Length / VERTICES_LENGTH * VERTICES_AND_TEX_COORDS_LENGTH];



            for (int i = 0; i < object3D.Vertices.Length / VERTICES_LENGTH; i++) 
            {
                for (int j = 0; j < VERTICES_LENGTH; j++) 
                {
                    vertices[i * VERTICES_AND_TEX_COORDS_LENGTH + j] = object3D.Vertices[i * VERTICES_LENGTH + j];
                }

                for (int j = 0; j < object3D.Faces.Length; j++) 
                {
                    for (int k = 0; k < object3D.Faces[j].Values.Length; k++) 
                    {
                        if (object3D.Faces[j].Values[k].Vertex - 1 == i) 
                        {
                            int texCoord = object3D.Faces[j].Values[k].VertexTexture - 1;


                            //for (int x = 0; x < TEX_COORD_LENGTH; x++)
                            //{
                            //    vertices[i * VERTICES_AND_TEX_COORDS_LENGTH + VERTICES_LENGTH + x] = 1 - object3D.TextureCoords[texCoord * TEX_COORD_LENGTH + x];
                            //}
                            vertices[i * VERTICES_AND_TEX_COORDS_LENGTH + VERTICES_LENGTH] = 1 - object3D.TextureCoords[texCoord * TEX_COORD_LENGTH];
                            vertices[i * VERTICES_AND_TEX_COORDS_LENGTH + VERTICES_LENGTH + 1] = object3D.TextureCoords[texCoord * TEX_COORD_LENGTH + 1];

                            j = object3D.Faces.Length;
                            break;
                        }
                    }
                }
            }


            uint[] indices = new uint[object3D.Faces.Length * VERTICES_LENGTH];

            for (int i = 0; i < object3D.Faces.Length; i++) 
            {
                indices[i * VERTICES_LENGTH] = (uint)object3D.Faces[i].X.Vertex - 1;
                indices[i * VERTICES_LENGTH + 1] = (uint)object3D.Faces[i].Y.Vertex - 1;
                indices[i * VERTICES_LENGTH + 2] = (uint)object3D.Faces[i].Z.Vertex - 1;
            }


            ObjectDefinition returnDef = new ObjectDefinition(
                vertices,
                indices,
                object3D.Vertices.Length / VERTICES_LENGTH,
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
