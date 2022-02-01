#version 420
#extension GL_OES_standard_derivatives : enable

layout(location = 0) out vec4 outputColor;
//out vec4 outputColor; 

in vec4 appliedColor;

in vec2 texCoord;

uniform sampler2D overlaySpritesheet;

in float enableLighting;

in float alpha_threshold;

in vec2[3] overlays;
in float[3] overlayColors;


uniform vec3 viewPosition;
in vec3 Normal;
in vec3 FragPos;

struct Material {
    sampler2D diffuse;
	sampler2D specular;
    float shininess;
}; 

uniform Material[2] material;
int materialIndex = 0;

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

	outputColor = texColor * appliedColor;

	for(int i = 0; i < 3; i++)
	{
		if(overlays[i][0] >= 0)
		{
			vec4 overlayCol = texture(overlaySpritesheet, overlays[i]);

			outputColor *= (overlayCol.a > 0 ? overlayCol : vec4(1, 1, 1, 1));

			outputColor.a = outputColor.a > 0 ? 1 : 0;
		}
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

	outputColor = vec4(result, outputColor.a);


//	if(FragPos.x > 71)
//		outputColor[3] = (72 - FragPos.x);
//	else if(FragPos.x < -37)
//		outputColor[3] = (FragPos.x + 38);
//
//	float alphaColor = outputColor[3];
//
//	if(FragPos.y < -82.5)
//	{
//		alphaColor = FragPos.y + 83.5;
//		outputColor[3] = outputColor[3] < alphaColor ? outputColor[3] : alphaColor;
//	}
//	else if(FragPos.y > 43.5)
//	{
//		alphaColor = 44.5 - FragPos.y;
//
//		outputColor[3] = outputColor[3] < alphaColor ? outputColor[3] : alphaColor;
//	}
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