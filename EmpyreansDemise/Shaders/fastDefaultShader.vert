#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in mat4 transform;
layout(location = 7) in vec4 aColor;
layout(location = 8) in vec4 compositeType; //composite vector of whether to enable the cam (0), the spritesheet position (1), the X and Y lengths of the spritesheet (2, 3)
layout(location = 9) in vec4 compositeType_2; //composite vector of spritesheet width, spritesheet height, use second texture, mix percent

layout(location = 10) in vec4 compositeType_3; //composite vector of inline thickness, outline thickness, alpha threshold, primary texture target
layout(location = 11) in vec4 aInlineColor;
layout(location = 12) in vec4 lightingCompositeType; //composite vector of whether the object should recieve lighting

out vec2 texCoord;
out vec4 appliedColor;
out float mixPercent;
out float twoTextures;

out float inlineThickness;
out float outlineThickness;
out vec4 inlineColor;
//out vec4 outlineColor;

out float alpha_threshold;

out float primaryTextureTarget;

out float enableLighting;
out vec3 Normal;
out vec3 FragPos;

uniform mat4 camera;

flat out int InstanceID; 

vec2 setTexCoord(vec2 texCoord, float columns, float rows, float column, float row);

float PHI = 1.61803398874989484820459;  // Φ = Golden Ratio   

float goldNoise(vec2 xy, float seed){
       return fract(tan(distance(xy*1.61803398874989484820459, xy)*seed)*xy.x);
}

void main(void)
{
	appliedColor = aColor;
	mixPercent = compositeType_2[3];
	texCoord = vec2(aTexCoord);
	twoTextures = compositeType_2[2];

	float columns = compositeType_2[0];
	float rows = compositeType_2[1];

    float row =  floor(compositeType[1] / columns);
	float column = compositeType[1] - row * columns;

//    if(rows != 2){
//		texCoord = setTexCoord(texCoord, columns, rows, column, row);
//	}

	texCoord = vec2(setTexCoord(texCoord, columns, rows, column, row));

	//Outline handling
	inlineColor = aInlineColor;
//	outlineColor = aOutlineColor;
	inlineThickness = compositeType_3[0];
	outlineThickness = compositeType_3[1];

	//Position handling
	float aspectRatio = compositeType[2] / compositeType[3];
	 
	vec3 pos = aPosition; 
	pos[0] *= aspectRatio; //allow for non-square objects


	alpha_threshold = compositeType_3[2];
	primaryTextureTarget = compositeType_3[3];

	enableLighting = lightingCompositeType[0];
	Normal = (vec4(aNormal, 0) * transform).xyz;

	if(compositeType[0] == 1)
	{
		gl_Position = vec4(pos, 1.0) * transform * camera;
	}
	else
	{
		gl_Position = vec4(pos, 1.0) * transform;
	}

	FragPos = vec3(vec4(pos, 1.0) * transform);

	InstanceID = gl_InstanceID; 
}

vec2 setTexCoord(vec2 texCoords, float columns, float rows, float column, float row)
{
	float minBoundX = column / columns;
    float maxBoundX = (column + compositeType[2]) / columns;

    float maxBoundY = row / rows;
    float minBoundY = (row + compositeType[3]) / rows;


	float xDiff = maxBoundX - minBoundX;
	float yDiff = maxBoundY - minBoundY;


	vec2 outTex = vec2(0, 0);

	outTex[0] = minBoundX;
	outTex[1] = minBoundY;


	outTex[0] += (texCoords[0] * xDiff);
	outTex[1] += (texCoords[1] * yDiff);

	return outTex;
}

