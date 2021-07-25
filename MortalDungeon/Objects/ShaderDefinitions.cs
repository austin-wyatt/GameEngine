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
            Shaders.PARTICLE_SHADER,
            Shaders.FAST_DEFAULT_SHADER,
            Shaders.SIMPLE_SHADER
        };
    }
    public static class Shaders 
    {
        public static readonly Shader DEFAULT_SHADER = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
        public static readonly Shader FAST_DEFAULT_SHADER = new Shader("Shaders/fastDefaultShader.vert", "Shaders/shader.frag");
        public static readonly Shader POINT_SHADER = new Shader("Shaders/pointShader.vert", "Shaders/pointShader.frag");
        public static readonly Shader LINE_SHADER = new Shader("Shaders/lineShader.vert", "Shaders/lineShader.frag");
        public static readonly Shader PARTICLE_SHADER = new Shader("Shaders/particleShader.vert", "Shaders/particleShader.frag");

        //Image transformation shaders
        public static readonly Shader SIMPLE_SHADER = new Shader("Shaders/simpleShader.vert", "Shaders/simpleShader.frag");
    } 
}
