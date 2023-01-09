#version 410 core

layout(location = 0) out vec4 outputColor;
//out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D Texture;

uniform float Alpha;

void main(void)
{
	vec4 color = texture(Texture, texCoord);

	color.w = 1;
	color.w = color.w * Alpha;

	outputColor = color;

	if(outputColor.w < 0.05)
		discard;
//	outputColor = vec4((texCoord.y), 0, 0, 1);
}
