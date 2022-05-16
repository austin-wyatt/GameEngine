using Empyrean.Engine_Classes.Rendering;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Lighting
{
    public static class LightingManager
    {
        public static void SetLightingParametersForShader(Shader shader, bool use = true)
        {
            if (use)
            {
                shader.Use();
            }

            shader.SetVector3("dirLight.ambient", new Vector3(RenderingConstants.LightColor));
            shader.SetVector3("dirLight.diffuse", new Vector3(RenderingConstants.LightColor) * 0.25f);
            shader.SetVector3("dirLight.direction", new Vector3(0, -0.447f, -0.894f));
            shader.SetFloat("dirLight.enabled", 1);

            //Shaders.FAST_DEFAULT_SHADER.SetVector3("pointLight.position", new Vector3(0, 0, 2f));
            //Shaders.FAST_DEFAULT_SHADER.SetVector3("pointLight.diffuse", new Vector3(1, 1, 1));
            //Shaders.FAST_DEFAULT_SHADER.SetFloat("pointLight.enabled", 1);
            //Shaders.FAST_DEFAULT_SHADER.SetFloat("pointLight.constant", 1.0f);
            //Shaders.FAST_DEFAULT_SHADER.SetFloat("pointLight.linear", 0.022f);
            //Shaders.FAST_DEFAULT_SHADER.SetFloat("pointLight.quadratic", 0.0019f);

            //shader.SetVector3("spotlight.position", ref Window._camera.Position);
            //shader.SetVector3("spotlight.direction", ref Window._camera._front);
            //shader.SetVector3("spotlight.ambient", new Vector3(0.1f, 0.1f, 0.1f));
            //shader.SetVector3("spotlight.diffuse", new Vector3(1, 0, 0));
            //shader.SetFloat("spotlight.cutoff", (float)Math.Cos(MathHelper.DegreesToRadians(25f)));
            //shader.SetFloat("spotlight.outerCutoff", (float)Math.Cos(MathHelper.DegreesToRadians(35f)));
            //shader.SetFloat("spotlight.enabled", 1);

            shader.SetVector3("viewPosition", ref Window._camera.Position);
        }

        public static void SetBasicLightingParametersForShader(Shader shader, bool use = true)
        {
            if (use)
            {
                shader.Use();
            }

            shader.SetVector3("dirLight.ambient", new Vector3(RenderingConstants.LightColor));
            shader.SetVector3("dirLight.diffuse", new Vector3(RenderingConstants.LightColor) * 0.25f);
            shader.SetVector3("dirLight.direction", new Vector3(0, -0.447f, -0.894f));
            shader.SetFloat("dirLight.enabled", 1);

            shader.SetVector3("viewPosition", ref Window._camera.Position);
        }
    }
}
