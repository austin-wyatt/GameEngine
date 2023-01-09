#version 330

out vec4 outputColor; 

in vec4 appliedColor;

void main()
{
	outputColor = appliedColor;
}