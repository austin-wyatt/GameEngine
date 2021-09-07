#version 410 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec4 color;

out vec4 appliedColor;

void main(void)
{
	appliedColor = color;

	gl_Position = vec4(aPosition.x, aPosition.y, aPosition.z, 1);
}


