#version 330 core

layout(location = 0) in vec3 aPosition;

out vec4 appliedColor;
out float mixPercent;

uniform mat4 transform;
uniform mat4 view;
uniform mat4 projection;

uniform vec4 aColor;

void main(void)
{
	appliedColor = aColor;

	gl_Position = vec4(aPosition, 1.0) * transform * view * projection;
}