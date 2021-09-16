#version 420
#extension GL_OES_standard_derivatives : enable

layout(location = 0) out vec4 outputColor;

in vec4 appliedColor;

in vec2 texCoord;


layout (binding = 0) uniform sampler2D texture0;

void main()
{
	vec4 texColor = texture(texture0, texCoord);

	outputColor = texColor * appliedColor;

//	outputColor = vec4(1, 0, 0, 1);

	if(outputColor.a < 0.05)
		discard;
}

