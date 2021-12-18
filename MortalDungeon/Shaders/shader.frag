﻿#version 420
#extension GL_OES_standard_derivatives : enable

layout(location = 0) out vec4 outputColor;
//out vec4 outputColor; 

in vec4 appliedColor;
in float mixPercent;
in float twoTextures;

in vec2 texCoord;

in vec2 texCoord2;

in float inlineThickness;
in float outlineThickness;
in vec4 inlineColor;
//in vec4 outlineColor;

uniform sampler2D texture1;

uniform float enableLighting;

in float alpha_threshold;

in float primaryTextureTarget;

uniform vec3 viewPosition;
in vec3 Normal;
in vec3 FragPos;

struct Material {
    sampler2D diffuse;
	sampler2D specular;
    float shininess;
}; 

uniform Material[4] material;
int materialIndex = primaryTextureTarget > 0 ? int(primaryTextureTarget - 1) : 0;

struct Light {
	vec3 position;
	vec3 direction;
	float cutoff; //pass as the cosine of the angle in radians
	float outerCutoff;

	float directionalLight;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

	float constant;
	float linear;
	float quadratic;
}; 

struct DirLight
{
	vec3 direction;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	float enabled;
};

struct PointLight
{
	vec3 position;
	
	float constant;
	float linear;
	float quadratic;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	float enabled;
};

struct SpotLight
{
	vec3 position;
	vec3 direction;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	float cutoff; //pass as the cosine of the angle in radians
	float outerCutoff;

	float enabled;
};

uniform DirLight dirLight;
uniform PointLight pointLight;
uniform SpotLight spotlight;



const float OUTLINE_ALPHA_STEP_WIDTH = 0.125;
const float OUTLINE_ALPHA_STEPS = 8;

//void CreateOutline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture);
void CreateInline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture);
void DoWork(in sampler2D primaryTexture);

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 matColor);  
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 matColor); 
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 matColor);

void main()
{
	vec4 texColor = texture(material[materialIndex].diffuse, texCoord);

	if(twoTextures == 0)
	{
		outputColor = texColor * appliedColor;
	}
	else
	{
		vec4 texColor2 = texture(texture1, texCoord2);

		texColor = mixPercent < 1 ? texColor * appliedColor : texColor;

		outputColor = mix(texColor, texColor2, mixPercent);
		
		 

		outputColor[3] = texColor[3]; //we want the alpha value from the original texture to stay
	}


	//Handle outline and inline
//	if(outlineThickness > 0)
//	{
//		CreateOutline(texColor, outlineColor, outlineThickness, primaryTexture);
//	}
	

	if(inlineThickness > 0)
	{
		CreateInline(texColor, inlineColor, inlineThickness, material[materialIndex].diffuse);
	}

	//if the alpha is below the alpha threshold the pixel is discarded
	if(outputColor.a < alpha_threshold)
		discard;

	if(enableLighting == 0)
		return;


	vec3 norm = normalize(Normal);
	vec3 viewDir = normalize(viewPosition - FragPos);

	vec3 result;

	if(dirLight.enabled != 0)
		result += CalcDirLight(dirLight, norm, viewDir, outputColor.xyz);

	if(pointLight.enabled != 0)
		result += CalcPointLight(pointLight, norm, FragPos, viewDir, outputColor.xyz);

	if(spotlight.enabled != 0)
		result += CalcSpotLight(spotlight, norm, FragPos, viewDir, outputColor.xyz);

	outputColor = vec4(result, 1);


	if(FragPos.x > 72)
		outputColor[3] = (73 - FragPos.x);

	if(FragPos.x < -38)
		outputColor[3] = (FragPos.x + 39);

	if(FragPos.y < -84)
		outputColor[3] = (FragPos.y + 85);

	if(FragPos.y > 45)
		outputColor[3] = (46 - FragPos.y);
	
}

void DoWork(in sampler2D primaryTexture)
{
	

//	if(gl_FrontFacing)  //discard if we are looking at the back of an object
//		discard;
}

//void CreateOutline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture)
//{
//	if(textureColor.w < 1)
//	{
//		if(textureColor.w > OUTLINE_ALPHA_STEP_WIDTH * (OUTLINE_ALPHA_STEPS - thickness))
//		{
//			outputColor = outlineColor;
//		}
//	}
//}

void CreateInline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture)
{
	float dx = dFdx(texCoord.x);
	float dy = dFdy(texCoord.y);


	vec4 colorU = texture2D(primaryTexture, vec2(texCoord.x, texCoord.y - dy * thickness));
    vec4 colorD = texture2D(primaryTexture, vec2(texCoord.x, texCoord.y + dy * thickness));
    vec4 colorL = texture2D(primaryTexture, vec2(texCoord.x + dx * thickness, texCoord.y));
    vec4 colorR = texture2D(primaryTexture, vec2(texCoord.x - dx * thickness, texCoord.y));
                

	outputColor = textureColor.a != 0.0 && (colorU.a == 0.0 || colorD.a == 0.0 || colorL.a == 0.0 || colorR.a == 0.0) ? outlineColor : outputColor;
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 matColor)
{
    vec3 lightDir = normalize(-light.direction);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading

//    vec3 reflectDir = reflect(-lightDir, normal);
//    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
	vec3 halfwayDir = normalize(lightDir + viewDir);//blinn-phong
	float spec = pow(max(dot(normal, halfwayDir), 0.0), material[materialIndex].shininess);

    // combine results
//    vec3 ambient  = light.ambient  * vec3(texture(material[materialIndex].diffuse, texCoord));
    vec3 ambient  = light.ambient  * matColor;
//    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material[materialIndex].diffuse, texCoord));
    vec3 diffuse  = light.diffuse  * diff * matColor;
    vec3 specular = light.specular * spec * vec3(texture(material[materialIndex].specular, texCoord));


    return (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 matColor)
{
    vec3 lightDir = normalize(light.position - fragPos);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
//    vec3 reflectDir = reflect(-lightDir, normal);
//    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
	vec3 halfwayDir = normalize(lightDir + viewDir);//blinn-phong
	float spec = pow(max(dot(normal, halfwayDir), 0.0), material[materialIndex].shininess);

    // attenuation
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + 
  			     light.quadratic * (distance * distance));    
    // combine results
    vec3 ambient  = light.ambient  * matColor;
//    vec3 ambient  = light.ambient  * vec3(texture(material[materialIndex].diffuse, texCoord));
    vec3 diffuse  = light.diffuse  * diff * matColor;
//    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material[materialIndex].diffuse, texCoord));
    vec3 specular = light.specular * spec * vec3(texture(material[materialIndex].specular, texCoord));
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
} 

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 matColor)
{
	vec3 lightDir = normalize(light.position - fragPos);

	float theta = dot(lightDir, normalize(-light.direction));
	float epsilon = light.cutoff - light.outerCutoff;
	float intensity = clamp((theta - light.outerCutoff) / epsilon, 0.0, 1.0);  

	float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
//    vec3 reflectDir = reflect(-lightDir, normal);
//    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
	vec3 halfwayDir = normalize(lightDir + viewDir);//blinn-phong
	float spec = pow(max(dot(normal, halfwayDir), 0.0), material[materialIndex].shininess);

    // combine results
//    vec3 ambient  = light.ambient  * vec3(texture(material[materialIndex].diffuse, texCoord));
    vec3 ambient  = light.ambient  * matColor;
//    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material[materialIndex].diffuse, texCoord)) * intensity;
    vec3 diffuse  = light.diffuse  * diff * matColor * intensity;
    vec3 specular = light.specular * spec * vec3(texture(material[materialIndex].specular, texCoord)) * intensity;
    return (ambient + diffuse + specular);
}