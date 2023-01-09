#version 420

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in vec2 aBlendMapTexCoord; //spritesheet position, material index
layout(location = 4) in vec4 aVertColor; //vertex color
layout(location = 5) in float fMixPercent; //color mix percent
layout(location = 6) in mat4 transform;


out vec2 texCoord;
out vec2 blendMapTexCoords;

out float alpha_threshold;

out float enableLighting;
out vec3 Normal;
out vec3 FragPos;

out float materialIndexF;

flat out int InstanceID; 

uniform mat4 camera;

out vec4 color;
out float mixPercent;

void main(void)
{
	texCoord = vec2(aTexCoord);
	blendMapTexCoords = vec2(aBlendMapTexCoord);

	color = vec4(aVertColor);
	mixPercent = fMixPercent;

	alpha_threshold = 0.15;


	//Position handling

	vec3 pos = aPosition; 

	enableLighting = 1;
	Normal = (vec4(aNormal, 0) * transform).xyz;


	gl_Position = vec4(pos, 1.0) * transform * camera;

	FragPos = vec3(vec4(pos, 1.0) * transform);

	InstanceID = gl_InstanceID; 
}


