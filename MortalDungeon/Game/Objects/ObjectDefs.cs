﻿using MortalDungeon.Objects;
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
        GRASS
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
        )
        { ID = ObjectIDs.CURSOR };
    }

    public static class TestObjects
    {
        public static readonly ObjectDefinition BASIC_SQUARE = new ObjectDefinition(
            new float[]{
            0.5f, 0.5f, 0.0f, 1.0f, 1.0f, // top right
            0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f, 0.5f, 0.0f, 0.0f, 1.0f // top left
            },
            new uint[]{
            0, 1, 3,
            1, 2, 3
            },
            4,
            new TextureInfo("Resources/container.png")
        );

        public static readonly ObjectDefinition TEST_OBJECT = new ObjectDefinition(
            new float[]{
            0.5f, 0.5f, 0.0f, 1.0f, 1.0f,
            0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
            -0.5f, 0.5f, 0.0f, 0.0f, 1.0f
            },
            new uint[]{
            0, 1, 3,
            1, 2, 3
            },
            4,
            new TextureInfo("Resources/container.png")
        );

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
        )
        { ID = ObjectIDs.BUTTON };
    }

    public static class EnvironmentObjects
    {
        private static readonly float[] TreeBounds = new float[]{
            -0.12187499f, -0.9694444f, 0.0f,
            -0.095312476f, -0.8055556f, 0.0f,
            -0.0859375f, -0.5916667f, 0.0f,
            -0.08906251f, -0.36111116f, 0.0f,
            -0.22031248f, -0.44444442f, 0.0f,
            -0.36093748f, -0.5083333f, 0.0f,
            -0.4578125f, -0.4666667f, 0.0f,
            -0.4609375f, -0.35277772f, 0.0f,
            -0.390625f, -0.30555558f, 0.0f,
            -0.38437498f, -0.19722223f, 0.0f,
            -0.425f, -0.116666675f, 0.0f,
            -0.39218748f, -0.030555606f, 0.0f,
            -0.30312502f, 0.07499999f, 0.0f,
            -0.26875f, 0.18055558f, 0.0f,
            -0.19218749f, 0.21388888f, 0.0f,
            -0.1953125f, 0.32222223f, 0.0f,
            -0.16093749f, 0.39999998f, 0.0f,
            -0.1484375f, 0.5083333f, 0.0f,
            -0.07343751f, 0.64444447f, 0.0f,
            -0.009374976f, 0.71111107f, 0.0f,
            0.087499976f, 0.6944444f, 0.0f,
            0.15625f, 0.57777774f, 0.0f,
            0.18437505f, 0.4722222f, 0.0f,
            0.21249998f, 0.3611111f, 0.0f,
            0.22968745f, 0.28055555f, 0.0f,
            0.30781245f, 0.16666669f, 0.0f,
            0.32187498f, 0.06388891f, 0.0f,
            0.35312498f, -0.055555582f, 0.0f,
            0.35781252f, -0.15277779f, 0.0f,
            0.30937505f, -0.25833333f, 0.0f,
            0.49374998f, -0.3777778f, 0.0f,
            0.50468755f, -0.58055556f, 0.0f,
            0.4765625f, -0.5916667f, 0.0f,
            0.29375005f, -0.48611116f, 0.0f,
            0.16250002f, -0.41388893f, 0.0f,
            0.107812524f, -0.39166665f, 0.0f,
            0.12187505f, -0.57777774f, 0.0f,
            0.10625005f, -0.79999995f, 0.0f,
            0.07343745f, -0.9944445f, 0.0f,
        };
        public static readonly ObjectDefinition TREE1 = new SpritesheetObject(2, Spritesheets.TestSheet).CreateObjectDefinition(ObjectIDs.Unknown, TreeBounds);

        //maps the octagon from a spritesheet to an arbitrary polygon (in this case the octagon)
        public static readonly ObjectDefinition HEXAGON_TILE = new ObjectDefinition(
            new float[]{
            -0.1796875f, -0.49166667f, 0.0f, 0.03239436781943626f, 0.09863014267442344f,
            -0.503125f, -0.18333328f, 0.0f, 0.0f, 0.0682191755151058f,
            -0.5015625f, 0.19999999f, 0.0f, 0.0001564945305286798f, 0.030410959268906013f,
            -0.18906248f, 0.5083333f, 0.0f, 0.031455402639394184f, 0.0f,
            0.22500002f, 0.49166667f, 0.0f, 0.07292645322949375f, 0.001643832144116792f,
            0.49531245f, 0.17500001f, 0.0f, 0.1f, 0.0328767109371363f,
            0.49374998f, -0.13888884f, 0.0f, 0.0998435084741663f, 0.06383561392531409f,
            0.20624995f, -0.5055555f, 0.0f, 0.07104851185219463f, 0.1f,
            0f, 0f, 0f, 0.05f, 0.05f
            },
            new uint[]{
            0, 1, 8,
            1, 2, 8,
            2, 3, 8,
            3, 4, 8,
            4, 5, 8,
            5, 6, 8,
            6, 7, 8,
            7, 0, 8,
            },
            9,
            new TextureInfo(Spritesheets.TestSheet, new int[] { 0 }),
            new Vector3(),
            new float[]{
            -0.1796875f, -0.49166667f, 0.0f,
            -0.503125f, -0.18333328f, 0.0f,
            -0.5015625f, 0.19999999f, 0.0f,
            -0.18906248f, 0.5083333f, 0.0f,
            0.22500002f, 0.49166667f, 0.0f,
            0.49531245f, 0.17500001f, 0.0f,
            0.49374998f, -0.13888884f, 0.0f,
            0.20624995f, -0.5055555f, 0.0f,
            }
        )
        { ID = ObjectIDs.HEXAGON_TILE };

        private static readonly float[] HexagonBounds = new float[]{
            -0.1796875f, -0.49166667f, 0.0f,
            -0.503125f, -0.18333328f, 0.0f,
            -0.5015625f, 0.19999999f, 0.0f,
            -0.18906248f, 0.5083333f, 0.0f,
            0.22500002f, 0.49166667f, 0.0f,
            0.49531245f, 0.17500001f, 0.0f,
            0.49374998f, -0.13888884f, 0.0f,
            0.20624995f, -0.5055555f, 0.0f,
        };
        public static readonly ObjectDefinition HEXAGON_TILE_SQUARE_Generic = new SpritesheetObject(2, Spritesheets.TestSheet).CreateObjectDefinition(ObjectIDs.HEXAGON_TILE, HexagonBounds);

        public static readonly ObjectDefinition GRASS_TILE = new SpritesheetObject(14, Spritesheets.TestSheet, 1).CreateObjectDefinition(ObjectIDs.GRASS);
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
            Point1.X, Point1.Y, Point1.Z, 0.2f, 0.0f,
            Point1.X + Thickness, Point1.Y, Point1.Z, 0.1f, 1.0f,
            Point2.X, Point2.Y, Point2.Z, 0.2f, 0.1f,
            Point2.X + Thickness, Point2.Y, Point2.Z, 0.1f, 0.1f,
            },
            new uint[]{
            0, 1, 2,
            2, 1, 3
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
        public int SideLength = 1; //allows multiple spreadsheet tiles to be used to define a texture
        public Spritesheet Spritesheet;

        public SpritesheetObject(int position, Spritesheet spritesheet, int sideLength = 1)
        {
            SpritesheetPosition = position;
            Spritesheet = spritesheet;
            SideLength = sideLength;
        }
        public ObjectDefinition CreateObjectDefinition(ObjectIDs ID = ObjectIDs.Unknown, float[] bounds = null)
        {
            int column = SpritesheetPosition % Spritesheet.Rows;
            int row = SpritesheetPosition / Spritesheet.Rows;

            float minBoundX = (float)column / Spritesheet.Columns;
            float maxBoundX = (float)(column + SideLength) / Spritesheet.Columns;

            float minBoundY = (float)(row) / Spritesheet.Rows;
            float maxBoundY = (float)(row + SideLength) / Spritesheet.Rows;

            if (maxBoundX > 1)
            {
                minBoundX = (float)(column - SideLength) / Spritesheet.Columns;
                maxBoundX = 1;
            }

            if (maxBoundY > 1) 
            {
                minBoundY = (float)(row - SideLength) / Spritesheet.Rows;
                maxBoundY = 1;
            }

            //Console.WriteLine(ID);
            //Console.WriteLine(minBoundX + ", " + maxBoundX);
            //Console.WriteLine(minBoundY + ", " + maxBoundY);

            float[] defaultBounds = new float[]{
                0.5f, 0.5f, 0.0f,
                0.5f, -0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                -0.5f, 0.5f, 0.0f,
            };

            ObjectDefinition returnDef = new ObjectDefinition(
            new float[] {
            0.5f, 0.5f, 0.0f, maxBoundX, minBoundY, // top right
            0.5f, -0.5f, 0.0f, maxBoundX, maxBoundY, // bottom right
            -0.5f, -0.5f, 0.0f, minBoundX, maxBoundY, // bottom left
            -0.5f, 0.5f, 0.0f, minBoundX, minBoundY, // top left
            },
            new uint[]{
            0, 1, 3,
            1, 2, 3
            },
            4,
            new TextureInfo(Spritesheet, new int[] { SpritesheetPosition }),
            default,
            bounds != null ? bounds : defaultBounds,
            false
            );

            returnDef.ID = ID;

            return returnDef;
        }
    }
}
