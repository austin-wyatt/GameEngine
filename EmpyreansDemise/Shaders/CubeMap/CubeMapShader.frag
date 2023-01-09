#version 410 core

layout(location = 0) out vec4 outputColor;

in vec3 texCoord;

uniform samplerCube texture0;


void main(void)
{
	outputColor = texture(texture0, texCoord);

//	outputColor = vec4(texCoord, 1);
}
