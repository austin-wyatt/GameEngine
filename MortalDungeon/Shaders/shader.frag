#version 330
#extension GL_OES_standard_derivatives : enable

layout(location = 0) out vec4 outputColor;
//out vec4 outputColor; 

in vec4 appliedColor;
in float mixPercent;
in float twoTextures;

in vec2 texCoord;

in vec2 texCoord2;

in vec2 xTexBounds;
in vec2 yTexBounds;

in float inlineThickness;
in float outlineThickness;
in vec4 inlineColor;
in vec4 outlineColor;

in float blurImage;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform float alpha_threshold;


void CreateOutline(vec4 textureColor, vec4 outlineColor, float thickness);
void CreateInline(vec4 textureColor, vec4 outlineColor, float thickness);

void main()
{
	vec4 texColor = texture(texture0, texCoord);

	if(twoTextures == 0)
	{
		outputColor = texColor * appliedColor;
	}
	else
	{
		vec4 texColor2 = texture(texture1, texCoord2);

		texColor = mixPercent < 1 ? texColor * appliedColor : texColor;

		outputColor = mix(texColor, texColor2, mixPercent);
		
		 

		outputColor[3] = texColor[3]; //we want the alpha value from the original texture to stay
	}


	//Handle outline and inline
	CreateOutline(texColor, outlineColor, outlineThickness);

	CreateInline(texColor, inlineColor, inlineThickness);
	
	
	//if the alpha is below the alpha threshold the pixel is discarded
	if(outputColor.a < alpha_threshold)
		discard;

	if(gl_FrontFacing)  //discard if we are looking at the back of an object
		discard;
}

void CreateOutline(vec4 textureColor, vec4 outlineColor, float thickness)
{
	float dx = dFdx(texCoord.x);
	float dy = dFdy(texCoord.y);


	vec4 colorU = texture2D(texture0, vec2(texCoord.x, texCoord.y - dy * thickness));
    vec4 colorD = texture2D(texture0, vec2(texCoord.x, texCoord.y + dy * thickness));
    vec4 colorL = texture2D(texture0, vec2(texCoord.x + dx * thickness, texCoord.y));
    vec4 colorR = texture2D(texture0, vec2(texCoord.x - dx * thickness, texCoord.y));
                
	
	outputColor = textureColor.a == 0.0 && (colorU.a != 0.0 || colorD.a != 0.0 || colorL.a != 0.0 || colorR.a != 0.0) ? outlineColor : outputColor;
//	outputColor = textureColor.a == 0.0 && (colorU.a != 0.0 || colorD.a != 0.0 || colorL.a != 0.0 || colorR.a != 0.0) 
//		&& (texCoord.x != xTexBounds[0] || texCoord.x != xTexBounds[1] || texCoord.y != yTexBounds[0] || texCoord.y != yTexBounds[1]) ? outlineColor : outputColor;
}

void CreateInline(vec4 textureColor, vec4 outlineColor, float thickness)
{
	float dx = dFdx(texCoord.x);
	float dy = dFdy(texCoord.y);


	vec4 colorU = texture2D(texture0, vec2(texCoord.x, texCoord.y - dy * thickness));
    vec4 colorD = texture2D(texture0, vec2(texCoord.x, texCoord.y + dy * thickness));
    vec4 colorL = texture2D(texture0, vec2(texCoord.x + dx * thickness, texCoord.y));
    vec4 colorR = texture2D(texture0, vec2(texCoord.x - dx * thickness, texCoord.y));
                

	outputColor = textureColor.a != 0.0 && (colorU.a == 0.0 || colorD.a == 0.0 || colorL.a == 0.0 || colorR.a == 0.0) ? outlineColor : outputColor;
}