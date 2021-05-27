#version 330

out vec4 outputColor; 

in vec4 appliedColor;

in vec2 texCoord;

uniform sampler2D texture0;
uniform float alpha_threshold;

void main()
{
	outputColor = (texture(texture0, texCoord) * appliedColor);

	if(outputColor.a <= alpha_threshold)
		discard;
}