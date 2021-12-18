#version 410 core

layout(location = 0) in vec3 aPosition;

out vec3 texCoord;

uniform mat4 camera;

void main(void)
{
	texCoord = aPosition;

	vec4 pos = vec4(aPosition, 1) * camera;

	gl_Position = pos.xyww; //use W twice to make the calculations think the object is at the maximum possible depth
}


