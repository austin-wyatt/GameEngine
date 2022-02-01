﻿using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Objects
{

    public static class ShaderList
    {
        public static readonly List<Shader> AllShaders = new List<Shader>() 
        { 
            Shaders.DEFAULT_SHADER, 
            Shaders.LINE_SHADER, 
            Shaders.POINT_SHADER,
            Shaders.FAST_DEFAULT_SHADER,
            Shaders.SIMPLE_SHADER,
            //Shaders.TILE_MAP_SHADER
        };
    }
    public static class Shaders 
    {
        public static readonly Shader DEFAULT_SHADER = new Shader("Shaders/OldShaders/oldShader.vert", "Shaders/OldShaders/oldShader.frag");
        public static readonly Shader FAST_DEFAULT_SHADER = new Shader("Shaders/fastDefaultShader.vert", "Shaders/shader.frag");
        public static readonly Shader POINT_SHADER = new Shader("Shaders/OldShaders/pointShader.vert", "Shaders/OldShaders/pointShader.frag");
        public static readonly Shader LINE_SHADER = new Shader("Shaders/OldShaders/lineShader.vert", "Shaders/OldShaders/lineShader.frag");
        public static readonly Shader TILE_MAP_SHADER = new Shader("Shaders/tileMapShader.vert", "Shaders/tileMapShader.frag");


        public static readonly Shader SIMPLE_SHADER = new Shader("Shaders/simpleShader.vert", "Shaders/simpleShader.frag");
        public static readonly Shader SKYBOX_SHADER = new Shader("Shaders/CubeMap/CubeMapShader.vert", "Shaders/CubeMap/CubeMapShader.frag");

        public static readonly Shader LIGHT_OBSTRUCTION_SHADER = new Shader("Shaders/lightObstructionShader.vert", "Shaders/lightObstructionShader.frag");
        public static readonly Shader LIGHT_SHADER = new Shader("Shaders/lightShader.vert", "Shaders/lightShader.frag");

        public static readonly Shader COLOR_SHADER = new Shader("Shaders/colorShader.vert", "Shaders/colorShader.frag");

        public static readonly Shader PARTICLE_SHADER = new Shader("Shaders/particleShader.vert", "Shaders/particleShader.frag");

        public static readonly Shader TEXT_SHADER = new Shader("Shaders/Text/textShader.vert", "Shaders/Text/textShader.frag");
        public static readonly Shader TILE_SHADER = new Shader("Shaders/Tiles/tileShader.vert", "Shaders/Tiles/tileShader.frag");
        public static readonly Shader UI_SHADER = new Shader("Shaders/UI/uiShader.vert", "Shaders/UI/uiShader.frag");
    }
}
