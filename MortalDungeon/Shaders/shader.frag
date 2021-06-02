#version 330

out vec4 outputColor; 

in vec4 appliedColor;
in float mixPercent;
in float twoTextures;

//uniform vec4 ourColor;
in vec2 texCoord;

in vec2 texCoord2;

uniform sampler2D texture0;
uniform sampler2D texture1;
//uniform float alpha_threshold;

void main()
{
	//outputColor = vertexColor;
	//outputColor = ourColor;
//	if(mixPercent == 1)
//	{
//		outputColor = texture(texture0, texCoord) * appliedColor;
//	}
//	else if(mixPercent > 0){
//		outputColor = mix(texture(texture0, texCoord), appliedColor, mixPercent);
//	}
//	else
//	{
//		outputColor = texture(texture0, texCoord);
//	}

	vec4 texColor = texture(texture0, texCoord);

	if(twoTextures == 0)
	{
		if(mixPercent == -10)
			outputColor = appliedColor;
		else
			outputColor = texColor * appliedColor;

		if(outputColor.a == 0)
			discard;
	}
	else
	{
		vec4 texColor2 = texture(texture1, texCoord2);

		if(mixPercent == -10)
			outputColor = appliedColor;
		else
			outputColor = mix(texColor, texColor2, mixPercent) * appliedColor;
	}
	

	if(texColor.a == 0)
		discard;

//	if(gl_FrontFacing)  //discard if we are looking at the back of an object
//		discard;

	
}