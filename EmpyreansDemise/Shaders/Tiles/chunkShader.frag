#version 420
#extension GL_OES_standard_derivatives : enable

layout (location = 0) out vec4 gColor;
layout (location = 1) out vec4 gPosition;
layout (location = 2) out vec4 gNormal;

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
    float shininess;
}; 

uniform Material[8] material;

uniform sampler2D BlendMap;
uniform vec3 BlendMapOrigin; //bottom left corner

//const float CHUNK_HEIGHT_FRAC = 1 / (9.0932667397 - 0.02);
const float CHUNK_HEIGHT_FRAC = 1 / (9.0932667397 + 0.0215);
//const float CHUNK_HEIGHT_FRAC = 1 / (9.0932667397);
const float CHUNK_WIDTH_FRAC = 1 / 7.75;

const float CHUNK_HEIGHT = 9.0932667397;

void main()
{
	vec2 blendTexCoord = vec2((FragPos.x - BlendMapOrigin.x) * CHUNK_WIDTH_FRAC, (BlendMapOrigin.y - FragPos.y) * CHUNK_HEIGHT_FRAC + 0.0013);

	vec4 blendMapColor = texture(BlendMap, blendTexCoord);

	vec4 firstMatColor = texture(material[0].diffuse, texCoord) * blendMapColor.r;
	vec4 secondMatColor = texture(material[1].diffuse, texCoord) * blendMapColor.g;
	vec4 thirdMatColor = texture(material[2].diffuse, texCoord) * blendMapColor.b;
	vec4 backgroundMatColor = texture(material[3].diffuse, texCoord) * blendMapColor.a;

	vec4 totalColor = firstMatColor + secondMatColor + thirdMatColor + backgroundMatColor;

	gColor = mixPercent == 0 ? 
		totalColor * color : 
		mix(totalColor, color, mixPercent);

	if(gColor.a == 0) discard;

	gColor.a = 1;

	gPosition = vec4(FragPos, 1);
	gNormal = vec4(normalize(Normal), 1);
}
