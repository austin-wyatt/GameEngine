#version 330
#extension GL_OES_standard_derivatives : enable

layout(location = 0) out vec4 outputColor;
//out vec4 outputColor; 

in vec4 appliedColor;
in float mixPercent;
in float twoTextures;

in vec2 texCoord;


uniform float alpha_threshold;

uniform sampler2D texture0;


void main()
{
	vec4 texColor = texture(texture0, texCoord);

	outputColor = texColor * appliedColor;

	//if the alpha is below the alpha threshold the pixel is discarded
	if(outputColor.a < alpha_threshold)
		discard;

//	if(gl_FrontFacing)  //discard if we are looking at the back of an object
//		discard;
}
