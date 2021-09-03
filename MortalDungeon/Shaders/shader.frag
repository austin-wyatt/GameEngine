#version 420
#extension GL_OES_standard_derivatives : enable

layout(location = 0) out vec4 outputColor;
//out vec4 outputColor; 

in vec4 appliedColor;
in float mixPercent;
in float twoTextures;

in vec2 texCoord;

in vec2 texCoord2;

in float inlineThickness;
in float outlineThickness;
in vec4 inlineColor;
in vec4 outlineColor;

in float alpha_threshold;

in float primaryTextureTarget;

layout (binding = 0) uniform sampler2D texture0;
layout (binding = 1) uniform sampler2D texture1;
layout (binding = 2) uniform sampler2D texture2;
layout (binding = 3) uniform sampler2D texture3;
layout (binding = 4) uniform sampler2D texture4;

const float OUTLINE_ALPHA_STEP_WIDTH = 0.125;
const float OUTLINE_ALPHA_STEPS = 8;

void CreateOutline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture);
void CreateInline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture);
void DoWork(in sampler2D primaryTexture);

void main()
{
	switch(int(primaryTextureTarget))
	{
		case(0):
			DoWork(texture0);
			break;
		case(2):
			DoWork(texture2);
			break;
		case(3):
			DoWork(texture3);
			break;
		case(4):
			DoWork(texture4);
			break;
		default:
			DoWork(texture0);
			break;
	};
}

void DoWork(in sampler2D primaryTexture)
{
	vec4 texColor = texture(primaryTexture, texCoord);

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
	if(outlineThickness > 0)
	{
		CreateOutline(texColor, outlineColor, outlineThickness, primaryTexture);
	}
	

	if(inlineThickness > 0)
	{
		CreateInline(texColor, inlineColor, inlineThickness, primaryTexture);
	}
	
	//if the alpha is below the alpha threshold the pixel is discarded
	if(outputColor.a < alpha_threshold)
		discard;

//	if(gl_FrontFacing)  //discard if we are looking at the back of an object
//		discard;
}

void CreateOutline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture)
{
	if(textureColor.w < 1)
	{
		if(textureColor.w > OUTLINE_ALPHA_STEP_WIDTH * (OUTLINE_ALPHA_STEPS - thickness))
		{
			outputColor = outlineColor;
		}
	}
}

void CreateInline(vec4 textureColor, vec4 outlineColor, float thickness, in sampler2D primaryTexture)
{
	float dx = dFdx(texCoord.x);
	float dy = dFdy(texCoord.y);


	vec4 colorU = texture2D(primaryTexture, vec2(texCoord.x, texCoord.y - dy * thickness));
    vec4 colorD = texture2D(primaryTexture, vec2(texCoord.x, texCoord.y + dy * thickness));
    vec4 colorL = texture2D(primaryTexture, vec2(texCoord.x + dx * thickness, texCoord.y));
    vec4 colorR = texture2D(primaryTexture, vec2(texCoord.x - dx * thickness, texCoord.y));
                

	outputColor = textureColor.a != 0.0 && (colorU.a == 0.0 || colorD.a == 0.0 || colorL.a == 0.0 || colorR.a == 0.0) ? outlineColor : outputColor;
}