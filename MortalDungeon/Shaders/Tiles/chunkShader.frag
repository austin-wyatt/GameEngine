#version 420
#extension GL_OES_standard_derivatives : enable

layout(location = 0) out vec4 outputColor;
//out vec4 outputColor; 

in vec2 texCoord;
in vec2 blendMapTexCoords;

in float enableLighting;

in float alpha_threshold;

in vec4 color;
in float mixPercent;

uniform vec3 viewPosition;
in vec3 Normal;
in vec3 FragPos;


struct Material {
    sampler2D diffuse;
//	sampler2D specular;
    float shininess;
}; 

uniform Material[8] material;

uniform sampler2D BlendMap;
uniform vec3 BlendMapOrigin; //bottom left corner


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


//const float CHUNK_HEIGHT_FRAC = 1 / (9.0932667397 - 0.02);
const float CHUNK_HEIGHT_FRAC = 1 / (9.0932667397 + 0.0215);
//const float CHUNK_HEIGHT_FRAC = 1 / (9.0932667397);
const float CHUNK_WIDTH_FRAC = 1 / 7.75;

const float CHUNK_HEIGHT = 9.0932667397;

void main()
{
	vec2 blendTexCoord = vec2((FragPos.x - BlendMapOrigin.x) * CHUNK_WIDTH_FRAC, (BlendMapOrigin.y - FragPos.y) * CHUNK_HEIGHT_FRAC + 0.0013);
//	vec2 blendTexCoord = vec2((blendMapTexCoords.x), blendMapTexCoords.y);

	vec4 blendMapColor = texture(BlendMap, blendTexCoord);

	vec4 firstMatColor = texture(material[0].diffuse, texCoord) * blendMapColor.r;
	vec4 secondMatColor = texture(material[1].diffuse, texCoord) * blendMapColor.g;
	vec4 thirdMatColor = texture(material[2].diffuse, texCoord) * blendMapColor.b;
	vec4 backgroundMatColor = texture(material[3].diffuse, texCoord) * blendMapColor.a;

	vec4 totalColor = firstMatColor + secondMatColor + thirdMatColor + backgroundMatColor;

	outputColor = mixPercent == 0 ? 
		totalColor * color : 
		mix(totalColor, color, mixPercent);

	if(outputColor.a == 0) discard;

	outputColor.a = 1;
	

//	if(outputColor.a < alpha_threshold)
//		discard;

//	if(enableLighting == 0)
//		return;

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
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading

	vec3 halfwayDir = normalize(lightDir + viewDir);//blinn-phong
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 16);

    // combine results
    vec3 ambient  = light.ambient  * matColor;
    vec3 diffuse  = light.diffuse  * diff * matColor;
//    vec3 specular = light.specular * spec * vec3(texture(material[materialIndex].specular, texCoord));


//    return (ambient + diffuse + specular);
    return (ambient + diffuse);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 matColor)
{
    vec3 lightDir = normalize(light.position - fragPos);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
	vec3 halfwayDir = normalize(lightDir + viewDir);//blinn-phong
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 16);

    // attenuation
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + 
  			     light.quadratic * (distance * distance));    
    // combine results
    vec3 ambient  = light.ambient  * matColor;
    vec3 diffuse  = light.diffuse  * diff * matColor;
//    vec3 specular = light.specular * spec * vec3(texture(material[materialIndex].specular, texCoord));
    ambient  *= attenuation;
    diffuse  *= attenuation;
//    specular *= attenuation;
//    return (ambient + diffuse + specular);
    return (ambient + diffuse);
} 

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir, vec3 matColor)
{
	vec3 lightDir = normalize(light.position - fragPos);

	float theta = dot(lightDir, normalize(-light.direction));
	float epsilon = light.cutoff - light.outerCutoff;
	float intensity = clamp((theta - light.outerCutoff) / epsilon, 0.0, 1.0);  

	float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
	vec3 halfwayDir = normalize(lightDir + viewDir);//blinn-phong
	float spec = pow(max(dot(normal, halfwayDir), 0.0), 16);

    // combine results
    vec3 ambient  = light.ambient  * matColor;
    vec3 diffuse  = light.diffuse  * diff * matColor * intensity;
//    vec3 specular = light.specular * spec * vec3(texture(material[materialIndex].specular, texCoord)) * intensity;
//    return (ambient + diffuse + specular);
    return (ambient + diffuse);
}