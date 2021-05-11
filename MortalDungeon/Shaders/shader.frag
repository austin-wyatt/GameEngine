#version 330

out vec4 outputColor; 

//in vec4 vertexColor;

//uniform vec4 ourColor;
in vec2 texCoord;

uniform sampler2D texture0;

void main()
{
	//outputColor = vertexColor;
	//outputColor = ourColor;
	outputColor = texture(texture0, texCoord);
}