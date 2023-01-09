#version 410 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;

out vec2 texCoord;

out vec3 Normal;
out vec3 FragPos;

uniform mat4 Transform;
uniform mat3 TexTransform;

uniform mat4 camera;

void main(void)
{
	gl_Position = vec4(aPosition, 1.0) * Transform * camera;
	FragPos = vec3(vec4(aPosition, 1.0) * Transform);


	texCoord = (TexTransform * vec3(aTexCoord, 1)).xy;
	Normal = (vec4(aNormal, 0) * Transform).xyz;
}


