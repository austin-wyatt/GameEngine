#version 330

out vec4 outputColor; 

in vec4 appliedColor;
in float mixPercent;
in float twoTextures;

in vec2 texCoord;

in vec2 texCoord2;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform float alpha_threshold;

void main()
{
	vec4 texColor = texture(texture0, texCoord);

	if(twoTextures == 0)
	{
		if(mixPercent == -10)
			outputColor = appliedColor;
		else
			outputColor = texColor * appliedColor;
	}
	else
	{
		vec4 texColor2 = texture(texture1, texCoord2);

		if(mixPercent == -10)
			outputColor = appliedColor;
		else
			outputColor = mix(texColor, texColor2, mixPercent) * appliedColor;
	}
	
	
	//if the alpha is below the alpha threshold the pixel is discarded
	if(outputColor.a < alpha_threshold)
		discard;

	if(gl_FrontFacing)  //discard if we are looking at the back of an object
		discard;
}