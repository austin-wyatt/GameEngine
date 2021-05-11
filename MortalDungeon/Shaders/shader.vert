#version 330 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 aTexCoord;

//layout(location = 2) in vec3 aColor;

//out vec4 vertexColor;
out vec2 texCoord;

uniform mat4 transform;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
	texCoord = aTexCoord;

	//gl_Position = vec4(aPosition, 1.0) * transform;
	gl_Position = vec4(aPosition, 1.0) * transform * view * projection;
	//vertexColor = vec4(aColor, 1.0);
}