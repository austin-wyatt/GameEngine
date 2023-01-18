#version 430 core
layout(location = 0) out vec4 outputColor;

in vec2 texCoord;
in vec4 appliedColor;

in float atlasIndex;

uniform sampler2D[8] glyphTextureAtlas;

void main(void)
{
	vec4 sampledColor = texture(glyphTextureAtlas[int(atlasIndex)], texCoord);

	outputColor = sampledColor * appliedColor;

//	outputColor = vec4(1, 0, 0, 1);

	if(outputColor.w < 0.05)
		discard;
}
