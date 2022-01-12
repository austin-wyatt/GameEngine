#version 420

layout(location = 0) out vec4 outputColor;

in vec4 appliedColor;

in vec2 texCoord;

uniform sampler2D texture0;

in float alpha_threshold;
in vec3 FragPos;

in vec4 ScissorData;

const float OUTLINE_MIN_VALUE0 = 0.4;
const float OUTLINE_MIN_VALUE1 = 0.5;
const float OUTLINE_MAX_VALUE0 = 1.1;
const float OUTLINE_MAX_VALUE1 = 1.1;

in float BOLD;
in float OUTLINE;

const vec4 OUTLINE_COLOR = vec4(0, 0, 0, 1);

void main()
{
	float yPos = (FragPos.y + 1) / 2;
	float xPos = (FragPos.x + 1) / 2;
	if(ScissorData.z > 0 && ScissorData.w > 0)
	{
		if((xPos < ScissorData.x) || (xPos > ScissorData.x + ScissorData.z) || 
		   (yPos < ScissorData.y) || (yPos > ScissorData.y + ScissorData.w))
		   discard;
	}

	vec4 texColor = texture(texture0, texCoord);

	outputColor = texColor * appliedColor;

	//outline
	float distAlphaMask = outputColor.a;
	if(OUTLINE > 0 && 
	   distAlphaMask >= OUTLINE_MIN_VALUE0 &&
	   distAlphaMask <= OUTLINE_MAX_VALUE1)
	{
		float oFactor = 1.0;
		if(distAlphaMask <= OUTLINE_MIN_VALUE1)
		{
			oFactor = smoothstep( OUTLINE_MIN_VALUE0, OUTLINE_MIN_VALUE1, distAlphaMask);
		}
		else
		{
			oFactor = smoothstep( OUTLINE_MAX_VALUE1, OUTLINE_MAX_VALUE0, distAlphaMask);
		}

//		outputColor = mix(outputColor, OUTLINE_COLOR, oFactor);
		outputColor = OUTLINE_COLOR;
	}

	if(BOLD > -1 && distAlphaMask >= 0.4 && distAlphaMask <= 0.5)
	{
		outputColor = appliedColor;
	}
	
	//if the alpha is below the alpha threshold the pixel is discarded
	if(outputColor.a < 0.3)
	{
		discard;
	}
	
	outputColor.a = 1;
}
