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
            Frequency = 1000,
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
