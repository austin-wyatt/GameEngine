#version 330

out vec4 outputColor; 

in vec4 appliedColor;
in float mixPercent;

//uniform vec4 ourColor;
in vec2 texCoord;

uniform sampler2D texture0;

void main()
{
	//outputColor = vertexColor;
	//outputColor = ourColor;
	if(mixPercent == 1)
	{
		outputColor = appliedColor;
	}
	else if(mixPercent > 0){
		outputColor = mix(texture(texture0, texCoord), appliedColor, mixPercent);
	}
	else
	{
		outputColor = texture(texture0, texCoord);
	}
}