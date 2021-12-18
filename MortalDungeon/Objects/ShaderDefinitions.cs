using MortalDungeon.Engine_Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Objects
{

    internal static class ShaderList
    {
        internal static readonly List<Shader> AllShaders = new List<Shader>() 
        { 
            Shaders.DEFAULT_SHADER, 
            Shaders.LINE_SHADER, 
            Shaders.POINT_SHADER,
            Shaders.FAST_DEFAULT_SHADER,
            Shaders.SIMPLE_SHADER,
            //Shaders.TILE_MAP_SHADER
        };
    }
    internal static class Shaders 
    {
        internal static readonly Shader DEFAULT_SHADER = new Shader("Shaders/OldShaders/oldShader.vert", "Shaders/OldShaders/oldShader.frag");
        internal static readonly Shader FAST_DEFAULT_SHADER = new Shader("Shaders/fastDefaultShader.vert", "Shaders/shader.frag");
        internal static readonly Shader POINT_SHADER = new Shader("Shaders/OldShaders/pointShader.vert", "Shaders/OldShaders/pointShader.frag");
        internal static readonly Shader LINE_SHADER = new Shader("Shaders/OldShaders/lineShader.vert", "Shaders/OldShaders/lineShader.frag");
        internal static readonly Shader TILE_MAP_SHADER = new Shader("Shaders/tileMapShader.vert", "Shaders/tileMapShader.frag");


        //Image transformation shaders
        internal static readonly Shader SIMPLE_SHADER = new Shader("Shaders/simpleShader.vert", "Shaders/simpleShader.frag");
        internal static readonly Shader SKYBOX_SHADER = new Shader("Shaders/CubeMap/CubeMapShader.vert", "Shaders/CubeMap/CubeMapShader.frag");

        internal static readonly Shader LIGHT_OBSTRUCTION_SHADER = new Shader("Shaders/lightObstructionShader.vert", "Shaders/lightObstructionShader.frag");
        internal static readonly Shader LIGHT_SHADER = new Shader("Shaders/lightShader.vert", "Shaders/lightShader.frag");

        internal static readonly Shader COLOR_SHADER = new Shader("Shaders/colorShader.vert", "Shaders/colorShader.frag");

        internal static readonly Shader PARTICLE_SHADER = new Shader("Shaders/particleShader.vert", "Shaders/particleShader.frag");
    } 
}
