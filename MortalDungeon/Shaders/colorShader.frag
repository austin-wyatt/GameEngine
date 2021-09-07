#version 410 core

layout(location = 0) out vec4 outputColor;

in vec4 appliedColor;

void main(void)
{
	outputColor = vec4(appliedColor);
}
