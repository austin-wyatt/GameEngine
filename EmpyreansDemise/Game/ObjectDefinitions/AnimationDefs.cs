﻿using Empyrean.Engine_Classes;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Objects
{
    #region Animation type enums
    public enum BaseTileAnimationType
    {
        SolidWhite,
        Transparent,
        Grass,
        Selected
    }
    #endregion

    public static class CURSOR_ANIMATION
    {
        private static RenderableObject cursor_Idle_1 = new RenderableObject(CursorObjects.MAIN_CURSOR, WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        public static Animation Idle = new Animation() 
        { 
            Frames = new List<RenderableObject>() { cursor_Idle_1 },
            Frequency = -1
        };

        public static List<Animation> List = new List<Animation>()
        {
            Idle
        };
    }

    public static class CURSOR_1_ANIMATION
    {
        private static RenderableObject cursor_Idle_1 = new RenderableObject(new SpritesheetObject(0, Spritesheets.Cursor_1).CreateObjectDefinition(), _Colors.White, Shaders.FAST_DEFAULT_SHADER_DEFERRED);
        private static RenderableObject cursor_Die_1 = new RenderableObject(new SpritesheetObject(1, Spritesheets.Cursor_1).CreateObjectDefinition(), _Colors.White, Shaders.FAST_DEFAULT_SHADER_DEFERRED);


        public static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { cursor_Idle_1 },
            Frequency = -1
        };

        public static Animation Die = new Animation()
        {
            Frames = new List<RenderableObject>() { cursor_Die_1 },
            Frequency = -1,
            Type = AnimationType.Die
        };

        public static List<Animation> List = new List<Animation>()
        {
            Idle,
            Die
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

    public enum TestSheetItems 
    {
        Guy_Idle_1 = 30,
        Guy_Idle_2,
        Guy_Idle_3,
        Guy_Die_1,
        Guy_Die_2,
        Guy_Die_3,
        Skeleton_Idle_1 = 40,
        Skeleton_Idle_2,
        Skeleton_Idle_3,
        Skeleton_Die_1,
        Skeleton_Die_2
    }

    public static class BAD_GUY_ANIMATION
    {
        private static RenderableObject guy_Idle_1 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Guy_Idle_1, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject guy_Idle_2 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Guy_Idle_2, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject guy_Idle_3 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Guy_Idle_3, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static RenderableObject guy_Die_1 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Guy_Die_1, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject guy_Die_2 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Guy_Die_2, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject guy_Die_3 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Guy_Die_3, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        //private static RenderableObject guy_Die_4 = new RenderableObject(new SpritesheetObject(36, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { guy_Idle_1, guy_Idle_2, guy_Idle_3 },
            //Frames = new List<RenderableObject>() { guy_Idle_1 },
            Frequency = 3,
            Repeats = -1
        };

        private static Animation Die = new Animation()
        {
            Frames = new List<RenderableObject>() { guy_Idle_1, guy_Idle_2, guy_Idle_3, guy_Die_1, guy_Die_2, guy_Die_3 },
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

    public static class SKELETON_ANIMATION
    {
        private static RenderableObject idle1 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Skeleton_Idle_1, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject idle2 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Skeleton_Idle_2, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject idle3 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Skeleton_Idle_3, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static RenderableObject die1 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Skeleton_Die_1, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        private static RenderableObject die2 = new RenderableObject(new SpritesheetObject((int)TestSheetItems.Skeleton_Die_2, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
        //private static RenderableObject die3 = new RenderableObject(new SpritesheetObject(36, Spritesheets.TestSheet).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

        private static Animation Idle = new Animation()
        {
            Frames = new List<RenderableObject>() { idle1, idle2, idle3 },
            Frequency = 8,
            Repeats = -1
        };

        private static Animation Die = new Animation()
        {
            Frames = new List<RenderableObject>() { idle1, idle2, idle3, die1, die2 },
            Frequency = 8,
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


    public static class MOUNTAIN_TWO_ANIMATION
    {
        private static RenderableObject mountain_Idle_1 = new RenderableObject(new SpritesheetObject(75, Spritesheets.TestSheet, 4, 2).CreateObjectDefinition(), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.FAST_DEFAULT_SHADER_DEFERRED);

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
