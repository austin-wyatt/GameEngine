#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in mat4 transform;
layout(location = 6) in vec4 aColor;
layout(location = 7) in vec4 compositeType; //composite vector of whether to enable the cam (0), the spritesheet position (1), the X and Y lengths of the spritesheet (2, 3)
layout(location = 8) in vec4 compositeType_2; //composite vector of spritesheet width, spritesheet height, use second texture, mix percent

layout(location = 9) in vec4 compositeType_3; //composite vector of inline thickness, outline thickness, alpha threshold, primary texture target
layout(location = 10) in vec4 aInlineColor;
layout(location = 11) in vec4 aOutlineColor;

out vec2 texCoord;
out vec2 texCoord2;
out vec4 appliedColor;
out float mixPercent;
out float twoTextures;

out vec2 xTexBounds;
out vec2 yTexBounds;

out float inlineThickness;
out float outlineThickness;
out vec4 inlineColor;
out vec4 outlineColor;

out float alpha_threshold;

out float primaryTextureTarget;

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

	float columns = compositeType_2[1];
	float rows = compositeType_2[0];

    float row =  floor(compositeType[1] / rows);
	float column = compositeType[1] - row * rows;

    
	texCoord = setTexCoord(texCoord, columns, rows, column, row);

	//Multi texture handling
	//going to hardcode values here for the time being (using fog spritesheet)
	columns = 2;
	rows = 2;
	row = floor(abs(goldNoise(vec2(transform[0][3], transform[1][3]), fract(transform[2][2]) + 1)) * 2);
	column = floor(abs(goldNoise(vec2(transform[0][3], transform[1][3]), fract(transform[1][2]) + 2)) * 2);

	texCoord2 = vec2(aTexCoord.x * compositeType_2[1] / columns + (column / columns), aTexCoord.y * compositeType_2[0] * -1 / rows + (row / rows));

	//Outline handling
	inlineColor = aInlineColor;
	outlineColor = aOutlineColor;
	inlineThickness = compositeType_3[0];
	outlineThickness = compositeType_3[1];

	//Position handling
	float aspectRatio = compositeType[2] / compositeType[3];

	vec3 pos = aPosition; 
	pos[0] *= aspectRatio; //allow for non-square objects


	alpha_threshold = compositeType_3[2];
	primaryTextureTarget = compositeType_3[3];


	if(compositeType[0] == 1)
	{
		gl_Position = vec4(pos, 1.0) * transform * camera;
	}
	else
	{
		gl_Position = vec4(pos, 1.0) * transform;
	}

	

	InstanceID = gl_InstanceID; 
}

vec2 setTexCoord(vec2 texCoord, float columns, float rows, float column, float row)
{
	float minBoundX = column / columns;
    float maxBoundX = (column + compositeType[2]) / columns;

    float maxBoundY = row / rows;
    float minBoundY = (row + compositeType[3]) / rows;

    if (maxBoundX > 1)
    {
        minBoundX = (column - compositeType[2]) / columns;
        maxBoundX = 1;
    }

    if (maxBoundY > 1) 
    {
        maxBoundY = (row - compositeType[3]) / rows;
        minBoundY = 1;
    }

	if(texCoord[0] == 0)
	{
		texCoord[0] = minBoundX;
	}
	else
	{
		texCoord[0] = maxBoundX;
	}

	if(texCoord[1] == 0)
	{
		texCoord[1] = minBoundY;
	}
	else
	{
		texCoord[1] = maxBoundY;
	}

	xTexBounds = vec2(minBoundX, maxBoundX);
	yTexBounds = vec2(minBoundY, maxBoundY);

	return texCoord;
}

