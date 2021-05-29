#version 330 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 aTexCoord;

out vec2 texCoord;
out vec4 appliedColor;
out float mixPercent;

//uniform mat4 view;
//uniform mat4 projection;
uniform mat4 camera;
uniform int enable_cam;

uniform mat4 transform;

uniform vec4 aColor;
//uniform float fMixPercent;

void main(void)
{
	texCoord = aTexCoord;
	appliedColor = aColor;
//	mixPercent = fMixPercent;
	mixPercent = 0;

//	gl_Position = vec4(aPosition, 1.0) * transform * view * projection;
//	gl_Position = vec4(aPosition, 1.0) * transform;
	if(enable_cam == 1)
	{
		gl_Position = vec4(aPosition, 1.0) * transform * camera;
	}
	else
	{
		gl_Position = vec4(aPosition, 1.0) * transform;
	}
}