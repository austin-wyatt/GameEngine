#version 430 core

layout(location = 0) out vec4 outputColor;

in vec2 texCoord;

in vec3 Normal;
in vec3 FragPos;

uniform vec3 viewPosition;

uniform sampler2D Texture;

uniform vec4 Color;

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


vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 matColor);  
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 matColor); 
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 matColor);

void main(void)
{
	outputColor = texture(Texture, texCoord);

	outputColor *= Color;	

	if(outputColor.a < 0.01)
		discard;

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
}


vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir, vec3 matColor)
{
    vec3 lightDir = normalize(-light.direction);
    float diff = max(dot(normal, lightDir), 0.0);

	vec3 halfwayDir = normalize(lightDir + viewDir);//blinn-phong
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 16);

    vec3 ambient  = light.ambient  * matColor;
    vec3 diffuse  = light.diffuse  * diff * matColor;

    return (ambient + diffuse);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 matColor)
{
    vec3 lightDir = normalize(light.position - fragPos);
    float diff = max(dot(normal, lightDir), 0.0);
	vec3 halfwayDir = normalize(lightDir + viewDir);//blinn-phong
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 16);

    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + 
  			     light.quadratic * (distance * distance));    
    vec3 ambient  = light.ambient  * matColor;
    vec3 diffuse  = light.diffuse  * diff * matColor;
    ambient  *= attenuation;
    diffuse  *= attenuation;

    return (ambient + diffuse);
} 

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 matColor)
{
	vec3 lightDir = normalize(light.position - fragPos);

	float theta = dot(lightDir, normalize(-light.direction));
	float epsilon = light.cutoff - light.outerCutoff;
	float intensity = clamp((theta - light.outerCutoff) / epsilon, 0.0, 1.0);  

	float diff = max(dot(normal, lightDir), 0.0);
	vec3 halfwayDir = normalize(lightDir + viewDir);//blinn-phong
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 16);

    vec3 ambient  = light.ambient  * matColor;
    vec3 diffuse  = light.diffuse  * diff * matColor * intensity;

    return (ambient + diffuse);
}