#version 330 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 aTexCoord;

out vec2 texCoord;
out vec4 appliedColor;

//uniform mat4 view;
//uniform mat4 projection;

uniform mat4 transform;

uniform vec4 aColor;

void main(void)
{
	texCoord = aTexCoord;
	appliedColor = aColor;
//	appliedColor = vec4(1, 1, 0, 1);

//	gl_Position = vec4(aPosition, 1.0) * transform * view * projection;
	gl_Position = vec4(aPosition, 1.0) * transform;
}