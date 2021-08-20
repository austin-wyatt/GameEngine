#version 330 core

layout(location = 0) out vec4 outputColor;

in vec2 primaryTextureCoordinates;
in vec2 mixedTextureCoordinates;

in vec2 outlineTextureCoordinates;
in float outline;
in vec4 outlineColor;


in vec4 primaryColor;
in float mixPercent;


in float alpha_threshold;


uniform sampler2D texture0;


void main()
{
	vec4 mainTexColor = texture(texture0, primaryTextureCoordinates);

	if(mixPercent == 0)
	{
		outputColor = mainTexColor * primaryColor;
	}
	else
	{
		vec4 mixTexColor = texture(texture0, mixedTextureCoordinates);

		mainTexColor = mixPercent < 1 ? mainTexColor * primaryColor : mainTexColor;

		outputColor = mix(mainTexColor, mixTexColor, mixPercent);
		
		outputColor[3] = mainTexColor[3]; //we want the alpha value from the original texture to stay
	}

	//Handle outline
	if(outline != 0)
	{
		vec4 outlineTexColor = texture(texture0, outlineTextureCoordinates);

		if(outlineTexColor[3] != 0)
		{
			outputColor = vec4(outlineColor);
		}
	}

//	outputColor = vec4(1, 0, 0, 1);

	if(outputColor.a < alpha_threshold)
		discard;

	if(gl_FrontFacing)  //discard if we are looking at the back of an object
		discard;
}
