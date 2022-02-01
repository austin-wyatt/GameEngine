#version 420
#extension GL_OES_standard_derivatives : enable

layout(location = 0) out vec4 outputColor;
//out vec4 outputColor; 

in vec4 appliedColor;

in vec2 texCoord;

in float alpha_threshold;

in float fragDepth;

in float inlineThickness;
in vec4 inlineColor;


struct Material {
    sampler2D diffuse;
	sampler2D specular;
    float shininess;
}; 

uniform Material[8] material;
in float materialIndex;


void CreateInline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture);
void DoWork(in sampler2D primaryTexture);

void main()
{
	vec4 texColor = texture(material[int(materialIndex)].diffuse, texCoord);

	outputColor = texColor * appliedColor;

	gl_FragDepth = fragDepth;

	if(inlineThickness > 0)
	{
		CreateInline(texColor, inlineColor, inlineThickness, material[int(materialIndex)].diffuse);
	}

	//if the alpha is below the alpha threshold the pixel is discarded
	if(outputColor.a < alpha_threshold)
		discard;
}

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