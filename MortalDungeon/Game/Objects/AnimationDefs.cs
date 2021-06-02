using MortalDungeon.Engine_Classes;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Objects
{
    public static class CURSOR_ANIMATION
    {
        private static RenderableObject cursor_Idle_1 = new RenderableObject(CursorObjects.MAIN_CURSOR, WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        public static Animation Idle = new Animation() 
        { 
            Frames = new List<RenderableObject>() { cursor_Idle_1 },
            Frequency = 30
        };

        public static List<Animation> List = new List<Animation>()
        {
            Idle
        };
    }

    public static class BUTTON_ANIMATION
    {
        private static RenderableObject button_Idle_1 = new RenderableObject(ButtonObjects.BUTTON_SPRITESHEET, WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { button_Idle_1 },
            Frequency = 30
        };

        public static List<Animation> List = new List<Animation>()
        {
            Idle
        };
    }

    public static class HEXAGON_ANIMATION
    {
        private static RenderableObject hexagon_Idle_1 = new RenderableObject(EnvironmentObjects.HEXAGON_TILE_SQUARE_Generic, WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject hexagon_Idle_2 = new RenderableObject(EnvironmentObjects.TREE1, WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { hexagon_Idle_1, hexagon_Idle_2 },
            Frequency = 6,
            Repeats = 10
        };

        private static Animation Misc_One = new Animation()
        {
            Frames = new List<RenderableObject>() { hexagon_Idle_1 },
            Frequency = 6,
            Repeats = 10,
            Type = AnimationType.Misc_One
        };

        public static List<Animation> List = new List<Animation>()
        {
            Idle,
            Misc_One
        };
    }

    public static class GRASS_ANIMATION
    {
        private static RenderableObject grass_Idle_1 = new RenderableObject(EnvironmentObjects.GRASS_TILE, WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        //private static RenderableObject grass_Idle_2 = new RenderableObject(EnvironmentObjects.TREE1, WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { grass_Idle_1 },
            Frequency = 0,
            Repeats = 0
        };

        private static Animation Misc_One = new Animation()
        {
            Frames = new List<RenderableObject>() { grass_Idle_1 },
            Frequency = 6,
            Repeats = 10,
            Type = AnimationType.Misc_One
        };

        public static List<Animation> List = new List<Animation>()
        {
            Idle,
            Misc_One
        };
    }

    public static class FIRE_BASE_ANIMATION
    {
        private static RenderableObject fire_Idle_1 = new RenderableObject(EnvironmentObjects.FIRE_BASE, WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { fire_Idle_1 },
            Frequency = 0,
            Repeats = 0
        };

        public static List<Animation> List = new List<Animation>()
        {
            Idle
        };
    }

    public static class BASE_TILE_ANIMATION
    {
        private static RenderableObject base_Idle_1 = new RenderableObject(EnvironmentObjects.BASE_TILE, WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { base_Idle_1 },
            Frequency = 0,
            Repeats = 0
        };

        public static List<Animation> List = new List<Animation>()
        {
            Idle
        };
    }

    public static class BAD_GUY_ANIMATION
    {
        private static RenderableObject guy_Idle_1 = new RenderableObject(new SpritesheetObject(30, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject guy_Idle_2 = new RenderableObject(new SpritesheetObject(31, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject guy_Idle_3 = new RenderableObject(new SpritesheetObject(32, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static RenderableObject guy_Die_1 = new RenderableObject(new SpritesheetObject(33, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject guy_Die_2 = new RenderableObject(new SpritesheetObject(34, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject guy_Die_3 = new RenderableObject(new SpritesheetObject(35, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject guy_Die_4 = new RenderableObject(new SpritesheetObject(36, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { guy_Idle_1, guy_Idle_2, guy_Idle_3 },
            Frequency = 3,
            Repeats = -1
        };

        private static Animation Die = new Animation()
        {
            Frames = new List<RenderableObject>() { guy_Idle_1, guy_Idle_2, guy_Idle_3, guy_Die_1, guy_Die_2, guy_Die_3, guy_Die_4 },
            Frequency = 5,
            Repeats = 0,
            Type = AnimationType.Die
        };

        public static List<Animation> List = new List<Animation>()
        {
            Idle,
            Die
        };
    }
    public static class MOUNTAIN_ANIMATION
    {
        private static RenderableObject mountain_Idle_1 = new RenderableObject(new SpritesheetObject(70, Spritesheets.TestSheet, 5, 3).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { mountain_Idle_1 },
            Frequency = 0,
            Repeats = -1
        };


        public static List<Animation> List = new List<Animation>()
        {
            Idle
        };
    }

    public static class CAVE_BACKGROUND_ANIMATION
    {
        private static RenderableObject cave_Idle_1 = new RenderableObject(new SpritesheetObject(0, Spritesheets.CaveSheet, 8, 8).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { cave_Idle_1 },
            Frequency = 0,
            Repeats = -1
        };


        public static List<Animation> List = new List<Animation>()
        {
            Idle
        };
    }

    public static class MOUNTAIN_TWO_ANIMATION
    {
        private static RenderableObject mountain_Idle_1 = new RenderableObject(new SpritesheetObject(75, Spritesheets.TestSheet, 4, 2).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { mountain_Idle_1 },
            Frequency = 0,
            Repeats = -1
        };


        public static List<Animation> List = new List<Animation>()
        {
            Idle
        };
    }
    public class LINE_ANIMATION
    {
        private LineObject lineObj;

        public List<Animation> List;

        private RenderableObject line_Idle_1;

        private Animation Idle;
        public LINE_ANIMATION(LineObject lineObject)
        {
            lineObj = lineObject;

            line_Idle_1 = new RenderableObject(lineObj.CreateLineDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

            Idle = new Animation()
            {
                Frames = new List<RenderableObject>() { line_Idle_1 },
                Frequency = 30
            };

            List = new List<Animation>()
            {
                Idle
            };
        }
    }
}
