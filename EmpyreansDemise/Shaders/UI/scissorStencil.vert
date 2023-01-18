#version 410 core

layout(location = 0) in vec3 aPosition;

uniform mat4 transform;

void main(void)
{
	gl_Position = vec4(aPosition, 1.0) * transform;
	gl_Position[2] = 1;
}


