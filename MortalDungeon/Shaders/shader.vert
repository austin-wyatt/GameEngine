#version 330 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 aTexCoord;

out vec2 texCoord;
out vec4 appliedColor;
out float mixPercent;

uniform mat4 transform;
uniform mat4 view;
uniform mat4 projection;

uniform vec4 aColor;
uniform float fMixPercent;

void main(void)
{
	texCoord = aTexCoord;
	appliedColor = aColor;
	mixPercent = fMixPercent;

	//gl_Position = vec4(aPosition, 1.0) * transform;
	gl_Position = vec4(aPosition, 1.0) * transform * view * projection;
	//vertexColor = vec4(aColor, 1.0);
}