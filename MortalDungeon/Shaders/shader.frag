#version 420
#extension GL_OES_standard_derivatives : enable

layout (location = 0) out vec4 gColor;
layout (location = 1) out vec4 gPosition;
layout (location = 2) out vec4 gNormal;


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

in float enableLighting;

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

uniform Material[8] material;
int materialIndex = int(primaryTextureTarget);

void CreateInline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture);

void main()
{
	vec4 texColor = texture(material[materialIndex].diffuse, texCoord);

	gColor = texColor * appliedColor;

	if(inlineThickness > 0)
	{
		CreateInline(texColor, inlineColor, inlineThickness, material[materialIndex].diffuse);
	}

	//if the alpha is below the alpha threshold the pixel is discarded
	if(gColor.a < alpha_threshold)
		discard;

	gPosition = vec4(FragPos, 1);
	gNormal = vec4(normalize(Normal), 1);

//	if(enableLighting == 0)
//		return;
}

void CreateInline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture)
{
	float dx = dFdx(texCoord.x);
	float dy = dFdy(texCoord.y);


	vec4 colorU = texture2D(primaryTexture, vec2(texCoord.x, texCoord.y - dy * thickness));
    vec4 colorD = texture2D(primaryTexture, vec2(texCoord.x, texCoord.y + dy * thickness));
    vec4 colorL = texture2D(primaryTexture, vec2(texCoord.x + dx * thickness, texCoord.y));
    vec4 colorR = texture2D(primaryTexture, vec2(texCoord.x - dx * thickness, texCoord.y));
                

	gColor = textureColor.a != 0.0 && (colorU.a == 0.0 || colorD.a == 0.0 || colorL.a == 0.0 || colorR.a == 0.0) ? outlineColor : gColor;
}