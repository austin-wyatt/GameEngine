#version 330 core

layout (location = 0) out vec4 gColor;
//layout (location = 1) out vec4 gPosition;
//layout (location = 2) out vec4 gNormal;

in vec4 appliedColor;

in vec2 texCoord;
in vec3 FragPos;
in vec3 Normal;

uniform sampler2D tex;

uniform vec3 viewPosition;

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

void main()
{
	vec4 texColor = texture(tex, texCoord);

	gColor = texColor * appliedColor;

	if(gColor.a == 0) discard;

//	gPosition = vec4(FragPos, 1);
//	gNormal = vec4(normalize(Normal), 1);

	vec3 norm = normalize(Normal);
	vec3 viewDir = normalize(viewPosition - FragPos);

	vec3 result;

	if(dirLight.enabled != 0)
		result += CalcDirLight(dirLight, norm, viewDir, gColor.xyz);

	if(pointLight.enabled != 0)
		result += CalcPointLight(pointLight, norm, FragPos, viewDir, gColor.xyz);

	if(spotlight.enabled != 0)
		result += CalcSpotLight(spotlight, norm, FragPos, viewDir, gColor.xyz);

	gColor = vec4(result, gColor.a);
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
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 16);

    // combine results
//    vec3 ambient  = light.ambient  * vec3(texture(material[materialIndex].diffuse, texCoord));
    vec3 ambient  = light.ambient  * matColor;
//    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material[materialIndex].diffuse, texCoord));
    vec3 diffuse  = light.diffuse  * diff * matColor;
    vec3 specular = light.specular * spec * vec3(texture(tex, texCoord));


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
	float spec = pow(max(dot(normal, halfwayDir), 0.0),16);

    // attenuation
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + 
  			     light.quadratic * (distance * distance));    
    // combine results
    vec3 ambient  = light.ambient  * matColor;
//    vec3 ambient  = light.ambient  * vec3(texture(material[materialIndex].diffuse, texCoord));
    vec3 diffuse  = light.diffuse  * diff * matColor;
//    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material[materialIndex].diffuse, texCoord));
    vec3 specular = light.specular * spec * vec3(texture(tex, texCoord));
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
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 16);

    // combine results
//    vec3 ambient  = light.ambient  * vec3(texture(material[materialIndex].diffuse, texCoord));
    vec3 ambient  = light.ambient  * matColor;
//    vec3 diffuse  = light.diffuse  * diff * vec3(texture(material[materialIndex].diffuse, texCoord)) * intensity;
    vec3 diffuse  = light.diffuse  * diff * matColor * intensity;
    vec3 specular = light.specular * spec * vec3(texture(tex, texCoord)) * intensity;
    return (ambient + diffuse + specular);
}